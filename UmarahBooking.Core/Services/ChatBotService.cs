using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using UmarahBooking.Core.Models;


namespace UmarahBooking.Core.Services
{
    public class ChatBotService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ChatMemoryService _memory;
        private readonly string _modelName;
        private readonly string _systemPrompt;
        private readonly int _maxMessagesToSend;

        public ChatBotService(IHttpClientFactory httpFactory, IConfiguration config, ChatMemoryService memory)
        {
            _httpFactory = httpFactory;
            _config = config;
            _memory = memory;

            _modelName = config["ChatBot:ModelName"];
            _systemPrompt = config["ChatBot:Prompt"];
            _maxMessagesToSend = config.GetValue<int?>("ChatBot:MaxMessagesToSend") ?? 5;

        }

        public async Task<string> AskAsync(string sessionId, string userMessage)
        {
            // Add user message to history
            _memory.AddMessage(sessionId, new ChatMessage { Role = "user", Content = userMessage });

            // Build messages payload from history (take last N)
            var history = _memory.GetHistory(sessionId);
            var messagesToSend = new List<object>
            {
                new { role = "system", content = _systemPrompt }
            };

            // take the last _maxMessagesToSend messages from history
            var last = history.Skip(Math.Max(0, history.Count - _maxMessagesToSend)).ToList();
            messagesToSend.AddRange(last.Select(m => new { role = m.Role, content = m.Content }));

            // build body
            var body = new
            {
                model = _modelName,
                messages = messagesToSend
            };

            var client = _httpFactory.CreateClient("ChatBot");
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync("chat/completions", content);

            var responseText = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                // log / throw meaningful error
                throw new Exception($"Chat Bot API error: {res.StatusCode}: {responseText}");
            }

            using var doc = JsonDocument.Parse(responseText);
            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // save assistant reply to memory
            _memory.AddMessage(sessionId, new ChatMessage { Role = "assistant", Content = reply });

            return reply;
        }
    }
}
