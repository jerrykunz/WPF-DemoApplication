using DemoApp.Id;
using DemoApp.Logging.SysLog;
using DemoApp.Model.Settings;
using DemoApp.Services;
using DemoApp.Stores;
using DemoApp.ViewModels;
using DemoAppDatabase;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DemoApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        

        public static App Instance;
        //public Window Window { get; set; }
        public MainWindowViewModel MainVm;
        public Mutex Mutex { get; private set; }


        public static readonly string ProcessName = "DemoApp"; //Process.GetCurrentProcess().MainModule.FileName;
        public static readonly string MainDirectoryPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        public const string LogFolderName = "Logs";
        public static readonly string LogFolderPath = Path.Combine(MainDirectoryPath, LogFolderName);

        public const string LanguagesFolderName = "Languages";
        public static readonly string LanguagesFolderPath = Path.Combine(MainDirectoryPath, LanguagesFolderName);

        public const string SoundsFolderName = "Sounds";
        public static readonly string SoundsFolderPath = Path.Combine(MainDirectoryPath, SoundsFolderName);

        public const string StylesFolderName = "Styles";
        public static readonly string StylesFolderPath = Path.Combine(MainDirectoryPath, StylesFolderName);

        public LanguageSettings PreviousLanguage { get; set; }
        public LanguageSettings CurrentLanguage { get; set; }

        //test
        public string CurrentStyleFolder { get; set; }
        public int ZoomLevel { get; set; }
        public int ZoomLevelMax { get; set; }

        public App()
        {
            log.Info("Starting program");

            if (CheckIfRunning())
            {
                App.Current.Shutdown();
            }

            Instance = this;

            DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            SettingsInitializer.InitializeSettings();


            //test
            //Settings.Devices.Default.SysLogHost = "127.0.0.1";
            Settings.Devices.Default.SysLogHost = "8.8.8.8";
            Settings.Devices.Default.SysLogProtocol = Config.SysLogProtocol.Udp;
            Settings.Devices.Default.SysLogSendLogsLevelMin = Config.Log4NetLogLevel.Debug;
            Settings.Devices.Default.SysLogPort = 514; //1468;
            Settings.Devices.Default.SysLogInUse = true;
            Settings.Devices.Default.Save();

            //Settings.Devices.Default.SysLogHost = "127.0.0.1";
            //Settings.Devices.Default.SysLogProtocol = Config.SysLogProtocol.Tcp;
            //Settings.Devices.Default.SysLogSendLogsLevelMin = Config.Log4NetLogLevel.Off;
            //Settings.Devices.Default.SysLogPort = 1468;
            //Settings.Devices.Default.SysLogInUse = true;
            //Settings.Devices.Default.Save();


            ApplySysLogSettings();
            FixSettings();

            CurrentStyleFolder = Settings.Preferences.Default.StyleFolder;
            ZoomLevel = 0;
            ZoomLevelMax = Settings.Preferences.Default.ZoomLevels;


            //Useless, because can no longer access MergedDictionaries => can't change languages, styles, etc
            //Main window will be created automatically after this function
            //Create main window manually
            //MainVm = AppServices.Instance.GetService<MainWindowViewModel>();
            //MainWindow = AppServices.Instance.GetService<MainWindow>();
            //MainWindow.DataContext = MainVm;
            //MainWindow.Show();

            //add flat window
            //var flatUiWindow = AppServices.Instance.GetService<FlatUIWindow>();
            //flatUiWindow.Show();
        }

        bool CheckIfRunning()
        {
            bool result;
            Mutex = new Mutex(true, ProcessName, out result);

            if (!result)
            {
                string info = "Another instance is already running.";
                log.Info(info);
                MessageBox.Show(info, ProcessName);
                return true;
            }

            return false;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            // Prevent default unhandled exception processing
            Debug.WriteLine(e.ToString());
            log.Error("UNHANDLED EXCEPTION: ", e.Exception);
            e.Handled = true;
            log.Info("Shutting down after unhandled exception...");
            Shutdown();
        }

        public void FixSettings()
        {
            //test
            Settings.Preferences.Default.AdminExitTimerEnabled = true;
            Settings.Preferences.Default.Save();



            //testing this
            //https://www.codeproject.com/Articles/1238550/Making-Application-Settings-Portable

            //in portable settings but pc-specific
            Settings.ShowCase.Default.RoamingNonPortable = "tateat";
            //in portable settings, non-pc-specific
            Settings.ShowCase.Default.RoamingPortable = "gagdsgas";

            //in windows profile?
            Settings.ShowCase.Default.Portable = " dagdag";
            //in appdata
            Settings.ShowCase.Default.NonPortable = "gfgfgf";
            Settings.ShowCase.Default.Save();
        }

        public void SwitchStyle(string styleFolder)
        {
            string styleFolderWithZoom = styleFolder + "\\ZoomLevel" + ZoomLevel;
            //SetResourceDictionary(GetStyleXAMLFilePath("StyleResourceDictionary_ex", styleFolder), "ResourceDictionaryName", "Style-");
            SetResourceDictionary(GetStyleXAMLFilePath("StyleResourceDictionary_ex", styleFolderWithZoom), "ResourceDictionaryName", "Style-");
            CurrentStyleFolder = styleFolder;
        }

        public void ZoomStyle()
        {
            string styleFolderWithZoom = CurrentStyleFolder + "\\ZoomLevel" + ++ZoomLevel % ZoomLevelMax;
            SetResourceDictionary(GetStyleXAMLFilePath("StyleResourceDictionary_ex", styleFolderWithZoom), "ResourceDictionaryName", "Style-");
        }

        private string GetStyleXAMLFilePath(string prefix, string styleFolder)
        {
            string locXamlFile = string.Empty;
            locXamlFile = prefix + ".xaml";
            return Path.Combine(StylesFolderPath, styleFolder, locXamlFile);
        }

        public void SwitchLayout(string layout)
        {
            //AppServices.Instance.GetService<INavigationStore>().LoadLayout(layout);

            //if (MainVm.CurrentViewModel != null)
            //{
            //    AppServices.Instance.GetService<INavigator>().ChangeViewModel(MainVm.CurrentViewModel.GetType(), true, true);
            //}
        }

        public void SwitchLanguage(string languageCode)
        {
            // HUOM: tämä on otettu jostain netistä silloin alussa. kaikki culturet eivät olekaan viidellä merkillä, esim. inarin saame = smn-FI. näin... toimintaan ei liene vaikutusta, mutta kuitenkin
            //docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.twoletterisolanguagename?view=net-6.0

            //No need to switch to the current language
            if (CultureInfo.CurrentCulture.Name.Equals(languageCode))
            {
                return;
            }

            //var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //Change current thread culture           
            var ci = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            //Was commented away so probably did not work properly?
            //ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
            //foreach (ProcessThread thread in currentThreads)
            //{
            //    Thread.CurrentThread.CurrentCulture = ci;
            //    Thread.CurrentThread.CurrentUICulture = ci;
            //}

            string now = DateTime.Now.ToString();

            SetDictionaries(languageCode);

            //Custom changes per vm
            var vms = AppServices.Instance.GetService<INavigationStore>().ViewModelByType; //Container.Instance.GetViewModels().ToList();
            foreach (var vm in vms.Values)
            {
                vm.OnLanguageChange();
            }

            var lang = Settings.Preferences.Default.LanguageSettings.Languages.FirstOrDefault(x => x.Code == languageCode);
            if (lang != null)
            {
                CurrentLanguage = lang;
            }
        }

        private void SetDictionaries(string languageCode)
        {
            try
            {
                SetResourceDictionary(GetXAMLFilePath("LocalizationDictionary_ex.", languageCode), "ResourceDictionaryName", "Loc-"/*"Locex-"*/);

                var helpStore = AppServices.Instance.GetService<ITextStore>();
                helpStore.PopulateDictionaries(languageCode);

            }
            catch (Exception ex)
            {
                log.Error("Could not set ResourceDictionary " + languageCode + ": "  +ex.Message);
            }

        }

        private string GetXAMLFilePath(string prefix, string languageCode)
        {
            string locXamlFile = prefix + languageCode + ".xaml";
            return Path.Combine(LanguagesFolderPath, languageCode, locXamlFile);
        }

        private void SetResourceDictionary(String inFile, string idField, string idStartValue)
        {
            if (File.Exists(inFile))
            {
                //Read ResourceDictionary File
                var languageDictionary = new ResourceDictionary();
                languageDictionary.Source = new Uri(inFile);

                //Get index of current localization dictionary in MergedDictionaries
                int langDictId = -1;
                for (int i = 0; i < Resources.MergedDictionaries.Count; i++)
                {
                    var md = Resources.MergedDictionaries[i];

                    // Make sure your Localization ResourceDictionarys have the ResourceDictionaryName
                    // key and that it is set to a value starting with "Loc-".
                    if (md.Contains(idField))
                    {
                        var astdf = md[idField].ToString();
                        if (md[idField].ToString().StartsWith(idStartValue)) // jos on esim. Loc-Default- niin silloinkin otetaan, hehheh
                        {
                            langDictId = i;
                            break;
                        }
                    }
                }
                if (langDictId == -1)
                {
                    //No dictionary found, add in newly loaded dictionary
                    Resources.MergedDictionaries.Add(languageDictionary);
                }
                else
                {
                    //Replace the current dictionary with the new one using the obtained index
                    Resources.MergedDictionaries[langDictId] = languageDictionary;
                }
            }
        }

        private void ApplySysLogSettings()
        {
            if (!Settings.Devices.Default.SysLogInUse)
                return;

            var logAppenders = LogManager.GetRepository().GetAppenders();

            IAppender syslogTextAppender = logAppenders.FirstOrDefault(x => x.Name == Logs.SysLogFileAppender);



            IAppender syslogAppender = null;
            if (Settings.Devices.Default.SysLogProtocol == Config.SysLogProtocol.Tcp)
            {
                syslogAppender = logAppenders.FirstOrDefault(x => x is TcpAppender);
            }
            else
            {
                //log4net default syslog updappender
                //syslogAppender = logAppenders.FirstOrDefault(x => x is UdpAppender);

                syslogAppender = logAppenders.FirstOrDefault(x => x is UdpAppenderCustom);
            }

            if (syslogAppender != null)
            {
                IPAddress sysLogHost = null;
                try
                {
                    sysLogHost = IPAddress.Parse(Settings.Devices.Default.SysLogHost);
                }
                catch (Exception ex)
                {
                    try
                    {
                        IPAddress[] ipAddresses = Dns.GetHostAddresses(Settings.Devices.Default.SysLogHost);
                        sysLogHost = ipAddresses[0];
                    }
                    catch (Exception ex2)
                    {
                        log.Error("Couldn't parse SysLog ip address from settings", ex);
                        log.Error("Couldn't resolve SysLog host from settings - reverting to localhost", ex2);
                        sysLogHost = IPAddress.Parse("127.0.0.1");
                    }
                }

                if (syslogAppender is TcpAppender)
                {
                    var tcpLogAppender = syslogAppender as TcpAppender;

                    tcpLogAppender.RemoteAddress = sysLogHost;
                    //port 6514 default
                    tcpLogAppender.RemotePort = Settings.Devices.Default.SysLogPort;
                    tcpLogAppender.Encoding = Encoding.GetEncoding(Settings.Devices.Default.SysLogEncoding);

                    var sysLogLayout = new SysLogLayout(Settings.Devices.Default.SysLogLayout);
                    sysLogLayout.FacilityCode = Settings.Devices.Default.SysLogFacilityCode;
                    sysLogLayout.StructuredDataPrefix = Settings.Devices.Default.SysLogStructuredDataPrefix;
                    sysLogLayout.PrependMessageLength = Settings.Devices.Default.SysLogPrependMessageLength.ToString();
                    sysLogLayout.OnlyFirstLine = Settings.Devices.Default.SysLogEntryFirstLineOnly;
                    sysLogLayout.ActivateOptions();
                    tcpLogAppender.Layout = sysLogLayout;

                    //var sysLogFilter = new LogExceptionToFileFilter();
                    //sysLogFilter.ActivateOptions();

                    //tcpLogAppender.AddFilter(sysLogFilter);
                    tcpLogAppender.ErrorHandler = AppServices.Instance.GetService<ISysLogErrorHandler>(); //new SyslogErrorHandler();
                    tcpLogAppender.ActivateOptions();
                }
                else
                {
                    //var udpLogAppender = syslogAppender as UdpAppender;
                    var udpLogAppender = syslogAppender as UdpAppenderCustom;

                    udpLogAppender.RemoteAddress = sysLogHost;
                    //port 514 default
                    udpLogAppender.RemotePort = Settings.Devices.Default.SysLogPort;
                    udpLogAppender.Encoding = Encoding.GetEncoding(Settings.Devices.Default.SysLogEncoding);

                    var sysLogLayout = new SysLogLayout(Settings.Devices.Default.SysLogLayout);
                    sysLogLayout.FacilityCode = Settings.Devices.Default.SysLogFacilityCode;
                    sysLogLayout.StructuredDataPrefix = Settings.Devices.Default.SysLogStructuredDataPrefix;
                    sysLogLayout.PrependMessageLength = Settings.Devices.Default.SysLogPrependMessageLength.ToString();
                    sysLogLayout.OnlyFirstLine = Settings.Devices.Default.SysLogEntryFirstLineOnly;
                    sysLogLayout.ActivateOptions();

                    udpLogAppender.Layout = sysLogLayout;

                    //var sysLogFilter = new LogExceptionToFileFilter();
                    //sysLogFilter.ActivateOptions();

                    //udpLogAppender.AddFilter(sysLogFilter);
                    udpLogAppender.ErrorHandler = AppServices.Instance.GetService<ISysLogErrorHandler>(); //new SyslogErrorHandler();
                    udpLogAppender.ActivateOptions();
                }

                //Do we want all the logs of log.txt in syslog too? What levels? Do it here.
                if (Settings.Devices.Default.SysLogSendLogsLevelMin != Config.Log4NetLogLevel.Off)
                {
                    Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

                    // Get the root logger from the hierarchy
                    Logger rootLoggerHierarchy = hierarchy.Root;


                    List<IAppender> appenders = new List<IAppender>();
                    appenders.Add(syslogAppender);

                    //write to syslog.txt as well
                    if (syslogTextAppender != null)
                    {
                        appenders.Add(syslogTextAppender);
                    }

                    //add a level filter appender so we can syslog just the log events we want
                    SysLogFilterAppender appender = new SysLogFilterAppender(Settings.Devices.Default.SysLogSendLogsLevelMin,
                                                                             Settings.Devices.Default.SysLogSendLogsLevelMax,
                                                                             appenders);

                    //SysLogFilterAppender appender = new SysLogFilterAppender(Settings.Devices.Default.SysLogSendLogsLevelMin,
                    //                                                         Settings.Devices.Default.SysLogSendLogsLevelMax,
                    //                                                         new List<IAppender> { syslogAppender });


                    rootLoggerHierarchy.AddAppender(appender);

                    var loggers = LogManager.GetCurrentLoggers();
                    foreach (ILog iLog in loggers)
                    {
                        //don't add to syslog loggers, or this class that will inherit root (it will show appenders 0 but still work)
                        if (iLog.Logger.Name == this.GetType().ToString() ||
                            iLog.Logger.Name == "SysLogTcp" ||
                            iLog.Logger.Name == "SysLogUdp" ||
                            iLog.Logger.Name == "SysLogError" ||
                            iLog.Logger.Name == "DemoApp.ViewModels.LogViewModel") //this one is for log testing purposes, so exclude it
                            continue;

                        //everywhere else though
                        if (iLog.Logger is Logger)
                        {
                            string name = ((Logger)iLog.Logger).Name;
                            ((Logger)iLog.Logger).AddAppender(appender);
                        }
                    }

                    //log.Error("TESTESTESTESTEST");

                    //test, list of loggers
                    //var loggers2 = LogManager.GetCurrentLoggers().Select(x => ((Logger)x.Logger));

                    //test, list of logger names
                    //var loggers3 = LogManager.GetCurrentLoggers().Select(x => ((Logger)x.Logger).Name);
                }

            }
            else
            {
                log.Error("Unable to find Tcp/UdpAppender in log4net.config - cannot configure SysLog");
            }

        }

    }
}
