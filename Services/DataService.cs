using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DevCommuBot.Data;
using DevCommuBot.Data.Models.Forums;
using DevCommuBot.Data.Models.Users;
using DevCommuBot.Data.Models.Warnings;

using Discord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        #endregion USER
        #region WARN
        public async Task WarnUser(User victime,ulong authorId, string details, WarningReason reasontype = WarningReason.NO_REASON)
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
        #endregion
        #region Starboard
        public Task<StarboardEntry?> GetStarboardEntry(ulong messageId, EntryType entryType)
        {
            return entryType switch
            {
                EntryType.OriginalMessage => _dataContext.Starboards.FirstOrDefaultAsync(st => st.StarboardMessageId == messageId),
                EntryType.Message => _dataContext.Starboards.FirstOrDefaultAsync(st => st.MessageId == messageId),
                _ => null,
            };
        }
        #endregion
        #region FORUM
        public async Task CreateForum(ulong forumId, ForumTag tag)
        {
            await _dataContext.Forums.AddAsync(new Forum
            {
                ChannelId = forumId,
                ClosedTag = tag,
            });
            await _dataContext.SaveChangesAsync();
        }
        public async Task<Forum> GetForum(ulong forumId)
            => await _dataContext.Forums
            .Include(f => f.ClosedTag)
            .Include(f => f.Moderators)
            .FirstOrDefaultAsync(f => f.ChannelId == forumId);
        public async Task UpdateForum(ulong forumId, ICollection<ForumRules> rules)
        {
            var forum = await GetForum(forumId);
            if (forum == null)
            {
                _ = new ArgumentException("Forum does not exist");
                return;
            }
            forum.Rules = rules;
            _dataContext.Forums.Update(forum);
            await _dataContext.SaveChangesAsync();
        }
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
        #endregion
    }
    enum EntryType
    {
        OriginalMessage,
        Message,
    }
}