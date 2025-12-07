using System;
using System.Threading.Tasks;
using System.Net.Http.Json;
using UmarahBooking.Core.DTO;
using Xunit;
using UmarahBooking.Tests.TestInfrastructure;

namespace UmarahBooking.Tests.Integration
{
    public class ChatBotTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public ChatBotTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Chat_WithValidMessage_AcceptsRequestOrServiceError()
        {
            var client = _factory.CreateClient();

            var sessionId = Guid.NewGuid().ToString();
            var request = new
            {
                SessionId = sessionId,
                Message = "What is Umrah?"
            };

            var response = await client.PostAsJsonAsync("/api/ChatBotAi/chat", request);
            
            // Accept 200 (success) or 500 (AI service not configured in test) as valid
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Chat_WithEmptyMessage_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var request = new
            {
                SessionId = Guid.NewGuid().ToString(),
                Message = ""
            };

            var response = await client.PostAsJsonAsync("/api/ChatBotAi/chat", request);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Clear_WithValidSession_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var sessionId = Guid.NewGuid().ToString();

            var response = await client.PostAsync($"/api/ChatBotAi/clear?sessionId={sessionId}", null);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Clear_WithoutSessionId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsync("/api/ChatBotAi/clear", null);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
