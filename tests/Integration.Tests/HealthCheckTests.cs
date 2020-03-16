namespace Integration.Tests
{
    using FluentAssertions;
    using Integration.Tests.Common;
    using System.Net;
    using System.Threading.Tasks;
    using Xunit;

    public class HealthCheckTests : IClassFixture<WebbApplicationFactory>
    {
        private readonly WebbApplicationFactory _webbApplicationFactory;

        public HealthCheckTests(WebbApplicationFactory webbApplicationFactory)
        {
            _webbApplicationFactory = webbApplicationFactory;
        }

        [Fact]
        public async Task HealthCheck_Status_Should_Be_Ok()
        {
            var client = _webbApplicationFactory.CreateClient();
            var response = await client.GetAsync("hc");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            response = await client.GetAsync("metrics-core");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
