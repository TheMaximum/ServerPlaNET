using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerPlaNET.Remote.Structs.Call
{
    [Serializable]
    [XmlType("methodCall")]
    public struct MethodCall
    {
        [XmlElement("methodName")]
        public string MethodName { get; set; }

        [XmlElement("params")]
        public GbxParameters GbxParameters { get; set; }

        [XmlIgnore]
        public GbxParameter[] Parameters { get { return GbxParameters.Parameters; } }

        public MethodCall(string methodName, object[] parameters)
        {
            MethodName = methodName;
            GbxParameters = new GbxParameters(parameters);
        }
    }
}
