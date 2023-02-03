using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using Camille.RiotGames.LolStatusV3;

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using static System.Collections.Specialized.BitVector32;

namespace DevCommuBot.Services
{
    internal class StarboardService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly UtilService _util;
        private readonly DataService _database;
        public const string EMOTE_STAR = "⭐";
        public StarboardService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<StarboardService>>();
            _util = services.GetRequiredService<UtilService>();
            _database = services.GetRequiredService<DataService>();

            _client.ReactionAdded += OnReactionStarboard;
        }

        private async Task OnReactionStarboard(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (!message.HasValue)
                await message.DownloadAsync();
            if (!channel.HasValue)
                await channel.DownloadAsync();
            if (_util.GetAllowedChannels().First(c => c!.Id == channel.Id) is null)
            {
                //React can be counted
                if (reaction.Emote.Name == EMOTE_STAR)
                {
                    //Stared a message in starboard channel(how it is possible)
                    if (channel.Id == UtilService.CHANNEL_STARBOARD_ID)
                        return;
                    if (!message.Value.Reactions.FirstOrDefault(r => r.Key.Name == EMOTE_STAR).Equals(default))
                    {
                        var reactions = message.Value.Reactions.FirstOrDefault(r => r.Key.Name == EMOTE_STAR).Value;
                        if (reactions.ReactionCount > UtilService.MIN_REACTION_STARBOARD)
                        {
                            //Message has already been submited to Starboard
                            if (await _database.HasAStarboardEntry(message.Id))
                            {
                                await UpdateScore(message.Id, reactions.ReactionCount);
                            }
                            else
                            {
                                var msg = await AnnounceNewEntry(message.Value, message.Value.Author);
                                await _database.CreateStarboardEntry(message.Value.Author.Id, message.Id, channel.Id, msg.Id, reactions.ReactionCount);
                            }
                                return;
                        }
                    }
                }
            }
        }
        private async Task UpdateScore(ulong messageId, int score)
        {
            var starboard = await _database.GetStarboardEntry(messageId, EntryType.OriginalMessage);
            if(starboard is null)
            {
                _ = new ArgumentException("Starboard does not exists");
                return;
            }
            IUserMessage originmessage = await _util.GetStarboardChannel().GetMessageAsync(starboard.MessageId) as IUserMessage;
            await originmessage.ModifyAsync(m => {
                if (m.Embed.IsSpecified)
                {
                    var oldEmbed = m.Embed.Value;
                    var embed = new EmbedBuilder()
                        .WithAuthor(oldEmbed.ToEmbedBuilder().Author)
                        .WithColor(_util.EmbedColor)
                        .AddField("Message:", oldEmbed.Fields[0])
                        .AddField("Message link:", oldEmbed.Fields[1])
                        .AddField("Stars", $"{score} {EMOTE_STAR}", true)
                        .Build();
                    m.Embed = embed;
                }
            });
        }

        private async Task<RestMessage> AnnounceNewEntry(IUserMessage message, IUser author)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(author)
                .WithColor(_util.EmbedColor)
                .AddField("Message:", $"`{message.Content}`")
                .AddField("Message link:", $"{message.GetJumpUrl()}")
                .AddField("Stars", $"5 {EMOTE_STAR}", true)
                .Build();
            var msg = await _util.GetStarboardChannel().SendMessageAsync(embed: embed);
            return msg;
        }
    }
}
