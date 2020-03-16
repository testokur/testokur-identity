namespace Integration.Tests
{
    using FluentAssertions;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Integration.Tests.Common;
    using Integration.Tests.Extensions;
    using Xunit;

    public class LoginPageTests : IClassFixture<WebbApplicationFactory>
    {
        private const string Path = "/account/login";

        private readonly WebbApplicationFactory _webbApplicationFactory;

        public LoginPageTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [InlineData("test-admin-user@testokur.com", "1234")]
        public async Task LogsInSeededUser_When_ValidUsernameAndPasswordProvided(
            string username,
            string password)
        {
            var client = _webbApplicationFactory.CreateClient();
            var response = await client.GetAsync(Path);
            response.EnsureSuccessStatusCode();

            var antiForgeryToken = await response.ExtractAntiForgeryToken();
            var formPostBodyData = new Dictionary<string, string>
                {
                    { "__RequestVerificationToken", antiForgeryToken },
                    { "Username", username },
                    { "Password", password },
                    { "button", "login" },
                    { "RememberLogin", "false" },
                    { "ReturnUrl", "http://localhost/signin-oidc" },
                };
            var request = response.CreatePostRequestWithCookies(formPostBodyData);

            response = await client.SendAsync(request);
            response.RequestMessage.RequestUri.Should().Be("http://localhost/signin-oidc");
        }
    }
}
