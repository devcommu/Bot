using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord.Interactions;

using Microsoft.Extensions.Logging;

using RiotSharp;
using RiotSharp.Endpoints.SummonerEndpoint;
using RiotSharp.Misc;

namespace DevCommuBot.Commands
{
    public class LeagueStatsCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public UtilService Utils { get; set; }
        public ILogger<LeagueStatsCommand> Logger { get; set; }

        [SlashCommand("leaguestat", "Obtenir des informations à propos d'un joueur")]
        public async Task GetStat([Summary("summonerName", "SUMMONER DU JOUEUR")] string pseudo, Region region = Region.Euw)
        {
            Summoner summoner;
            try
            {
                summoner = await Utils.Riot.Summoner.GetSummonerByNameAsync(region, pseudo);

            }catch(RiotSharpException ex)
            {
                Logger.LogError($"Error in LeagueStat (Summoner): {ex.Message}");
                await RespondAsync("Cet utilisateur n'existe pas?");
                    return;
            }
            if(summoner.Level< 30)
            {
                //under level 30 no ranked
                await RespondAsync($"{summoner.Name} est niveau {summoner.Level}! Il n'a pas encore accès au ranked!");
                return;
            }
            try
            {
                var league = await Utils.Riot.League.GetLeagueEntriesBySummonerAsync(region, summoner.Id);
                await RespondAsync($"{summoner.Name} est niveau {summoner.Level} Rank: {league[0].Tier} {league[0].Rank} avec {league[0].LeaguePoints} PL");
            }catch(RiotSharpException ex)
            {
                Logger.LogError($"Error in LeagueStat (League): {ex.Message}");
                await RespondAsync($"{summoner.Name} est niveau {summoner.Level}! Il m'est impossible d'accéder à son classement || il a pas fait ses games de placement la honte ||!");
                return;
            }
        }
    }
}
