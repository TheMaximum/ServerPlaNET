using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerPlaNET.Remote.Structs.Call
{
    [Serializable]
    public struct GbxFault
    {
        [XmlElement("value")]
        public ParameterValue? Value { get; set; }
    }
}
