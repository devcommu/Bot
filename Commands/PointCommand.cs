using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord.Interactions;
using Discord.WebSocket;

namespace DevCommuBot.Commands
{
    public class PointCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UtilService _util;
        private readonly PointService _pointService;
        private readonly DataService _database;

        [SlashCommand("points", "Get your bank account")]
        public async Task Points([Summary(name: "user", description: "User to get points")] SocketGuildUser mentionned = null)
        {
            if (mentionned is not null)
            {
                //Get mentionned's point
                var account = await _database.GetAccount(mentionned.Id);
                if (account is null)
                {
                    await RespondAsync("This user doesn't own an account!");
                }
                else
                {
                    await RespondAsync($"He has {account.Points} points!");
                }
            }
            else
            {
                //get own points
                var account = await _database.GetAccount(Context.User.Id);
                if (account is null)
                {
                    //Create account
                    // Show message before processing creating account to avoid taking 1hour
                    await RespondAsync("Your account has been created");
                    await _database.CreateAccount(Context.User.Id);
                }
                else
                {
                    await RespondAsync($"You have {account.Points} points!");
                }
            }
        }
    }
}