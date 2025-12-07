using System.Runtime.Serialization;

namespace Manisik.Enums
{
    public enum AirlineCompany
    {
        [EnumMember(Value = "Saudia")]
        Saudia,
        [EnumMember(Value = "EgyptAir")]
        EgyptAir,
        [EnumMember(Value = "Flynas")]
        Flynas,
        [EnumMember(Value = "Flyadeal")]
        Flyadeal,
        [EnumMember(Value = "AirCairo")]
        AirCairo,
        [EnumMember(Value = "NileAir")]
        NileAir
    }
}
