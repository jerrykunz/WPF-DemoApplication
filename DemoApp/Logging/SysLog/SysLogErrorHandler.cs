using DemoApp.Id;
using log4net;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog
{
    public class SyslogErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public ErrorCode? ErrorCode { get; set; }

        public SyslogErrorEventArgs()
        {
            Message = string.Empty;
            Exception = null;
            ErrorCode = null;
        }
    }

    public class SyslogErrorHandler : ISysLogErrorHandler
    {
        private static readonly ILog syslogErrorLog = LogManager.GetLogger(Logs.SysLogError);

        public event EventHandler<SyslogErrorEventArgs> SysLogError;

        public void Error(string message)
        {
            syslogErrorLog.Error(message);
            SysLogError?.Invoke(this, new SyslogErrorEventArgs { Message = message });


            //Console.WriteLine($"Error: {message}");
        }

        public void Error(string message, Exception e)
        {
            syslogErrorLog.Error(message + ", Exception: " + e.Message);
            SysLogError?.Invoke(this, new SyslogErrorEventArgs { Message = message, Exception = e });

            //Console.WriteLine($"Error: {message}, Exception: {e.Message}");
        }

        public void Error(string message, Exception e, ErrorCode errorCode)
        {
            syslogErrorLog.Error(message + ", Exception: " + e.Message + ", ErrorCode: " + errorCode);
            SysLogError?.Invoke(this, new SyslogErrorEventArgs { Message = message, Exception = e, ErrorCode = errorCode });

            //Console.WriteLine($"Error: {message}, Exception: {e.Message}, ErrorCode: {errorCode}");
        }
    }
}
