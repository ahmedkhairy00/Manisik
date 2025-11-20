using System.Runtime.Serialization;

namespace Manisik.Enums
{
    public enum RoomType
    {
        [EnumMember(Value = "Single")]
        Single,
        [EnumMember(Value = "Double")]
        Double,
        [EnumMember(Value = "Suite")]
        Suite,
        [EnumMember(Value = "Family")]
        Family
    }
}
