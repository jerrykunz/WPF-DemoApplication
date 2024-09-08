﻿using DemoApp.Helpers;
using log4net.Core;
using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog.Converters
{
    public class ThreadNameConverter : PatternLayoutConverter
    {
        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                ThreadIdConverter.Converter.Format(writer, loggingEvent);
            }

            writer.Write(PrintableAsciiSanitizer.Sanitize(Thread.CurrentThread.Name, 48));
        }
    }
}
