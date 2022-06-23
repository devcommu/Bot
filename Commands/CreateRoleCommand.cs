using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace DevCommuBot.Commands
{
    public class CreateRoleCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public UtilService Utils { get; set; }
        public ILogger<CommandHandler> Logger { get; set; }

        [SlashCommand("createrole", "Create your own role [Only for boosted person]")]
        public async Task CreateCommand([Summary("roleName", "The name of your role")] string name, [Summary("color", "Color of your role in hexadecimal")] string color, string iconUrl = null)
        {
            if (Context.Channel.Id != UtilService.CHANNEL_BOOSTERS_ID)
            {
                _ = RespondAsync("Vous ne pouvez pas utilisez cette commande ici", ephemeral: true);
                return;
            }
            if (Utils.CreateroleCooldown.ContainsKey(Context.User.Id))
            {
                //A cooldown entry exist for this user
                if (DateTimeOffset.Now.ToUnixTimeSeconds() < (Utils.CreateroleCooldown[Context.User.Id] + 60 * 3))
                {
                    _ = RespondAsync("Merci de respecter le cooldown de 5minutes!", ephemeral: true);
                    return;
                }
            }
            color = color.Replace("#", "");
            if (int.TryParse(color, System.Globalization.NumberStyles.HexNumber, null, out int finalColor))
            {
                if (Utils.HasCustomRole(Context.User as SocketGuildUser))
                {
                    var memberRole = Utils.GetCustomRole(Context.User as SocketGuildUser);
                    await memberRole.ModifyAsync(r =>
                    {
                        r.Name = name;
                        r.Color = new Color((uint)finalColor);
                        r.Hoist = true;
                    });
                    if (!string.IsNullOrEmpty(iconUrl))
                    {
                        // S/O King
                        Stream imageStream;
                        HttpClient client = new()
                        {
                            BaseAddress = new Uri(iconUrl)
                        };
                        imageStream = await client.GetStreamAsync("");
                        if (imageStream != null)
                        {
                            await memberRole.ModifyAsync(r =>
                            {
                                r.Icon = new Image(imageStream);
                            });
                        }
                    }
                    _ = RespondAsync($"Vous venez de mettre à jour votre rôle {memberRole.Mention}");
                }
                else
                {
                    if (Utils.GetGuild().Roles.Any(r => r.Name.ToLower() == name.ToLower()) || (name.ToLower() is "everyone" or "here"))
                    {
                        //AVOID FAKE MODS && everyone
                        _ = RespondAsync("Le nom du rôle souhaité existe déjà");
                        return;
                    }
                    var role = await Utils.GetGuild().CreateRoleAsync(name, null, color: new Color((uint)finalColor), true, false, options:new()
                    {
                        AuditLogReason = "Booster creation role",
                    });
                    await role.ModifyAsync(r =>
                    {
                        r.Position = Utils.GetBoostersRole().Position + 1;
                    });
                    await (Context.User as SocketGuildUser).AddRoleAsync(role);
                    if (!string.IsNullOrEmpty(iconUrl))
                    {
                        // S/O King
                        Stream imageStream;
                        HttpClient client = new()
                        {
                            BaseAddress = new Uri(iconUrl)
                        };
                        imageStream = await client.GetStreamAsync("");
                        if (imageStream != null)
                        {
                            await role.ModifyAsync(r =>
                            {
                                r.Icon = new Image(imageStream);
                            });
                        }
                    }
                    RespondAsync($"Vous venez de crée le role: {role.Mention}");
                }
                Utils.CreateroleCooldown[Context.User.Id] = DateTimeOffset.Now.ToUnixTimeSeconds();
                return;
            }
            else
            {
                _ = RespondAsync("Merci de faire parvenir un hexadeciaml pour la couleur!");
            }
        }

        public override void OnModuleBuilding(InteractionService commandService, ModuleInfo module)
        {
            //_logger.LogDebug("CreateRoleCommand");
            Console.WriteLine("CreateRoleCommand");
            base.OnModuleBuilding(commandService, module);
        }
    }
}