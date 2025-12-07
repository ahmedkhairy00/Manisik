using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UmarahBooking.Core.DTO;
using Xunit;
using UmarahBooking.Tests.TestInfrastructure;

namespace UmarahBooking.Tests.Integration
{
    public class BookingTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public BookingTests(TestWebApplicationFactory factory)
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
            var email = $"bookinguser_{Guid.NewGuid()}@example.com";
            var register = new
            {
                Email = email,
                Password = "P@ssw0rd!",
                FirstName = "Booking",
                LastName = "User",
                PhoneNumber = "0123456789",
                Country = "EG"
            };

            var regResp = await client.PostAsJsonAsync("/api/Auth/Register", register);
            var regBody = await regResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            return regBody!.Data!.Token!;
        }

        [Fact]
        public async Task GetMyBookings_AsUser_ReturnsUserBookings()
        {
            var client = _factory.CreateClient();
            var token = await GetUserTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/Booking/MyBookings");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task GetAllBookings_AsAdmin_ReturnsAllBookings()
        {
            var client = _factory.CreateClient();
            var token = await GetAdminTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/Booking/AllBookings");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task SearchByStatus_AsAdmin_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();
            var token = await GetAdminTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/Booking/SearchByStatus?status=0"); // 0 = Pending
            
            // Accept both 200 (data found) and 404 (no data) as valid responses
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllBookings_AsAnonymous_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Booking/AllBookings");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
