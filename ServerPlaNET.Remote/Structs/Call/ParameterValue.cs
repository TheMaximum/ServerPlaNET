using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace ServerPlaNET.Remote.Structs.Call
{
    [Serializable]
    public struct ParameterValue
    {
        [XmlElement("string")]
        public string String { get; set; }

        [XmlElement("i4")]
        public int? Int { get; set; }

        [XmlElement("double")]
        public double? Double { get; set; }

        [XmlIgnore]
        public bool? Boolean { get; set; }

        [XmlElement("struct")]
        public GbxStructParameters? Struct { get; set; }

        [XmlElement("array")]
        public ParameterValue[] Array { get; set; }

        [XmlElement("boolean")]
        [Browsable(false)]
        public int? BooleanSerialize
        {
            get { return (this.Boolean.HasValue) ? (this.Boolean.Value ? 1 : 0) : null; }
            set { this.Boolean = XmlConvert.ToBoolean(value.ToString()); }
        }

        [XmlIgnore]
        public bool StringSpecified { get { return String != null; } }

        [XmlIgnore]
        public bool IntSpecified { get { return Int.HasValue; } }

        [XmlIgnore]
        public bool DoubleSpecified { get { return Double.HasValue; } }

        [XmlIgnore]
        public bool BooleanSerializeSpecified { get { return Boolean.HasValue; } }

        [XmlIgnore]
        public bool StructSpecified { get { return Struct.HasValue; } }

        [XmlIgnore]
        public bool ArraySpecified { get { return Array != null; } }

        public ParameterValue(object value)
        {
            String = null;
            Int = null;
            Double = null;
            Boolean = null;
            Struct = null;
            Array = null;

            switch (value)
            {
                case string str:
                    String = str;
                    break;
                case int integer:
                    Int = integer;
                    break;
                case double doub:
                    Double = doub;
                    break;
                case bool boolean:
                    Boolean = boolean;
                    break;
            }
        }

        public object GetValue()
        {
            object value = null;

            if (StringSpecified)
            {
                value = String;
            }
            else if (IntSpecified)
            {
                value = Int;
            }
            else if (DoubleSpecified)
            {
                value = Double;
            }
            else if (BooleanSerializeSpecified)
            {
                value = Boolean;
            }
            else if (StructSpecified)
            {
                value = Struct.Value.Members;
            }
            else if (ArraySpecified)
            {
                value = Array;
            }

            return value;
        }
    }
}
