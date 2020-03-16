namespace Integration.Tests
{
    using FluentAssertions;
    using IdentityModel.Client;
    using Integration.Tests.Common;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure;
    using Xunit;

    public class ClientCredentialTests : IClassFixture<WebbApplicationFactory>
    {
        private const string TokenEndpoint = "https://testokur/connect/token";
        private const string PublicClientSecret = "PublicClientSecret";

        private readonly WebbApplicationFactory _webbApplicationFactory;

        public ClientCredentialTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Fact]
        public async Task When_ValidCredentialsProvided_Token_ShouldBeCreated()
        {
            var tokenClient = _webbApplicationFactory.CreateClient();
            var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest()
                {
                    Address = TokenEndpoint,
                    ClientId = Clients.Public,
                    ClientSecret = PublicClientSecret,
                    Scope = "testokurapi",
                });

            tokenResponse.IsError.Should().BeFalse();
            tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
        }
    }
}
