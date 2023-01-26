using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DevCommuBot.Data;
using DevCommuBot.Data.Models.Forums;
using DevCommuBot.Data.Models.Giveaways;
using DevCommuBot.Data.Models.Users;
using DevCommuBot.Data.Models.Warnings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevCommuBot.Services
{
    internal class DataService
    {
        private readonly ILogger _logger;
        private readonly DataContext _dataContext;

        private readonly UtilService _util;
        private readonly IServiceProvider _services;

        public DataService(IServiceProvider services)
        {
            _logger = services.GetRequiredService<ILogger<DataService>>();
            _dataContext = services.GetRequiredService<DataContext>();
            _services = services;
        }

        #region USER

        public async Task CreateAccount(ulong userId)
        {
            await _dataContext.Users.AddAsync(new User
            {
                UserId = userId,
            });
            await _dataContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get Account of user
        /// </summary>
        /// <param name="userId">User's discord id</param>
        /// <returns><see cref="User"/> if account exists or <see cref="null"/></returns>
        public async Task<User?> GetAccount(ulong userId)
            => await _dataContext.Users
            .Include(u => u.Warnings)
            .ThenInclude(uw => uw.Warning)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        public async Task<User> ForceGetAccount(ulong userId)
        {
            if (await _dataContext.Users.FirstOrDefaultAsync(u => u.UserId == userId) is not null)
                return await GetAccount(userId)!;
            await CreateAccount(userId);
            return await GetAccount(userId)!;
        }
        /// <summary>
        /// Update account
        /// </summary>
        /// <param name="user">User to be updated</param>
        /// <returns></returns>
        public async Task UpdateAccount(User user)
        {
            _dataContext.Users.Update(user);
            await _dataContext.SaveChangesAsync();
        }
        #endregion USER

        #region WARN

        public async Task WarnUser(User victime, ulong authorId, string details, WarningReason reasontype = WarningReason.NO_REASON)
        {
            var warning = new Warning()
            {
                AuthorId = authorId,
                Details = details,
                Reason = reasontype,
                Created = DateTime.UtcNow
            };
            await _dataContext.Warnings.AddAsync(warning);
            victime.Warnings.Add(new()
            {
                User = victime,
                UserId = victime.Id,
                Warning = warning,
                WarningId = warning.Id
            });
            await _dataContext.SaveChangesAsync();
        }

        #endregion WARN

        #region STARBOARD
        public Task<bool> HasAStarboardEntry(ulong messageId)
            => _dataContext.Starboards.AnyAsync(m => m.MessageId == messageId);
        //TODO: Better code!
        public Task<StarboardEntry?> GetStarboardEntry(ulong messageId, EntryType entryType)
        {
            return entryType switch
            {
                EntryType.OriginalMessage => _dataContext.Starboards.FirstOrDefaultAsync(st => st.StarboardMessageId == messageId),
                EntryType.Message => _dataContext.Starboards.FirstOrDefaultAsync(st => st.MessageId == messageId),
                _ => null,
            };
        }

        public async Task CreateStarboardEntry(ulong authorId, ulong messageId, ulong channelId, ulong createdMessageId, int stars = default)
        {
            await _dataContext.Starboards.AddAsync(new StarboardEntry
            {
                ArrivedTime = DateTime.UtcNow,
                AuthorId = authorId,
                ChannelId = channelId,
                MessageId = messageId,
                StarboardMessageId = createdMessageId,
                Score = (stars == default) ? 5 : stars
            });
            await _dataContext.SaveChangesAsync();
        }
        public async Task UpdateScoreStarboard(StarboardEntry entry, int score)
        {
            entry.Score = score;
            await _dataContext.SaveChangesAsync();
        }
        public async Task UpdateScoreStarboard(ulong messageId = default, ulong starboardMessageId = default, int score = default)
        {
            var entry = await _dataContext.Starboards.FirstOrDefaultAsync(st => st.MessageId == messageId || st.StarboardMessageId == starboardMessageId);
            if (entry != null)
            {
                entry.Score = score;
                await _dataContext.SaveChangesAsync();
            }
        }
            #endregion Starboard

            #region FORUM

            public async Task CreateForum(ulong forumId)
        {
            await _dataContext.Forums.AddAsync(new Forum
            {
                ChannelId = forumId,
            });
            await _dataContext.SaveChangesAsync();
        }

        public async Task<Forum> GetForum(ulong forumId)
            => await _dataContext.Forums
            .Include(f => f.Entries)
            .FirstOrDefaultAsync(f => f.ChannelId == forumId);

        /*public async Task UpdateForum(ulong forumId)
        {
            var forum = await GetForum(forumId);
            if (forum == null)
            {
                _ = new ArgumentException("Forum does not exist");
                return;
            }
            _dataContext.Forums.Update(forum);
            await _dataContext.SaveChangesAsync();
        }*/

        public async Task UpdateForum(ulong forumId, string name = "", string description = "")
        {
            var forum = await GetForum(forumId);
            if (forum == null)
            {
                _ = new ArgumentException("Forum does not exist");
                return;
            }
            if (name != "")
                forum.Name = name;
            if (description != "")
                forum.MessageDescription = description;
            _dataContext.Forums.Update(forum);
            await _dataContext.SaveChangesAsync();
        }

        #endregion FORUM

        #region GIVEAWAY

        public async Task CreateGiveaway(ulong authorId, ulong messageId, string messageDescription, string wonObject, int amountOfWinners, DateTime startAt, DateTime EndAt, GiveawayState state, GiveawayCondition condition = GiveawayCondition.NO_CONDITION)
        {
            await _dataContext.Giveaways.AddAsync(new Giveaway
            {
                AuthorId = authorId,
                MessageId = messageId,
                MessageDescription = messageDescription,
                WonObject = wonObject,
                AmountOfWinners = amountOfWinners,
                StartAt = startAt,
                EndAt = EndAt,
                Condition = condition,
                State = state
            });
            await _dataContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get Giveaway by message id
        /// </summary>
        /// <param name="messageId">message that contains giveaway</param>
        /// <returns></returns>
        public Task<Giveaway> GetGiveaway(ulong messageId)
        {
            return _dataContext.Giveaways.FirstOrDefaultAsync(g => g.MessageId == messageId);
        }

        public Task<Giveaway> GetGiveaway(int giveawayId)
        {
            return _dataContext.Giveaways.FirstOrDefaultAsync(g => g.Id == giveawayId);
        }

        public async Task UpdateGiveaway(ulong messageId, string messageDescription = "", string winObject = "", int winners = 0, DateTime startAt = default, DateTime EndAt = default, GiveawayCondition condition = GiveawayCondition.NO_CONDITION, GiveawayState state = default)
        {
            var giveaway = await GetGiveaway(messageId);
            if (giveaway == null)
            {
                _ = new ArgumentException("Giveaway does not exist");
                return;
            }
            if (messageDescription != "")
                giveaway.MessageDescription = messageDescription;
            if (winObject != "")
                giveaway.WonObject = winObject;
            if (winners != giveaway.AmountOfWinners)
                giveaway.AmountOfWinners = winners;
            if (startAt != default)
                giveaway.StartAt = startAt;
            if (EndAt != default)
                giveaway.EndAt = EndAt;
            if (giveaway.Condition != condition && condition != default)
                giveaway.Condition = condition;
            if (giveaway.State != state && state != default)
                giveaway.State = state;
            _dataContext.Giveaways.Update(giveaway);
            await _dataContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get active giveaways
        /// </summary>
        /// <returns>List of active giveaways</returns>
        public Task<List<Giveaway>> GetRunningGiveaways()
        {
            return _dataContext.Giveaways.Where(x => x.State == GiveawayState.RUNNING).ToListAsync();
        }

        /// <summary>
        /// Add a player to a giveaway
        /// </summary>
        /// <param name="messageId">Giveaway message's id</param>
        /// <param name="userId">Id of user</param>
        /// <returns></returns>
        public async Task AddEntryGiveaway(ulong messageId, ulong userId)
        {
            var giveaway = await GetGiveaway(messageId);
            if (giveaway == null)
            {
                _ = new ArgumentException("Giveaway does not exist");
                return;
            }
            if (giveaway.State != GiveawayState.RUNNING)
            {
                _ = new ArgumentException("Giveaway is not running");
                return;
            }
            giveaway.Participants.Add(userId);
            _dataContext.Giveaways.Update(giveaway);
            await _dataContext.SaveChangesAsync();
        }

        /// <summary>
        /// Add a player to the giveaway
        /// </summary>
        /// <param name="giveawayId">Giveaway's id</param>
        /// <param name="userId">User's Id</param>
        /// <returns></returns>
        public async Task AddEntryGiveaway(int giveawayId, ulong userId)
        {
            var giveaway = await GetGiveaway(giveawayId);
            if (giveaway == null)
            {
                _ = new ArgumentException("Giveaway does not exist");
                return;
            }
            if (giveaway.State != GiveawayState.RUNNING)
            {
                _ = new ArgumentException("Giveaway is not running");
                return;
            }
            giveaway.Participants.Add(userId);
            _dataContext.Giveaways.Update(giveaway);
            await _dataContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get player list that joined a giveaway
        /// </summary>
        /// <param name="messageId">Giveaway's message id</param>
        /// <returns><see cref="null"/> if giveaway do not exist</returns>
        public async Task<List<ulong>> GetGiveawayEntries(ulong messageId)
        {
            var giveaway = await GetGiveaway(messageId);
            if (giveaway == null)
            {
                _ = new ArgumentException("Giveaway does not exist");
                return null;
            }
            return giveaway.Participants;
        }

        #endregion GIVEAWAY
    }

    internal enum EntryType
    {
        OriginalMessage,
        Message,
    }
}