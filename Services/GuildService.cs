﻿using System;
using System.Collections.Generic;
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
        private Dictionary<ulong, int> StarboardMessages = new(); // ulong => message id , int => number of star

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
            _client.Ready += OnClientReady;
            /*_client.Ready += OnReady;
            _client.InteractionCreated += OnInteraction;*/
            _client.GuildMemberUpdated += OnGuildUpdate;
            _client.MessageReceived += OnMessageReceive;
            _client.ReactionAdded += OnReactionAdded;
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (!message.HasValue)
                await message.DownloadAsync();
            if (channel.HasValue)
                await channel.DownloadAsync();
            if (_util.GetAllowedChannels().First(c => c!.Id == channel.Id) is null)
            {
                //React can be counted
                if (reaction.Emote.Name == "⭐")
                {
                    //Stared a message in starboard channel(how it is possible)
                    if (channel.Id == UtilService.CHANNEL_STARBOARD_ID)
                        return;
                    if (!message.Value.Reactions.FirstOrDefault(r => r.Key.Name == "⭐").Equals(default))
                    {
                        var reactions = message.Value.Reactions.FirstOrDefault(r => r.Key.Name == "⭐").Value;
                        if (reactions.ReactionCount > UtilService.MIN_REACTION_STARBOARD)
                        {
                            //Message has already been submited to Starboard
                            //TODO: Update count of star in messages
                            if (StarboardMessages.ContainsKey(message.Id))
                                return;
                            //TODO:
                        }
                    }
                }
            }
        }

        private async Task OnClientReady()
        {
            var channel = _util.GetGuild().GetTextChannel(UtilService.CHANNEL_ROLES_ID);
            var obj = await channel.GetMessagesAsync(limit: 5).ToListAsync();
            var messages = obj[0];
            var message = messages.Where(m => m.Author.Id == _client.CurrentUser.Id)?.FirstOrDefault();
            if (message != null)
            {
            }
            else
            {
                //Pas de message envoyé.
                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Choisissez un role")
                    .WithCustomId("role-selection")
                    .WithMinValues(1)
                    .WithMaxValues(1)
                    .AddOption("Projects", "projects", "Accès aux notification de webhook(inutile)")
                    .AddOption("Gaming", "gaming", "Obtenez le role gaming et devenez cool (imo)")
                    .AddOption("Developer Chats", "devchats", "Accédez à un salon sous estimé de la communauté")
                    .AddOption("Free Games", "free-games", "Accédez à un salon de jeux gratuits")
                    .AddOption("Discord Games", "discord-games", "Accédez à un salon de jeux discord(Okureta etc..)");

                var builder = new ComponentBuilder()
                    .WithSelectMenu(menuBuilder);
                await channel.SendMessageAsync("Récupérez vos rôles ici:", components: builder.Build());
            }
            _client.Ready -= OnClientReady;
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
            //Verfier si ça parle d'hebergeur
            if (message.Content.Contains("hébergeur") || message.Content.Contains("hebergeur"))
            {
                //verifier perm user
            }
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
                        await _util.MemberUnboosted(member);
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
                            .WithDescription("> **Merci d'avoir boosté!!!**\nEn boostant vous avez accès à la commande `/createrole` vous permettant de créer votre propre rôle")
                            .WithCurrentTimestamp()
                            .Build();
                        await _util.GetBoostersChannel().SendMessageAsync(text: $"Merci d'avoir booster le discord {member.Mention}", embed: embed);
                    }
                }
            }
            return;
        }

        private Task OnInteraction(SocketInteraction arg)
        {
            if (arg is SocketMessageComponent component)
            {
                //Button Integrations?
            }
            return Task.CompletedTask;
        }

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
                .WithDescription("Bienvenue!")
                .WithFooter($"Nous sommes maintenant {member.Guild.MemberCount} membres.")
                .Build();
            _util.GetWelcomeChannel().SendMessageAsync(embed: embedMessage);
            return Task.CompletedTask;
        }
    }
}