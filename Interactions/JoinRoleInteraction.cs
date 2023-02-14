using System.Linq;
using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace DevCommuBot.Interactions
{
    public class JoinRoleInteraction : InteractionModuleBase<SocketInteractionContext>
    {
        public ILogger<CommandHandler> Logger { get; set; }
        public UtilService Utils { get; set; }

        [ComponentInteraction("role-selection")]
        public Task JoinRole(string[] selectedRoles)
        {
            var roleName = selectedRoles[0];
            SocketGuildUser member = Context.User as SocketGuildUser;
            SocketRole role;
            switch (roleName)
            {
                case "projects":
                    role = Context.Guild.GetRole(UtilService.ROLE_PROJECTS_ID);
                    break;

                case "gaming":
                    role = Utils.GetGuild().GetRole(UtilService.ROLE_GAMING_ID);
                    break;

                case "devchats":
                    role = Utils.GetGuild().GetRole(UtilService.ROLE_DEVCHATS_ID);
                    break;

                case "free-games":
                    role = Utils.GetGuild().GetRole(UtilService.ROLE_FREEGAMES_ID);
                    break;

                case "discord-games":
                    role = Utils.GetGuild().GetRole(UtilService.ROLE_DISCORDGAMES_ID);
                    break;
                case "giveaways":
                    role = Utils.GetGuild().GetRole(UtilService.ROLE_GIVEAWAYS_ID);
                    break;

                default:
                    RespondAsync("Oups! Je n'ai pas trouver le rôle que tu souhaites obtenir! ERR_INTERACTIONS:ROLE_NOT_FOUND", ephemeral: true);
                    return Task.CompletedTask;
            }
            if (member.Roles.Any(r => r.Id == role.Id))
            {
                member.RemoveRoleAsync(role);
                RespondAsync($"Vous venez de vous retirez le role {role.Name}", ephemeral: true);
                return Task.CompletedTask;
            }
            member.AddRoleAsync(role);
            RespondAsync($"Vous venez de récuperer le role {role.Name}({role.Mention})", ephemeral: true);
            return Task.CompletedTask;
        }
    }
}