namespace Integration.Tests.WindowsClient
{
    using IdentityModel.Client;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Identity.Models;
    using TestOkur.Serialization;

    internal static class Utils
    {
        private const string TokenEndpoint = "https://testokur/connect/token";
        private const string WinClientSecret = "WinClientSecret";
        private const string PrivateClientSecret = "PrivateClientSecret";
        private const string Scope = "testokurapi";
        private const string AdminUserName = "test-admin-user@testokur.com";
        private const string AdminPassword = "1234";

        public static async Task LoginAdminAsync(HttpClient client)
        {
            var extra = new Dictionary<string, string>
            {
                { "deviceId", "test-device1" },
            };
            var tokenResponse = await RequestPasswordTokenAsync(client, AdminUserName, AdminPassword, extra);
            client.SetBearerToken(tokenResponse.AccessToken);
        }

        public static async Task SetPrivateClientBearerTokenAsync(HttpClient client)
        {
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest()
                {
                    Address = TokenEndpoint,
                    ClientId = Clients.Private,
                    ClientSecret = PrivateClientSecret,
                    Scope = Scope,
                });

            client.SetBearerToken(tokenResponse.AccessToken);
        }

        public static async Task DeactivateUserAsync(HttpClient client, string email)
        {
            await SetPrivateClientBearerTokenAsync(client);
            var response = await client.PostAsync($"/api/v1/users/deactivate?email={email}", null);
            response.EnsureSuccessStatusCode();
        }

        public static async Task<bool> ActivateUserAsync(HttpClient client, string email)
        {
            await SetPrivateClientBearerTokenAsync(client);
            var response = await client.PostAsync($"/api/v1/users/activate?email={email}", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<bool>();
        }

        public static async Task CreateCustomer(HttpClient client, CreateCustomerUserModel model)
        {
            await SetPrivateClientBearerTokenAsync(client);
            var response = await client.PostAsync("/api/v1/users/create", model.ToJsonContent());
            response.EnsureSuccessStatusCode();
        }

        public static Task<TokenResponse> RequestPasswordTokenAsync(
            HttpClient tokenClient,
            string email,
            string password,
            Dictionary<string, string> extra)
        {
            return tokenClient.RequestPasswordTokenAsync(
                new PasswordTokenRequest()
                {
                    Address = TokenEndpoint,
                    ClientId = Clients.Win,
                    ClientSecret = WinClientSecret,
                    Scope = Scope,
                    UserName = email,
                    Password = password,
                    Parameters = new Parameters(extra),
                });
        }

        public static async Task<IEnumerable<ApplicationUser>> GetUsersAsync(HttpClient client)
        {
            const string Path = "/api/v1/users";

            var extra = new Dictionary<string, string>
            {
                { "deviceId", "test-device1" },
            };
            var tokenResponse = await RequestPasswordTokenAsync(client, AdminUserName, AdminPassword, extra);
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.GetAsync(Path);
            return await response.Content.ReadAsAsync<IEnumerable<ApplicationUser>>();
        }
    }
}
