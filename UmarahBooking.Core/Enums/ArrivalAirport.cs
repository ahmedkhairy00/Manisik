using System.Runtime.Serialization;

namespace UmarahBooking.Core.Enums
{
    public enum ArrivalAirport
    {
        [EnumMember(Value = "Jeddah")]
        Jeddah,
        [EnumMember(Value = "Madinah")]
        Madinah,
        [EnumMember(Value = "Taif")]
        Taif
    }
}

