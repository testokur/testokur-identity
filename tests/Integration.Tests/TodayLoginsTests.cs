namespace Integration.Tests
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using IdentityModel.Client;
    using Integration.Tests.Common;
    using Integration.Tests.WindowsClient;
    using TestOkur.Identity.Models;
    using Xunit;

    public class TodayLoginsTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;

        public TodayLoginsTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task ShouldReturnUserEmails(IFixture fixture)
        {
            const string email = "test-admin-user@testokur.com";
            const string password = "1234";
            const string Path = "/api/v1/user-activities/today-logins";

            for (var i = 0; i < 5; i++)
            {
                var model = fixture.Create<CreateCustomerUserModel>();
                model.Email = fixture.Create<MailAddress>().Address;
                await LoginRandomUser(model);
            }

            var client = _webbApplicationFactory.CreateClient();
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, email, password, extra);
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.GetAsync(Path);
            var loggedInUserEmails = await response.Content.ReadAsAsync<IEnumerable<string>>();
            loggedInUserEmails.Should().NotBeEmpty();
        }

        private async Task LoginRandomUser(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            await Utils.ActivateUserAsync(client, model.Email);
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
        }
    }
}
