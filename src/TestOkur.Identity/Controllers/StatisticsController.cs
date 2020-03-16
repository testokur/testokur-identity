namespace TestOkur.Identity.Controllers
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Identity.Models;

    [Route("api/v1/stats")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public StatisticsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var today = DateTime.Today.ToUniversalTime();

            return Ok(new StatisticsModel()
            {
                ExpiredUsersToday = await GetExpiredUsersToday(today),
                NewUserActivatedCountToday = await _dbContext.ActivityLogs.CountAsync(a => a.Type == ActivityLogType.UserActivated &&
                                                                                      a.DateTimeUtc >= today),
                TotalIndividualLoginCountInDay = await _dbContext.ActivityLogs
                    .Where(a => a.Type == ActivityLogType.SuccessfulLogin && a.DateTimeUtc >= today)
                    .OrderByDescending(a => a.DateTimeUtc)
                    .Select(a => a.CreatedBy)
                    .Distinct()
                    .CountAsync(),
                SubscriptionExtendedCountToday = await _dbContext.ActivityLogs.CountAsync(a => a.Type == ActivityLogType.UserSubscriptionExtended &&
                                                                                               a.DateTimeUtc >= today),
                TotalActiveUserCount = await _dbContext.Users.CountAsync(u => u.Active && u.ExpiryDateUtc > today),
                TotalUserCount = await _dbContext.Users.CountAsync(),
            });
        }

        private async Task<string> GetExpiredUsersToday(DateTime today)
        {
            var users = await _dbContext.Users.Where(u => u.ExpiryDateUtc != null &&
                                                          u.ExpiryDateUtc.Value.Date == today.Date)
                .Select(u => u.Email)
                .ToListAsync();
            return string.Join(", ", users);
        }
    }
}
