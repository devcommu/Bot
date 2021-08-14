using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevCommuBot.Services
{
    internal class GuildService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly IConfigurationRoot _config;
        private readonly UtilService _util;
        public GuildService(IServiceProvider services)
        {
            _config = services.GetRequiredService<IConfigurationRoot>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<GuildService>>();
            _util = services.GetRequiredService<UtilService>();
            _client.UserJoined += OnUserJoin;
            _client.LeftGuild += OnUserLeft;
            _client.Ready += OnReady;
            _client.InteractionCreated += OnInteraction;
            _client.GuildMemberUpdated += OnGuildUpdate;
        }

        private async Task OnGuildUpdate(Cacheable<SocketGuildUser, ulong> cachedMember, SocketGuildUser member)
        {
            //Boost tracker
            SocketGuildUser oldMember = cachedMember.Value ?? await cachedMember.DownloadAsync();
            if(oldMember.Roles.Count != member.Roles.Count)
            {
                //Roles Moved
                var added = member.Roles.Except(oldMember.Roles).ToList();
                var removed = oldMember.Roles.Except(member.Roles).ToList();
                if(removed.Count is not 0)
                {
                    //A roles have been removed to member
                    if(removed.Any(r=> r.Id == UtilService.ROLE_BOOSTERS_ID))
                    {
                        //user is not boosting any more
                        var roles = "";
                        member.Roles.ToList().ForEach(r =>
                        {
                            roles += $"{r.Mention} \n";
                        });
                        _util.SendLog($"{member} unboosted", $"> He has these roles:\n{roles}", member);
                        //Check if user had a custom role:
                        if (_util.HasCustomRole(member))
                        {
                            //user had a custom role
                            var customRole = member.Roles.First(r => r.Position > _util.GetBoostersRole().Position);
                            await member.RemoveRoleAsync(customRole, options: new()
                            {
                                AuditLogReason = "User has stopped boost"
                            });
                            _util.SendLog($"Removed CustomRole to {member}", $"Role: {customRole}\n Cause: No more boosting!", member);
                            //TODO: Remove Role!
                        }

                    }
                }
                if(added.Count is not 0)
                {
                    if(added.Any(r=> r.Id == UtilService.ROLE_BOOSTERS_ID))
                    {
                        //User has boosted!
                        var embed = new EmbedBuilder()
                            .WithAuthor(member)
                            .WithColor(_util.EmbedColor)
                            .WithTitle($"{member} vient de booster!")
                            .WithDescription("> **Merci d'avoir booster!!!**\nEn boostant vous avez accès à la commande `/createrole` vous permettant ainsi de crée votre propre role")
                            .WithCurrentTimestamp()
                            .Build();
                        _util.GetBoostersChannel().SendMessageAsync(embed: embed);
                    }
                }
            }
            return;
        }

        private Task OnInteraction(SocketInteraction arg)
        {
            if(arg is SocketSlashCommand command)
            {
                var member = command.User as SocketGuildUser;

                if(command.Data.Name == "points")
                {
                    //TODO: Use Database
                    if (command.Data.Options is not null)
                        command.RespondAsync("he has 0 points");
                    else
                        command.RespondAsync("you have 0points");
                }

                if(command.Data.Name == "joinrole")
                {
                    if (command.Data.Options.FirstOrDefault().Value.Equals("projects"))
                    {
                        //user chose to join "projets" role
                        if(member.Roles.Any(r=>r.Id == UtilService.ROLE_PROJECTS_ID))
                        {
                            //user has already the role
                            command.RespondAsync("Vous poddédez déjà ce role");
                        }
                        else
                        {
                            member.AddRoleAsync(_util.GetProjectsRole());
                            command.RespondAsync("Vous venez de rejoindre le role Projects vous donnant accès au salon: <#874785010601832468>");
                        }
                    }
                    else
                    {
                        //user chose to join "gaming" role
                        if (member.Roles.Any(r => r.Id == UtilService.ROLE_GAMING_ID))
                        {  
                            //user has already the role
                            command.RespondAsync("Vous possédez déjà ce role!");
                        }
                        else
                        {
                            member.AddRoleAsync(_util.GetGamingRole());
                            command.RespondAsync("Vous venez de rejoindre le role Gaming vous donnant accès au salon: <#null>");
                        }
                    }
                }
                if(command.Data.Name == "hms")
                {
                    //Partnership information
                    var compo = new ComponentBuilder()
                        .WithButton("HostMyServers", null, ButtonStyle.Link, url: "https://www.hostmyservers.fr/")
                        .Build();
                    var embedHms = new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithAuthor("HostMyServers", "https://www.hostmyservers.fr/assets/images/logo/Logo-HMS-color-icon.png")
                        .WithTitle("HostMyServers - Location de VPS et Serveur Gaming")
                        .WithDescription("HostMyServers propose des VPS et des serveurs Gaming (Minecraft, Mcpe, Gmod), mais aussi des noms de domaine et des hébergements web. Depuis 2014, l’hébergeur présente plusieurs offres avec un rapport qualité/prix imbattable.\n\n> *Utilisez le code promo suivant*:\n **DEVCOMMU**\nAfin de profiter de 20% de réduction!")
                        .WithCurrentTimestamp()
                        .WithFooter("Partenaire depuis le 07/08/2021")
                        .Build();
                    //Why not emepheral?
                    command.RespondAsync(embed: embedHms, component: compo);
                }
                if(command.Data.Name == "createrole")
                {
                    if(command.Channel.Id != UtilService.CHANNEL_BOOSTERS_ID)
                    {
                        command.RespondAsync("You can not use this command here", ephemeral: true);
                        return Task.CompletedTask;
                    }

                }
            }
            if(arg is SocketMessageComponent component)
            {
                //Button Integrations?

            }
            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            _logger.LogInformation("Registering commands");
            var cmds = await _client.Rest.GetGuildApplicationCommands(UtilService.GUILD_ID);
            if (cmds.FirstOrDefault(c=> c.ApplicationId == _client.CurrentUser.Id) is not null)
            {
                //Register commands
                List<SlashCommandOptionBuilder> listOptionUser = new()
                {
                    new()
                    {
                        Name = "user",
                        Required = false,
                        Type = ApplicationCommandOptionType.User,
                        Description = "User your want to see points"
                    }
                };
                var listOptionJoin = new List<SlashCommandOptionBuilder>()
                {
                    new()
                    {
                        Name = "role",
                        Required = true,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Role you want to join",
                        Choices = new()
                        {
                            new ApplicationCommandOptionChoiceProperties()
                            {
                                Name = "Project",
                                Value = "projects"
                            },
                            new()
                            {
                                Name = "Gaming",
                                Value = "gaming"
                            }
                        }
                    }
                };
                var listOptionRole = new List<SlashCommandOptionBuilder>()
                {
                    new()
                    {
                        Name = "rolename",
                        Required = true,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Name of the role you want to create",
                    },
                    new()
                    {
                        Name = "color",
                        Required = true,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Color of your role in hexadecimal",
                    }
                };
                SlashCommandBuilder pointsCommand = new()
                {
                    Name = "points",
                    Description = "Get Your amount of points",
                    Options = listOptionUser,
                };
                SlashCommandBuilder joinroleCommand = new()
                {
                    Name = "joinrole",
                    Description = "Join a role",
                    Options = listOptionJoin,
                };
                SlashCommandBuilder hmsCommand = new()
                {
                    Name = "hms",
                    Description = "Get information about our partner"
                };
                SlashCommandBuilder createRoleCommand = new()
                {
                    Name = "createrole",
                    Description = "Create your own role!(boosters)",
                    Options = listOptionRole
                };
                try
                {
                    await _client.Rest.CreateGuildCommand(pointsCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(joinroleCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(hmsCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(createRoleCommand.Build(), UtilService.GUILD_ID);
                }
                catch (ApplicationCommandException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                    _logger.LogError(json);
                }
            }
        }

        private Task OnUserLeft(SocketGuild arg)
        {
            throw new NotImplementedException();
        }

        private Task OnUserJoin(SocketGuildUser arg)
        {
            throw new NotImplementedException();
        }
    }
}
