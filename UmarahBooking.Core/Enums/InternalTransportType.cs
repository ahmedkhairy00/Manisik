using System.Runtime.Serialization;

namespace Manisik.Enums
{
    public enum InternalTransportType
    {

        [EnumMember(Value = "PublicBus")]
        PublicBus ,

        [EnumMember(Value = "Train")]
        Train ,

        [EnumMember(Value = "UberCareem")]
        UberCareem,

        [EnumMember(Value = "PrivateCar")]
        PrivateCar 


    }
}
