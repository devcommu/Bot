using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DevCommuBot.Commands
{
    public class WarnCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UtilService _util;
        private readonly PointService _pointService;
        private readonly DataService _database;

        [SlashCommand("warn", "warn an user")]
        public Task WarnUser(SocketGuildUser user, string reason)
        {
            //ModerateMember return false even if admin shit is crazy
            if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                WarnModal modal = new();
                modal.Title += user.Username;
                return Context.Interaction.RespondWithModalAsync<WarnModal>("warn_modal");
            }
            else
            {
                return RespondAsync("Vous ne possédez pas la permission d'utiliser cette commande", ephemeral: true);
            }
        }

        [ModalInteraction("warn_modal")]
        public async Task WarnModalRespond(WarnModal modal)
        {
            var user = Context.Guild.GetUser(ulong.Parse(modal.UserId));
            if (user is null)
            {
                await RespondAsync($"L'utilisateur ayant pour ID {modal.UserId} n'a pas pu être warn");
                return;
            }
            Data.Models.Users.User account = await _database.GetAccount(user.Id);
            if (account is null)
            {
                await RespondAsync($"L'utilisateur {user.Username} ne possède pas de compte, création du compte...");
                await _database.CreateAccount(user.Id);
                account = await _database.GetAccount(user.Id);
                await Context.Interaction.ModifyOriginalResponseAsync(resp => resp.Content = $"{user.Mention} a été warn pour \"{modal.Reason}\" par {Context.User.Mention}");
            }
            else
                await RespondAsync($"{user.Mention} a été warn pour \"{modal.Reason}\" par {Context.User.Mention}");
            await _database.WarnUser(account, Context.User.Id, modal.Reason);
        }
    }

    public class WarnModal : IModal
    {
        public string Title { get; set; } = "Warn ";

        [InputLabel("User Id?")]
        [ModalTextInput("warn_id", style: TextInputStyle.Short, placeholder: "04444", maxLength: 30)]
        public string UserId { get; set; }

        [InputLabel("Reason?")]
        [ModalTextInput("warn_reason", style: TextInputStyle.Paragraph, placeholder: "Non", maxLength: 30)]
        public string Reason { get; set; }
    }
}