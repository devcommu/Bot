using System.Linq;
using System.Threading.Tasks;

using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.SummonerV4;

using DevCommuBot.Services;

using Discord.Interactions;

using Microsoft.Extensions.Logging;

namespace DevCommuBot.Commands
{
    public class LeagueStatsCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public UtilService Utils { get; set; }
        public ILogger<LeagueStatsCommand> Logger { get; set; }

        [SlashCommand("leaguestat", "Obtenir des informations à propos d'un joueur")]
        public async Task GetStat([Summary("summonerName", "SUMMONER DU JOUEUR")] string pseudo, PlatformRoute region = PlatformRoute.EUW1)
        {
            await RespondAsync($"> Récupération des données pour l'utilisateur *{pseudo}*#{region}");
            Summoner summoner = await Utils.Riot.SummonerV4().GetBySummonerNameAsync(region, pseudo);
            Logger.LogDebug("Réponse reçu");
            if (summoner is null)
            {
                await ModifyOriginalResponseAsync(m =>
                {
                    m.Content = "Cet utilisateur n'existe pas";
                });
                return;
            }
            if (summoner.SummonerLevel < 30)
            {
                //under level 30 no ranked
                await ModifyOriginalResponseAsync(x=>x.Content=$"{summoner.Name} est niveau {summoner.SummonerLevel}! Il n'a pas encore accès au ranked!");
                return;
            }
            await ModifyOriginalResponseAsync(m => m.Content = $"{summoner.Name} est niveau {summoner.SummonerLevel}!\n> Récupération des ranked....");
            var leagues = await Utils.Riot.LeagueV4().GetLeagueEntriesForSummonerAsync(region, summoner.Id);
            if (leagues is null)
            {
                await ModifyOriginalResponseAsync(m=>m.Content=$"{summoner.Name} est niveau {summoner.SummonerLevel}! Il m'est impossible d'accéder à son classement || il a pas fait ses games de placement la honte ||!");
                return;
            }
            var league = leagues.FirstOrDefault(l => l.QueueType == QueueType.RANKED_SOLO_5x5);
            if(league is null)
            {
                await ModifyOriginalResponseAsync(m => m.Content = $"{summoner.Name} est niveau {summoner.SummonerLevel}! Il m'est impossible d'accéder à son classement solo || Il joue en flex mdr... ||!");
                return;
            }
            await ModifyOriginalResponseAsync(m=>m.Content=$"{summoner.Name} est niveau {summoner.SummonerLevel} Rang: {league.Tier} {league.Rank} avec {league.LeaguePoints} PL");
        }
    }
}