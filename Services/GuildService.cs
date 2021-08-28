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
            if (arg is SocketSlashCommand command)
                HandleSlashCommand(command);
            if(arg is SocketMessageComponent component)
            {
                //Button Integrations?

            }
            return Task.CompletedTask;
        }
        private async Task HandleSlashCommand(SocketSlashCommand command)
        {
            var member = command.User as SocketGuildUser;
            switch (command.Data.Name)
            {
                case "points":
                    if (command.Data.Options is not null)
                        await command.RespondAsync("he has 0 points");
                    else
                        await command.RespondAsync("you have 0points");
                    break;
                case "joinrole":
                    if (command.Data.Options.FirstOrDefault().Value.Equals("projects"))
                    {
                        //user chose to join "projets" role
                        if (member.Roles.Any(r => r.Id == UtilService.ROLE_PROJECTS_ID))
                        {
                            //user has already the role
                            await command.RespondAsync("Vous poddédez déjà ce role");
                        }
                        else
                        {
                            await member.AddRoleAsync(_util.GetProjectsRole());
                            await command.RespondAsync("Vous venez de rejoindre le role Projects vous donnant accès au salon: <#874785010601832468>");
                        }
                    }
                    else
                    {
                        //user chose to join "gaming" role
                        if (member.Roles.Any(r => r.Id == UtilService.ROLE_GAMING_ID))
                        {
                            //user has already the role
                            await command.RespondAsync("Vous possédez déjà ce role!");
                        }
                        else
                        {
                            await member.AddRoleAsync(_util.GetGamingRole());
                            await command.RespondAsync("Vous venez de rejoindre le role Gaming vous donnant accès au salon: <#null>");
                        }
                    }
                    break;
                case "hms":
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
                    await command.RespondAsync(embed: embedHms, component: compo);
                    break;
                case "createrole":
                    if (command.Channel.Id != UtilService.CHANNEL_BOOSTERS_ID)
                    {
                        await command.RespondAsync("Vous ne pouvez pas utilisez cette commande ici", ephemeral: true);
                        return;
                    }
                    if (_util.HasCustomRole(member))
                    {
                        await command.RespondAsync("Vous possédez déjà un grade custom! La modification du grade n'est pas encore permise!", ephemeral: true);
                        return;
                    }
                    var roleName = command.Data.Options.FirstOrDefault(op => op.Name == "rolename").Value as string;
                    var color = command.Data.Options.FirstOrDefault(op => op.Name == "color").Value as string;
                    //If user inserted an #
                    color = color.Replace("#", "");
                    if(_util.GetGuild().Roles.Any(r=>r.Name.ToLower() == roleName.ToLower()))
                    {
                        //AVOID USING everyone and here
                        await command.RespondAsync("Le nom du rôle souhaité existe déjà");
                        return;
                    }
                    if(int.TryParse(color, System.Globalization.NumberStyles.HexNumber, null, out int test))
                    {
                        var role = await _util.GetGuild().CreateRoleAsync(roleName, null, color: new Color((uint)test), false, null);
                        await role.ModifyAsync(r =>
                        {
                            r.Position = _util.GetBoostersRole().Position + 1;
                        });
                        await member.AddRoleAsync(role);
                        command.RespondAsync($"Vous venez de crée le role: {role.Mention}");
                    }
                    else
                    {
                        await command.RespondAsync("Merci de faire parvenir un hexadeciaml pour la couleur!");
                    }
                    break;
                case "mute":
                    if (member.GuildPermissions.KickMembers)
                    {
                        SocketGuildUser victim = command.Data.Options.FirstOrDefault(op => op.Name == "user").Value as SocketGuildUser;
                        if(int.TryParse(command.Data.Options.FirstOrDefault(op => op.Name == "duration")?.Value as string, out int duration))
                        {
                            command.RespondAsync($"{victim} has been muted ", ephemeral: true);
                        }
                        else
                        {
                            command.RespondAsync($"An error has occured with duration ", ephemeral: true);
                        }
                        
                    }
                    await command.RespondAsync("Vous n'avez pas la permission d'éxectuer cette commande", ephemeral: true);
                    break;
                case "warn":
                    if (member.GuildPermissions.Administrator)
                    {
                        SocketGuildUser victim = command.Data.Options.FirstOrDefault(op => op.Name == "user")?.Value as SocketGuildUser;
                        string reason = command.Data.Options.FirstOrDefault(op => op.Name == "user").Value as string;
                        command.RespondAsync("Warned..", ephemeral: true);

                    }
                    break;
            }
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
                var listOptionWarn = new List<SlashCommandOptionBuilder>()
                {
                    new()
                    {
                        Name = "user",
                        Required = true,
                        Type = ApplicationCommandOptionType.User,
                        Description = "The user you want to warn",
                    },
                    new()
                    {
                        Name = "reason",
                        Required = true,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Reason of warn",
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
                SlashCommandBuilder warnCommand = new()
                {
                    Name = "warn",
                    Description = "Warn an user",
                    Options = listOptionWarn
                };
                SlashCommandBuilder muteCommand = new()
                {
                    Name = "mute",
                    Description = "mute an user",
                    Options = new()
                    {
                        new()
                        {
                            Name = "user",
                            Required = true,
                            Type = ApplicationCommandOptionType.User,
                            Description = "User to be muted"
                        },
                        new()
                        {
                            Name = "duration",
                            Required = true,
                            Type = ApplicationCommandOptionType.Integer,
                            Description = "Mute duration in seconds",
                        }
                    }
                };
                try
                {
                    await _client.Rest.CreateGuildCommand(pointsCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(joinroleCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(hmsCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(createRoleCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(warnCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(muteCommand.Build(), UtilService.GUILD_ID);
                }
                catch (ApplicationCommandException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                    _logger.LogError(json);
                }
            }
        }

        private Task OnUserLeft(SocketGuild member)
        {
            //zzzzzzzzzzzzzzz
            return Task.CompletedTask;
        }

        private Task OnUserJoin(SocketGuildUser member)
        {
            _util.GetWelcomeChannel().SendMessageAsync();
            return Task.CompletedTask;
        }
    }
}
