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
        private readonly Dictionary<ulong, ulong> CoolDown = new();
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
            if (IsValid(message))
            {
                
            }
            else
            {
                return;
            }
        }

        public bool IsValid(SocketUserMessage msg)
        {
            if (!_util.GetAllowedChannels().Contains(msg.Channel as SocketGuildChannel))
                return false;
            if (ContainsBadWords(msg.Content))
                return false;
            return true;
        }
        public bool ContainsBadWords(string message)
        {
            var messArray = message.Split(' ');
            bool found = false;
            List<string> badwords = GetBadWords();
            foreach(var mess in messArray)
            {
                if (badwords.Contains(mess))
                {
                    found = true;
                }
            }
            return found;
        }
        public static List<string> GetBadWords()
        {
            return File.ReadAllLines("badwords.txt").ToList();
        }
    }
}
