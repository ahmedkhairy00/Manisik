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
    public class AuthTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public AuthTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_Login_Workflow()
        {
            var client = _factory.CreateClient();

            var email = $"testuser_{Guid.NewGuid()}@example.com";
            var register = new
            {
                Email = email,
                Password = "P@ssw0rd!",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "0123456789",
                Country = "EG"
            };

            var regResp = await client.PostAsJsonAsync("/api/Auth/Register", register);
            regResp.EnsureSuccessStatusCode();

            var regBody = await regResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.NotNull(regBody);
            Assert.True(regBody!.Success);
            Assert.NotNull(regBody.Data);
            Assert.False(string.IsNullOrEmpty(regBody.Data!.Token));

            // Login
            var login = new
            {
                Email = email,
                Password = "P@ssw0rd!"
            };

            var loginResp = await client.PostAsJsonAsync("/api/Auth/Login", login);
            loginResp.EnsureSuccessStatusCode();
            var loginBody = await loginResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.NotNull(loginBody);
            Assert.True(loginBody!.Success);
            Assert.NotNull(loginBody.Data);
            Assert.False(string.IsNullOrEmpty(loginBody.Data!.Token));
        }

        [Fact]
        public async Task AdminEndpoints_And_Me_Workflow()
        {
            var client = _factory.CreateClient();

            // Login as seeded admin
            var login = new
            {
                Email = "testadmin@example.com",
                Password = "P@ssw0rd1!"
            };

            var loginResp = await client.PostAsJsonAsync("/api/Auth/Login", login);
            loginResp.EnsureSuccessStatusCode();
            var loginBody = await loginResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.True(loginBody!.Success);
            var token = loginBody.Data!.Token;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Call Me
            var meResp = await client.GetAsync("/api/Auth/Me");
            meResp.EnsureSuccessStatusCode();
            var meBody = await meResp.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
            Assert.True(meBody!.Success);

            // Get Users (admin)
            var usersResp = await client.GetAsync("/api/Auth/Users");
            usersResp.EnsureSuccessStatusCode();
            var usersBody = await usersResp.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<UserDto>>>();
            Assert.True(usersBody!.Success);

            // Get Users by Role
            var usersByRoleResp = await client.GetAsync("/api/Auth/UsersByRole/User");
            usersByRoleResp.EnsureSuccessStatusCode();
            var usersByRoleBody = await usersByRoleResp.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<UserDto>>>();
            Assert.True(usersByRoleBody!.Success);

            // Create a new user to assign role
            var newUserEmail = $"roleuser_{Guid.NewGuid()}@example.com";
            var register = new
            {
                Email = newUserEmail,
                Password = "P@ssw0rd!",
                FirstName = "Role",
                LastName = "User",
                PhoneNumber = "0123456789",
                Country = "EG"
            };

            var regResp = await client.PostAsJsonAsync("/api/Auth/Register", register);
            regResp.EnsureSuccessStatusCode();
            var regBody2 = await regResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.True(regBody2!.Success);

            // Find created user id via Users endpoint
            var allUsers = (await client.GetFromJsonAsync<ApiResponse<IEnumerable<UserDto>>>("/api/Auth/Users")).Data!;
            var created = allUsers.First(u => u.Email == newUserEmail);

            // Assign role
            var assignResp = await client.PostAsJsonAsync("/api/Auth/AssignRole", new { UserId = created.Id, RoleName = "HotelManager" });
            assignResp.EnsureSuccessStatusCode();
            var assignBody = await assignResp.Content.ReadFromJsonAsync<ApiResponse<string>>();
            Assert.True(assignBody!.Success);

            // Remove role
            var removeResp = await client.PostAsJsonAsync("/api/Auth/RemoveRole", new { UserId = created.Id, RoleName = "HotelManager" });
            removeResp.EnsureSuccessStatusCode();
            var removeBody = await removeResp.Content.ReadFromJsonAsync<ApiResponse<string>>();
            Assert.True(removeBody!.Success);

            // Get MyBookings (admin has no bookings but endpoint should return 200 or empty)
            var myBookingsResp = await client.GetAsync("/api/Auth/MyBookings");
            myBookingsResp.EnsureSuccessStatusCode();
            var myBookingsBody = await myBookingsResp.Content.ReadFromJsonAsync<ApiResponse<UserWithBookingsDto>>();
            Assert.True(myBookingsBody!.Success);
        }
    }
}
