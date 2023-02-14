using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord;
using Discord.Interactions;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DevCommuBot.Commands
{
    public class InfoCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public UtilService Utils { get; set; }
        public ILogger<InfoCommand> Logger { get; set; }

        [SlashCommand("informations", "Get information about the discord and the bot", runMode: RunMode.Async)]
        public async Task GetInfo()
        {
            Logger.LogDebug("Debut Requete");
            //Storing contributors:
            await RespondAsync("Retrieving datas...");
            using var client = new HttpClient();
            HttpRequestMessage mess = new(HttpMethod.Get, "https://api.github.com/repos/devcommu/Bot/contributors");
            mess.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; rv:60.0) Gecko/20100101 Firefox/60.0");
            var webResult = await client.SendAsync(mess);
            var rslt = await webResult.Content.ReadAsStringAsync();
            Logger.LogDebug($"Resultat requete: {rslt}");
            var contributors = JsonConvert.DeserializeObject<ICollection<GithubContributor>>(rslt);
            string contString = "";
            foreach (var contributor in contributors)
            {
                contString += $"[{contributor.login}]({contributor.html_url}) ({contributor.contributions} contributions)\n";
            }
            Logger.LogDebug($"Message à afficher: {contString}");
            if (contString == "")
                contString = "No contributors yet";
            var embedBot = new EmbedBuilder()
                .WithTitle("Bot's Information")
                .WithUrl("https://github.com/devcommu/Bot")
                .WithAuthor(Context.Client.CurrentUser)
                .WithDescription($"> **This bot is made by the community for the community**\n> The bot is using Discord.NET v.{DiscordConfig.Version}")
                .AddField("Contributors:", contString)
                .AddField("How to contribute?", "If you want to contribute please read the README in our GitHub repository [here](https://github.com/devcommu/Bot)")
                .WithFooter("Made by the community for the community")
                .WithCurrentTimestamp()
                .Build();
            var embedDiscord = new EmbedBuilder()
                .WithTitle("Discord's Information")
                .WithUrl("https://open.spotify.com/track/2Jof799F251cl0DyUW2dqW?si=151adee765c744e8")
                .WithDescription("Coming soon?")
                .WithFooter("<Ingé Son>")
                .WithCurrentTimestamp()
                .Build();
            Logger.LogDebug("Récupération de la réponse!");
            var originalResponse = await Context.Interaction.GetOriginalResponseAsync();
            await originalResponse.ModifyAsync(x =>
            {
                x.Content = "";
                x.Embeds = new Embed[] { embedBot, embedDiscord };
            });
            //ToDo: Use https://api.github.com/repos/devcommu/Bot/contributors
        }

        public override void OnModuleBuilding(InteractionService commandService, ModuleInfo module)
        {
            Console.WriteLine("Informations");
            base.OnModuleBuilding(commandService, module);
        }
    }

    [JsonObject]
    internal class GithubContributor
    {
        public string login { get; set; }
        public int contributions { get; set; }
        public string avatar_url { get; set; }
        public string html_url { get; set; }
    }
}