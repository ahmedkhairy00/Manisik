using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using UmarahBooking.Core.Models;

namespace UmarahBooking.Core.Services
{
    public class ChatMemoryService
    {
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _store =
          new ConcurrentDictionary<string, List<ChatMessage>>();

        private readonly int _maxMessagesPerSession;

        public ChatMemoryService(IConfiguration cfg)
        {
            // keep last 5 messages 
            _maxMessagesPerSession = cfg.GetValue<int?>("Chat:MaxMessagesPerSession") ?? 5;
        }

        public List<ChatMessage> GetHistory(string sessionId)
        {
            return _store.GetOrAdd(sessionId, _ => new List<ChatMessage>());
        }

        public void AddMessage(string sessionId, ChatMessage message)
        {
            var list = _store.GetOrAdd(sessionId, _ => new List<ChatMessage>());
            list.Add(message);

            // prune older messages to keep size small
            while (list.Count > _maxMessagesPerSession)
                list.RemoveAt(0);
        }

        public void ClearSession(string sessionId)
        {
            _store.TryRemove(sessionId, out _);
        }
    }
}
