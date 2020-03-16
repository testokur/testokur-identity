namespace Integration.Tests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Net.Http.Headers;

    public static class HttpResponseMessageExtensions
    {
        public static async Task<string> ExtractAntiForgeryToken(this HttpResponseMessage response)
        {
            var responseAsString = await response.Content.ReadAsStringAsync();
            return await Task.FromResult(ExtractAntiForgeryToken(responseAsString));
        }

        public static HttpRequestMessage CreatePostRequestWithCookies(
            this HttpResponseMessage response,
            Dictionary<string, string> formPostBodyData)
        {
            var path = response.RequestMessage.RequestUri.PathAndQuery;
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new FormUrlEncodedContent(formPostBodyData.ToList()),
            };
            var cookies = response.ExtractCookiesFromResponse();

            cookies.Keys.ToList().ForEach(key =>
            {
                httpRequestMessage.Headers.Add("Cookie", new CookieHeaderValue(key, cookies[key]).ToString());
            });

            return httpRequestMessage;
        }

        private static string ExtractAntiForgeryToken(string htmlResponseText)
        {
            var match = Regex.Match(htmlResponseText, @"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>");
            return match.Success ? match.Groups[1].Captures[0].Value : null;
        }

        private static IDictionary<string, string> ExtractCookiesFromResponse(this HttpResponseMessage response)
        {
            var result = new Dictionary<string, string>();

            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                SetCookieHeaderValue.ParseList(values.ToList()).ToList().ForEach(cookie =>
                {
                    result.Add(cookie.Name.Value, cookie.Value.Value);
                });
            }

            return result;
        }
    }
}
