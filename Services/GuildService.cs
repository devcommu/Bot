using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevCommuBot.Services
{
    internal class GuildService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly IConfigurationRoot _config;
        private readonly UtilService _util;
        private readonly PointService _pointService;
        private readonly DataService _database;

        public GuildService(IServiceProvider services)
        {
            _config = services.GetRequiredService<IConfigurationRoot>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<GuildService>>();
            _util = services.GetRequiredService<UtilService>();
            _database = services.GetRequiredService<DataService>();
            _pointService = services.GetRequiredService<PointService>();

            _client.UserJoined += OnUserJoin;
            _client.LeftGuild += OnUserLeft;
            /*_client.Ready += OnReady;
            _client.InteractionCreated += OnInteraction;*/
            _client.GuildMemberUpdated += OnGuildUpdate;
            _client.MessageReceived += OnMessageReceive;
        }

        private Task OnMessageReceive(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message)
                return Task.CompletedTask;
            if (message.Source is not MessageSource.User)
                return Task.CompletedTask;
            /*if (_util.GetAllowedChannels().Exists(c => c is not null and c.Id == message.Channel.Id) is false)
                return Task.CompletedTask;*/
            _pointService.HandleMessage(message);
            return Task.CompletedTask;
        }

        private async Task OnGuildUpdate(Cacheable<SocketGuildUser, ulong> cachedMember, SocketGuildUser member)
        {
            //Boost tracker
            SocketGuildUser oldMember = cachedMember.Value ?? await cachedMember.DownloadAsync();
            if (oldMember.Roles.Count != member.Roles.Count)
            {
                //Roles Moved
                var added = member.Roles.Except(oldMember.Roles).ToList();
                var removed = oldMember.Roles.Except(member.Roles).ToList();
                if (removed.Count is not 0)
                {
                    //A roles have been removed to member
                    if (removed.Any(r => r.Id == UtilService.ROLE_BOOSTERS_ID))
                    {
                        //user is not boosting any more
                        var roles = "";
                        member.Roles.ToList().ForEach(r =>
                        {
                            roles += $"{r.Mention} \n";
                        });
                        _util.SendLog($"{member} unboosted", $"> He has these roles:\n{roles}", member);
                        //Check if user had a custom role:
                        if (_util.HasCustomRole(member))
                        {
                            //user had a custom role
                            var customRole = member.Roles.First(r => r.Position > _util.GetBoostersRole().Position);
                            await member.RemoveRoleAsync(customRole, options: new()
                            {
                                AuditLogReason = "User has stopped boost"
                            });
                            _util.SendLog($"Removed CustomRole to {member}", $"Role: {customRole}\n Cause: No more boosting!", member);
                            //TODO: Remove Role!
                        }
                    }
                }
                if (added.Count is not 0)
                {
                    if (added.Any(r => r.Id == UtilService.ROLE_BOOSTERS_ID))
                    {
                        //User has boosted!
                        var embed = new EmbedBuilder()
                            .WithAuthor(member)
                            .WithColor(_util.EmbedColor)
                            .WithTitle($"{member} vient de booster!")
                            .WithDescription("> **Merci d'avoir booster!!!**\nEn boostant vous avez accès à la commande `/createrole` vous permettant ainsi de crée votre propre role")
                            .WithCurrentTimestamp()
                            .Build();
                        _util.GetBoostersChannel().SendMessageAsync(text: member.Mention, embed: embed);
                    }
                }
            }
            return;
        }

        private Task OnInteraction(SocketInteraction arg)
        {
            if (arg is SocketSlashCommand command)
                HandleSlashCommand(command);
            if (arg is SocketMessageComponent component)
            {
                //Button Integrations?
            }
            return Task.CompletedTask;
        }

        private async Task HandleSlashCommand(SocketSlashCommand command)
        {
            var member = command.User as SocketGuildUser;
            switch (command.Data.Name)
            {
                case "points":
                    /*User? account;
                    if(command.Data.Options is not null)
                    {
                        account = await _database.GetAccount((command.Data.Options.FirstOrDefault().Value as SocketGuildUser).Id);
                        if (account is null)
                        {
                            await command.RespondAsync("This user doesn't own an account!");
                        }
                        else
                        {
                            await command.RespondAsync($"He has {account.Points} points!");
                        }
                    }
                    else
                    {
                        account = await _database.GetAccount(member.Id);
                        if (account is null)
                        {
                            //Create account
                            // Show message before processing creating account to avoid taking 1hour
                            await command.RespondAsync("Your account has been created");
                            await _database.CreateAccount(member.Id);
                        }
                        else
                        {
                            await command.RespondAsync($"You have {account.Points} points!");
                        }
                    }*/
                    break;
                //Replace by message in rules?
                case "joinrole":
                    if (command.Data.Options.FirstOrDefault().Value.Equals("projects"))
                    {
                        //user chose to join "projets" role
                        if (member.Roles.Any(r => r.Id == UtilService.ROLE_PROJECTS_ID))
                        {
                            //user has already the role
                            await command.RespondAsync("Vous possédez déjà ce role");
                        }
                        else
                        {
                            await member.AddRoleAsync(_util.GetProjectsRole());
                            await command.RespondAsync("Vous venez de rejoindre le role Projects vous donnant accès au salon: <#874785010601832468>");
                        }
                    }
                    else
                    {
                        //user chose to join "gaming" role
                        if (member.Roles.Any(r => r.Id == UtilService.ROLE_GAMING_ID))
                        {
                            //user has already the role
                            await command.RespondAsync("Vous possédez déjà ce role!");
                        }
                        else
                        {
                            await member.AddRoleAsync(_util.GetGamingRole());
                            await command.RespondAsync("Vous venez de rejoindre le role Gaming vous donnant accès au salon: <#null>");
                        }
                    }
                    break;

                case "hms":
                    //Partnership information
                    /*var compo = new ComponentBuilder()
                        .WithButton("HostMyServers", null, ButtonStyle.Link, url: "https://www.hostmyservers.fr/")
                        .Build();
                    var embedHms = new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithAuthor("HostMyServers", "https://www.hostmyservers.fr/assets/images/logo/Logo-HMS-color-icon.png")
                        .WithTitle("HostMyServers - Location de VPS et Serveur Gaming")
                        .WithDescription("HostMyServers propose des VPS et des serveurs Gaming (Minecraft, Mcpe, Gmod), mais aussi des noms de domaine et des hébergements web. Depuis 2014, l’hébergeur présente plusieurs offres avec un rapport qualité/prix imbattable.\n\n> *Utilisez le code promo suivant*:\n **DEVCOMMU**\nAfin de profiter de 20% de réduction!")
                        .WithCurrentTimestamp()
                        .WithFooter("Partenaire depuis le 07/08/2021")
                        .Build();
                    //Why not emepheral?
                    _ = command.RespondAsync(embed: embedHms, component: compo);*/
                    break;

                case "createrole":
                    /*_logger.LogDebug("CreateRole called");
                    if (command.Channel.Id != UtilService.CHANNEL_BOOSTERS_ID)
                    {
                        _ = command.RespondAsync("Vous ne pouvez pas utilisez cette commande ici", ephemeral: true);
                        return;
                    }
                    if (_util.CreateroleCooldown.ContainsKey(member.Id))
                    {
                        //A cooldown entry exist for this user
                        if(DateTimeOffset.Now.ToUnixTimeSeconds() < (_util.CreateroleCooldown[member.Id] + 60 * 3))
                        {
                            command.RespondAsync("Merci de respecter le cooldown de 5minutes!", ephemeral: true);
                            return;
                        }
                    }
                    var roleName = command.Data.Options.FirstOrDefault(op => op.Name == "rolename").Value as string;
                    var color = command.Data.Options.FirstOrDefault(op => op.Name == "color").Value as string;
                    var iconUrl = command.Data.Options.FirstOrDefault(op => op.Name == "iconurl")?.Value as string;
                    //If user inserted an #
                    color = color.Replace("#", "");
                    if(int.TryParse(color, System.Globalization.NumberStyles.HexNumber, null, out int finalColor))
                    {
                        _logger.LogDebug("Create role executed");
                        if (_util.HasCustomRole(member))
                        {
                            var memberRole = _util.GetCustomRole(member);
                            await memberRole.ModifyAsync(r =>
                            {
                                r.Name = roleName;
                                r.Color = new Color((uint)finalColor);
                                r.Hoist = true;
                            });
                            if (!string.IsNullOrEmpty(iconUrl))
                            {
                                // S/O King
                                var base64 = ImageHelper.ConvertImageURLToBase64(iconUrl);
                                if (base64 != null)
                                {
                                    using HttpClient Client = new();
                                    Client.DefaultRequestHeaders.Add("Authorization", $"Bot {_config["token"]}");
                                    var content = new StringContent($"{{\"icon\": \"data:image/jpeg;base64, {base64}\"}}", Encoding.UTF8, "application/json");
                                    var resp = await Client.PatchAsync($"https://discord.com/api/v9/guilds/584987515388428300/roles/{memberRole.Id}", content);
                                    _logger.LogDebug(resp.StatusCode.ToString());
                                }
                            }
                            _ = command.RespondAsync($"Vous venez de mettre à jour votre rôle {memberRole.Mention}");
                        }
                        else
                        {
                            if (_util.GetGuild().Roles.Any(r => r.Name.ToLower() == roleName.ToLower()) || (roleName.ToLower() is "everyone" or "here"))
                            {
                                //AVOID FAKE MODS && everyone
                                _ = command.RespondAsync("Le nom du rôle souhaité existe déjà");
                                return;
                            }
                            var role = await _util.GetGuild().CreateRoleAsync(roleName, null, color: new Color((uint)finalColor), true, new()
                            {
                                AuditLogReason = "Booster creation role",
                            });
                            await role.ModifyAsync(r =>
                            {
                                r.Position = _util.GetBoostersRole().Position + 1;
                            });
                            await member.AddRoleAsync(role);
                            if (!string.IsNullOrEmpty(iconUrl))
                            {
                                // S/O King
                                var base64 = ImageHelper.ConvertImageURLToBase64(iconUrl);
                                if (base64 != null)
                                {
                                    using HttpClient Client = new();
                                    Client.DefaultRequestHeaders.Add("Authorization", $"Bot {_config["token"]}");
                                    var content = new StringContent($"{{\"icon\": \"data:image/jpeg;base64, {base64}\"}}", Encoding.UTF8, "application/json");
                                    var resp = await Client.PatchAsync($"https://discord.com/api/v9/guilds/584987515388428300/roles/{role.Id}", content);
                                    _logger.LogDebug(resp.StatusCode.ToString());
                                }
                            }
                            _ = command.RespondAsync($"Vous venez de crée le role: {role.Mention}");
                        }
                        _util.CreateroleCooldown[member.Id] = DateTimeOffset.Now.ToUnixTimeSeconds();
                        return;
                    }
                    else
                    {
                        _ = command.RespondAsync("Merci de faire parvenir un hexadeciaml pour la couleur!");
                    }*/
                    break;

                case "mute":
                    /*
                    if (member.GuildPermissions.KickMembers)
                    {
                        SocketGuildUser victim = command.Data.Options.FirstOrDefault(op => op.Name == "user").Value as SocketGuildUser;
                        if (int.TryParse(command.Data.Options.FirstOrDefault(op => op.Name == "duration")?.Value as string, out int duration))
                        {
                            _ = command.RespondAsync($"{victim} has been muted ", ephemeral: true);
                        }
                        else
                        {
                            _ = command.RespondAsync($"An error has occured with duration ", ephemeral: true);
                        }
                    }
                    await command.RespondAsync("Vous n'avez pas la permission d'éxectuer cette commande", ephemeral: true);
                    */
                    break;

                case "warn":
                    /*
                    if (member.GuildPermissions.Administrator)
                    {
                        SocketGuildUser victim = command.Data.Options.FirstOrDefault(op => op.Name == "user")?.Value as SocketGuildUser;
                        string reason = command.Data.Options.FirstOrDefault(op => op.Name == "user").Value as string;
                        _ = command.RespondAsync("Warned..", ephemeral: true);
                    }*/
                    break;

                case "gitpreview":
                    /*//I litteraly translated this js part to c#
                    //https://github.com/HimbeersaftLP/MagicalHourglass/blob/master/bot.js
                    // s/o Himbeersaft i guess
                    string url = command.Data.Options.FirstOrDefault(st => st.Name == "url")?.Value as string;
                    Regex regex = new(@"http(?:s|):\/\/github\.com\/(.*?\/.*?\/)blob\/(.*?\/.*?)#L([0-9]+)-?L?([0-9]+)?");
                    Regex FileEndRegex = new(@".*\.([a-zA-Z0-9]*)");
                    if (regex.IsMatch(url))
                    {
                        var match = regex.Match(url);
                        _logger.LogDebug($"Caught {match.Groups.Count}");
                        if(!int.TryParse(match.Groups[3].Value, out int lineAsked))
                        {
                            _logger.LogDebug($"le nombre: {match.Groups[3].Value} n'est pas reconnu comme un nombre");
                            await command.RespondAsync("Une erreur est survenue :(");
                            return;
                        }
                        int lineTo = -5;
                        if (match.Groups.Count > 5)
                        {
                            if(!int.TryParse(match.Groups[5].Value, out lineTo))
                            {
                                _logger.LogDebug($"le nombre: {match.Groups[5].Value} n'est pas reconnu comme un nombre");
                                await command.RespondAsync("Une erreur est survenue :(");
                            }
                        }
                        await command.RespondAsync("Searching Code!!");
                        using HttpClient httpClient = new();
                        var githubUrl = $"https://raw.githubusercontent.com/{match.Groups[1]}{match.Groups[2]}";
                        var response = await httpClient.GetAsync(githubUrl);
                        var originalResponse = await command.GetOriginalResponseAsync();
                        _logger.LogDebug($"Pour: {lineAsked} : {response.StatusCode}");
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var lines = content.Split("\n");
                            _logger.LogDebug($"Vérification valeur lineAsked");
                            if (lineAsked> lines.Length || lineAsked<0)
                            {
                                _logger.LogDebug($"Invalid lineFrom gave {lineAsked}");
                                await originalResponse.ModifyAsync(m=>
                                {
                                    m.Content = ":robot: Ligne Introuvable!";
                                });
                                return;
                            }
                            if(lineTo == -5)
                            {
                                //Didnt ask to reach lineTo
                                int from = lineAsked - 5;
                                int to = lineAsked + 5;
                                var langMatch = FileEndRegex.Match(match.Groups[2].Value);
                                var lang = langMatch.Groups[1];
                                var cleanFileName = match.Groups[2].Value.Replace(@"\?.+", "");
                                var msg = $"Lignes {from} - {to} de {cleanFileName}\n```{lang}\n";
                                for(int i = from; i<=to; i++)
                                {
                                    msg += $"{lines[i]}\n";
                                }
                                msg += "\n```";
                                await originalResponse.ModifyAsync(m =>
                                {
                                    m.Content = msg;
                                });
                                _logger.LogDebug($"Matched {lang}");
                            }
                            else
                            {
                                if (lineTo > lines.Length || lineTo < 0 || lineTo < lineAsked)
                                {
                                    _logger.LogDebug($"Invalid lineTo gave {lineTo}");
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
                            _logger.LogDebug($"Unable to reach host: {githubUrl}");
                        }
                    }
                    else
                        _ = command.RespondAsync($"Il faut préciser une url suivant ce regex: `{regex}`");
                    */
                    break;
            }
        }

        /*private async Task OnReady()
        {
            _logger.LogInformation("Registering commands");
            var cmds = await _client.Rest.GetGuildApplicationCommands(UtilService.GUILD_ID);
            if (cmds.FirstOrDefault(c=> c.ApplicationId == _client.CurrentUser.Id) is not null)
            {
                //Register commands
                List<SlashCommandOptionBuilder> listOptionUser = new()
                {
                    new()
                    {
                        Name = "user",
                        Required = false,
                        Type = ApplicationCommandOptionType.User,
                        Description = "User your want to see points"
                    }
                };
                var listOptionJoin = new List<SlashCommandOptionBuilder>()
                {
                    new()
                    {
                        Name = "role",
                        Required = true,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Role you want to join",
                        Choices = new()
                        {
                            new ApplicationCommandOptionChoiceProperties()
                            {
                                Name = "Project",
                                Value = "projects"
                            },
                            new()
                            {
                                Name = "Gaming",
                                Value = "gaming"
                            }
                        }
                    }
                };
                var listOptionRole = new List<SlashCommandOptionBuilder>()
                {
                    new()
                    {
                        Name = "rolename",
                        Required = true,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Name of the role you want to create",
                    },
                    new()
                    {
                        Name = "color",
                        Required = true,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Color of your role in hexadecimal",
                    },
                    new()
                    {
                        Name = "iconurl",
                        Required = false,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Icon's url"
                    }
                };
                var listOptionWarn = new List<SlashCommandOptionBuilder>()
                {
                    new()
                    {
                        Name = "user",
                        Required = true,
                        Type = ApplicationCommandOptionType.User,
                        Description = "The user you want to warn",
                    },
                    new()
                    {
                        Name = "reason",
                        Required = true,
                        Type = ApplicationCommandOptionType.String,
                        Description = "Reason of warn",
                    }
                };
                SlashCommandBuilder pointsCommand = new()
                {
                    Name = "points",
                    Description = "Get Your amount of points",
                    Options = listOptionUser,
                };
                SlashCommandBuilder joinroleCommand = new()
                {
                    Name = "joinrole",
                    Description = "Join a role",
                    Options = listOptionJoin,
                };
                SlashCommandBuilder hmsCommand = new()
                {
                    Name = "hms",
                    Description = "Get information about our partner"
                };
                SlashCommandBuilder createRoleCommand = new()
                {
                    Name = "createrole",
                    Description = "Create your own role!(boosters)",
                    Options = listOptionRole
                };
                SlashCommandBuilder warnCommand = new()
                {
                    Name = "warn",
                    Description = "Warn an user",
                    Options = listOptionWarn
                };
                SlashCommandBuilder muteCommand = new()
                {
                    Name = "mute",
                    Description = "mute an user",
                    Options = new()
                    {
                        new()
                        {
                            Name = "user",
                            Required = true,
                            Type = ApplicationCommandOptionType.User,
                            Description = "User to be muted"
                        },
                        new()
                        {
                            Name = "duration",
                            Required = true,
                            Type = ApplicationCommandOptionType.Integer,
                            Description = "Mute duration in seconds",
                        }
                    }
                };
                SlashCommandBuilder gitPreviewCommand = new()
                {
                    Name = "gitpreview",
                    Description = "Preview a code",
                    Options = new()
                    {
                        new()
                        {
                            Name = "url",
                            Required = true,
                            Type = ApplicationCommandOptionType.String,
                            Description = "repo link"
                        }
                    }
                };
                SlashCommandBuilder EmbedCommand = new()
                {
                    Name = "embed",
                    Description = "Send an embed message",
                    Options = new()
                    {
                        new()
                        {
                            Name = "title",
                            Required = true,
                            Type = ApplicationCommandOptionType.String,
                            Description = "Emble Title"
                        },
                        new()
                        {
                            Name = "Description",
                            Required = false,
                            Type = ApplicationCommandOptionType.String,
                            Description = "Embed's description"
                        },
                        new()
                        {
                            Name = "Author",
                            Required = false,
                            Type = ApplicationCommandOptionType.User,
                            Description = "Embed's Author"
                        },
                        new()
                        {
                            Name = "Footer",
                            Required = true,
                            Type = ApplicationCommandOptionType.String,
                            Description = "Embed's Footer"
                        },
                        new()
                        {
                            Name = "Link",
                            Required = false,
                            Type = ApplicationCommandOptionType.String,
                            Description = "Link in title"
                        }
                    },
                };
                try
                {
                    await _client.Rest.CreateGuildCommand(pointsCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(joinroleCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(hmsCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(createRoleCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(muteCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(warnCommand.Build(), UtilService.GUILD_ID);
                    await _client.Rest.CreateGuildCommand(gitPreviewCommand.Build(), UtilService.GUILD_ID);
                    //Waiting 20secs for registering 2commands zzzzzzzzzzzzzzzz
                    _ =  _client.Rest.CreateGuildCommand(warnCommand.Build(), UtilService.GUILD_ID);
                    _ =  _client.Rest.CreateGuildCommand(muteCommand.Build(), UtilService.GUILD_ID);
                }
                catch (ApplicationCommandException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                    _logger.LogError(json);
                }
            }
        }*/

        private Task OnUserLeft(SocketGuild member)
        {
            //zzzzzzzzzzzzzzz
            return Task.CompletedTask;
        }

        private Task OnUserJoin(SocketGuildUser member)
        {
            var embedMessage = new EmbedBuilder()
                .WithAuthor(member)
                .WithColor(_util.EmbedColor)
                .WithDescription("Welcome!")
                .WithFooter($"We are now {member.Guild.MemberCount} members.")
                .Build();
            _util.GetWelcomeChannel().SendMessageAsync(embed: embedMessage);
            return Task.CompletedTask;
        }
    }
}