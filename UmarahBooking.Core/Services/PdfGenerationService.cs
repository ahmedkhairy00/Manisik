using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Services
{
    /// <summary>
    /// Service for generating PDF documents (Visa and Ticket)
    /// </summary>
    public class PdfGenerationService : IPdfGenerationService
    {
        public PdfGenerationService()
        {
            // Set QuestPDF license type (Community is free for projects with less than $1M annual revenue)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Generates a Visa PDF document
        /// </summary>
        public byte[] GenerateVisaPdf(VisaPdfData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header
                    page.Header().Element(c => ComposeVisaHeader(c, data));

                    // Content
                    page.Content().Element(c => ComposeVisaContent(c, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Generates a Ticket PDF document
        /// </summary>
        public byte[] GenerateTicketPdf(TicketPdfData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header
                    page.Header().Element(c => ComposeTicketHeader(c, data));

                    // Content
                    page.Content().Element(c => ComposeTicketContent(c, data));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        #region Visa PDF Components

        private void ComposeVisaHeader(IContainer container, VisaPdfData data)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Kingdom of Saudi Arabia")
                            .FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Text("Ministry of Foreign Affairs")
                            .FontSize(12).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(100).AlignRight().Text("VISA")
                        .FontSize(24).Bold().FontColor(Colors.Green.Darken2);
                });

                column.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Green.Darken2);

                column.Item().AlignCenter().Text("UMRAH VISA")
                    .FontSize(18).Bold().FontColor(Colors.Green.Darken3);

                column.Item().PaddingBottom(15);
            });
        }

        private void ComposeVisaContent(IContainer container, VisaPdfData data)
        {
            container.Column(column =>
            {
                // Photo and Basic Info Section
                column.Item().Row(row =>
                {
                    // Photo Container - embed image using QuestPDF Image() method (equivalent to <img> tag)
                    row.ConstantItem(100).Height(120).Border(1).BorderColor(Colors.Grey.Medium).Background(Colors.White).Element(photoContainer =>
                    {
                        if (data.PhotoBytes != null && data.PhotoBytes.Length > 0)
                        {
                            // ✅ This is the image tag - embeds the photo bytes as an actual image in the PDF
                            photoContainer.Image(data.PhotoBytes, ImageScaling.FitArea);
                        }
                        else
                        {
                            // Placeholder when no photo available
                            photoContainer.AlignCenter().AlignMiddle()
                                .Text("PHOTO").FontSize(12).FontColor(Colors.Grey.Medium);
                        }
                    });

                    row.RelativeItem().PaddingLeft(20).Column(col =>
                    {
                        col.Item().Text($"Name: {data.FullName}").FontSize(12).Bold();
                        col.Item().Text($"Passport No: {data.PassportNumber}").FontSize(11);
                        col.Item().Text($"Nationality: {data.Nationality}").FontSize(11);
                        col.Item().Text($"Date of Birth: {data.DateOfBirth:dd/MM/yyyy}").FontSize(11);
                    });
                });

                column.Item().PaddingVertical(15);

                // Visa Details Section
                column.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(visaDetails =>
                {
                    visaDetails.Item().Text("VISA DETAILS").FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                    visaDetails.Item().PaddingTop(10);

                    visaDetails.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Visa Type:").Bold();
                            col.Item().Text(data.VisaType);
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Valid Until:").Bold();
                            col.Item().Text(data.VisaExpiryDate.ToString("dd/MM/yyyy"));
                        });
                    });

                    visaDetails.Item().PaddingTop(10);

                    visaDetails.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Duration of Stay:").Bold();
                            col.Item().Text($"{data.StayDuration} days");
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Number of Entries:").Bold();
                            col.Item().Text(data.EntryCount == 1 ? "Single" : $"{data.EntryCount} entries");
                        });
                    });

                    visaDetails.Item().PaddingTop(10);

                    visaDetails.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Issuing Authority:").Bold();
                            col.Item().Text(data.IssuingAuthority);
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Booking Reference:").Bold();
                            col.Item().Text(data.BookingNumber);
                        });
                    });

                    visaDetails.Item().PaddingTop(10);

                    // Travel Dates
                    visaDetails.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Travel Start Date:").Bold();
                            col.Item().Text(data.TravelStartDate.ToString("dd/MM/yyyy"));
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Return Date:").Bold();
                            col.Item().Text(data.TravelEndDate.ToString("dd/MM/yyyy"));
                        });
                    });
                });

                column.Item().PaddingTop(20);

                // Important Notice
                column.Item().Border(1).BorderColor(Colors.Orange.Medium).Padding(10).Column(notice =>
                {
                    notice.Item().Text("IMPORTANT NOTICE").Bold().FontColor(Colors.Orange.Darken2);
                    notice.Item().PaddingTop(5);
                    notice.Item().Text("This visa is valid only for Umrah purposes. The holder must comply with all Saudi Arabian laws and regulations during their stay.");
                });
            });
        }

        #endregion

        #region Ticket PDF Components

        private void ComposeTicketHeader(IContainer container, TicketPdfData data)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("MANISIK").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Umrah Booking Platform").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(150).AlignRight().Text("E-TICKET")
                        .FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                });

                column.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                column.Item().PaddingBottom(10);
            });
        }

        private void ComposeTicketContent(IContainer container, TicketPdfData data)
        {
            container.Column(column =>
            {
                // Flight Info Header
                column.Item().Background(Colors.Blue.Darken2).Padding(15).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(data.DepartureAirport).FontSize(24).Bold().FontColor(Colors.White);
                        col.Item().Text("Departure").FontSize(10).FontColor(Colors.White);
                    });

                    row.ConstantItem(80).AlignCenter().AlignMiddle()
                        .Text("✈️").FontSize(30);

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text(data.ArrivalAirport).FontSize(24).Bold().FontColor(Colors.White);
                        col.Item().Text("Arrival").FontSize(10).FontColor(Colors.White);
                    });
                });

                column.Item().PaddingVertical(15);

                // Passenger Details
                column.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(passenger =>
                {
                    passenger.Item().Text("PASSENGER DETAILS").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                    passenger.Item().PaddingTop(10);

                    passenger.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Passenger Name:").Bold();
                            col.Item().Text(data.FullName);
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Passport Number:").Bold();
                            col.Item().Text(data.PassportNumber);
                        });
                    });

                    passenger.Item().PaddingTop(10);

                    passenger.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Nationality:").Bold();
                            col.Item().Text(data.Nationality);
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Date of Birth:").Bold();
                            col.Item().Text(data.DateOfBirth.ToString("dd/MM/yyyy"));
                        });
                    });
                });

                column.Item().PaddingVertical(10);

                // Flight Details
                column.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(flight =>
                {
                    flight.Item().Text("FLIGHT DETAILS").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                    flight.Item().PaddingTop(10);

                    flight.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Flight Number:").Bold();
                            col.Item().Text(data.FlightNumber);
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Airline:").Bold();
                            col.Item().Text(data.CarrierName);
                        });
                    });

                    flight.Item().PaddingTop(10);

                    flight.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Date & Time:").Bold();
                            col.Item().Text(data.DepartureDate.ToString("dd MMM yyyy, HH:mm"));
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Seat:").Bold();
                            col.Item().Text(string.IsNullOrEmpty(data.SeatNumber) ? "To be assigned" : data.SeatNumber);
                        });
                    });

                    flight.Item().PaddingTop(10);

                    flight.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Booking Reference (PNR):").Bold();
                            col.Item().Text(data.BookingNumber).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                        });
                    });
                });

                column.Item().PaddingVertical(10);

                // Return Flight Details (if available)
                if (data.ReturnDate.HasValue)
                {
                    column.Item().Background(Colors.Green.Lighten4).Padding(15).Column(returnFlight =>
                    {
                        returnFlight.Item().Text("RETURN FLIGHT").FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                        returnFlight.Item().PaddingTop(10);

                        returnFlight.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Return Date:").Bold();
                                col.Item().Text(data.ReturnDate.Value.ToString("dd MMM yyyy, HH:mm"));
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Flight Number:").Bold();
                                col.Item().Text(string.IsNullOrEmpty(data.ReturnFlightNumber) ? "TBA" : data.ReturnFlightNumber);
                            });
                        });

                        returnFlight.Item().PaddingTop(10);

                        returnFlight.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Route:").Bold();
                                col.Item().Text($"{data.ArrivalAirport} → {data.DepartureAirport}");
                            });
                        });
                    });

                    column.Item().PaddingVertical(10);
                }

                column.Item().PaddingTop(15);

                // Important Notice
                column.Item().Border(1).BorderColor(Colors.Orange.Medium).Padding(10).Column(notice =>
                {
                    notice.Item().Text("IMPORTANT").Bold().FontColor(Colors.Orange.Darken2);
                    notice.Item().PaddingTop(5);
                    notice.Item().Text("Please arrive at the airport at least 3 hours before departure. Carry a valid passport and this e-ticket confirmation.");
                });
            });
        }

        #endregion

        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                    row.RelativeItem().AlignRight().Text("Manisik | www.manisik.com")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        }
    }
}
