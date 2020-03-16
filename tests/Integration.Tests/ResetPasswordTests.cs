namespace Integration.Tests
{
    using FluentAssertions;
    using Integration.Tests.Common;
    using Integration.Tests.Extensions;
    using Integration.Tests.WindowsClient;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using TestOkur.Identity.Models;
    using TestOkur.Serialization;
    using Xunit;

    public class ResetPasswordTests : IClassFixture<WebbApplicationFactory>
    {
        private const string Path = "/account/reset-password";

        private readonly WebbApplicationFactory _webbApplicationFactory;

        public ResetPasswordTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task ShouldResetPassword(string newPassword, CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            await Utils.ActivateUserAsync(client, model.Email);
            var response =
                await client.PostAsync($"/api/v1/users/generate-password-reset-token?email={model.Email}", null);

            var token = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
            token.Should().NotBeNullOrEmpty();
            response = await client.GetAsync($"{Path}?token={token}&email={model.Email}");
            var antiForgeryToken = await response.ExtractAntiForgeryToken();
            var formPostBodyData = new Dictionary<string, string>
                {
                    { "__RequestVerificationToken", antiForgeryToken },
                    { "Token", token },
                    { "Email", model.Email },
                    { "NewPassword", newPassword },
                    { "NewPasswordRepeat", newPassword },
                };
            var request = response.CreatePostRequestWithCookies(formPostBodyData);
            response = await client.SendAsync(request);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [Fact]
        public async Task WhenTokenAndEmailMissingFromTheUrl_Then_Server_ShouldReturnBadRequest()
        {
            var client = _webbApplicationFactory.CreateClient();
            var response = await client.GetAsync(Path);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task WhenUrlContainsValidTokenAndEmail_Then_Server_ShouldReturnSuccess()
        {
            var client = _webbApplicationFactory.Server.CreateClient();
            const string token = "token";
            const string email = "email";
            var response = await client.GetAsync($"{Path}?token={token}&email={email}");
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [Theory]
        [TestOkurData]
        public async Task AdminShouldBeAbleToResetUserPassword(CreateCustomerUserModel createCustomerUserModel)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, createCustomerUserModel);
            await Utils.ActivateUserAsync(client, createCustomerUserModel.Email);
            await Utils.LoginAdminAsync(client);

            var model = new ChangePasswordByAdminModel
            {
                NewPassword = "TEST1234!",
                SubjectId = createCustomerUserModel.Id,
            };
            (await client.PostAsync("api/v1/users/reset-user-password-by-admin", model.ToJsonContent()))
                .EnsureSuccessStatusCode();
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, createCustomerUserModel.Email, model.NewPassword, extra);
            tokenResponse.IsError.Should().BeFalse();
        }
    }
}
