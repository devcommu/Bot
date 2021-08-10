﻿using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevCommuBot.Services
{
    internal class UtilService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;

        public const ulong GUILD_ID = 584987515388428300;
        public UtilService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<GuildService>>();
        }

        public SocketGuild GetGuild()
            => _client.Guilds.FirstOrDefault(g => g.Id == GUILD_ID);
    }
}