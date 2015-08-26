using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Foundation.Diagnostics;
using System.Diagnostics.Tracing;

namespace PhotoFilter
{
    class MyLoggingChannel
    {
        static LoggingChannel _loggingChannel = new LoggingChannel("MyLoggingChannel");
        public static void Trace(string message)
        {
            _loggingChannel.LogMessage(message, LoggingLevel.Information);
        }
    }

    class PerfTipsEventSource : EventSource
    {
        public void StartMeasure()
        {
            WriteEvent(1);
        }
        public void StopMeasure()
        {
            WriteEvent(2);
        }
        public static PerfTipsEventSource Log = new PerfTipsEventSource();
    }
}
