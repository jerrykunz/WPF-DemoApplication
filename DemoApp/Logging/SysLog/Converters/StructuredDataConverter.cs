﻿using DemoApp.Helpers;
using log4net.Core;
using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog.Converters
{
    /// <summary>
    /// Converts data found within the properties of a logging event into Key/Value pairs to be displayed using syslog's Extended Data format as described 
    /// in RFC 5424 section 6.3: http://tools.ietf.org/html/rfc5424#section-6.3
    /// </summary>
    public class StructuredDataConverter : PatternLayoutConverter
    {
        public StructuredDataConverter()
        {
            // This converter handles the exception
            IgnoresException = false;  //TODO deal with this. Sealed?
        }

        private static string SanitizeSdName(string sdName)
        {
            // sanitize the SD-NAME as per http://tools.ietf.org/html/rfc5424#section-6.3.3
            // SD-NAME         = 1*32PRINTUSASCII; except '=', SP, ']', %d34 (")

            return PrintableAsciiSanitizer.Sanitize(sdName, 32, new byte[] { 0x5D, 0x22, 0x3D });
        }

        private static string SanitizeSdParamValue(string sdParamValue)
        {
            // sanitize the SD-PARAM-VALUE as per http://tools.ietf.org/html/rfc5424#section-6.3.3
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            foreach (char ch in sdParamValue)
            {
                if (ch == '"' || ch == '\\' || ch == ']')
                {
                    // escape the character by prepending a literal '\'
                    stringBuilder.Append('\\');
                }
                stringBuilder.Append(ch);
            }

            return stringBuilder.ToString();
        }

        private static void AddStructuredData(TextWriter writer, string sdParamName, string sdParamValue)
        {
            if (!string.IsNullOrEmpty(sdParamValue))
            {
                writer.Write(" ");
                writer.Write(SanitizeSdName(sdParamName));
                writer.Write("=\"");
                writer.Write(SanitizeSdParamValue(sdParamValue));
                writer.Write("\"");
            }
        }

        private void HandleException(TextWriter writer, LoggingEvent loggingEvent)
        {
            System.Exception exceptionObject = loggingEvent.ExceptionObject;

            if (exceptionObject != null)
            {
                AddStructuredData(writer, "ExceptionSource", exceptionObject.Source);
                AddStructuredData(writer, "ExceptionType", exceptionObject.GetType().FullName);
                AddStructuredData(writer, "ExceptionMessage", exceptionObject.Message);
                AddStructuredData(writer, "EventHelp", exceptionObject.HelpLink);

                StackTrace trace = new StackTrace(exceptionObject, true);

                if (trace.FrameCount > 0)
                {
                    StackFrame frame = trace.GetFrame(0);

                    var method = frame.GetMethod();
                    string methodReturnType = "";

                    if (method is MethodInfo)
                    {
                        methodReturnType = (method as MethodInfo).ReturnType.Name;
                    }

                    var fullMethodName = string.Format("{0} {1}.{2}({3})", methodReturnType, method.ReflectedType.FullName, method.Name, string.Join(",", method.GetParameters().Select(o => string.Format("{0} {1}", o.ParameterType, o.Name)).ToArray()));

                    AddStructuredData(writer, "ExceptionMethodName", fullMethodName);
                    AddStructuredData(writer, "ExceptionFileName", frame.GetFileName());
                    AddStructuredData(writer, "ExceptionLineNumber", frame.GetFileLineNumber().ToString());
                }

                if (loggingEvent.Properties.Contains("log4net:syslog-exception-log"))
                {
                    AddStructuredData(writer, "EventLog", loggingEvent.Properties["log4net:syslog-exception-log"].ToString());
                }
            }
            else
            {
                string exceptionString = loggingEvent.GetExceptionString();
                if (!string.IsNullOrEmpty(exceptionString))
                {
                    AddStructuredData(writer, "ExceptionMessage", exceptionString);
                }
            }
        }

        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write("[");
            writer.Write(loggingEvent.Properties["log4net:StructuredDataPrefix"]);

            var properties = loggingEvent.GetProperties();
            foreach (var key in properties.GetKeys())
            {
                if (!key.StartsWith("log4net:") && properties[key] != null) // ignore built-in log4net diagnostics. keep the NDC stack in there.
                {
                    AddStructuredData(writer, key, properties[key].ToString());
                }
            }

            AddStructuredData(writer, "EventSeverity", loggingEvent.Level.DisplayName);
            HandleException(writer, loggingEvent);

            writer.Write("]");
        }
    }
}
