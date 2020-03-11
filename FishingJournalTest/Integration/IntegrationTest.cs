using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace FishJournalTest.Integration
{
    public abstract class IntegrationTest : IClassFixture<TestWebApplicationFactory<TestStartup>>
    {
        private SetCookieHeaderValue _antiForgeryCookie;
        private string _antiForgeryToken;

        private static readonly Regex AntiForgeryFormFieldRegex =
            new Regex(@"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>");

        private async Task<string> EnsureAntiForgeryToken(HttpClient client)
        {
            if (_antiForgeryToken != null) return _antiForgeryToken;

            var response = await client.GetAsync("/Account/Login");
            response.EnsureSuccessStatusCode();
            if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                _antiForgeryCookie = SetCookieHeaderValue.ParseList(values.ToList()).SingleOrDefault(c =>
                    c.Name.StartsWith(".AspNetCore.AntiForgery.", StringComparison.InvariantCultureIgnoreCase));
            }

            Assert.NotNull(_antiForgeryCookie);
            client.DefaultRequestHeaders.Add("Cookie",
                new CookieHeaderValue(_antiForgeryCookie.Name, _antiForgeryCookie.Value).ToString());

            var responseHtml = await response.Content.ReadAsStringAsync();
            var match = AntiForgeryFormFieldRegex.Match(responseHtml);
            _antiForgeryToken = match.Success ? match.Groups[1].Captures[0].Value : null;
            Assert.NotNull(_antiForgeryToken);

            return _antiForgeryToken;
        }

        private async Task<Dictionary<string, string>> EnsureAntiForgeryTokenForm(HttpClient client,
            Dictionary<string, string> formData = null)
        {
            if (formData == null) formData = new Dictionary<string, string>();

            formData.Add("__RequestVerificationToken", await EnsureAntiForgeryToken(client));
            return formData;
        }

        protected async Task PerformLogin(HttpClient client, string username, string password)
        {
            var formData = await EnsureAntiForgeryTokenForm(client, new Dictionary<string, string>
            {
                {"Email", username},
                {"Password", password}
            });
            var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(formData));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // The current pair of anti-forgery cookie-token is not valid anymore
            // Since the tokens are generated based on the authenticated user!
            // We need a new token after authentication (The cookie can stay the same)
            _antiForgeryToken = null;
        }
    }
}