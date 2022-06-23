using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevCommuBot.Services
{
    internal class GuildService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly IConfigurationRoot _config;
        private readonly UtilService _util;
        private readonly PointService _pointService;
        private readonly DataService _database;

        public GuildService(IServiceProvider services)
        {
            _config = services.GetRequiredService<IConfigurationRoot>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<GuildService>>();
            _util = services.GetRequiredService<UtilService>();
            _database = services.GetRequiredService<DataService>();
            _pointService = services.GetRequiredService<PointService>();

            _client.UserJoined += OnUserJoin;
            _client.LeftGuild += OnUserLeft;
            _client.Ready += OnClientReady;
            /*_client.Ready += OnReady;
            _client.InteractionCreated += OnInteraction;*/
            _client.GuildMemberUpdated += OnGuildUpdate;
            _client.MessageReceived += OnMessageReceive;
        }

        private async Task OnClientReady()
        {
            var channel = _util.GetGuild().GetTextChannel(738875544858525829);
            var obj = await channel.GetMessagesAsync(limit: 5).ToListAsync();
            var messages = obj[0];
            var message = messages.Where(m => m.Author.Id == _client.CurrentUser.Id)?.FirstOrDefault();
            if(message != null)
            {

            }
            else
            {
                //Pas de message envoyé.
                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Choisissez un role")
                    .WithCustomId("role-selection")
                    .WithMinValues(1)
                    .WithMaxValues(1)
                    .AddOption("Projects", "projects", "Accès aux notification de webhook(inutile)")
                    .AddOption("Gaming", "gaming", "Obtenez le role gaming et devenez cool (imo)");

                var builder = new ComponentBuilder()
                    .WithSelectMenu(menuBuilder);
                await channel.SendMessageAsync("Choisissez un role:", components: builder.Build());
            }
            _client.Ready -= OnClientReady;
        }

        private Task OnMessageReceive(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message)
                return Task.CompletedTask;
            if (message.Source is not MessageSource.User)
                return Task.CompletedTask;
            /*if (_util.GetAllowedChannels().Exists(c => c is not null and c.Id == message.Channel.Id) is false)
                return Task.CompletedTask;*/
            _pointService.HandleMessage(message);
            //Verfier si ça parle d'hebergeur
            if(message.Content.Contains("hébergeur") || message.Content.Contains("hebergeur"))
            {
                //verifier perm user
            }
            return Task.CompletedTask;
        }

        private async Task OnGuildUpdate(Cacheable<SocketGuildUser, ulong> cachedMember, SocketGuildUser member)
        {
            //Boost tracker
            SocketGuildUser oldMember = cachedMember.Value ?? await cachedMember.DownloadAsync();
            if (oldMember.Roles.Count != member.Roles.Count)
            {
                //Roles Moved
                var added = member.Roles.Except(oldMember.Roles).ToList();
                var removed = oldMember.Roles.Except(member.Roles).ToList();
                if (removed.Count is not 0)
                {
                    //A roles have been removed to member
                    if (removed.Any(r => r.Id == UtilService.ROLE_BOOSTERS_ID))
                    {
                        //user is not boosting any more
                        var roles = "";
                        member.Roles.ToList().ForEach(r =>
                        {
                            roles += $"{r.Mention} \n";
                        });
                        _util.SendLog($"{member} a retiré son boost", $"> Il avait ses roles:\n{roles}", member);
                        //Check if user had a custom role:
                        if (_util.HasCustomRole(member))
                        {
                            //user had a custom role
                            var customRole = member.Roles.First(r => r.Position > _util.GetBoostersRole().Position);
                            await member.RemoveRoleAsync(customRole, options: new()
                            {
                                AuditLogReason = "Le joueur ne boost plus"
                            });
                            _util.SendLog($"{member} a perdu son role personnalisé", $"Role: {customRole}\n Raison: Ne boost plus", member);                        }
                    }
                }
                if (added.Count is not 0)
                {
                    if (added.Any(r => r.Id == UtilService.ROLE_BOOSTERS_ID))
                    {
                        //User has boosted!
                        var embed = new EmbedBuilder()
                            .WithAuthor(member)
                            .WithColor(_util.EmbedColor)
                            .WithTitle($"{member} vient de booster!")
                            .WithDescription("> **Merci d'avoir boosté!!!**\nEn boostant vous avez accès à la commande `/createrole` vous permettant de créer votre propre rôle")
                            .WithCurrentTimestamp()
                            .Build();
                        _util.GetBoostersChannel().SendMessageAsync(text: member.Mention, embed: embed);
                    }
                }
            }
            return;
        }

        private Task OnInteraction(SocketInteraction arg)
        {
            if (arg is SocketMessageComponent component)
            {
                //Button Integrations?
            }
            return Task.CompletedTask;
        }


        private Task OnUserLeft(SocketGuild member)
        {
            //zzzzzzzzzzzzzzz
            return Task.CompletedTask;
        }

        private Task OnUserJoin(SocketGuildUser member)
        {
            var embedMessage = new EmbedBuilder()
                .WithAuthor(member)
                .WithColor(_util.EmbedColor)
                .WithDescription("Welcome!")
                .WithFooter($"We are now {member.Guild.MemberCount} members.")
                .Build();
            _util.GetWelcomeChannel().SendMessageAsync(embed: embedMessage);
            return Task.CompletedTask;
        }
    }
}