using System;
using System.Collections.Generic;
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
                var forum = database.GetForum(newForum.Id);
                if (forum is null)
                {
                    //Hmm forum created when i was sleeping
                    await newForum.ModifyAsync(f =>
                    {
                        var closedTag = new ForumTagBuilder("Closed", isModerated: true, emoji: Emote.Parse("🔒")).Build();
                        if (!f.Tags.IsSpecified)
                        {
                            f.Tags = new List<ForumTagProperties>()
                        {
                            closedTag
                        };
                        }
                        else
                        {
                            var newValue = f.Tags.Value.ToList();
                            newValue.Add(closedTag);
                            f.Tags = newValue;
                        }
                    });
                    var closed = newForum.Tags.FirstOrDefault(x => x.Name == "Closed");
                    _ = database.CreateForum(newForum.Id, closed).ContinueWith(x =>
                    {
                        _util.SendLog("Forum Discovered!", "I just discovered a forum that i didnt heard about!!\n Now adding it to the database.");
                    });
                }
                if (oldForum.Name != newForum.Name)
                {
                    //Update Name
                    //Not gonna lie so useless!
                }
            }
        }

        private async Task OnCreateForum(SocketChannel channel)
        {
            if (channel is SocketForumChannel forum)
            {
                //
                await forum.ModifyAsync(f =>
                {
                    var closedTag = new ForumTagBuilder("Closed", isModerated: true, emoji: Emote.Parse("🔒")).Build();
                    if (!f.Tags.IsSpecified)
                    {
                        f.Tags = new List<ForumTagProperties>()
                        {
                            closedTag
                        };
                    }
                    else
                    {
                        var newValue = f.Tags.Value.ToList();
                        newValue.Add(closedTag);
                        f.Tags = newValue;
                    }
                });
                var closed = forum.Tags.FirstOrDefault(x => x.Name == "Closed");
                _ = database.CreateForum(channel.Id, closed).ContinueWith(x =>
                {
                    _util.SendLog("New forum created!", $"Successfully registered the forum in the database! forum named: {forum.Name}");
                });
            }
        }

        private async Task OnThreadMessageReceived(SocketMessage msg)
        {
            if (msg.Channel is SocketThreadChannel channel)
            {
                if (channel.ParentChannel is SocketForumChannel forum)
                {
                    var forumDb = database.GetForum(forum.Id);
                    if (forumDb is null)
                    {
                        //Hmm forum created when i was sleeping
                        await forum.ModifyAsync(f =>
                        {
                            var closedTag = new ForumTagBuilder("Closed", isModerated: true, emoji: Emote.Parse("🔒")).Build();
                            if (!f.Tags.IsSpecified)
                            {
                                f.Tags = new List<ForumTagProperties>()
                                {
                                    closedTag
                                };
                            }
                            else
                            {
                                var newValue = f.Tags.Value.ToList();
                                newValue.Add(closedTag);
                                f.Tags = newValue;
                            }
                        });
                        var closed = forum.Tags.FirstOrDefault(x => x.Name == "Closed");
                        _ = database.CreateForum(forum.Id, closed).ContinueWith(x =>
                        {
                            _util.SendLog("Forum Discovered!", "I just discovered a forum that i didnt heard about!!\n Now adding it to the database.");
                        });
                    }
                    //check github link
                }
            }
        }

        private async Task OnThreadUpdate(Discord.Cacheable<SocketThreadChannel, ulong> oldThreadCached, SocketThreadChannel newThread)
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
            if (thread.ParentChannel is SocketForumChannel forum)
            {
                _logger.LogDebug("THREAD CREATED in FORUM");
                await thread.JoinAsync();
                var forumDb = await database.GetForum(forum.Id);
                if (forumDb is null)
                {
                    //Hmm forum created when i was sleeping
                    await forum.ModifyAsync(f =>
                    {
                        var closedTag = new ForumTagBuilder("Closed", isModerated: true, emoji: Emote.Parse("🔒")).Build();
                        if (!f.Tags.IsSpecified)
                        {
                            f.Tags = new List<ForumTagProperties>()
                            {
                                closedTag
                            };
                        }
                        else
                        {
                            var newValue = f.Tags.Value.ToList();
                            newValue.Add(closedTag);
                            f.Tags = newValue;
                        }
                    });
                    var closed = forum.Tags.FirstOrDefault(x => x.Name == "Closed");
                    _ = database.CreateForum(forum.Id, closed).ContinueWith(x =>
                    {
                        _util.SendLog("Forum Discovered!", "I just discovered a forum that i didnt heard about!!\n Now adding it to the database.");
                    });
                    if(forumDb.MessageDescription != "")
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

            }
        }
    }
}