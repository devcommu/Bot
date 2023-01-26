using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.Interactions;

namespace DevCommuBot.Commands
{
    internal class InfoCommand : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("info", "Get information about the discord and the bot")]
        public async Task GetInfo()
        {
            //ToDo: Use  https://api.github.com/repos/devcommu/Bot/contributors
        }
    }
}
