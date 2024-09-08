using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DemoApp.Logging.SysLog.Converters
{
    public class IpAddressConverter : PatternLayoutConverter
    {
        private string _ipAddress;
        protected override void Convert(TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
        {
            if (string.IsNullOrEmpty(_ipAddress))
            {
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            _ipAddress = ip.ToString();
                        }
                    }
                }
                catch { }
            }

            writer.Write(_ipAddress);
        }
    }
    
}
