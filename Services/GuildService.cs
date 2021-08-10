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
                if(command.Data.Name == "points")
                {
                    if (command.Data.Options is not null)
                        command.RespondAsync("he has 0 points");
                    else
                        command.RespondAsync("you have 0points");
                }
            }
            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            _logger.LogInformation("Registering commands");
            var cmds = await _client.Rest.GetGuildApplicationCommands(UtilService.GUILD_ID);
            if (cmds.FirstOrDefault(c=> c.ApplicationId == _client.CurrentUser.Id) is null)
            {
                //Register commands
                var list = new List<SlashCommandOptionBuilder>()
                {
                    new SlashCommandOptionBuilder()
                    {
                        Name = "user",
                        Required = false,
                        Type = ApplicationCommandOptionType.User,
                        Description = "User your want to see points"
                    }
                };
                SlashCommandBuilder guildCommand = new()
                {
                    Name = "points",
                    Description = "Get Your amount of points",
                    Options = list,
                };
                try
                {
                    await _client.Rest.CreateGuildCommand(guildCommand.Build(), UtilService.GUILD_ID);
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
