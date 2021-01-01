using System;
using System.Xml.Serialization;

namespace ServerPlaNET.Remote.Structs.Call
{
    [Serializable]
    public struct GbxParameter
    {
        [XmlElement("name")]
        public string? Name { get; set; }

        [XmlElement("value")]
        public ParameterValue ParameterValue { get; set; }

        [XmlIgnore]
        public object Value { get { return ParameterValue.GetValue(); } }

        [XmlIgnore]
        public bool NameSpecified { get { return Name != null; } }

        public GbxParameter(object value)
        {
            Name = null;
            ParameterValue = new ParameterValue(value);
        }
    }
}
