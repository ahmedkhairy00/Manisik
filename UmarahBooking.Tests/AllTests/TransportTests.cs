using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UmarahBooking.Core.DTO;
using Xunit;
using UmarahBooking.Tests.TestInfrastructure;

namespace UmarahBooking.Tests.Integration
{
    public class TransportTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public TransportTests(TestWebApplicationFactory factory)
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

        private async Task<string> GetUserTokenAsync(HttpClient client)
        {
            var email = $"transportuser_{Guid.NewGuid()}@example.com";
            var register = new
            {
                Email = email,
                Password = "P@ssw0rd!",
                FirstName = "Transport",
                LastName = "User",
                PhoneNumber = "0123456789",
                Country = "EG"
            };

            var regResp = await client.PostAsJsonAsync("/api/Auth/Register", register);
            var regBody = await regResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            return regBody!.Data!.Token!;
        }

        // Ground Transport Tests
        [Fact]
        public async Task GetAllGroundTransports_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/GroundTransport/GetAllGroundTransports");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task SearchByType_WithValidType_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/GroundTransport/SearchByType?transportType=0"); // 0 = Bus
            
            // Accept both 200 (data found) and 404 (no data) as valid
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AdvancedSearch_WithFilters_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/GroundTransport/AdvancedSearch?take=10&skip=0");
            
            // Should always return 200 OK with empty list if no data (not 404)
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task GetMyPendingGroundBookings_AsUser_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var token = await GetUserTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/GroundTransportBooking/MyPendingGroundBookings");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        // International Transport Tests
        [Fact]
        public async Task GetAllTransports_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/InternationalTransport/GetAllTransports");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task SearchByRoute_WithValidRoute_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/InternationalTransport/SearchByRoute?departureAirport=CairoInternational&arrivalAirport=Jeddah");
            
            // Accept both 200 (data found) and 404 (no data) as valid
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMyPendingTransportBookings_AsUser_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var token = await GetUserTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/InternationalTransportBooking/MyPendingTransportBookings");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task CreateGroundTransport_AsAnonymous_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var transportDto = new { };
            var response = await client.PostAsJsonAsync("/api/GroundTransport/CreateGroundTransport", transportDto);
            
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateTransport_AsAnonymous_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var transportDto = new { };
            var response = await client.PostAsJsonAsync("/api/InternationalTransport/CreateTransport", transportDto);
            
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
