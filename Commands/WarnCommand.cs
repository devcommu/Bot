using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord.Interactions;
using Discord.WebSocket;

namespace DevCommuBot.Commands
{
    public class WarnCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UtilService _util;
        private readonly PointService _pointService;
        private readonly DataService _database;

        [SlashCommand("warn", "warn an user")]
        public Task WarnUser(SocketGuildUser user, string reason)
        {
            if ((Context.User as SocketGuildUser).GuildPermissions.ModerateMembers)
            {
                return RespondAsync("WIP", ephemeral: true);
            }
            else
            {
                return RespondAsync("Vous ne possédez pas la permission d'utiliser cette commande", ephemeral: true);
            }
        }
    }
}