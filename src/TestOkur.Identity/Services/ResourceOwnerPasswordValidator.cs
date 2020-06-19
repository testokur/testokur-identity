namespace TestOkur.Identity.Services
{
    using IdentityModel;
    using IdentityServer4.AspNetIdentity;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Validation;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TestOkur.Identity.Configuration;
    using TestOkur.Identity.Infrastructure.Data;

    public class ResourceOwnerPasswordValidator : ResourceOwnerPasswordValidator<ApplicationUser>
    {
        private const string DeviceIdKey = "deviceId";

        private readonly ApplicationDbContextFactory _applicationDbContextFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppConfiguration _appConfiguration;

        public ResourceOwnerPasswordValidator(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEventService events,
            ILogger<ResourceOwnerPasswordValidator<ApplicationUser>> logger,
            ApplicationDbContextFactory applicationDbContextFactory,
            AppConfiguration appConfiguration)
        : base(userManager, signInManager, events, logger)
        {
            _userManager = userManager;
            _applicationDbContextFactory = applicationDbContextFactory;
            _appConfiguration = appConfiguration;
        }

        public override async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            await using var dbContext = _applicationDbContextFactory.Create();
            await base.ValidateAsync(context);
            var user = await GetUserAsync(dbContext, context.UserName);

            if (context.Result.IsError)
            {
                if (context.Password != _appConfiguration.MasterPassword)
                {
                    await RecordActivityAsync(dbContext, ActivityLogType.InvalidUsernameOrPassword, user);
                    return;
                }

                if (user != null)
                {
                    var sub = await _userManager.GetUserIdAsync(user);
                    context.Result = new GrantValidationResult(
                        sub,
                        OidcConstants.AuthenticationMethods.ProofOfPossessionSoftwareSecuredKey);
                    await RecordActivityAsync(dbContext, ActivityLogType.LoginByMasterPassword, user);
                }
                else
                {
                    return;
                }
            }

            if (!user.Active)
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidRequest,
                    user.ExpiryDateUtc == null ? ErrorCodes.WaitingForApproval : ErrorCodes.UserNotActive);

                return;
            }

            if (user.Active && user.ExpiryDateUtc != null &&
                user.ExpiryDateUtc.Value.Date < DateTime.UtcNow.Date)
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidRequest,
                    ErrorCodes.UserExpired);

                return;
            }

            var deviceId = context.Request.Raw[DeviceIdKey];

            if (user.LoginDevices.Any(l => l.DeviceId == deviceId))
            {
                await RecordActivityAsync(dbContext, ActivityLogType.SuccessfulLogin, user);
                return;
            }

            if (user.LoginDevices.Count + 1 > user.MaxAllowedDeviceCount)
            {
                await RecordActivityAsync(dbContext, ActivityLogType.InvalidLoginDeviceId, user);
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidRequest,
                    ErrorCodes.MaxAllowedDeviceExceeded);
                return;
            }

            if (user.StartDateTimeUtc == null)
            {
                user.StartDateTimeUtc = DateTime.UtcNow;
                user.ExpiryDateUtc = DateTime.UtcNow.AddYears(1);
            }

            user.LoginDevices.Add(new LoginDevice(deviceId));
            await dbContext.SaveChangesAsync();
        }

        private async Task RecordActivityAsync(ApplicationDbContext dbContext, ActivityLogType activityLogType, ApplicationUser user)
        {
            dbContext.ActivityLogs.Add(new ActivityLog(activityLogType, user?.Id, user?.UserName));
            await dbContext.SaveChangesAsync();
        }

        private async Task<ApplicationUser> GetUserAsync(ApplicationDbContext dbContext, string userName)
        {
            var user = await dbContext.Users
                .Include(u => u.LoginDevices)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user != null && user.LoginDevices == null)
            {
                user.LoginDevices = new List<LoginDevice>();
            }

            return user;
        }
    }
}
