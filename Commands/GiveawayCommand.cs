using System;
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
        private Timer? _timer;
        private ISocketMessageChannel _channel;
        private ulong _messageId;

        [SlashCommand("giveaway", "commence un giveaway", runMode: RunMode.Async)]
        public async Task TheGiveawayCommand([Summary("duree", "Temps durant lequel les utilisateurs peuvent rejoindre (en minutes)")] int duration, [Summary("winner", "Nombre de vainqueur")] int winnerCount, [Summary("texte", "Texte à afficher")] string summary)
        {
            Logger.LogDebug("Recu!");
            if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                Logger.LogDebug("Je redige!");
                await RespondAsync($"Vous avez commencé un évenement avec {winnerCount} vainqueur et d'une durée de {duration}minutes", ephemeral: true);
                Logger.LogDebug("Message envoyée!");

                var interac = new ComponentBuilder()
                    .WithButton("Oui je le veux", "supergiveawy", ButtonStyle.Success)
                    .Build();
                Logger.LogDebug("Envoie interaction");
                var message = await Context.Channel.SendMessageAsync(summary, components: interac);
                Utils.Giveaways.Add(message.Id, new());
                _timer = new Timer((1000 * 60) * duration);
                _timer.Elapsed += OnTimerEnd;
                _timer.Start();
                _channel = Context.Channel;
                _messageId = message.Id;
            }
            else
            {
                Logger.LogDebug("HMMMMMM");
                await RespondAsync("Vous ne possédez pas la permission d'utiliser cette commande!", ephemeral: true);
            }
            return;
        }

        private void OnTimerEnd(object sender, ElapsedEventArgs e)
        {
            Logger.LogDebug($"Fin du compte à rebours");
            var rnd = new Random();
            ulong winner = Utils.Giveaways[_messageId][rnd.Next(Utils.Giveaways[_messageId].Count)];
            _channel.SendMessageAsync($"Félicitations <@{winner}> ||{winner}|| tu as gagné le lot, tu sais qui mp.");
            Logger.LogDebug($"Vainqueur: {winner}");
            Utils.Giveaways.Remove(_messageId);
            _timer.Stop();
        }
    }
}