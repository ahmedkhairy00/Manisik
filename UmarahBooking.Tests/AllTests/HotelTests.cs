using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UmarahBooking.Core.DTO;
using Xunit;
using UmarahBooking.Tests.TestInfrastructure;

namespace UmarahBooking.Tests.Integration
{
    public class HotelTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public HotelTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<string> GetUserTokenAsync(HttpClient client)
        {
            var email = $"hoteluser_{Guid.NewGuid()}@example.com";
            var register = new
            {
                Email = email,
                Password = "P@ssw0rd!",
                FirstName = "Hotel",
                LastName = "User",
                PhoneNumber = "0123456789",
                Country = "EG"
            };

            var regResp = await client.PostAsJsonAsync("/api/Auth/Register", register);
            var regBody = await regResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            return regBody!.Data!.Token!;
        }

        [Fact]
        public async Task GetAllFiltered_WithCityFilter_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Hotel/GetAllFiltered?city=Makkah");
            
            // Accept both 200 (hotels found) and 404 (no hotels) as valid
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllFiltered_WithoutFilters_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Hotel/GetAllFiltered");
            
            // Accept both 200 (hotels found) and 404 (no hotels) as valid
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetHotelById_WithInvalidId_ReturnsNotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Hotel/GetHotelById/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMyPendingHotelBookings_AsUser_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var token = await GetUserTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/HotelBooking/MyPendingHotelBookings");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
        }

        [Fact]
        public async Task GetMyPendingHotelBookings_AsAnonymous_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/HotelBooking/MyPendingHotelBookings");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
