namespace UmarahBooking.Core.DTO
{
    public class ChatRequest
    {
        public string SessionId { get; set; }
        public string Message { get; set; }
        // Optional: force retrieval-only mode (only query DB, do not call AI)
        public bool RetrievalOnly { get; set; } = false;
    }
}
