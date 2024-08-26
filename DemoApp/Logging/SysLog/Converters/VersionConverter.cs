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
    public class VersionConverter : PatternLayoutConverter
    {
        private string _version;
        protected override void Convert(TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
        {
            if (string.IsNullOrEmpty(_version))
                _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            writer.Write(_version);
        }
    }
    
}
