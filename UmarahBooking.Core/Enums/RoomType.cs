using System.Runtime.Serialization;

namespace UmarahBooking.Core.Enums
{
    public enum RoomType
    {
        [EnumMember(Value = "Single")]
        Single,
        [EnumMember(Value = "Double")]
        Double,
        [EnumMember(Value = "Triple")]
        Triple,
        [EnumMember(Value = "Quadruple")]
        Quadruple,
        [EnumMember(Value = "Suite")]
        Suite,
        [EnumMember(Value = "Family")]
        Family
        
    }
}

