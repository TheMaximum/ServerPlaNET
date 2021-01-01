using System;
using System.Collections.Generic;
using System.Text;

namespace ServerPlaNET.Remote.Structs.Responses
{
    [Serializable]
    public struct QueryResponse
    {
        public UInt32 Length;
        public UInt32 Handle;
    }
}
