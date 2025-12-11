using System.Runtime.Serialization;
namespace UmarahBooking.Core.Enums
{
    public enum DepartureAirport
    {
        [EnumMember(Value = "Cairo")]
        Cairo,        // CAI//0
        [EnumMember(Value = "BorgElArabAlexandria")]
        BorgElArabAlexandria,      // HBE//1
        [EnumMember(Value = "SharmElSheikh")]
        SharmElSheikh,// SSH//2
        [EnumMember(Value = "Hurghada")]
        Hurghada,     // HRG//3
        [EnumMember(Value = "Assiut")]
        Assiut,       // ATZ//4
        [EnumMember(Value = "Sohag")]
        Sohag,        // HMB//5
        [EnumMember(Value = "SafagaPort")]
        SafagaPort,//6
        [EnumMember(Value = "AlexandriaPort")]
        AlexandriaPort,
        [EnumMember(Value = "HurghadaPort")]
        HurghadaPort,
    }
}

