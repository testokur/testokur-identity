namespace TestOkur.Identity.Services
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;

    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContextFactory _applicationDbContextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager, ApplicationDbContextFactory applicationDbContextFactory)
        {
            _userManager = userManager;
            _applicationDbContextFactory = applicationDbContextFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await using var dbContext = _applicationDbContextFactory.Create();
            var user = await dbContext.Users.FirstAsync(u => u.Id == context.Subject.GetSubjectId());
            context.IssuedClaims = context.Subject.Claims.ToList();

            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                context.IssuedClaims.Add(new Claim(JwtClaimTypes.Role, role));
            }

            context.IssuedClaims.Add(new Claim(
                CustomClaimTypes.MaxAllowedDeviceCount,
                user.MaxAllowedDeviceCount.ToString()));
            context.IssuedClaims.Add(new Claim(
                CustomClaimTypes.Active,
                user.Active.ToString()));
            context.IssuedClaims.Add(new Claim(
                CustomClaimTypes.CanScan,
                user.CanScan.ToString()));
            context.IssuedClaims.Add(new Claim(
                CustomClaimTypes.LicenseTypeId,
                user.LicenseTypeId.ToString()));
            context.IssuedClaims.Add(new Claim(
                CustomClaimTypes.MaxAllowedStudentCount,
                user.MaxAllowedStudentCount.ToString()));
            context.IssuedClaims.Add(new Claim(
                CustomClaimTypes.StartDateTime,
                user.StartDateTimeUtc.ToString()));
            context.IssuedClaims.Add(new Claim(
                CustomClaimTypes.ExpiryDate,
                user.ExpiryDateUtc.ToString()));
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;

            return Task.CompletedTask;
        }
    }
}
