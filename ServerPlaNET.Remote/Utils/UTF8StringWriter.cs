using System.IO;
using System.Text;

namespace ServerPlaNET.Remote.Utils
{
    public class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
