using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevCommuBot.Services
{
    internal class UtilService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;

        public const ulong GUILD_ID = 584987515388428300;

        public const ulong ROLE_PROJECTS_ID = 874785049516605491;
        public UtilService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<GuildService>>();
        }

        public SocketGuild GetGuild()
            => _client.Guilds.FirstOrDefault(g => g.Id == GUILD_ID);

        public List<SocketGuildChannel> GetAllowedChannels()
            => new()
            {
                _client.GetChannel(0) as SocketGuildChannel,
            };

        public SocketRole GetProjectsRole()
            => GetGuild()?.GetRole(ROLE_PROJECTS_ID);
    }
}
