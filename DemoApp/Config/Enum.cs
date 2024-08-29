using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Config
{
    public enum LooseViews
    {
        None,
        PerView,
        All
    }

    public enum Log4NetLogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        Off
    }

    public enum SysLogProtocol
    {
        Tcp,
        Udp
    }

    public enum Test
    {
        Test1,
        Test2
    }

    public enum LogViewModelLogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public enum PopupMode
    {
        Inactive,
        OneButton,
        TwoButtons
    }

    public enum PopupClickedButton
    {
        Ok,
        Yes,
        No
    }
}
