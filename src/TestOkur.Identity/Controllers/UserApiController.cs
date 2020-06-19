namespace TestOkur.Identity.Controllers
{
    using IdentityServer4.Extensions;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Identity.Models;

    [Route("api/v1/users")]
    [Produces("application/json")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserApiController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpPost("extend")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ExtendAccountAsync([FromQuery, Required]string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            user.Extend();
            _dbContext.ActivityLogs.Add(new ActivityLog(ActivityLogType.UserSubscriptionExtended, user.Id, User.Identity.Name));
            var devices = (await _dbContext.Users
                .Include(u => u.LoginDevices)
                .FirstAsync(u => u.Id == id)).LoginDevices;
            _dbContext.RemoveRange(devices);
            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("update")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUserAsync(UpdateUserModel model)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null)
            {
                return BadRequest(ErrorCodes.UserDoesNotExists);
            }

            user.UpdateFromModel(model);

            _dbContext.ActivityLogs.Add(new ActivityLog(ActivityLogType.UserUpdated, user.Id, User.Identity.Name));
            await _dbContext.SaveChangesAsync();
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [HttpGet("pending")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetPendingUsers()
        {
            var pendingUsers = await _dbContext.Users
                .Where(u => u.ExpiryDateUtc == null && !u.Active)
                .AsNoTracking()
                .ToListAsync();
            return Ok(pendingUsers);
        }

        [HttpPost("create")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateCustomerUserAsync(CreateCustomerUserModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                return BadRequest(ErrorCodes.UserExists);
            }

            user = model.ToApplicationUser();
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var claims = new[]
            {
                new Claim(JwtClaimTypes.Email, model.Email),
                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
            };
            result = await _userManager.AddClaimsAsync(user, claims);
            await _userManager.AddToRoleAsync(user, Roles.Customer);

            if (result.Succeeded)
            {
                _dbContext.ActivityLogs.Add(new ActivityLog(ActivityLogType.UserCreated, user.Id, User.Identity.Name));
                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("generate-password-reset-token")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GeneratePasswordResetTokenAsync([FromQuery, Required]string email)
        {
            var user = await _userManager.FindByNameAsync(email);

            return user == null
                ? (IActionResult)BadRequest(ErrorCodes.UserDoesNotExists)
                : Ok(HttpUtility.UrlEncode(await _userManager.GeneratePasswordResetTokenAsync(user)));
        }

        [HttpGet]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUsersAsync()
        {
            return Ok(await _dbContext.Users.AsNoTracking().ToListAsync());
        }

        [HttpGet("{userName}/details")]
        [Authorize(AuthorizationPolicies.Admin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserAsync(string userName)
        {
            return Ok(await _dbContext.Users.FirstAsync(u => u.UserName == userName));
        }

        [HttpPost("deactivate")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeactivateUserAsync([FromQuery, Required]string email)
        {
            var user = await _dbContext.Users.FirstAsync(u => u.Email == email);
            user.Active = false;
            _dbContext.ActivityLogs.Add(new ActivityLog(ActivityLogType.UserDeActivated, user.Id, User.Identity.Name));
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("change-password")]
        [Authorize(AuthorizationPolicies.Customer, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeCustomerPasswordAsync(ChangeCustomerPasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(User.GetSubjectId());
            if (user.UserName == "demo@testokur.com")
            {
                return Ok();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                _dbContext.ActivityLogs.Add(new ActivityLog(ActivityLogType.PasswordChanged, user.Id, User.Identity.Name));
                await _dbContext.SaveChangesAsync();
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);

            return Ok();
        }

        [HttpPost("reset-user-password-by-admin")]
        [Authorize(AuthorizationPolicies.Admin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ResetPasswordByAdminAsync(ChangePasswordByAdminModel model)
        {
            var user = await _userManager.FindByIdAsync(model.SubjectId);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            _dbContext.ActivityLogs.Add(new ActivityLog(ActivityLogType.PasswordResetByAdmin, user.Id, User.Identity.Name));
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("activate")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ActivateUserAsync([FromQuery, Required]string email)
        {
            var user = await _dbContext.Users.FirstAsync(u => u.Email == email);

            if (user.Active)
            {
                return Ok();
            }

            user.Activate();
            _dbContext.ActivityLogs.Add(new ActivityLog(ActivityLogType.UserActivated, user.Id, User.Identity.Name));
            await _dbContext.SaveChangesAsync();

            return Ok(user.ExpiryDateUtc == null);
        }

        [HttpPut("{id}/{role}")]
        [Authorize(AuthorizationPolicies.Admin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddUserToRoleAsync(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.AddToRoleAsync(user, role);
            return Ok();
        }

        [HttpDelete("{id}/{role}")]
        [Authorize(AuthorizationPolicies.Admin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteUserFromRoleAsync(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.RemoveFromRoleAsync(user, role);
            return Ok();
        }

        [HttpGet("distributors")]
        [Authorize(AuthorizationPolicies.Private, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetDistributorsAsync()
        {
            return Ok(await _userManager.GetUsersInRoleAsync(Roles.Distributor));
        }
    }
}
