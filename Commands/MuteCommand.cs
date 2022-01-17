using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord.Interactions;
using Discord.WebSocket;

namespace DevCommuBot.Commands
{
    public class MuteCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UtilService _util;
        private readonly PointService _pointService;
        private readonly DataService _database;

        [SlashCommand("mute", "mute an user")]
        public async Task MuteUser(SocketGuildUser user, string reason, [Summary("duration", "duration time in seconds")] int duration = -1)
        {
            if ((Context.User as SocketGuildUser).GuildPermissions.ModerateMembers)
            {
                _ = RespondAsync("WIP", ephemeral: true);
            }
            else
            {
                _ = RespondAsync("Vous ne possédez pas la permission d'utiliser cette commande!", ephemeral: true);
            }
        }
    }
}