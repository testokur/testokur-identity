namespace TestOkur.Identity.Controllers
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;

    [Route("api/v1/user-activities")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserActivityController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public UserActivityController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserActivities([FromQuery, Required]string userId)
        {
            return Ok(await _dbContext.ActivityLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.DateTimeUtc)
                .AsNoTracking()
                .ToListAsync());
        }

        [HttpGet("today-logins")]
        public async Task<IActionResult> GetTodayLoginsAsync()
        {
            var today = DateTime.Today.ToUniversalTime();
            var userEmails = await _dbContext.ActivityLogs
                .Where(a => a.Type == ActivityLogType.SuccessfulLogin && a.DateTimeUtc >= today)
                .OrderByDescending(a => a.DateTimeUtc)
                .Select(a => a.CreatedBy)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();

            return Ok(userEmails);
        }
    }
}
