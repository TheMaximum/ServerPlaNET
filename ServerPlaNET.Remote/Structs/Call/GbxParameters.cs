using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerPlaNET.Remote.Structs.Call
{
    [Serializable]
    [XmlType("params")]
    public struct GbxParameters
    {
        [XmlElement("param")]
        public GbxParameter[] Parameters { get; set; }

        public GbxParameters(object[] parameters)
        {
            Parameters = new GbxParameter[parameters.Length];

            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                Parameters[parameterIndex] = new GbxParameter(parameters[parameterIndex]);
            }
        }
    }
}
