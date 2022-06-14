using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord;
using Discord.Interactions;

namespace DevCommuBot.Commands
{
    public class PartnerCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UtilService _util;

        [SlashCommand("partner", "Give details about our partner")]
        public Task Partner()
        {
            var compo = new ComponentBuilder()
                        .WithButton("HostMyServers", null, ButtonStyle.Link, url: "https://www.hostmyservers.fr/")
                        .Build();
            var embedHms = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithAuthor("HostMyServers", "https://www.hostmyservers.fr/assets/images/logo/Logo-HMS-color-icon.png")
                .WithTitle("HostMyServers - Location de VPS et Serveur Gaming")
                .WithDescription("HostMyServers propose des VPS et des serveurs Gaming (Minecraft, Mcpe, Gmod), mais aussi des noms de domaine et des hébergements web. Depuis 2014, l’hébergeur présente plusieurs offres avec un rapport qualité/prix imbattable.\n\n> *Utilisez le code promo suivant*:\n **DEVCOMMU**\nAfin de profiter de 20% de réduction!")
                .WithCurrentTimestamp()
                .WithFooter("Partenaire depuis le 07/08/2021")
                .Build();
            //Why not emepheral?
            return RespondAsync(embed: embedHms, components: compo);
        }
    }
}