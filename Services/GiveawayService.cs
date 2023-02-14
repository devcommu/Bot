using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DevCommuBot.Data.Models.Giveaways;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevCommuBot.Services
{
    internal class GiveawayService
    {
        private readonly DataService Database;
        private readonly DiscordSocketClient Client;
        private readonly ILogger<GiveawayService> Logger;
        private readonly UtilService Utils;
        private System.Timers.Timer GiveawayTimer;
        public bool IsGiveawayWaiting { get; set; } = false;
        public readonly Discord.Color Color = new(142, 190, 246);
        public readonly CultureInfo Culture;
        public GiveawayService(IServiceProvider services)
        {
            Database = services.GetRequiredService<DataService>();
            Client = services.GetRequiredService<DiscordSocketClient>();
            Logger = services.GetRequiredService<ILogger<GiveawayService>>();
            Utils = services.GetRequiredService<UtilService>();
            Client.Ready += OnClientStart;
            Client.ButtonExecuted += OnButtonClickedGiveaway;
            Culture = CultureInfo.CreateSpecificCulture("fr-FR");
        }

        private async Task OnButtonClickedGiveaway(SocketMessageComponent msgComp)
        {
            var msg = msgComp.Message;
            var giveaway = await Database.GetGiveaway(msg.Id);
            var user = msgComp.User;
            
            if (giveaway is not null)
            {
                var account = await Database.ForceGetAccount(user.Id);
                if(account.AllowGiveaway is false)
                {
                    _ = msgComp.RespondAsync("Vous êtes interdit de giveaway!", ephemeral: true);
                    return;
                }
                //First check if giveaway is still running
                if (giveaway.State == GiveawayState.NOT_STARTED)
                {
                    _ = msgComp.RespondAsync("Ce giveaway n'a toujours pas commencé!", ephemeral: true);
                    return;
                }
                if(giveaway.State == GiveawayState.ENDED)
                {
                    _ = msgComp.RespondAsync("Ce giveaway est terminé depuis le " + giveaway.EndAt.ToString(Culture), ephemeral: true);
                    return;
                }
                //Now check if user already in it
                if (giveaway.Participants.Contains(user.Id))
                {
                    //Todo: Remove from giveaway
                    _ = msgComp.RespondAsync("Vous êtes déjà inscrit au giveaway!\n> Il n'est maleureusement pas possible de se désinscrire pour le moment", ephemeral: true);
                    return;
                }
                giveaway.Participants.Add(user.Id);
                await Database.UpdateGiveaway(giveaway);
                _ = msgComp.RespondAsync("Votre inscription a bien été prise en compte! Revenez le : " + giveaway.EndAt.ToString(Culture), ephemeral: true);
                Logger.LogDebug($"Inscription au giveaway N°{giveaway.Id}, {user.Username}({user.Id}) s'est inscrit.");
            }
        }

        private async Task OnClientStart()
        {
            //Only when bot start
            Client.Ready -= OnClientStart;
            await CheckForgotten();
            GiveawayTimer = new(1000 * 60 * 1);
            GiveawayTimer.Elapsed += GiveawayTimer_Elapsed;
            GiveawayTimer.Start();
            //Check if giveaways has ended when afk
        }

        private async void GiveawayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await CheckGiveaways();
        }
        private async Task CheckGiveaways()
        {
            var giveaways = await Database.GetRunningGiveaways();
            var time = DateTime.Now;
            if (giveaways.Count == 0)
                return;
            foreach(var giveaway in giveaways.Where(g=>g.EndAt < time))
            {
                await EndGiveaway(giveaway);
            }
        }
        private async Task CheckForgotten()
        {
            var giveaways = await Database.GetRunningGiveaways();
            if (giveaways.Count == 0)
                return;
            Logger.LogDebug($"Found {giveaways.Count} giveaways active");
            var time = DateTime.Now;
            foreach (var giveaway in giveaways)
            {
                if(giveaway.EndAt < time)
                {
                    //Should have been finished
                    Logger.LogDebug("Ended giveaway found still running!");
                    await EndGiveaway(giveaway);
                }
            }
        }
        private async Task EndGiveaway(Giveaway giveaway)
        {
            //Ending giveaway
            var originalMessage = await Utils.GetGuild().GetTextChannel(giveaway.ChannelId).GetMessageAsync(giveaway.MessageId) as IUserMessage;
            if(originalMessage is null)
            {
                Logger.LogWarning("Giveaway message not found!");
                Utils.SendLog("Erreur Giveaway", $"Le message du giveaway n'a pas été trouvé! (id: {giveaway.MessageId}) dans le salon: {giveaway.ChannelId}\n> A titre d'information le giveaway n'est toujours pas finit. Il continuera de tourner jusqu'à ce que le message soit trouvé.");
                return;
            }
            //Modifying message to let know users that giveaway ended
            Logger.LogDebug("On modifie le message pour empêcher les utilisateurs de rentrer dans le giveaway");
            await originalMessage.ModifyAsync(m => {
                m.Content = "⏳ Le tirage du giveaway est en cours! ⌛";
                m.Components = null;
            });
            var nbParticipants = giveaway.Participants.Count;
            if(nbParticipants == 0)
            {
                giveaway.State = GiveawayState.ENDED;
                await Database.UpdateGiveaway(giveaway);
                Utils.SendLog($"Enorme Flop giveaway N°{giveaway.Id}", "Le giveaway a 0 participants! Donc personne ne sera tiré au sort\nPour pas foutre la honte, on va pas poster de message\n> **Oui c'est un GROS flop**");
                return;
            }
            List<ulong> winners = new();
            if (giveaway.AmountOfWinners >= nbParticipants)
            {
                winners = giveaway.Participants;
                Utils.SendLog($"Flop giveaway N°{giveaway.Id}", "le giveaway a eu moins de participants ou autant que de gagnants.\n> **Oui c'est un flop**");
            }
            else
            {
                //Now time to get the winners
                //TODO: Remake this code (TOTALLY BUGGY)
                var rnd = new Random();
                for (int i = 0; i < giveaway.AmountOfWinners; i++)
                {
                    int nb = rnd.Next();
                    var choosenOne = giveaway.Participants[nb];
                    if (winners.Contains(choosenOne))
                    {
                        i--;
                        continue;
                    }
                    winners.Add(choosenOne);
                }
            }
            string winnersString = "";
            foreach(var winId in winners)
            {
                winnersString += $"<@{winId}>\n";
            }
            Utils.SendLog(new EmbedBuilder()

                .WithTitle($"Fin du giveaway N°{giveaway.Id}")
                .WithUrl(originalMessage.GetJumpUrl())
                .WithAuthor("GiveawayManager", "https://icons-for-free.com/iconfiles/png/512/gift-131994967882425958.png")
                .AddField("Crée par", $"<@{giveaway.AuthorId}>", true)
                .AddField("Commencé le:", giveaway.StartAt.ToString(Culture), true)
                .AddField("Nombre de participants:", giveaway.Participants.Count, true)
                .AddField("Nombre de gagnants:", giveaway.AmountOfWinners, true)
                .AddField("Lot gagné:", giveaway.WonObject, true)
                .AddField("Gagnants:", winnersString, true)
                .AddField("Gagnants debug:", string.Join("\n", winners))
                );
            var embed = new EmbedBuilder()
                .WithTitle($"Fin du giveaway N°{giveaway.Id}")
                .WithUrl(originalMessage.GetJumpUrl())
                .WithColor(Color)
                .WithAuthor("GiveawayManager", "https://icons-for-free.com/iconfiles/png/512/gift-131994967882425958.png")
                .WithDescription("Félicitations! Contacter l'organisateur en mp!")
                .AddField("Lot gagné: ", giveaway.WonObject)
                .AddField("Commencé le:", giveaway.StartAt.ToString(Culture))
                .AddField("Organisateur:", $"<@{giveaway.AuthorId}>")
                .AddField("Gagnants: ", winnersString)
                .WithCurrentTimestamp()
                .WithFooter("Merci aux participants!")
                .Build();
            await originalMessage.ReplyAsync("Bravo à eux: \n"+winnersString, embed: embed);
            await originalMessage.ModifyAsync(m => m.Content = "Ce giveaway est terminé!");
            giveaway.WinnersId = winners;
            giveaway.State = GiveawayState.ENDED;
            await Database.UpdateGiveaway(giveaway);
        }
        public async Task ReRollWinner(Giveaway giveaway, ulong userRemoved)
        {
            if (!giveaway.WinnersId.Contains(userRemoved))
            {
                Utils.SendLog($"Erreur Reroll giveaway N°{giveaway.Id}", $"Tentative de reroll en retirant l'utilisateur: {userRemoved} cependant il n'avait pas gagné"); 
            }
            var rnd = new Random();
            var winner = giveaway.Participants[rnd.Next(giveaway.Participants.Count)];
            if (giveaway.WinnersId.Contains(winner))
            {
                Utils.SendLog($"Erreur Reroll giveaway N°{giveaway.Id}", $"on a retiré un utilisateur qui a déjà gagné!!\n> A titre d'information le reroll a été stoppé");
                return;
            }
            giveaway.WinnersId.Remove(userRemoved);
            giveaway.WinnersId.Add(winner);
            Utils.SendLog($"Reroll giveaway N°{giveaway.Id}", $"Lors du reroll, le nouveau vainqueur est: <@{winner}> ({winner}");
            await Utils.GetGuild().GetTextChannel(giveaway.ChannelId).SendMessageAsync($"> **Reroll du giveaway N°_{giveaway.Id}_**\nLa personne qui a été sélectionné est:\n<@{winner}>\n||ID: _{winner}_||");
            await Database.UpdateGiveaway(giveaway);
        }
    }
}
