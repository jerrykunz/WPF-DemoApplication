using DemoApp.Model.Settings;
using DemoApp.Services;
using DemoApp.Stores;
using DemoApp.ViewModels;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
            AppServices.Instance.GetService<INavigationStore>().LoadLayout(layout);

            if (MainVm.CurrentViewModel != null)
            {
                AppServices.Instance.GetService<INavigator>().ChangeViewModel(MainVm.CurrentViewModel.GetType(), true, true);
            }
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

    }
}
