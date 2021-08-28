using Discord;
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

        public const ulong CHANNEL_LOGS_ID = 875824516675305502;
        public const ulong CHANNEL_BOOSTERS_ID = 764467968490864691;
        public const ulong CHANNEL_WELCOME_ID = 881262458398986280;

        public const ulong ROLE_PROJECTS_ID = 874785049516605491;
        public const ulong ROLE_GAMING_ID = 875757898087678034;
        public const ulong ROLE_BOOSTERS_ID = 642107269940772871; //Role created by Discord

        public readonly Color EmbedColor = new(19, 169, 185);

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

        public SocketRole GetGamingRole()
            => GetGuild()?.GetRole(ROLE_GAMING_ID);

        public SocketRole GetBoostersRole()
            => GetGuild()?.GetRole(ROLE_BOOSTERS_ID);

        public SocketTextChannel GetLogChannel()
            => _client.GetChannel(CHANNEL_LOGS_ID)as SocketTextChannel;

        public SocketTextChannel GetBoostersChannel()
            => _client.GetChannel(CHANNEL_BOOSTERS_ID) as SocketTextChannel;

        public SocketTextChannel GetWelcomeChannel()
            => _client.GetChannel(CHANNEL_WELCOME_ID) as SocketTextChannel;

        public void SendLog(string title, string description, SocketGuildUser author = null)
        {
            var embed = new EmbedBuilder()
                .WithColor(EmbedColor)
                .WithAuthor(author)
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter("2021")
                .WithCurrentTimestamp()
                .Build();
            GetLogChannel().SendMessageAsync(embed: embed);
        }

        public bool HasCustomRole(SocketGuildUser member)
            => member.Roles.Any(role => role.Position > GetBoostersRole().Position) && member.GuildPermissions.Administrator is not true;
    }
}
