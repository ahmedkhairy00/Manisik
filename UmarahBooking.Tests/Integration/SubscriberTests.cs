using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UmarahBooking.Core.DTO;
using Xunit;
using UmarahBooking.Tests.TestInfrastructure;

namespace UmarahBooking.Tests.Integration
{
    public class SubscriberTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public SubscriberTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<string> GetAdminTokenAsync(HttpClient client)
        {
            var login = new { Email = "testadmin@example.com", Password = "P@ssw0rd1!" };
            var loginResp = await client.PostAsJsonAsync("/api/Auth/Login", login);
            var loginBody = await loginResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            return loginBody!.Data!.Token!;
        }

        [Fact]
        public async Task Subscribe_WithValidEmail_SubscribesSuccessfully()
        {
            var client = _factory.CreateClient();

            var email = $"subscriber_{Guid.NewGuid()}@example.com";
            var dto = new SubscriberDto { Email = email };

            var response = await client.PostAsJsonAsync("/api/Subscriber/Subscribe", dto);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task Subscribe_WithDuplicateEmail_ReturnsError()
        {
            var client = _factory.CreateClient();

            var email = $"duplicate_{Guid.NewGuid()}@example.com";
            var dto = new SubscriberDto { Email = email };

            // First subscription
            var response1 = await client.PostAsJsonAsync("/api/Subscriber/Subscribe", dto);
            response1.EnsureSuccessStatusCode();

            // Second subscription (duplicate)
            var response2 = await client.PostAsJsonAsync("/api/Subscriber/Subscribe", dto);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response2.StatusCode);
        }

        [Fact]
        public async Task Subscribe_WithInvalidEmail_ReturnsError()
        {
            var client = _factory.CreateClient();

            var dto = new SubscriberDto { Email = "invalid-email" };

            var response = await client.PostAsJsonAsync("/api/Subscriber/Subscribe", dto);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task List_AsAdmin_ReturnsAllSubscribers()
        {
            var client = _factory.CreateClient();
            var token = await GetAdminTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/Subscriber/List");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<SubscriberDto>>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task List_AsAnonymous_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Subscriber/List");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
