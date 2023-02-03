using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Camille.RiotGames;

using DevCommuBot.Data.Models.Forums;
using DevCommuBot.Data.Models.Users;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevCommuBot.Services
{
    public class UtilService
    {
        private readonly DiscordSocketClient _client;
        private readonly DataService database;
        private readonly ILogger _logger;

        public const ulong GUILD_ID = 584987515388428300;

        public const ulong CHANNEL_LOGS_ID = 875824516675305502;
        public const ulong CHANNEL_BOOSTERS_ID = 764467968490864691;
        public const ulong CHANNEL_WELCOME_ID = 881262458398986280;
        public const ulong CHANNEL_STARBOARD_ID = 990223161331167322;
        public const ulong CHANNEL_ROLES_ID = 1056941109877669898;
        public const ulong CHANNEL_VOSPROJETS_ID = 1027262989428068364;
        public const ulong CHANNEL_DEBATS_ID = 1028042928121200700;

        public const ulong ROLE_PROJECTS_ID = 874785049516605491;
        public const ulong ROLE_GAMING_ID = 875757898087678034;
        public const ulong ROLE_BOOSTERS_ID = 642107269940772871; //Role created by Discord
        public const ulong ROLE_DEVCHATS_ID = 1005237731309404170;
        public const ulong ROLE_FREEGAMES_ID = 1056927799090348113;// will get pinged when free games are available
        public const ulong ROLE_DISCORDGAMES_ID = 1057061877563277312; //Play on Okureta

        public const int MIN_REACTION_STARBOARD = 5;

        public readonly Color EmbedColor = new(19, 169, 185);

        //Todo: no need to have cooldown
        public readonly Dictionary<ulong, long> CreateroleCooldown = new();
        public Dictionary<ulong, List<ulong>> Giveaways = new(); // <message id, List<user>>
        public readonly RiotGamesApi Riot;
        private readonly IConfigurationRoot _config;

        public UtilService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<UtilService>>();
            _config = services.GetRequiredService<IConfigurationRoot>();
            database = services.GetRequiredService<DataService>();
            Riot = RiotGamesApi.NewInstance(_config["riotToken"]);
            _client.ButtonExecuted += OnButtonExecuted;
        }

        private Task OnButtonExecuted(SocketMessageComponent cmp)
        {
            //Todo: Use database
            //Giveaway check
            if (database.GetGiveaway(cmp.Message.Id) is not null)
            {
                //Giveaway
                if (Giveaways[cmp.Message.Id].Contains(cmp.User.Id))
                {
                    cmp.RespondAsync("Vous êtes déjà inscris!", ephemeral: true);
                }
                else
                {
                    Giveaways[cmp.Message.Id].Add(cmp.User.Id);
                    cmp.RespondAsync("Enregistré!", ephemeral: true);
                }
            }
            else
            {
                //Unable to find this giveaway, maybe end of it
                cmp.RespondAsync("Impossible de retrouver ce giveaway désolé, est il déjà terminé?", ephemeral: true);
            }
            return Task.CompletedTask;
        }

        public async Task MemberUnboosted(SocketGuildUser member)
        {
            var roles = "";
            member.Roles.ToList().ForEach(r =>
            {
                roles += $"{r.Mention} \n";
            });
            SendLog($"{member} a retiré son boost", $"> Il avait ses roles:\n{roles}", member);
            //Check if user had a custom role:
            if (HasCustomRole(member))
            {
                //user had a custom role
                var customRole = member.Roles.First(r => r.Position > GetBoostersRole().Position);
                await member.RemoveRoleAsync(customRole, options: new()
                {
                    AuditLogReason = "Le joueur ne boost plus"
                });
                SendLog($"{member} a perdu son role personnalisé", $"> Il possédait le role personnalisé:\n{customRole.Mention}['{customRole.Name}']\n**Suppresion du role requise!**", member);
            }
            //Todo: Rework, should only user BoosterAdvantage and nothing else
            var user = await database.GetAccount(member.Id);
            if (user is null)
                return;
            if (user.BoosterAdvantage is not null)
            {
                if (user.BoosterAdvantage.VocalId is not null)
                {
                    var channel = GetGuild().GetVoiceChannel(user.BoosterAdvantage.VocalId.Value);
                    if (channel is not null)
                    {
                        await channel.DeleteAsync();
                        SendLog("Suppression d'un channel vocal", $"> Le channel vocal {channel.Mention} a été supprimé car le boosteur n'a plus boosté le discord", member);
                    }
                }
                if (user.BoosterAdvantage.RoleId is not null)
                {
                    var role = GetGuild().GetRole(user.BoosterAdvantage.RoleId.Value);
                    if (role is not null)
                    {
                        await role.DeleteAsync();
                        SendLog("Suppression d'un role", $"> Le role {role.Mention} a été supprimé car le boosteur n'a plus boosté le discord", member);
                    }
                }
                user.BoosterAdvantage = null;
                await database.UpdateAccount(user);
            }
        }

        public async Task MemberBoosted(SocketGuildUser member)
        {
            var embed = new EmbedBuilder()
                            .WithAuthor(member)
                            .WithColor(EmbedColor)
                            .WithTitle($"{member} vient de booster!")
                            .WithDescription("> **Merci d'avoir boosté!!!**\nEn boostant vous avez accès à la commande `/createrole` vous permettant de créer votre propre rôle")
                            .WithCurrentTimestamp()
                            .Build();
            await GetBoostersChannel().SendMessageAsync(text: $"Merci d'avoir booster le discord {member.Mention}", embed: embed);
            SendLog($"{member} vient de booster le discord", $"> Merci à lui!", member);
            var user = await database.GetAccount(member.Id);
            if (user is null)
                return;
            _ = CreateBoosterAdvantage(user, member);
        }
        
        internal async Task CreateBoosterAdvantage(User user, SocketGuildUser member)
        {
            user.BoosterAdvantage = new BoosterAdvantage
            {
                Since = member.PremiumSince.Value.DateTime
            };
            await database.UpdateAccount(user);
        }

        /// <summary>
        /// Check if a channel is a forum channel
        /// </summary>
        /// <param name="channel">channel</param>
        /// <returns>true if channel is a forum</returns>
        public bool IsAForum(SocketGuildChannel channel)
        {
            if (channel is SocketThreadChannel thread)
                return IsAForum(thread.ParentChannel);
            //this discord has 2 forums that is not linked to development so it will count as not a forum
            if (channel.Id is CHANNEL_VOSPROJETS_ID or CHANNEL_DEBATS_ID)
                return false;
            return channel is SocketForumChannel;
        }

        /// <summary>
        /// Try to get a forum or create one
        /// </summary>
        /// <param name="forum">the forum channel</param>
        /// <returns>The Forum instance</returns>
        internal async Task<Forum> ForceGetForum(SocketForumChannel forum)
        {
            var forumDb = await database.GetForum(forum.Id);
            if (forumDb is null)
            {
                //Hmm forum created when i was sleeping
                await forum.ModifyAsync(f =>
                {
                    var closedTag = new ForumTagBuilder("Closed", isModerated: true, emoji: new Emoji("🔒")).Build();
                    if (!f.Tags.IsSpecified)
                    {
                        //No Tags will create some
                        f.Tags = new List<ForumTagProperties>()
                        {
                                closedTag
                        };
                    }
                    else
                    {
                        var newValue = f.Tags.Value.ToList();
                        newValue.Add(closedTag);
                        f.Tags = newValue;
                    }
                });
                await database.CreateForum(forum.Id);
                SendLog("Forum Discovered!", "Registered a new forum!!\n Now adding it to the database.");
                forumDb = await database.GetForum(forum.Id);
            }
            return forumDb;
        }

        public bool IsAForum(ISocketMessageChannel channel)
        {
            if (channel is SocketThreadChannel thread)
                return IsAForum(thread.ParentChannel);
            //this discord has 2 forums that is not linked to development so it will count as not a forum
            if (channel.Id is CHANNEL_VOSPROJETS_ID or CHANNEL_DEBATS_ID)
                return false;
            return channel is SocketForumChannel;
        }

        public SocketGuild GetGuild()
            => _client.Guilds.FirstOrDefault(g => g.Id == GUILD_ID);

        /// <summary>
        /// Return allowed channels for earning points
        /// </summary>
        /// <returns></returns>
        public List<SocketGuildChannel> GetAllowedChannels()
            => new()
            {
                _client.GetChannel(744957552143368243) as SocketGuildChannel, //PHP
                _client.GetChannel(744999327608602665) as SocketGuildChannel, //javascript
                _client.GetChannel(744999373691420823) as SocketGuildChannel, //java
                _client.GetChannel(745177811672760470) as SocketGuildChannel, //html
                _client.GetChannel(788071924453867561) as SocketGuildChannel, //go
                _client.GetChannel(777236245322662019) as SocketGuildChannel, //python
                _client.GetChannel(784890400234143804) as SocketGuildChannel, //others
                _client.GetChannel(0) as SocketGuildChannel,
                _client.GetChannel(0) as SocketGuildChannel,
                _client.GetChannel(0) as SocketGuildChannel,
            };

        public SocketRole GetProjectsRole()
            => GetGuild()?.GetRole(ROLE_PROJECTS_ID);

        public SocketRole GetGamingRole()
            => GetGuild()?.GetRole(ROLE_GAMING_ID);

        public SocketRole GetBoostersRole()
            => GetGuild()?.GetRole(ROLE_BOOSTERS_ID);

        public SocketTextChannel GetLogChannel()
            => _client.GetChannel(CHANNEL_LOGS_ID) as SocketTextChannel;

        public SocketTextChannel GetBoostersChannel()
            => _client.GetChannel(CHANNEL_BOOSTERS_ID) as SocketTextChannel;

        public SocketTextChannel GetWelcomeChannel()
            => _client.GetChannel(CHANNEL_WELCOME_ID) as SocketTextChannel;

        public SocketTextChannel GetStarboardChannel()
            => _client.GetChannel(CHANNEL_STARBOARD_ID) as SocketTextChannel;

        public void SendLog(string title, string description, SocketGuildUser author = null)
        {
            var embed = new EmbedBuilder()
                .WithColor(EmbedColor)
                .WithAuthor(author)
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter("Still in development")
                .WithCurrentTimestamp()
                .Build();
            GetLogChannel().SendMessageAsync(embed: embed);
        }

        public bool HasCustomRole(SocketGuildUser member)
            => member.Roles.Any(role => role.Position > GetBoostersRole().Position) && member.GuildPermissions.Administrator is not true;

        public SocketRole GetCustomRole(SocketGuildUser member)
            => member.GuildPermissions.Administrator ? null : member.Roles.FirstOrDefault(role => role.Position > GetBoostersRole().Position); //Maybe last?
    }
}