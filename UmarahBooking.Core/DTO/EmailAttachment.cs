namespace UmarahBooking.Core.DTO
{
    /// <summary>
    /// Represents an email attachment for sending files via email
    /// </summary>
    public class EmailAttachment
    {
        /// <summary>
        /// The file name with extension (e.g., "visa.pdf")
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The binary content of the file
        /// </summary>
        public byte[] Content { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// The MIME content type (e.g., "application/pdf")
        /// </summary>
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
