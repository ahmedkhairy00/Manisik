using System.Runtime.Serialization;

namespace Manisik.Enums
{
    public enum InternalTransportType
    {
<<<<<<< Updated upstream
        [EnumMember(Value = "PublicBus")]
        PublicBus ,

        [EnumMember(Value = "Train")]
        Train ,

        [EnumMember(Value = "UberCareem")]
        UberCareem,

        [EnumMember(Value = "PrivateCar")]
        PrivateCar 

=======
        [EnumMember(Value = "PrivateCar")]
        PrivateCar,
        [EnumMember(Value = "SharedBus")]
        SharedBus,
        [EnumMember(Value = "Taxi")]
        Taxi,
>>>>>>> Stashed changes
    }
}
