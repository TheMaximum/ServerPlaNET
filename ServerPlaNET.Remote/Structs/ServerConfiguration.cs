using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerPlaNET.Remote.Structs
{
    public struct ServerConfiguration
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string ApiVersion { get; set; }
        public bool EnableCallbacks { get; set; }
    }
}
