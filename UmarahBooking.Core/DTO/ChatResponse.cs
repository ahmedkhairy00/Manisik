using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    internal class ChatResponse
    {
        public string Answer { get; set; }
        public bool FromCacheOrDB { get; set; } = false;
        public string Source { get; set; } // "DB", "AI", "Hybrid"

    }
}
