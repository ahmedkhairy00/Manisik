using UmarahBooking.Core.DTO;

namespace UmarahBooking.Core.Interfaces
{
    /// <summary>
    /// Service interface for generating PDF documents (Visa and Ticket)
    /// </summary>
    public interface IPdfGenerationService
    {
        /// <summary>
        /// Generates a Visa PDF document
        /// </summary>
        /// <param name="data">Visa data for the PDF</param>
        /// <returns>PDF file as byte array</returns>
        byte[] GenerateVisaPdf(VisaPdfData data);

        /// <summary>
        /// Generates a Ticket PDF document
        /// </summary>
        /// <param name="data">Ticket data for the PDF</param>
        /// <returns>PDF file as byte array</returns>
        byte[] GenerateTicketPdf(TicketPdfData data);
    }
}
