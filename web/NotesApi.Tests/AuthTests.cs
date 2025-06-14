using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using NotesApi;
using System.Net;

namespace NotesApi.Tests
{
    public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Login_Should_Return_JwtToken()
        {
            // Arrange
            var loginRequest = new
            {
                username = "test5",
                password = "123456"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var accessToken = doc.RootElement.GetProperty("accessToken").GetString();

            Assert.False(string.IsNullOrEmpty(accessToken));
        }

        [Fact]
        public async Task Access_Protected_Endpoint_With_Token_Should_Succeed()
        {
            // Step 1: 登入取得 token
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                username = "test5",
                password = "123456"
            });

            loginResponse.EnsureSuccessStatusCode();

            var tokenJson = await loginResponse.Content.ReadAsStringAsync();
            var accessToken = JsonDocument.Parse(tokenJson).RootElement.GetProperty("accessToken").GetString();

            // Step 2: 設定 token 並呼叫受保護的 API
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var protectedResponse = await _client.GetAsync("/api/notes");

            Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
        }
    }
}
