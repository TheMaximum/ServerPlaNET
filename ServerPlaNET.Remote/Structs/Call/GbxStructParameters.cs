using System;
using System.Xml.Serialization;

namespace ServerPlaNET.Remote.Structs.Call
{
    [Serializable]
    public struct GbxStructParameters
    {
        [XmlElement("member")]
        public GbxParameter[] Members { get; set; }

        public GbxStructParameters(object[] members)
        {
            Members = new GbxParameter[members.Length];

            for (int parameterIndex = 0; parameterIndex < members.Length; parameterIndex++)
            {
                Members[parameterIndex] = new GbxParameter(members[parameterIndex]);
            }
        }
    }
}
