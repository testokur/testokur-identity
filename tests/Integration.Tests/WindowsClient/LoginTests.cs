namespace Integration.Tests.WindowsClient
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Integration.Tests.Common;
    using Microsoft.EntityFrameworkCore;
    using TestOkur.Identity;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Identity.Models;
    using Xunit;
    using JwtClaimTypes = IdentityModel.JwtClaimTypes;

    public class LoginTests : IClassFixture<WebbApplicationFactory>
    {
        private const string MasterPassword = "master-password";

        private readonly WebbApplicationFactory _webbApplicationFactory;

        public LoginTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task MasterPassword_Should_Login_Any_User(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            await Utils.ActivateUserAsync(client, model.Email);
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse =
                await Utils.RequestPasswordTokenAsync(client, model.Email, MasterPassword, extra);
            tokenResponse.IsError.Should().BeFalse();
        }

        [Fact]
        public async Task When_InvalidCredentialsPosted_Then_InvalidCredentials_Error_Should_Be_Returned()
        {
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device" },
                };
            var tokenClient = _webbApplicationFactory.CreateClient();
            var tokenResponse = await Utils.RequestPasswordTokenAsync(tokenClient, "random@email.com", "wrongpass", extra);
            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.Error.Should().Contain("invalid_grant");
        }

        [Theory]
        [TestOkurData]
        public async Task When_ValidCredentialsPosted_Then_ClaimsShouldBeReturned(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            await Utils.ActivateUserAsync(client, model.Email);
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeFalse();
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtTokenHandler.ReadToken(tokenResponse.AccessToken) as JwtSecurityToken;
            securityToken.Claims.Should().Contain(c =>
                c.Type == JwtClaimTypes.Role && c.Value == Roles.Customer);
            securityToken.Claims.Should().Contain(c => c.Type == JwtClaimTypes.Subject);
            securityToken.Claims.Should().Contain(c =>
                c.Type == CustomClaimTypes.MaxAllowedDeviceCount &&
                int.Parse(c.Value) == model.MaxAllowedDeviceCount);
            securityToken.Claims.Should().Contain(c =>
                c.Type == CustomClaimTypes.Active &&
                bool.Parse(c.Value));
            securityToken.Claims.Should().Contain(c =>
                c.Type == CustomClaimTypes.CanScan &&
                bool.Parse(c.Value) == model.CanScan);
            securityToken.Claims.Should().Contain(c =>
                c.Type == CustomClaimTypes.MaxAllowedStudentCount &&
                int.Parse(c.Value) == model.MaxAllowedStudentCount);
            securityToken.Claims.Should().Contain(c =>
                c.Type == CustomClaimTypes.LicenseTypeId &&
                int.Parse(c.Value) == model.LicenseTypeId);
            DateTime dateTime;
            securityToken.Claims.Should().Contain(c =>
                c.Type == CustomClaimTypes.StartDateTime &&
                DateTime.TryParse(c.Value, out dateTime) &&
                dateTime.Year == DateTime.UtcNow.Year);
            securityToken.Claims.Should().Contain(c =>
                c.Type == CustomClaimTypes.ExpiryDate &&
                DateTime.TryParse(c.Value, out dateTime) &&
                dateTime.Year == DateTime.UtcNow.Year + 1);
        }

        [Theory]
        [TestOkurData]
        public async Task When_ValidCredentialsPosted_And_LoginDeviceCountExceeded_Then_ErrorShouldReturn(CreateCustomerUserModel model)
        {
            model.MaxAllowedDeviceCount = 1;
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            await Utils.ActivateUserAsync(client, model.Email);

            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeFalse();

            extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device2" },
                };
            tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.ErrorDescription.Should().Be(ErrorCodes.MaxAllowedDeviceExceeded);
        }

        [Theory]
        [TestOkurData]
        public async Task When_UserWaitingForApproval_Then_UserCannotLogin(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.ErrorDescription.Should().Be(ErrorCodes.WaitingForApproval);
        }

        [Theory]
        [TestOkurData]
        public async Task When_UserHasExpired_Then_UserShouldNotBeAbleToLogin(CreateCustomerUserModel model)
        {
            var httpClient = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(httpClient, model);
            await Utils.ActivateUserAsync(httpClient, model.Email);
            var dbContextFactory = _webbApplicationFactory.Services
                    .GetService(typeof(ApplicationDbContextFactory))
                as ApplicationDbContextFactory;
            await using (var dbContext = dbContextFactory.Create())
            {
                var user = await dbContext.Users.FirstAsync(u => u.Id == model.Id.ToString());
                user.ExpiryDateUtc = DateTime.UtcNow.AddDays(-1);
                await dbContext.SaveChangesAsync();
            }

            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(httpClient, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.ErrorDescription.Should().Be(ErrorCodes.UserExpired);
        }

        [Theory]
        [TestOkurData]
        public async Task When_UserIsNotActive_Then_UserShouldNotBeAbleToLogin(CreateCustomerUserModel model)
        {
            var httpClient = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(httpClient, model);
            var dbContextFactory = _webbApplicationFactory.Services
                    .GetService(typeof(ApplicationDbContextFactory))
                as ApplicationDbContextFactory;
            await using (var dbContext = dbContextFactory.Create())
            {
                var user = await dbContext.Users.FirstAsync(u => u.Id == model.Id.ToString());
                user.ExpiryDateUtc = DateTime.UtcNow.AddDays(100);
                await dbContext.SaveChangesAsync();
            }

            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(httpClient, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.ErrorDescription.Should().Be(ErrorCodes.UserNotActive);
        }
    }
}
