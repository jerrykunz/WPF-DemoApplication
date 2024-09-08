using DemoApp.Helpers;
using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DemoApp.Logging.SysLog.Converters
{
    public class AppNameConverter : PatternLayoutConverter
    {
        private string _appName;
        protected override void Convert(TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
        {
            if (string.IsNullOrEmpty(_appName))
                _appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            writer.Write(PrintableAsciiSanitizer.Sanitize(_appName, 48));
        }
    }
    
}
