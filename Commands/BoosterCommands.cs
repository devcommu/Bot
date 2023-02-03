using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DevCommuBot.Services;

using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace DevCommuBot.Commands
{
    [Group("booster", "Commandes pour les boosters")]
    public class BoosterCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public UtilService Utils { get; set; }
        public ILogger<CommandHandler> Logger { get; set; }
        internal DataService Database { get; set; }

        [SlashCommand("role", "Create/Modify your custom role")]
        public async Task CreateRoleCommand([Summary("roleName", "The name of your role")] string name, [Summary("color", "Color of your role in hexadecimal")] string color, string iconUrl = null)
        {
            if (Context.Channel.Id != UtilService.CHANNEL_BOOSTERS_ID)
            {
                _ = RespondAsync("Vous ne pouvez pas utilisez cette commande ici", ephemeral: true);
                return;
            }
            var account = await Database.ForceGetAccount(Context.User.Id);
            if (account.BoosterAdvantage is null)
            {
                await Utils.CreateBoosterAdvantage(account, (SocketGuildUser)Context.User);
            }
            //TODO: Remove no one care of this
            /*
            if (Utils.CreateroleCooldown.ContainsKey(Context.User.Id))
            {
                //A cooldown entry exist for this user
                if (DateTimeOffset.Now.ToUnixTimeSeconds() < (Utils.CreateroleCooldown[Context.User.Id] + 60 * 3))
                {
                    _ = RespondAsync("Merci de respecter le cooldown de 5minutes!", ephemeral: true);
                    return;
                }
            }*/
            color = color.Replace("#", "");
            if (int.TryParse(color, System.Globalization.NumberStyles.HexNumber, null, out int finalColor))
            {
                if (Utils.HasCustomRole(Context.User as SocketGuildUser))
                {
                    //User already has a custom role, he wants to manage it
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

                    account.BoosterAdvantage.RoleId = memberRole.Id;
                }
                else
                {
                    //Creation Role
                    if (Utils.GetGuild().Roles.Any(r => r.Name.ToLower() == name.ToLower()) || (name.ToLower() is "everyone" or "here"))
                    {
                        //AVOID FAKE MODS && everyone
                        _ = RespondAsync("Le nom du rôle souhaité existe déjà");
                        return;
                    }
                    var role = await Utils.GetGuild().CreateRoleAsync(name, null, color: new Color((uint)finalColor), true, false, options: new()
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
                    var embed = new EmbedBuilder()
                        .WithAuthor(Context.User)
                        .WithColor(Utils.EmbedColor)
                        .WithTitle("Création de role")
                        .WithDescription("Vous venez de crée le role: " + role.Mention)
                        .WithCurrentTimestamp()
                        .WithFooter("Merci du boost!")
                        .Build();
                    _ = RespondAsync($"Création effectué!", embed: embed);
                    account.BoosterAdvantage.RoleId = role.Id;
                }
                Utils.CreateroleCooldown[Context.User.Id] = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            else
            {
                _ = RespondAsync("Merci de faire parvenir un hexadeciaml pour la couleur!");
            }
        }

        [SlashCommand("voice", "Create/Modify your custom voice channel")]
        public async Task ManageVoiceCommand([Summary("channelName", "The name of your channel")] string name)
        {
            if (Context.Channel.Id != UtilService.CHANNEL_BOOSTERS_ID)
            {
                _ = RespondAsync("Vous ne pouvez pas utilisez cette commande ici", ephemeral: true);
                return;
            }
            var account = await Database.ForceGetAccount(Context.User.Id);
            if (account.BoosterAdvantage is null)
            {
                await Utils.CreateBoosterAdvantage(account, (SocketGuildUser)Context.User);
            }
            if(account.BoosterAdvantage.VocalId is null)
            {
                //Create voice channel!
                //TODO: Create voice channel
            }
            else
            {
                //Manage voice channel
                var channel = Utils.GetGuild().GetVoiceChannel(account.BoosterAdvantage.VocalId.Value);
                if(channel is null)
                {
                    _ = RespondAsync("Il semblerait que vous possédiez déjà un salon vocal, mais impossible de le trouver! Aurait-il été supprimé?", ephemeral: true);
                    //Debug:
                    Utils.SendLog("Erreur boosters", $"> Problème rencontré: Impossible de trouver le channel de {Context.User}\n> Useless data:\n|| Channel null, valeur db: {account.BoosterAdvantage.VocalId.Value} ||\n");
                    return;
                }
                var oldName = channel.Name;
                await channel.ModifyAsync(c =>
                {
                    c.Name = name;
                });
                _ = RespondAsync($"Mise à jour effectué!\n> Ancien nom: {oldName}\n> Nouveau nom: {name}");
                Utils.SendLog("Boosters", $"> {Context.User} vient de modifier son salon vocal de {oldName} à {name}");
            }
        }

        public override void OnModuleBuilding(InteractionService commandService, ModuleInfo module)
        {
            Console.WriteLine("Booster");
            base.OnModuleBuilding(commandService, module);
        }
    }
}