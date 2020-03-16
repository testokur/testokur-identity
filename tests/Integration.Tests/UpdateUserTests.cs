namespace Integration.Tests
{
    using FluentAssertions;
    using Integration.Tests.Common;
    using Integration.Tests.WindowsClient;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Models;
    using TestOkur.Serialization;
    using Xunit;
    using Xunit.Abstractions;

    public class UpdateUserTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;
        private readonly ITestOutputHelper _testOutputHelper;

        public UpdateUserTests(WebbApplicationFactory webbApplicationFactory, ITestOutputHelper testOutputHelper)
        {
            _webbApplicationFactory = webbApplicationFactory;
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [TestOkurData]
        public async Task ShouldUpdateUser(CreateCustomerUserModel createUserModel, UpdateUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();

            // Create User
            await Utils.CreateCustomer(client, createUserModel);

            // Activate User
            await Utils.ActivateUserAsync(client, createUserModel.Email);

            // Update User
            model.UserId = createUserModel.Id;
            await Utils.SetPrivateClientBearerTokenAsync(client);
            var response = await client.PostAsync("/api/v1/users/update", model.ToJsonContent());
            response.EnsureSuccessStatusCode();
            var extra = new Dictionary<string, string>
                {
                    { "deviceId", "test-device1" },
                };
            var tokenResponse = await Utils.RequestPasswordTokenAsync(client, model.Email, createUserModel.Password, extra);
            if (tokenResponse.IsError)
            {
                _testOutputHelper.WriteLine(tokenResponse.Error);
                _testOutputHelper.WriteLine(tokenResponse.ErrorDescription);
            }

            tokenResponse.IsError.Should().BeFalse();
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtTokenHandler.ReadToken(tokenResponse.AccessToken) as JwtSecurityToken;
            securityToken.Claims.Should().Contain(c =>
                c.Type == CustomClaimTypes.LicenseTypeId &&
                int.Parse(c.Value) == model.LicenseTypeId);
        }
    }
}
