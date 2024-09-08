using DemoApp.Helpers;
using log4net.Core;
using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DemoApp.Logging.SysLog.Converters
{
    public class HostnameConverter : PatternLayoutConverter
    {
        public static string HostName = IPGlobalProperties.GetIPGlobalProperties().HostName;

        //public static string DomainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;

        internal static string GetLocalhostFqdn()
        {
            //return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", HostName, DomainName);
            return string.Format(CultureInfo.InvariantCulture, "{0}", HostName);
        }

        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write(PrintableAsciiSanitizer.Sanitize(GetLocalhostFqdn().ToUpperInvariant(), 255));
        }
    }
    
    
}
