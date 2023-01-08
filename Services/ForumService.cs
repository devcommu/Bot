using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevCommuBot.Services
{
    public class ForumService
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly UtilService _util;
        private readonly DataService database;

        public ForumService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<ForumService>>();
            _util = services.GetRequiredService<UtilService>();
            database = services.GetRequiredService<DataService>();
            //Forum posts
            _client.ThreadCreated += OnCreateThread;
            _client.ThreadUpdated += OnThreadUpdate;
            _client.MessageReceived += OnThreadMessageReceived;
            //Forum
            _client.ChannelCreated += OnCreateForum;
            _client.ChannelUpdated += OnUpdateForum;
        }

        private async Task OnUpdateForum(SocketChannel oldChannel, SocketChannel newChannel)
        {
            if (newChannel is SocketForumChannel newForum)
            {
                var oldForum = (SocketForumChannel)oldChannel;
                var forumDb = await _util.ForceGetForum(newForum);
            }
        }

        private async Task OnCreateForum(SocketChannel channel)
        {
            if (channel is SocketForumChannel forum)
            {
                var forumDb = await _util.ForceGetForum(forum);
            }
        }
        private async Task OnThreadMessageReceived(SocketMessage msg)
        {
            if (!_util.IsAForum(msg.Channel))
                return;
            if (msg.Author.IsBot)
                return;
            var forum = (msg.Channel is SocketThreadChannel thread) ? (SocketForumChannel)thread.ParentChannel : (SocketForumChannel)msg.Channel;
            var forumDb = await _util.ForceGetForum(forum);
            //check github link
        }

        private async Task OnThreadUpdate(Cacheable<SocketThreadChannel, ulong> oldThreadCached, SocketThreadChannel newThread)
        {
            if (newThread.ParentChannel is SocketForumChannel forum)
            {
                if (!newThread.HasJoined)
                {
                    await newThread.JoinAsync();
                }
                /*
                if (!oldThreadCached.HasValue)
                    await oldThreadCached.DownloadAsync();
                var oldThread = oldThreadCached.Value;
                //more soon?
                */
            }
        }

        private async Task OnCreateThread(SocketThreadChannel thread)
        {
            if (!_util.IsAForum(thread.ParentChannel))
                return;
            var forum = (SocketForumChannel)thread.ParentChannel;
            _logger.LogDebug("THREAD CREATED in FORUM");
            await thread.JoinAsync();
            var forumDb = await _util.ForceGetForum(forum);
            if (forumDb.MessageDescription != "")
            {
                var embed = new EmbedBuilder()
                    .WithAuthor(_client.CurrentUser)
                    .WithTitle("Forum Rules")
                    .WithDescription(forumDb.MessageDescription)
                    .WithColor(_util.EmbedColor)
                    .AddField("Post qui ne respecte pas les règles?", "Mentionnez l'un des modérateurs et il s'en occupera!")
                    .AddField("Votre problème est résolu?", "> Si une réponse à permis de résoudre votre problème, merci d'utiliser la commande `/forumpost accept id` id correspondant à l'id du message! Si vous avez résolu votre problème tout seul, essayez d'expliquer comment vous avez fait pour aider les autres puis utilisez la même commande pour vous même!")
                    .WithFooter("Automated messages!")
                    .WithCurrentTimestamp()
                    .Build();
                var msg = await thread.SendMessageAsync(embed: embed);
                await msg.PinAsync();
            }
        }

        public static ForumTag? GetClosedTag(SocketForumChannel forum) => GetTag(forum, "Closed");
        private static ForumTag? GetTag(SocketForumChannel forum, string tagName)
        {
            return forum.Tags.FirstOrDefault(ft => ft.Name == tagName);
        }
    }
}