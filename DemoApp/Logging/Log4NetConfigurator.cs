using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Logging
{
    public class Log4NetConfigurator
    {
        //Removes the configuration loaded from log4net.config at startup
        static void ResetConfiguration()
        {
            log4net.LogManager.ResetConfiguration();
        }

        //Creates new configuration according to settings
        static void CreateConfigurationFromSettings()
        {

        }
    }
}
