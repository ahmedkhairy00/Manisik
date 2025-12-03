using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Manisik.Enums
{
    public enum flightDegree
    {
        [EnumMember(Value = "Economy")]
        Economy ,
        [EnumMember(Value = "Business")]
        Business ,
        [EnumMember(Value = "FirstClass")]
        FirstClass, 
    }
}
