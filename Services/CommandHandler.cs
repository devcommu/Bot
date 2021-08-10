using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevCommuBot.Services
{
    internal class CommandHandler
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandHandler(IServiceProvider serviceProvider)
        {
            _config = serviceProvider.GetRequiredService<IConfigurationRoot>();
            _client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            _commands = serviceProvider.GetRequiredService<CommandService>();

            _commands.CommandExecuted += OnCommandExecuted;
            _client.MessageReceived += HandleCommand;
        }
        // Event used to check if a message is a command
        private async Task HandleCommand(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message)
                return;
            if (message.Source is not Discord.MessageSource.User)
                return;
            int argPos = 0;

        }

        private async Task OnCommandExecuted(Discord.Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {

        }
    }
}
