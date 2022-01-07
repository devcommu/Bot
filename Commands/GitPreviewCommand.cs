using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord.Interactions;

namespace DevCommuBot.Commands
{
    public class GitPreviewCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("gitpreview", "Preview code from github")]
        public async Task GitPreview(string url)
        {
            //I litteraly translated this js part to c#
            //https://github.com/HimbeersaftLP/MagicalHourglass/blob/master/bot.js
            // s/o Himbeersaft i guess
            Regex regex = new(@"http(?:s|):\/\/github\.com\/(.*?\/.*?\/)blob\/(.*?\/.*?)#L([0-9]+)-?L?([0-9]+)?");
            Regex FileEndRegex = new(@".*\.([a-zA-Z0-9]*)");
            if (regex.IsMatch(url))
            {
                var match = regex.Match(url);
                if (!int.TryParse(match.Groups[3].Value, out int lineAsked))
                {
                    await RespondAsync("Une erreur est survenue :(");
                    return;
                }
                int lineTo = -5;
                if (match.Groups.Count > 5)
                {
                    if (!int.TryParse(match.Groups[5].Value, out lineTo))
                    {
                        await RespondAsync("Une erreur est survenue :(");
                    }
                }
                await RespondAsync("Searching Code!!");
                using HttpClient httpClient = new();
                var githubUrl = $"https://raw.githubusercontent.com/{match.Groups[1]}{match.Groups[2]}";
                var response = await httpClient.GetAsync(githubUrl);
                var originalResponse = await Context.Interaction.GetOriginalResponseAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var lines = content.Split("\n");
                    if (lineAsked > lines.Length || lineAsked < 0)
                    {
                        await originalResponse.ModifyAsync(m =>
                        {
                            m.Content = ":robot: Ligne Introuvable!";
                        });
                        return;
                    }
                    if (lineTo == -5)
                    {
                        //Didnt ask to reach lineTo
                        int from = lineAsked - 5;
                        int to = lineAsked + 5;
                        var langMatch = FileEndRegex.Match(match.Groups[2].Value);
                        var lang = langMatch.Groups[1];
                        var cleanFileName = match.Groups[2].Value.Replace(@"\?.+", "");
                        var msg = $"Lignes {from} - {to} de {cleanFileName}\n```{lang}\n";
                        for (int i = from; i <= to; i++)
                        {
                            msg += $"{lines[i]}\n";
                        }
                        msg += "\n```";
                        await originalResponse.ModifyAsync(m =>
                        {
                            m.Content = msg;
                        });
                    }
                    else
                    {
                        if (lineTo > lines.Length || lineTo < 0 || lineTo < lineAsked)
                        {
                            await originalResponse.ModifyAsync(m =>
                            {
                                m.Content = ":robot: Ligne Introuvable!";
                            });
                            return;
                        }
                        await originalResponse.ModifyAsync(m =>
                        {
                            m.Content = ":robot: On Dort!";
                        });
                        return;
                    }
                }
                else
                {
                    await originalResponse.ModifyAsync(m =>
                    {
                        m.Content = ":robot: Impossible d'atteindre github";
                    });
                }
            }
            else
                await RespondAsync($"Il faut préciser une url suivant ce regex: `{regex}`");
            return;
        }
    }
}