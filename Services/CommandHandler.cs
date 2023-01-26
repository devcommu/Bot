using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevCommuBot.Services
{
    public class CommandHandler
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly InteractionService _slashCommand;
        private readonly UtilService _util;
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
            _util = serviceProvider.GetService<UtilService>();

            _commands.CommandExecuted += OnCommandExecuted;
            _client.InteractionCreated += OnInteraction;
            _client.Ready += OnReady;
        }

        private async Task OnReady()
        {
            await _slashCommand.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
            _logger.LogDebug("Registering commands");
            await _slashCommand.RegisterCommandsToGuildAsync(584987515388428300).ContinueWith(x =>
            {
                _logger.LogDebug("Finished Registering commands.");
                if (x.IsFaulted)
                {
                    _logger.LogError("Une Errreur est survenue");
                    _logger.LogError(x.Exception.Message);
                    _logger.LogError(x.Exception.Source);
                    _logger.LogError(x.Exception.InnerException?.Message);
                    _logger.LogError(x.Exception.StackTrace);
                }
                if (x.Exception != null)
                    _logger.LogError($"Exception: {x.Exception.InnerException.Message}");

                if (x.IsCompletedSuccessfully)
                {
                    _logger.LogDebug($"{x.Result.Count} commandes ont été envoyés!");
                }
            });
            _client.Ready -= OnReady;
        }

        private async Task OnInteraction(SocketInteraction interaction)
        {
            _logger.LogDebug("Received Interactions!");
            var ctx = new SocketInteractionContext(_client, interaction);
            await _slashCommand.ExecuteCommandAsync(ctx, _services);
        }

        private async Task OnCommandExecuted(Discord.Optional<CommandInfo> arg1, ICommandContext arg2, Discord.Commands.IResult arg3)
        {
        }
    }
}