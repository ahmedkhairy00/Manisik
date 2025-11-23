using System.Runtime.Serialization;

namespace Manisik.Enums
{
    public enum InternalTransportType
    {
        [EnumMember(Value = "PrivateCar")]
        PrivateCar,
        [EnumMember(Value = "SharedBus")]
        SharedBus,
        [EnumMember(Value = "Taxi")]
        Taxi,
    }
}
