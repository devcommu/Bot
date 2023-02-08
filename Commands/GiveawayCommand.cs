using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;

using DevCommuBot.Services;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace DevCommuBot.Commands
{
    public class GiveawayCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public ILogger<GiveawayCommand> Logger { get; set; }
        public UtilService Utils { get; set; }
        internal GiveawayService Giveaways { get; set; }
        internal DataService Database { get; set; }
        private Timer? _timer;
        private ISocketMessageChannel _channel;
        private ulong _messageId;
        [SlashCommand("giveaway", "commence un giveaway", runMode: RunMode.Async), RequireUserPermission(GuildPermission.Administrator)]
        public async Task TheGiveawayCommand([Summary("message", "Message qui sera annoncé")]string msg, [Summary("lot", "lot à gagner")] string wonObject, [Summary("gagnants", "Nombre de gagnants")] int winnerCount, [Summary("fin", "date fin du giveaway au format dd/mm/yyyy hh:ii")] string dateString, [Summary("debut", "date du début au format dd/mm/yyyy hh:ii")]string startDateString = "", string promotelink = "")
        {
            Logger.LogDebug("Recu!");
            if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                var culture = CultureInfo.CreateSpecificCulture("fr-Fr");
                if (!DateTime.TryParse(dateString, culture, DateTimeStyles.None, out var endTime)) 
                {
                    await RespondAsync("L'erreur entrée pour l'argument `fin` n'existe pas. Une date valide est attendue au format `dd/mm/yyyy hh:ii`", ephemeral: true);
                    return;
                }
                Logger.LogDebug("Date: " + endTime.ToString());
                Logger.LogDebug("Je redige!");
                await RespondAsync($"Vous avez commencé un évenement avec {winnerCount} vainqueur et d'une durée de {(endTime-DateTime.Now).Days} jours", ephemeral: true);
                Logger.LogDebug("Message envoyée!");
                //Super unique id
                var interac = new ComponentBuilder()
                    .WithButton("Je souhaite participer", $"giveaway-{Guid.NewGuid()}", ButtonStyle.Success)
                    .Build();
                Logger.LogDebug("Envoie interaction");
                var embed = new EmbedBuilder()
                    .WithColor(new Color(142, 190, 246))
                    .WithDescription(msg)
                    .WithAuthor(Context.User)
                    .WithFooter("Fin du giveaway: " + endTime.ToString(Giveaways.Culture))
                    .AddField("Lot", wonObject)
                    .AddField("Nombre de gagnants", winnerCount)
                    .Build();
                var message = await Context.Channel.SendMessageAsync(msg, components: interac);
                await Database.CreateGiveaway(Context.User.Id, Context.Channel.Id, message.Id, msg, wonObject, winnerCount, DateTime.Now, endTime, Data.Models.Giveaways.GiveawayState.RUNNING, PromoteLink: promotelink);
                Utils.SendLog("Giveaway", $"Un giveaway a été lancé par {Context.User.Mention} dans le salon {((SocketTextChannel)Context.Channel).Mention} avec le message {msg} et le lot {wonObject} avec {winnerCount} gagnants.\n Fin du giveaway: {endTime}", (SocketGuildUser)Context.User);
            }
            else
            {
                await RespondAsync("Vous ne possédez pas la permission d'utiliser cette commande!", ephemeral: true);
            }
            return;
        }
        [SlashCommand("giveawayreroll", "Reroll un giveaway"), RequireUserPermission(GuildPermission.Administrator)]
        public async Task GiveawayReroll() 
        {
            await RespondAsync("Comming soon!");
        }
    }
}