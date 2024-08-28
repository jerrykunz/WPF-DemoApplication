using DemoApp.Config;
using DemoApp.Logging.SysLog;
using DemoApp.Stores;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DemoApp.ViewModels
{
    class LogViewModel : ActivityViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog sysLog = LogManager.GetLogger(SettingsInitializer.LoggerSysLog);

        ISysLogErrorHandler _syslogErrorHandler;

        private ITextStore _textStore;


        public string HtmlTextLog { get; private set; }
        public string HtmlTextSyslog { get; private set; }



        public ObservableCollection<LogViewModelLogLevel> LogLevels { get { return new ObservableCollection<LogViewModelLogLevel>((LogViewModelLogLevel[])Enum.GetValues(typeof(LogViewModelLogLevel))); } }

        private LogViewModelLogLevel _selectedLogLevel;
        public LogViewModelLogLevel SelectedLogLevel
        {
            get { return _selectedLogLevel; }
            set { _selectedLogLevel = value; OnPropertyChanged(nameof(SelectedLogLevel)); }
        }

        public string LogMessage { get; set; }




        public ObservableCollection<LogViewModelLogLevel> SyslogLevels { get { return new ObservableCollection<LogViewModelLogLevel>((LogViewModelLogLevel[])Enum.GetValues(typeof(LogViewModelLogLevel))); } }

        private LogViewModelLogLevel _selectedSyslogLevel;
        public LogViewModelLogLevel SelectedSyslogLevel
        {
            get { return _selectedSyslogLevel; }
            set { _selectedSyslogLevel = value; OnPropertyChanged(nameof(SelectedSyslogLevel)); }
        }

        public string SyslogMessage { get; set; }




        #region ICommand

        //RelayCommands work like in Locker AFAIK
        //DelegateCommand is similar, but doesn't test command availabiltiy automatically.
        //Also easy usage of types with <T>

        private ICommand _logSaveCommand;
        public ICommand LogSaveCommand
        {
            get
            {
                if (_logSaveCommand == null)
                {
                    _logSaveCommand = new DelegateCommand(LogSave);
                }
                return _logSaveCommand;
            }
        }

        private ICommand _syslogSendCommand;
        public ICommand SyslogSendCommand
        {
            get
            {
                if (_syslogSendCommand == null)
                {
                    _syslogSendCommand = new DelegateCommand(SysLogSend);
                }
                return _syslogSendCommand;
            }
        }

        #endregion

        public LogViewModel(IActivityStore activityStore,
                            ITextStore textStore,
                            ISysLogErrorHandler syslogErrorHandler) : base(activityStore)
        {
            _textStore = textStore;
            _syslogErrorHandler = syslogErrorHandler;
            _syslogErrorHandler.SysLogError += _syslogErrorHandler_SysLogError;

            HtmlTextLog = _textStore.GetString("LogViewLogText.html");
            _selectedLogLevel = LogViewModelLogLevel.Debug;
            LogMessage = "Test message";

            HtmlTextSyslog = _textStore.GetString("LogViewSyslogText.html");
            _selectedSyslogLevel = LogViewModelLogLevel.Debug;
            SyslogMessage = "Test message";
        }

        private void _syslogErrorHandler_SysLogError(object sender, SyslogErrorEventArgs e)
        {

        }

        public void LogSave()
        {
            switch(SelectedLogLevel)
            {
                case LogViewModelLogLevel.Debug:
                    log.Debug(LogMessage);
                    break;
                case LogViewModelLogLevel.Info:
                    log.Info(LogMessage);
                    break;
                case LogViewModelLogLevel.Warn:
                    log.Warn(LogMessage);
                    break;
                case LogViewModelLogLevel.Error:
                    log.Error(LogMessage);
                    break;
                case LogViewModelLogLevel.Fatal:
                    log.Fatal(LogMessage);
                    break;
            }
        }

        public void SysLogSend()
        {
            switch (SelectedLogLevel)
            {
                case LogViewModelLogLevel.Debug:
                    sysLog.Debug(SyslogMessage);
                    break;
                case LogViewModelLogLevel.Info:
                    sysLog.Info(SyslogMessage);
                    break;
                case LogViewModelLogLevel.Warn:
                    sysLog.Warn(SyslogMessage);
                    break;
                case LogViewModelLogLevel.Error:
                    sysLog.Error(SyslogMessage);
                    break;
                case LogViewModelLogLevel.Fatal:
                    sysLog.Fatal(SyslogMessage);
                    break;
            }
        }

    }
}
