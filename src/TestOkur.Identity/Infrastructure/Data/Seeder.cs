namespace TestOkur.Identity.Infrastructure.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using TestOkur.Identity.Configuration;

    public class Seeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var semaphoreSlim = new SemaphoreSlim(1, 1);
            await semaphoreSlim.WaitAsync(1000);
            try
            {
                await SeedRolesAsync(serviceProvider.GetRequiredService<RoleManager<IdentityRole>>());
                await SeedAdminUsersAsync(
                    serviceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                    serviceProvider.GetRequiredService<AppConfiguration>().AdminUsers);
            }
            finally
            {
                semaphoreSlim.Release(1);
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await CreateRoleIfNotExists(roleManager, Roles.Admin);
            await CreateRoleIfNotExists(roleManager, Roles.Distributor);
            await CreateRoleIfNotExists(roleManager, Roles.Customer);
        }

        private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string role)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private static async Task SeedAdminUsersAsync(
            UserManager<ApplicationUser> userManager,
            IEnumerable<AdminUserInfo> adminUsers)
        {
            if (adminUsers == null)
            {
                return;
            }

            foreach (var user in adminUsers)
            {
                await CreateAdminUserIfNotExistsAsync(userManager, user);
            }
        }

        private static async Task CreateAdminUserIfNotExistsAsync(
            UserManager<ApplicationUser> userManager,
            AdminUserInfo userInfo)
        {
            var user = await userManager.FindByNameAsync(userInfo.Email);

            if (user == null)
            {
                user = new ApplicationUser(userInfo.Id, userInfo.Email)
                {
                    ActivationTimeUtc = DateTime.Now,
                    Active = true,
                    ExpiryDateUtc = DateTime.Now.AddYears(10),
                };
                var result = await userManager.CreateAsync(user, userInfo.Password);

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                var claims = new[]
                {
                        new Claim(JwtClaimTypes.Name, $"{userInfo.FirstName} {userInfo.LastName}"),
                        new Claim(JwtClaimTypes.GivenName, userInfo.FirstName),
                        new Claim(JwtClaimTypes.FamilyName, userInfo.LastName),
                        new Claim(JwtClaimTypes.Email, userInfo.Email),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                };
                result = await userManager.AddClaimsAsync(user, claims);
                await userManager.AddToRoleAsync(user, Roles.Admin);

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }
    }
}
