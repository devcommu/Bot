﻿using System;
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
        internal DataService Database { get; set; }
        private Timer? _timer;
        private ISocketMessageChannel _channel;
        private ulong _messageId;

        [SlashCommand("giveaway", "commence un giveaway", runMode: RunMode.Async), RequireUserPermission(GuildPermission.Administrator)]
        public async Task TheGiveawayCommand([Summary("message", "Message qui sera annoncé")]string msg, [Summary("lot", "lot à gagner")] string wonObject, [Summary("gagnants", "Nombre de gagnants")] int winnerCount, [Summary("durée", "durée du giveaway en minutes")] int duration)
        {
            Logger.LogDebug("Recu!");
            if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                Logger.LogDebug("Je redige!");
                var end = DateTime.Now.AddMinutes(duration);
                await RespondAsync($"Vous avez commencé un évenement avec {winnerCount} vainqueur et d'une durée de {duration}minutes", ephemeral: true);
                Logger.LogDebug("Message envoyée!");
                var interac = new ComponentBuilder()
                    .WithButton("Je suis intéressé", "supergiveawy", ButtonStyle.Success)
                    .Build();
                Logger.LogDebug("Envoie interaction");
                var message = await Context.Channel.SendMessageAsync(msg, components: interac);
                await Database.CreateGiveaway(Context.User.Id, message.Id, msg, wonObject, winnerCount, DateTime.Now, end, Data.Models.Giveaways.GiveawayState.RUNNING);
                Utils.Giveaways.Add(message.Id, new());
                /*_timer = new Timer((1000 * 60) * duration);
                _timer.Elapsed += OnTimerEnd;
                _timer.Start();
                _channel = Context.Channel;
                _messageId = message.Id;*/
                //Todo: Complete this!
            }
            else
            {
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