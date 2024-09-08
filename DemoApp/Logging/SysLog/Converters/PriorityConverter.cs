using log4net.Core;
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
    public class PriorityConverter : PatternLayoutConverter
    {
        public static string ConvertLevelToPriority(LoggingEvent loggingEvent)
        {
            int facility = 16; // local0
            if (loggingEvent.Properties["log4net:FacilityCode"] != null && !string.IsNullOrEmpty(loggingEvent.Properties["log4net:FacilityCode"].ToString()))
                if (!int.TryParse(loggingEvent.Properties["log4net:FacilityCode"].ToString(), out facility))
                    facility = 16;

            int gravity = 7; // debugging;
            if (loggingEvent.Level >= Level.Emergency)
                gravity = 0;
            else if (loggingEvent.Level >= Level.Fatal)
                gravity = 2;
            else if (loggingEvent.Level >= Level.Error)
                gravity = 3;
            else if (loggingEvent.Level >= Level.Warn)
                gravity = 4;
            else if (loggingEvent.Level >= Level.Info)
                gravity = 6;

            return (facility * 8 + gravity).ToString();
        }

        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write(ConvertLevelToPriority(loggingEvent));
        }
    }
    
}
