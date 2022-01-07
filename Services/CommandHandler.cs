using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DevCommuBot.Services
{
    internal class CommandHandler
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly InteractionService _slashCommand;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        public CommandHandler(IServiceProvider serviceProvider)
        {
            _services = serviceProvider;
            _config = serviceProvider.GetRequiredService<IConfigurationRoot>();
            _client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            _commands = serviceProvider.GetRequiredService<CommandService>();
            _slashCommand = serviceProvider.GetRequiredService<InteractionService>();
            _logger = serviceProvider.GetService<ILogger<CommandHandler>>();

            _logger.LogDebug("Registering commands");
            _slashCommand.AddModulesAsync(Assembly.GetExecutingAssembly(), serviceProvider);
            _commands.CommandExecuted += OnCommandExecuted;
            _client.MessageReceived += HandleCommand;
            _client.InteractionCreated += OnInteraction;
            _client.Ready += OnReady;
        }

        private async Task OnReady()
        {
            await _slashCommand.RegisterCommandsGloballyAsync().ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    _logger.LogDebug("Errreur survenue");
                    _logger.LogDebug(x.Exception.Message);
                }
                if (x.IsCompletedSuccessfully)
                {
                    foreach (var command in x.Result)
                    {
                        _logger.LogDebug($"{command.Name} a été envoyé");
                    }
                }
            });
            await _slashCommand.RegisterCommandsToGuildAsync(584987515388428300);
        }

        private async Task OnInteraction(SocketInteraction interaction)
        {
            _logger.LogDebug("Received Interactions!");
            var ctx = new SocketInteractionContext(_client, interaction);
            await _slashCommand.ExecuteCommandAsync(ctx, _services);
        }

        // Event used to check if a message is a command
        private async Task HandleCommand(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message)
                return;
            if (message.Source is not Discord.MessageSource.User)
                return;
            int argPos = 0;
            //Handle Command?
        }

        private async Task OnCommandExecuted(Discord.Optional<CommandInfo> arg1, ICommandContext arg2, Discord.Commands.IResult arg3)
        {

        }
    }
}
