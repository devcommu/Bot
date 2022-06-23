using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DevCommuBot.Services
{
    public class ApexStatsService
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger _logger;
        private readonly string TOKEN_STATS;
        public ApexStatsService(IServiceProvider serviceProvider)
        {
            _config = serviceProvider.GetRequiredService<IConfigurationRoot>();
            _logger = serviceProvider.GetRequiredService<ILogger<ApexStatsService>>();
            TOKEN_STATS = _config["apexToken"];
        }

        public async Task<ApexStats?> GetApexStats(string playerName, ApexStatConsole console)
        {
            using HttpClient httpClient = new()
            {
                BaseAddress = new Uri("https://api.mozambiquehe.re/")
            };
            var result = await httpClient.GetStringAsync($"bridge?auth={TOKEN_STATS}&player={playerName}&platform={GetConsoleName(console)}");
            ApexStats apexStats = JsonConvert.DeserializeObject<ApexStats>(result);
            return apexStats;

        }
        /// <summary>
        /// Get console name for API usage
        /// </summary>
        /// <returns>Console Name</returns>
        public static string GetConsoleName(ApexStatConsole console)
        {
            return console switch
            {
                ApexStatConsole.PC => "PC",
                ApexStatConsole.PLAYSTATION => "PS4",
                ApexStatConsole.XBOX => "X1",
                _ => "UNKNOWN"
            };
        }
    }
    public enum ApexStatConsole : int
    {
        PC = 1,
        PLAYSTATION,
        XBOX,
        SWITCH
    }
    public class ApexStats
    {
        [JsonProperty("global")]
        public GlobalStats Global { get; set; }
        [JsonProperty("realtime")]
        public object Realtime { get; set; }
        [JsonProperty("legends")]
        public object Legends { get; set; }
        [JsonProperty("mozambiquehere_internal")]
        public object _internal { get; set; }// skip
        [JsonProperty("ALS")]
        public object ALS { get; set; }// skip
        [JsonProperty("total")]
        public object Total { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }
    }
    public class GlobalStats
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("uid")]
        public ulong Uid { get; set; }

        public string Platform { get; set; }

        public int Level { get; set; }
        // TODO: Implement....
        [JsonProperty("rank")]
        public GlobalRankStats Rank { get; set; }
    }
    public class GlobalRankStats
    {
        [JsonProperty("rankScore")]
        public int Score { get; set; }
        [JsonProperty("rankName")]
        public string RankName { get; set; }
        [JsonProperty("rankDiv")]
        public int Division { get; set; }
        public string RankImg { get; set; }
        [JsonProperty("rankedSeason")]
        public string SeasonRaw { get; set; }
    }
}
