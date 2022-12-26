using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord;
using Discord.Interactions;

using Microsoft.Extensions.Logging;

namespace DevCommuBot.Commands
{
    public class ApexStatsCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public ILogger<ApexStatsCommand> Logger { get; set; }
        public ApexStatsService ApexService { get; set; }

        [SlashCommand("apex", "obtenir les statistiques d'un joueur apex")]
        public async Task ApexCommand([Summary("PlayerName", "Nom du joueur apex")] string playername, [Summary("Console", "Sur quelle console cherché")] ApexStatConsole console)
        {
            await RespondAsync("Chargement de la réponse: <a:compteur:764900344462311445>");
            var reply = await GetOriginalResponseAsync();
            Logger.LogDebug("Envoie de la requête!");
            var apexStats = await ApexService.GetApexStats(playername, console);
            Logger.LogDebug("Requête envoyé!");
            if (apexStats.Error is not null)
            {
                await reply.ModifyAsync(r => r.Content = $"> **Une erreur est survenue!**\n{apexStats.Error}");
            }
            else
            {
                await reply.ModifyAsync(r =>
                {
                    r.Content = $"Statistique du joueur {apexStats.Global.Name}";
                    var embed = new EmbedBuilder()
                    .WithTitle($"ApexStats: {apexStats.Global.Name}")
                    .WithColor(new(50, 80, 71))
                    .WithAuthor(Context.User)
                    .WithThumbnailUrl(apexStats.Global.Rank.RankImg)
                    .AddField("Ranked:", $"> Score: {apexStats.Global.Rank.Score} Classement: {apexStats.Global.Rank.RankName}#{apexStats.Global.Rank.Division}")
                    .AddField("Arena: ", $"> Score: {apexStats.Global.Arena.Score} Classement: {apexStats.Global.Arena.RankName}#{apexStats.Global.Arena.Division}")
                    .WithCurrentTimestamp()
                    .WithFooter("Data from Apex Legends Status")
                    .Build();
                    r.Embed = embed;
                });
            }
        }
    }
}