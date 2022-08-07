using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevCommuBot.Data.Models.Users;
using DevCommuBot.Services;

using Discord.Interactions;
using Discord.WebSocket;

namespace DevCommuBot.Commands
{
    internal class ModerationCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UtilService _util;
        private readonly PointService _pointService;
        private readonly DataService _database;

        [SlashCommand("moderation","Main Command for moderators"), RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task MainModerationCommand(SocketGuildUser user)
        {
            await RespondAsync("Command without any usage", ephemeral: true);
        }
        [SlashCommand("moderation star", "moderate starboard"), RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ModerateStarboard(SocketGuildUser moderator, [Summary("MessageId", "L'id du message posté dans le starboard")]ulong messageId, [Summary("Status", "Nouveau status a attribué au message")] StarboardEntryStatus status)
        {
            //TODO
        }
    }
}
