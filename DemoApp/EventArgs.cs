using DemoApp.Config;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp
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

    public class PopupEventArgs : EventArgs
    {
        public int QueueNumber { get; set; }
        public PopupClickedButton Button { get; set; }
    }
}
