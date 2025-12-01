using System.Runtime.Serialization;
namespace Manisik.Enums
{
    public enum DepartureAirport
    {
        [EnumMember(Value = "CairoInternational")] 
        CairoInternational,        // CAI//0
        [EnumMember(Value = "BorgElArabAlexandria")]
        BorgElArabAlexandria,      // HBE//1
        [EnumMember(Value = "SharmElSheikhInternational")]
        SharmElSheikhInternational,// SSH//2
        [EnumMember(Value = "HurghadaInternational")]
        HurghadaInternational,     // HRG//3
        [EnumMember(Value = "AssiutInternational")]
        AssiutInternational,       // ATZ//4
        [EnumMember(Value = "SohagInternational")]
        SohagInternational ,        // HMB//5
        [EnumMember(Value = "SafagaPort")]
        SafagaPort,//6
        [EnumMember(Value = "AlexandriaPort")]
        AlexandriaPort,
        [EnumMember(Value = "HurghadaPort")]
        HurghadaPort,





       [EnumMember(Value = "Jeddah")]
        Jeddah,
        [EnumMember(Value = "Madinah")]
        Madinah,
        [EnumMember(Value = "Taif")]
        Taif,
    }
}
