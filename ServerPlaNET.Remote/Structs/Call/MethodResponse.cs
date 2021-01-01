using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerPlaNET.Remote.Structs.Call
{
    [Serializable]
    [XmlType("methodResponse")]
    public struct MethodResponse
    {
        [XmlElement("params")]
        public GbxParameters GbxParameters { get; set; }

        [XmlIgnore]
        public GbxParameter[] Parameters { get { return GbxParameters.Parameters; } }

        [XmlElement("fault")]
        public GbxFault? Fault { get; set; }

        [XmlIgnore]
        public bool FaultSpecified { get { return Fault.HasValue; } }

        public MethodResponse(object[] parameters)
        {
            GbxParameters = new GbxParameters(parameters);
            Fault = null;
        }
    }
}
