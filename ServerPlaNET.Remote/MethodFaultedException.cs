using ServerPlaNET.Remote.Structs.Call;
using System;
using System.Linq;

namespace ServerPlaNET.Remote
{
    public class MethodFaultedException : Exception
    {
        public new string Message { get; private set; }

        public MethodFaultedException(MethodResponse response)
        {
            object faultCode = response.Fault.Value.Value.Value.Struct.Value.Members
                .Single(member => member.Name == "faultCode")
                .Value;

            string faultString = (string)response.Fault.Value.Value.Value.Struct.Value.Members
                .Single(member => member.Name == "faultString")
                .Value;

            Message = $"{faultCode} - {faultString}";
        }
    }
}
