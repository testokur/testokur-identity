namespace Integration.Tests
{
    using System.Threading.Tasks;
    using Integration.Tests.Common;
    using Integration.Tests.WindowsClient;
    using TestOkur.Identity.Models;
    using Xunit;

    public class DeleteUserTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;

        public DeleteUserTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Theory]
        [TestOkurData]
        public async Task ShouldDelete(CreateCustomerUserModel model)
        {
            var client = _webbApplicationFactory.CreateClient();
            await Utils.CreateCustomer(client, model);
            await Utils.LoginAdminAsync(client);
            var response = await client.DeleteAsync($"/api/v1/users/{model.Id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
