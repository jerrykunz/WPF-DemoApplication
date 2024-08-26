using log4net.Core;
using log4net.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog
{
    /// <summary>
    /// Writes exceptions to a local file on disk, and passes along the log file location for other appenders to use.
    /// </summary>
    public class LogExceptionToFileFilter : FilterSkeleton
    {
        public override FilterDecision Decide(LoggingEvent loggingEvent)
        {
            if (loggingEvent.ExceptionObject != null)
            {
                // Allow events with exceptions
                return FilterDecision.Accept;
            }
            else
            {
                // Deny events without exceptions
                return FilterDecision.Deny;
            }
        }
    }
}
