using Manisik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Core.Interfaces
{
    public interface IInternationalTransportBookingService
    {
      Task<BookingInternationalTransport> BookInternationalTransportAsync(int userId, TransportBookingDto dto);
    }
}
