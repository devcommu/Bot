using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord.Interactions;

using Microsoft.Extensions.Logging;

using RiotSharp;
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
            try
            {
                var summoner = await Utils.Riot.Summoner.GetSummonerByNameAsync(region, pseudo);
                var league = await Utils.Riot.League.GetLeagueEntriesBySummonerAsync(region, summoner.Id);
                Logger.LogDebug($"leagueposition: {league.Count}");
                await RespondAsync($"{summoner.Name} est niveau {summoner.Level} Rank: {league[0].Tier} {league[0].Rank} avec {league[0].LeaguePoints} PL");

            }catch(RiotSharpException ex)
            {
                Logger.LogError($"Error in LeagueStat: {ex.Message}");
            }
        }
    }
}
