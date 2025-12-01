using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.Services
{
    public class InternationalTransportService
    {
        public TimeSpan calculateDuration(DateTime derpatureDate , DateTime arrivalDate)
        {
            return arrivalDate - derpatureDate;
        }
        //var duration = calculateDuration(, arrDate).ToString(@"hh\:mm");

    }
}
