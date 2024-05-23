using DemoApp.Services;
using DemoApp.Stores;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DemoApp.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Activity
        private IActivityStore _activityStore;
        private bool _mouseDown;
        #endregion

        #region Navigation

        INavigator _navigator;
        INavigationStore _navigationStore;

        #region ViewModels
        public IViewModel PreviousViewModel
        {
            get { return _navigationStore.PreviousViewModel; }
            private set { _navigationStore.PreviousViewModel = value; OnPropertyChanged(nameof(PreviousViewModel)); }
        }

        public Type PreviousViewModelType
        {
            get { return _navigationStore.PreviousViewModelType; }
            private set { _navigationStore.PreviousViewModelType = value; OnPropertyChanged(nameof(PreviousViewModelType)); }
        }

        public IViewModel CurrentViewModel
        {
            get { return _navigationStore.CurrentViewModel; }
            private set { _navigationStore.CurrentViewModel = value; OnPropertyChanged(nameof(CurrentViewModel)); }
        }

        public IReadOnlyCollection<IViewModel> PreviousViewModels
        {
            get { return _navigationStore.PreviousViewModels; }
        }

        public IReadOnlyCollection<IViewModel> PreviousViewModelTypes
        {
            get { return _navigationStore.PreviousViewModelTypes; }
        }
        #endregion

        #region Views

        public UserControl CurrentView
        {
            get { return _navigationStore.CurrentView; }
            private set { _navigationStore.CurrentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public Type CurrentViewType
        {
            get { return _navigationStore.CurrentViewType; }
            private set { _navigationStore.CurrentViewType = value; OnPropertyChanged(nameof(CurrentViewType)); }
        }

        //NOTE: Usercontrols have x:Name, that can be used instead. Doesn't seem useful for now though.
        public string CurrentViewName
        {
            get { return _navigationStore.CurrentViewName; }
            private set { _navigationStore.CurrentViewName = value; OnPropertyChanged(nameof(CurrentViewName)); }
        }

        public UserControl PreviousView
        {
            get { return _navigationStore.PreviousView; }
            private set { _navigationStore.PreviousView = value; OnPropertyChanged(nameof(PreviousView)); }
        }

        public Type PreviousViewType
        {
            get { return _navigationStore.PreviousViewType; }
            private set { _navigationStore.PreviousViewType = value; OnPropertyChanged(nameof(PreviousViewType)); }
        }

        public string PreviousViewName
        {
            get { return _navigationStore.PreviousViewName; }
            private set { _navigationStore.PreviousViewName = value; OnPropertyChanged(nameof(PreviousViewName)); }
        }

        public IReadOnlyCollection<UserControl> PreviousViews
        {
            get { return _navigationStore.PreviousViews; }
        }

        public IReadOnlyCollection<Type> PreviousViewTypes
        {
            get { return _navigationStore.PreviousViewTypes; }
        }

        public IReadOnlyCollection<string> PreviousViewNames
        {
            get { return _navigationStore.PreviousViewNames; }
        }

        #endregion

        #endregion


        //private IResourceManager _resourceManager;
        //private LogSender _logSender;

        private bool _maximized;
        public bool Maximized
        {
            get { return _maximized; }
            set { _maximized = value; OnPropertyChanged(nameof(Maximized)); }
        }

        public double Width { get; private set; }
        public double Height { get; private set; }

        #region ICommand

        private ICommand _closingCommand;
        public ICommand ClosingCommand
        {
            get
            {
                if (_closingCommand == null)
                {
                    _closingCommand = new DelegateCommand(Closing);
                }
                return _closingCommand;
            }
        }

        private ICommand _closedCommand;
        public ICommand ClosedCommand
        {
            get
            {
                if (_closedCommand == null)
                {
                    _closedCommand = new DelegateCommand(Closed);
                }
                return _closedCommand;
            }
        }

        private ICommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new DelegateCommand(Exit);
                }
                return _exitCommand;
            }
        }

        private ICommand _fullScreenCommand;
        public ICommand FullScreenCommand
        {
            get
            {
                if (_fullScreenCommand == null)
                {
                    _fullScreenCommand = new DelegateCommand(FullScreen);
                }
                return _fullScreenCommand;
            }
        }

        private ICommand _registerUserActivityCommand;
        public ICommand RegisterUserActivityCommand
        {
            get
            {
                if (_registerUserActivityCommand == null)
                {
                    _registerUserActivityCommand = new DelegateCommand(OnRegisterUserActivityCommand);
                }
                return _registerUserActivityCommand;
            }
        }

        private ICommand _mouseDownCommand;
        public ICommand MouseDownCommand
        {
            get
            {
                if (_mouseDownCommand == null)
                {
                    _mouseDownCommand = new DelegateCommand(MouseDown);
                }
                return _mouseDownCommand;
            }
        }

        private ICommand _mouseUpCommand;
        public ICommand MouseUpCommand
        {
            get
            {
                if (_mouseUpCommand == null)
                {
                    _mouseUpCommand = new DelegateCommand(MouseUp);
                }
                return _mouseUpCommand;
            }
        }

        private ICommand _minimizeCommand;
        public ICommand MinimizeCommand
        {
            get
            {
                if (_minimizeCommand == null)
                {
                    _minimizeCommand = new DelegateCommand(Minimize);
                }
                return _minimizeCommand;
            }
        }

        private ICommand _maximizeCommand;
        public ICommand MaximizeCommand
        {
            get
            {
                if (_maximizeCommand == null)
                {
                    _maximizeCommand = new DelegateCommand(Maximize);
                }
                return _maximizeCommand;
            }
        }

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new DelegateCommand(Close);
                }
                return _closeCommand;
            }
        }

        private ICommand _borderMouseDownCommand;
        public ICommand BorderMouseDownCommand
        {
            get
            {
                if (_borderMouseDownCommand == null)
                {
                    _borderMouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(BorderMouseDown);
                }
                return _borderMouseDownCommand;
            }
        }

        private ICommand _borderMouseLeftButtonDownCommand;
        public ICommand BorderMouseLeftButtonDownCommand
        {
            get
            {
                if (_borderMouseLeftButtonDownCommand == null)
                {
                    _borderMouseLeftButtonDownCommand = new DelegateCommand<MouseButtonEventArgs>(BorderMouseLeftButtonDown);
                }
                return _borderMouseLeftButtonDownCommand;
            }
        }

        private ICommand _handleMouseCommand;
        public ICommand HandleMouseCommand
        {
            get
            {
                if (_handleMouseCommand == null)
                {
                    _handleMouseCommand = new DelegateCommand<MouseButtonEventArgs>(HandleMouse);
                }
                return _handleMouseCommand;
            }
        }


        #endregion

        public MainWindowViewModel(INavigator navigator,
                                   IActivityStore activityStore,
                                   INavigationStore navigationStore)
        {
            Width = Application.Current.MainWindow.Width;
            Height = Application.Current.MainWindow.Height;

            _navigator = navigator;
            _navigationStore = navigationStore;
            _navigationStore.Changed += _navigationStore_Changed;

            _activityStore = activityStore;

            //If using StartupURI, can be used when creating MainWindow manually as well, it does not hurt
            App.Instance.MainVm = this;

            //mergeddictionaries not updated yet, no languages or styles set there
            //when app ctor finishes, these will be overwritten i guess
            //move this to initviewmodel?

            //Set UI language
            var lang = Settings.Preferences.Default.LanguageSettings.Languages[1];
            App.Instance.SwitchLanguage(lang.Code);

            //Set style test
            App.Instance.SwitchStyle("default");


            ////Set theme
            //if (Settings.Preferences.Default.ThemesUseCustom &&
            //    Settings.Preferences.Default.ThemeSettings.Themes.Count > 0)
            //{
            //    var currentTheme = Settings.Preferences.Default.ThemeSettings.Themes.FirstOrDefault(x => x.InUse);

            //    if (currentTheme != null)
            //    {
            //        //Set style
            //        App.Instance.SwitchStyle(currentTheme.Style);

            //        //Set Layout
            //        App.Instance.SwitchLayout(currentTheme.Layout);
            //    }
            //    else
            //    {
            //        //TODO: throw error
            //    }
            //}



            //Logsender and ResourceManager

            //_logSender = AppServices.Instance.GetService<LogSender>();
            //_logSender.Start();

            //_resourceManager = AppServices.Instance.GetService<IResourceManager>();

            //try
            //{
            //    _resourceManager.Start();
            //}
            //catch (Exception ex)
            //{
            //    log.Error(ex);
            //}

            //Vm and view init

            Maximized = Application.Current.MainWindow.WindowState == WindowState.Maximized;
            _navigator.ChangeViewModel<InitViewModel>(true, true);
        }

        private void _navigationStore_Changed()
        {
            OnPropertyChanged(nameof(PreviousViewModel));
            OnPropertyChanged(nameof(PreviousViewModelType));
            OnPropertyChanged(nameof(PreviousViewModels));
            OnPropertyChanged(nameof(PreviousViewModelTypes));
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(CurrentView));
        }

        #region ICommand Functions
        private void MouseDown()
        {
            if (_mouseDown)
                return;

            _mouseDown = true;

            Task.Run(() =>
            {
                while (true)
                {
                    if (!_mouseDown)
                        break;
                    Debug.WriteLine("MOUSEDOWN");
                    _activityStore.UpdateActivity();
                    Thread.Sleep(250);
                }
            });
        }

        private void MouseUp()
        {
            Debug.WriteLine("MOUSEUP");
            _activityStore.UpdateActivity();
            _mouseDown = false;
        }

        public void OnRegisterUserActivityCommand()
        {
            _activityStore.UpdateActivity();
        }

        public void Closing()
        {
            //doesn't work
            //bool wasCodeClosed = new StackTrace().GetFrames().FirstOrDefault(x => x.GetMethod() == typeof(Window).GetMethod("Close")) != null;
            //if (wasCodeClosed)
            //{
            //    log.Info("X-button pressed, closing program...");
            //}

            log.Info("Program closing...");
        }

        public void Closed()
        {
            log.Info("Program closed...");
        }

        public void Exit()
        {
            log.Info("Alt+F4 pressed, closing program...");
            App.Current.Shutdown();
        }

        public void FullScreen()
        {
            switch (Application.Current.MainWindow.WindowState)
            {
                case WindowState.Minimized:
                case WindowState.Normal:
                    log.Info("Alt+Enter pressed, switching to fullscreen");
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                    Application.Current.MainWindow.WindowStyle = WindowStyle.None;
                    Application.Current.MainWindow.Topmost = true;
                    break;

                case WindowState.Maximized:
                    log.Info("Alt+Enter pressed, switching to normal size");
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                    Application.Current.MainWindow.Topmost = false;
                    break;
            }
        }

        public void Minimize()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        public void Maximize()
        {           
            switch(Application.Current.MainWindow.WindowState)
            {              
                case WindowState.Maximized:
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    break;
                default:
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                    break;
            }
            Maximized = Application.Current.MainWindow.WindowState == WindowState.Maximized;
        }

        public void Close()
        {
            log.Info("X pressed, closing program...");
            App.Current.Shutdown();
        }

        public void HandleMouse(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public void BorderMouseDown(MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                Application.Current.MainWindow.DragMove();
            }
            ;
        }

        public void BorderMouseLeftButtonDown(MouseButtonEventArgs e)
        {

            if (e.ClickCount == 2)
            {
                if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.Width = Width;
                    Application.Current.MainWindow.Height = Height;

                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
                else
                {
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                }
            }

            Maximized = Application.Current.MainWindow.WindowState == WindowState.Maximized;
        }
        #endregion
    }
}
