using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog
{
    public interface ISysLogAppender
    {
        bool Testing { get; set; }
        event EventHandler<EventArgs> TestSuccess;
        event EventHandler<SyslogErrorEventArgs> TestFailed;
    }
}
