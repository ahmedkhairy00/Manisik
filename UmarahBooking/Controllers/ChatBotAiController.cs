using Microsoft.AspNetCore.Mvc;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Services;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotAiController : ControllerBase
    {
        private readonly ChatMemoryService _chatMemoryService;
        private readonly ChatBotService _chatBotService;

        public ChatBotAiController(ChatMemoryService chatMemoryService, ChatBotService chatBotService)
        {
            _chatMemoryService = chatMemoryService;
            _chatBotService = chatBotService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.SessionId) || string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest("SessionId and Message are required");

            try
            {
                var answer = await _chatBotService.AskAsync(request.SessionId, request.Message);
                return Ok(new { answer });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "AI service error", detail = ex.Message });
            }
        }

        [HttpPost("clear")]
        public IActionResult Clear([FromQuery] string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return BadRequest();
            _chatMemoryService.ClearSession(sessionId);
            return Ok();
        }
    }
}
