using DemoApp.Helpers;
using log4net.Core;
using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog.Converters
{
    /// <summary>
    /// Provides the ID value of the current processing thread.
    /// </summary>
    public class ThreadIdConverter : PatternLayoutConverter
    {
        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            var id = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
            writer.Write(PrintableAsciiSanitizer.Sanitize(id, 48));
        }

        private static ThreadIdConverter _converter = new ThreadIdConverter();
        internal static ThreadIdConverter Converter
        {
            get
            {
                return _converter;
            }
        }
    }
}
