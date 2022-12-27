using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DevCommuBot.Commands
{
    public class EmbedCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("embed", "Crée un embed")]
        public Task CreateEmbed(SocketGuildChannel channel = null)
        {
            if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                // I don't like how it's made
                var modalBuilder = new ModalBuilder()
                    .WithTitle("Création d'embed")
                    .AddTextInput("Title", "title");
                return Context.Interaction.RespondWithModalAsync(modalBuilder.Build());
            }
            else
            {
                return RespondAsync("Vous ne possédez pas la permission d'utiliser cette commande", ephemeral: true);
            }
        }
    }

    public class EmbedModal : IModal
    {
        public string Title => "Création d'embed";
        public string ChannelId { get; set; }
    }
}