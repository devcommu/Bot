using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevCommuBot.Services
{
    internal class StartupService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _services;
        private readonly UtilService utilService;

        public StartupService(IServiceProvider services)
        {
            _services = services;
            _config = _services.GetRequiredService<IConfigurationRoot>();
            _discord = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<CommandService>();
            _discord.Ready += OnReady;
        }

        public async Task StartAsync()
        {
            string discordToken = _config["Token"];
            if (string.IsNullOrWhiteSpace(discordToken))
            {
                throw new Exception("Token missing from config.json! Please enter your token there (root directory)");
            }

            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OnReady()
        {
            await _discord.SetGameAsync($"Maybe reading r/piracy");
        }
    }
}