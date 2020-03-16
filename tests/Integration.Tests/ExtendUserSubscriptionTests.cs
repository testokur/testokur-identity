namespace Integration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Integration.Tests.Common;
    using Integration.Tests.WindowsClient;
    using TestOkur.Identity.Models;
    using Xunit;

    public class ExtendUserSubscriptionTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;

        public ExtendUserSubscriptionTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task UserSubscriptionShouldBeExtended(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            var result = await Utils.ActivateUserAsync(client, model.Email);
            result.Should().BeTrue();
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeFalse();
            var expiryDateTime = (await Utils.GetUsersAsync(client)).First(u => u.Email == model.Email)
                .ExpiryDateUtc.Value;
            Math.Round(expiryDateTime.Subtract(DateTime.UtcNow).TotalDays).Should().Be(365);
            await client.PostAsync($"/api/v1/users/extend?id={model.Id}", null);
            expiryDateTime = (await Utils.GetUsersAsync(client)).First(u => u.Email == model.Email)
                .ExpiryDateUtc.Value;
            Math.Round(expiryDateTime.Subtract(DateTime.UtcNow).TotalDays).Should().Be(365 * 2);
        }
    }
}
