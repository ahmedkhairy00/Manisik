using System.Runtime.Serialization;

namespace Manisik.Enums
{
    public enum InternationalTransportType
    {
        [EnumMember(Value = "Plane")]
        Plane,
        [EnumMember(Value = "Ship")]
        Ship
    }
}
