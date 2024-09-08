using Bluegrams.Application;
using DemoApp.Id;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp
{
    public static class SettingsInitializer
    {
        private static bool _initialized;
        public static string LoggerSysLog;
        public static void InitializeSettings()
        {
            if (!_initialized)
            {
                PortableSettingsProvider.ApplyProvider(Settings.Devices.Default);
                PortableSettingsProvider.ApplyProvider(Settings.Preferences.Default);
                //PortableSettingsProvider.ApplyProvider(Settings.Data.Default);
                //PortableSettingsProvider.ApplyProvider(Settings.Misc.Default);

                string executableName = Process.GetCurrentProcess().MainModule.FileName;
                PortableSettingsProvider.SettingsFileName = App.ProcessName + ".config";

                if (Settings.Devices.Default.SysLogProtocol == Config.SysLogProtocol.Tcp)
                    LoggerSysLog = Logs.SysLogTcp;
                else
                    LoggerSysLog = Logs.SysLogUdp;

                _initialized = true;
            }
        }
    }
}
