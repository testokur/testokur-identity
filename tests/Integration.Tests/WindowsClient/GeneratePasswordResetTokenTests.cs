namespace Integration.Tests.WindowsClient
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Integration.Tests.Common;
    using TestOkur.Identity;
    using TestOkur.Identity.Models;
    using Xunit;

    public class GeneratePasswordResetTokenTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;

        public GeneratePasswordResetTokenTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Fact]
        public async Task WhenUserDoesNotExist_ThenBadRequestShouldReturn()
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.SetPrivateClientBearerTokenAsync(client);
            var response = await client.PostAsync($"/api/v1/users/generate-password-reset-token?email=test{DateTime.Now.Ticks}@gmail.com", null);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).Contains(ErrorCodes.UserDoesNotExists);
        }

        [Theory]
        [TestOkurData]
        public async Task WhenValidEmailAddressPosted_ThenTokenShouldBeGenerated(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            var response =
                await client.PostAsync($"/api/v1/users/generate-password-reset-token?email={model.Email}", null);

            var token = await response.Content.ReadAsStringAsync();
            token.Should().NotBeNullOrEmpty();
        }
    }
}
