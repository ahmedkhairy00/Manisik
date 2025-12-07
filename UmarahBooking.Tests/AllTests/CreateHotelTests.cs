using System.Net.Http.Headers;
using System.Net.Http.Json;
using UmarahBooking.Core.DTO;
using UmarahBooking.Tests.TestInfrastructure;
using Xunit;

namespace UmarahBooking.Tests.Integration
{
    public class CreateHotelTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public CreateHotelTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<string> GetAdminTokenAsync(HttpClient client)
        {
            var login = new
            {
                Email = "testadmin@example.com",
                Password = "P@ssw0rd1!"
            };

            var response = await client.PostAsJsonAsync("/api/Auth/Login", login);
            response.EnsureSuccessStatusCode();
            
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            return body!.Data!.Token!;
        }

        [Fact]
        public async Task CreateHotel_AsAdmin_ReturnsCreated()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await GetAdminTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var hotelDto = new HotelDto
            {
                Name = "Test Hotel",
                City = "Makkah",
                Address = "123 Test St",
                StarRating = 5,
                DistanceToHaram = 0.5m,
                Description = "A test hotel",
                DescriptionAr = "فندق تجريبي"
            };

            // Act
            // We need to send multipart/form-data because the endpoint expects [FromForm] and IFormFile
            using var content = new MultipartFormDataContent();
            
            // Add simple properties
            content.Add(new StringContent(hotelDto.Name), nameof(HotelDto.Name));
            content.Add(new StringContent(hotelDto.City), nameof(HotelDto.City));
            content.Add(new StringContent(hotelDto.Address), nameof(HotelDto.Address));
            content.Add(new StringContent(hotelDto.StarRating.ToString()), nameof(HotelDto.StarRating));
            content.Add(new StringContent(hotelDto.DistanceToHaram.ToString()), nameof(HotelDto.DistanceToHaram));
            content.Add(new StringContent(hotelDto.Description), nameof(HotelDto.Description));
            content.Add(new StringContent(hotelDto.DescriptionAr), nameof(HotelDto.DescriptionAr));

            // Add dummy image
            var dummyImageContent = new ByteArrayContent(new byte[10]);
            dummyImageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            content.Add(dummyImageContent, "image", "test.jpg");

            var response = await client.PostAsync("/api/Hotel/CreateHotel", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<HotelDto>>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
            Assert.Equal("Test Hotel", body.Data!.Name);
            Assert.NotNull(body.Data.Id);
        }
    }
}
