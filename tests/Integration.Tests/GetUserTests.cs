namespace Integration.Tests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IdentityModel.Client;
    using Integration.Tests.Common;
    using Integration.Tests.WindowsClient;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Identity.Models;
    using Xunit;

    public class GetUserTests : IClassFixture<WebbApplicationFactory>
    {
        private const string TokenEndpoint = "https://testokur/connect/token";
        private const string Path = "/api/v1/users";
        private const string PrivateClientSecret = "PrivateClientSecret";
        private const string Scope = "testokurapi";

        private readonly WebbApplicationFactory _webbApplicationFactory;

        public GetUserTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task NonAdminUsers_Should_NotBeAbleToFetchUsers(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.Server.CreateClient();
            await Utils.CreateCustomer(client, model);
            await Utils.ActivateUserAsync(client, model.Email);
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.GetAsync(Path);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ShouldReturnAllUsers()
        {
            var client = _webbApplicationFactory.Server.CreateClient();
            var users = await Utils.GetUsersAsync(client);
            users.Should().NotBeEmpty();
        }

        [Fact]
        public async Task PrivateClientShouldBeAbleToFetchUsers()
        {
            var client = _webbApplicationFactory.CreateClient();
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest()
                {
                    Address = TokenEndpoint,
                    ClientId = Clients.Private,
                    ClientSecret = PrivateClientSecret,
                    Scope = Scope,
                });
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.GetAsync(Path);
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsAsync<IEnumerable<ApplicationUser>>()).Should().NotBeEmpty();
        }
    }
}
