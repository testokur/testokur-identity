namespace Integration.Tests.WindowsClient
{
    using FluentAssertions;
    using IdentityModel.Client;
    using Integration.Tests.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TestOkur.Identity.Models;
    using TestOkur.Serialization;
    using Xunit;

    public class ChangePasswordTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;

        public ChangePasswordTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task When_ValidPasswordProvided_Then_PasswordShouldBeUpdated(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            await Utils.ActivateUserAsync(client, model.Email);

            //Login
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeFalse();

            // Change Password
            var changePasswordRequest = new ChangeCustomerPasswordModel()
            {
                CurrentPassword = model.Password,
                NewPassword = "67890",
            };
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.PostAsync(
                "/api/v1/users/change-password",
                changePasswordRequest.ToJsonContent());
            response.EnsureSuccessStatusCode();

            //Login Again
            tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, changePasswordRequest.NewPassword, extra);
            tokenResponse.IsError.Should().BeFalse();
            tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
        }
    }
}
