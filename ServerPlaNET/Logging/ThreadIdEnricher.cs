using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerPlaNET.Logging
{
    class ThreadIdEnricher : ILogEventEnricher
    {
        public static int ProcessId
        {
            get
            {
                if (processId == null)
                {
                    using (Process process = Process.GetCurrentProcess())
                    {
                        processId = process.Id;
                    }
                }
                return processId.Value;
            }
        }
        private static int? processId;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ThreadId", string.Format("{0}/{1}", ProcessId, Thread.CurrentThread.ManagedThreadId)));
        }
    }
}
