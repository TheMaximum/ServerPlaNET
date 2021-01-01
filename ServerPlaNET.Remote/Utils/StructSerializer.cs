using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ServerPlaNET.Remote.Utils
{
    public static class StructSerializer
    {
        public static T Deserialize<T>(byte[] bytes)
            where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return structure;
        }
    }
}
