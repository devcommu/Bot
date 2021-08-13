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
        }

        private Task OnInteraction(SocketInteraction arg)
        {
            if(arg is SocketSlashCommand command)
            {
                //test as always
                var member = command.User as SocketGuildUser;
                if(command.Data.Name == "points")
                {
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
                    var compo = new ComponentBuilder()
                        .WithButton("HostMyServers", null, ButtonStyle.Link, url: "https://www.hostmyservers.fr/")
                        .Build();
                    command.RespondAsync("hmm", component: compo);
                    //test
                    command.Channel.SendMessageAsync("Test?", component: compo);
                }
            }
            if(arg is SocketMessageComponent component)
            {
                if (component.Data.CustomId == "button_hms")
                {

                }
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
                try
                {
                    await _client.Rest.CreateGuildCommand(pointsCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(joinroleCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(hmsCommand.Build(), UtilService.GUILD_ID);
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
