using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog
{
    public interface ISysLogErrorHandler : IErrorHandler
    {
        event EventHandler<SyslogErrorEventArgs> SysLogError;
    }
}
