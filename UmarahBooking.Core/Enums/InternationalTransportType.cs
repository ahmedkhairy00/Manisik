using System.Runtime.Serialization;

namespace UmarahBooking.Core.Enums
{
    public enum InternationalTransportType
    {
        [EnumMember(Value = "Plane")]
        Plane,
        [EnumMember(Value = "Ship")]
        Ship
    }
}

