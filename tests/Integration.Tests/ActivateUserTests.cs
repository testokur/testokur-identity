namespace Integration.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Integration.Tests.Common;
    using Integration.Tests.WindowsClient;
    using TestOkur.Identity.Models;
    using Xunit;

    public class ActivateUserTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;

        public ActivateUserTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task When_UserIsActivatedForTheFirstTime_Then_ActionShouldReturnTrue(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            var result = await Utils.ActivateUserAsync(client, model.Email);
            result.Should().BeTrue();
        }

        [Theory]
        [TestOkurData]
        public async Task When_UserActivatedAfterDeactivation_Then_ActionShouldReturnFalse(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            var result = await Utils.ActivateUserAsync(client, model.Email);
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, model.Password, extra);
            tokenResponse.IsError.Should().BeFalse();
            await Utils.DeactivateUserAsync(client, model.Email);
            result = await Utils.ActivateUserAsync(client, model.Email);
            result.Should().BeFalse();
        }
    }
}
