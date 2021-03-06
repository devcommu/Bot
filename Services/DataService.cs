using System;
using System.Linq;
using System.Threading.Tasks;

using DevCommuBot.Data;
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
    }
}