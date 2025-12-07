// ChatBotService.cs – نسخة كاملة 2025
// تقرأ كل Models + Enums + بيانات الجداول وتبعتها للـ AI
// لو الـ AI ردّ بنتيجة → تُعرض، لو لا → fallback لـ AI خارجي

using Manisik.Enums;
using Manisik.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UmarahBooking.Core.Interfaces;
using UmarahBooking.Core.Models;

namespace UmarahBooking.Core.Services
{
    public class ChatBotService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ChatMemoryService _memory;
        private readonly IUnitOfWork _uow;
        private readonly string _modelName;
        private readonly string _systemPrompt;

        public ChatBotService(
            IHttpClientFactory httpFactory,
            IConfiguration config,
            ChatMemoryService memory,
            IUnitOfWork uow)
        {
            _httpFactory = httpFactory;
            _config = config;
            _memory = memory;
            _uow = uow;

            _modelName = config["ChatBot:ModelName"] ?? "gpt-4o-mini";
            _systemPrompt = config["ChatBot:Prompt"] ??
                "You are an assistant for UmarahBooking. When appropriate prefer factual DB answers for booking/payment/room/hotel queries.";
        }

        public async Task<string> AskAsync(string sessionId, string userMessage)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(userMessage)) throw new ArgumentNullException(nameof(userMessage));

            _memory.AddMessage(sessionId, new ChatMessage { Role = "user", Content = userMessage });

            var snapshot = BuildDataSnapshot();
            var dataPrompt = SnapshotToPrompt(snapshot);
            var aiReply = await AskAiWithDataAsync(userMessage, dataPrompt);

            if (string.IsNullOrWhiteSpace(aiReply))
                aiReply = await AskAiFallbackAsync(sessionId, userMessage, dataPrompt);

            _memory.AddMessage(sessionId, new ChatMessage { Role = "assistant", Content = aiReply });
            return aiReply;
        }

        #region Data Snapshot Builder
        private ChatBotDataSnapshot BuildDataSnapshot()
        {
            var snap = new ChatBotDataSnapshot();

            var enumTypes = typeof(BookingStatus).Assembly.GetTypes()
                                  .Where(t => t.IsEnum).ToList();
            foreach (var et in enumTypes)
                snap.EnumValues[et.Name] = Enum.GetNames(et);

            var repoProps = _uow.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(IBaseRepository<>));

            foreach (var prop in repoProps)
            {
                var entityType = prop.PropertyType.GetGenericArguments()[0];
                var repo = prop.GetValue(_uow);
                var getAllMi = repo?.GetType().GetMethod("GetAllAsQuerable");
                if (getAllMi == null) continue;

                var q = getAllMi.Invoke(repo, null) as IQueryable<object>;
                var rows = q?.Take(200).ToList() ?? new List<object>();
                snap.TableRows[entityType.Name] = rows;
            }

            return snap;
        }

        private string SnapshotToPrompt(ChatBotDataSnapshot snap)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Below is a complete dump of our database schema and actual data.");
            sb.AppendLine("Use it to answer the user question accurately.");
            sb.AppendLine();

            sb.AppendLine("=== ENUMS ===");
            foreach (var kv in snap.EnumValues)
                sb.AppendLine($"{kv.Key}: {string.Join(", ", kv.Value)}");

            sb.AppendLine();
            sb.AppendLine("=== TABLES (max 200 row each) ===");
            foreach (var kv in snap.TableRows)
            {
                sb.AppendLine();
                sb.AppendLine($"Table: {kv.Key}");
                if (!kv.Value.Any())
                {
                    sb.AppendLine("-- empty --");
                    continue;
                }
                var sample = kv.Value.Take(10).ToList();
                var cols = sample.First().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Take(5).Select(p => p.Name).ToArray();
                sb.AppendLine($"Columns: {string.Join(", ", cols)}");
                foreach (var row in sample)
                {
                    var vals = cols.Select(c => row.GetType().GetProperty(c)?.GetValue(row)?.ToString() ?? "null");
                    sb.AppendLine(string.Join(" | ", vals));
                }
                if (kv.Value.Count > 10) sb.AppendLine("...more rows");
            }

            return sb.ToString();
        }
        #endregion

        #region AI Call with Data
        private async Task<string> AskAiWithDataAsync(string userQuestion, string dataPrompt)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine(dataPrompt);
            prompt.AppendLine();
            prompt.AppendLine("User Question:");
            prompt.AppendLine(userQuestion);
            prompt.AppendLine();
            prompt.AppendLine("Reply ONLY with one of these two JSON formats:");
            prompt.AppendLine("1) { \"type\": \"table\", \"rows\": [ [\"col1\", \"col2\"], [\"val1\", \"val2\" ] ] }");
            prompt.AppendLine("2) { \"type\": \"text\", \"content\": \"answer here\" }");
            prompt.AppendLine("If data is not enough, return { \"type\": \"text\", \"content\": null }");

            var client = _httpFactory.CreateClient("ChatBot");
            
            // Fireworks AI / OpenAI compatible format
            var body = new
            {
                model = _modelName,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant for UmarahBooking. Use the provided database information to answer accurately. Your response must be in strict JSON format as requested." },
                    new { role = "user", content = prompt.ToString() }
                },
                max_tokens = 600,
                temperature = 0.1
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var res = await client.PostAsync("chat/completions", content);
                if (!res.IsSuccessStatusCode) return null;

                var raw = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(raw);
                var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                if (string.IsNullOrWhiteSpace(reply)) return null;

                return FormatAiDataReply(reply);
            }
            catch { return null; }
        }

        private string FormatAiDataReply(string replyJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(replyJson);
                var root = doc.RootElement;

                if (root.GetProperty("type").GetString() == "text")
                {
                    var txt = root.GetProperty("content").GetString();
                    return string.IsNullOrWhiteSpace(txt) || txt.Equals("null", StringComparison.OrdinalIgnoreCase)
                           ? null
                           : txt;
                }

                if (root.GetProperty("type").GetString() == "table")
                {
                    var rows = root.GetProperty("rows").EnumerateArray()
                                   .Select(r => r.EnumerateArray().Select(c => c.GetString()).ToArray()).ToList();
                    if (!rows.Any()) return null;

                    var sb = new StringBuilder();
                    foreach (var row in rows)
                        sb.AppendLine(string.Join(" | ", row));
                    return sb.ToString();
                }
            }
            catch { /* ignore */ }
            return null;
        }
        #endregion

        #region Fallback AI
        private async Task<string> AskAiFallbackAsync(string sessionId, string userMessage, string schema)
        {
            var system = _systemPrompt + "\nIf the question is about your database models, prefer DB answers; otherwise, answer conversationally.";
            var history = _memory.GetHistory(sessionId);
            var messages = new List<object>
            {
                new { role = "system", content = system },
                new { role = "system", content = "Brief schema (for context):\n" + Truncate(schema, 800) }
            };
            messages.AddRange(history.Skip(Math.Max(0, history.Count - 6))
                .Select(m => new { role = m.Role, content = m.Content }));
            messages.Add(new { role = "user", content = userMessage });

            var client = _httpFactory.CreateClient("ChatBot");
            var body = new
            {
                model = _modelName,
                messages = messages,
                max_tokens = 400
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var res = await client.PostAsync("chat/completions", content);

                if (!res.IsSuccessStatusCode)
                    return $"AI provider error: {res.StatusCode}";

                var responseText = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseText);
                var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                return reply ?? "عفواً، لم يتم تلقي رد من مزوّد الخدمة.";
            }
            catch (Exception ex)
            {
                return $"Failed to reach AI provider: {ex.Message}";
            }
        }

        private string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= max ? s : s.Substring(0, max - 3) + "...";
        }
        #endregion
    }

    #region Helpers
    internal class ChatBotDataSnapshot
    {
        public Dictionary<string, string[]> EnumValues { get; set; } = new();
        public Dictionary<string, List<object>> TableRows { get; set; } = new();
    }
    #endregion
}