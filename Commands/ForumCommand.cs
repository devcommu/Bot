using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord.Interactions;

using Microsoft.Extensions.Logging;

namespace DevCommuBot.Commands
{
    [Group("forum", "Controler un forum")]
    public class ForumCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public ILogger<ApexStatsCommand> Logger { get; set; }
        public UtilService Utils { get; set; }

        /*[SlashCommand("info", "oui")]
        public async Task GetForumInformation()
        {
            if (!Utils.IsAForum(Context.Channel))
            {
                await RespondAsync("Cette commande doit être utilisé dans un forum", ephemeral: true);
                return;
            }
        }*/
        [Group("info", "Obtenir les informations d'un forum")]
        internal class ForumInfoCommand : InteractionModuleBase<SocketInteractionContext>
        {
            public ILogger<ApexStatsCommand> Logger { get; set; }
            public UtilService Utils { get; set; }

            [SlashCommand("post", "Obtenir les informations d'un post")]
            public async Task GetPostInformation()
            {
                if (!Utils.IsAForum(Context.Channel))
                {
                    await RespondAsync("Cette commande doit être utilisé dans un forum", ephemeral: true);
                    return;
                }
            }
        }
        
    }
}