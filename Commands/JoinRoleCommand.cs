using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace DevCommuBot.Commands
{
    public class JoinRoleCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public ILogger<CommandHandler> Logger { get; set; }
        public UtilService Utils { get; set; }

        [ComponentInteraction("role-selection")]
        public Task JoinRole(string[] selectedRoles)
        {
            var roleName = selectedRoles[0];
            SocketGuildUser member = Context.User as SocketGuildUser;
            SocketRole role;
            if(roleName == "projects")
            {
                role = Utils.GetGuild().GetRole(UtilService.ROLE_PROJECTS_ID);
            }
            else if(roleName == "gaming")
            {
                role = Utils.GetGuild().GetRole(UtilService.ROLE_GAMING_ID);
            }
            else if (roleName == "devchats")
            {
                role = Utils.GetGuild().GetRole(UtilService.ROLE_DEVCHATS_ID);
            }
            else
            {
                RespondAsync("Weird?", ephemeral: true);
                return Task.CompletedTask;

            }
            if(member.Roles.Any(r=>r.Id == role.Id))
            {
                RespondAsync("Vous possédez déjà ce grade!", ephemeral: true);
                return Task.CompletedTask;
            }
            member.AddRoleAsync(role);
            RespondAsync($"Vous venez de récuperer le role {role.Name}", ephemeral: true);
            return Task.CompletedTask;
        }
    }
}
