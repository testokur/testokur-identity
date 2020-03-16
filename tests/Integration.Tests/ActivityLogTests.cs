namespace Integration.Tests
{
    using FluentAssertions;
    using IdentityModel.Client;
    using Integration.Tests.Common;
    using Integration.Tests.WindowsClient;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Identity.Models;
    using TestOkur.Serialization;
    using Xunit;

    public class ActivityLogTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;

        public ActivityLogTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task AllUserActivitiesShouldBeLogged(
            CreateCustomerUserModel createCustomerUserModel,
            string newPassword,
            UpdateUserModel model)
        {
            createCustomerUserModel.MaxAllowedDeviceCount = 1;
            var client = _webbApplicationFactory.Server.CreateClient();

            //Create User
            await Utils.CreateCustomer(client, createCustomerUserModel);

            //Activate User
            await Utils.ActivateUserAsync(client, createCustomerUserModel.Email);

            //InvalidUserNamePassword
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device" },
                };
            await Utils.RequestPasswordTokenAsync(client, createCustomerUserModel.Email, "wrongpass", extra);

            //SuccessfulLogin
            await Utils.RequestPasswordTokenAsync(client, createCustomerUserModel.Email, createCustomerUserModel.Password, extra);

            //InvalidLoginDevice
            extra["deviceId"] = "new-device";
            await Utils.RequestPasswordTokenAsync(client, createCustomerUserModel.Email, createCustomerUserModel.Password, extra);

            //Change Password
            extra["deviceId"] = "test-device";
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, createCustomerUserModel.Email, createCustomerUserModel.Password, extra);
            tokenResponse.IsError.Should().BeFalse();

            var changePasswordRequest = new ChangeCustomerPasswordModel()
            {
                CurrentPassword = createCustomerUserModel.Password,
                NewPassword = newPassword,
            };
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.PostAsync(
                "/api/v1/users/change-password",
                changePasswordRequest.ToJsonContent());
            response.EnsureSuccessStatusCode();

            model.UserId = createCustomerUserModel.Id;
            await Utils.SetPrivateClientBearerTokenAsync(client);
            response = await client.PostAsync("/api/v1/users/update", model.ToJsonContent());
            response.EnsureSuccessStatusCode();

            //Deactivate User
            await Utils.DeactivateUserAsync(client, model.Email);
            var dbContextFactory = _webbApplicationFactory.Services.GetService(typeof(ApplicationDbContextFactory))
                as ApplicationDbContextFactory;

            //Assert
            await using var dbContext = dbContextFactory.Create();
            var logs = await dbContext.ActivityLogs
                .Where(a => a.UserId == createCustomerUserModel.Id.ToString())
                .ToListAsync();
            logs.Should().Contain(l => l.Type == ActivityLogType.UserCreated);
            logs.Should().Contain(l => l.Type == ActivityLogType.UserActivated);
            logs.Should().Contain(l => l.Type == ActivityLogType.InvalidUsernameOrPassword);
            logs.Should().Contain(l => l.Type == ActivityLogType.SuccessfulLogin);
            logs.Should().Contain(l => l.Type == ActivityLogType.InvalidLoginDeviceId);
            logs.Should().Contain(l => l.Type == ActivityLogType.PasswordChanged);
            logs.Should().Contain(l => l.Type == ActivityLogType.UserUpdated);
            logs.Should().Contain(l => l.Type == ActivityLogType.UserDeActivated);
        }
    }
}
