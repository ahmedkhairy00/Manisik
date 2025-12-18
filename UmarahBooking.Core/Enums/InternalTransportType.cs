using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace UmarahBooking.Core.Enums
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum InternalTransportType
    {

        [EnumMember(Value = "PublicBus")]
        PublicBus ,

        [EnumMember(Value = "Taxi")]
        Train ,

        [EnumMember(Value = "UberCareem")]
        UberCareem,

        [EnumMember(Value = "PrivateCar")]
        PrivateCar 


    }
}

