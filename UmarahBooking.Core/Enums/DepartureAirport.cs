using System.Runtime.Serialization;
namespace Manisik.Enums
{
    public enum DepartureAirport
    {
        [EnumMember(Value = "CairoInternational")] 
        CairoInternational,        // CAI
        [EnumMember(Value = "BorgElArabAlexandria")]
        BorgElArabAlexandria,      // HBE
        [EnumMember(Value = "SharmElSheikhInternational")]
        SharmElSheikhInternational,// SSH
        [EnumMember(Value = "HurghadaInternational")]
        HurghadaInternational,     // HRG
        [EnumMember(Value = "AssiutInternational")]
        AssiutInternational,       // ATZ
        [EnumMember(Value = "SohagInternational")]
        SohagInternational         // HMB
    }
}
