using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevCommuBot.Services
{
    internal class PointService
    {

        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly UtilService _util;
        private readonly DataService _database;

        private readonly Dictionary<ulong, long> MessageCooldown = new();
        public PointService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<GuildService>>();
            _util = services.GetRequiredService<UtilService>();
            _database = services.GetRequiredService<DataService>();
        }

        public async void HandleMessage(SocketUserMessage message)
        {

        }

        public bool ContainsBadWords(string message)
        {
            return false;
        }
        public List<string> GetBadWords()
        {
            return File.ReadAllLines("badwords.txt").ToList();
        }
    }
}
