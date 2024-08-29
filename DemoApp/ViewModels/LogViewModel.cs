using DemoApp.Config;
using DemoApp.Id;
using DemoApp.Logging.SysLog;
using DemoApp.Model;
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

            HtmlTextLog = _textStore.GetString("LogViewLogText.html");
            _selectedLogLevel = LogViewModelLogLevel.Debug;
            LogMessage = "Test message";

            HtmlTextSyslog = _textStore.GetString("LogViewSyslogText.html");
            _selectedSyslogLevel = LogViewModelLogLevel.Debug;
            SyslogMessage = "Test message";
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

            App.Instance.MainVm.AddToPopupQueue(new PopupModel
            {
                Mode = PopupMode.OneButton,
                Title = "Notification",
                Text = "Message saved to logfile succesfully",
                Button1Text = "Ok"
            });
        }

        public void SysLogSend()
        {
            App.Instance.SysLogAppender.Testing = true;

            switch (SelectedLogLevel)
            {
                case LogViewModelLogLevel.Debug:
                    sysLog.Debug(Logs.SysLogTestMessageId + SyslogMessage);
                    break;
                case LogViewModelLogLevel.Info:
                    sysLog.Info(Logs.SysLogTestMessageId + SyslogMessage);
                    break;
                case LogViewModelLogLevel.Warn:
                    sysLog.Warn(Logs.SysLogTestMessageId + SyslogMessage);
                    break;
                case LogViewModelLogLevel.Error:
                    sysLog.Error(Logs.SysLogTestMessageId + SyslogMessage);
                    break;
                case LogViewModelLogLevel.Fatal:
                    sysLog.Fatal(Logs.SysLogTestMessageId + SyslogMessage);
                    break;
            }

            App.Instance.SysLogAppender.Testing = false;
        }

        private void Appender_TestSuccess(object sender, EventArgs e)
        {
            App.Instance.MainVm.AddToPopupQueue(new PopupModel
            {
                Mode = PopupMode.OneButton,
                Title = "Notification",
                Text = "Message sent succesfully",
                Button1Text = "Ok"
            });
        }

        private void Appender_TestFailed(object sender, SyslogErrorEventArgs e)
        {
            App.Instance.MainVm.AddToPopupQueue(new PopupModel
            {
                Mode = PopupMode.OneButton,
                Title = "Error",
                Text = e.Message,
                Button1Text = "Ok"
            });
        }

        public override void OnEnter()
        {
            App.Instance.SysLogAppender.TestFailed += Appender_TestFailed;
            App.Instance.SysLogAppender.TestSuccess += Appender_TestSuccess;

            base.OnEnter();
        }

        public override void OnExit()
        {
            App.Instance.SysLogAppender.TestFailed -= Appender_TestFailed;
            App.Instance.SysLogAppender.TestSuccess -= Appender_TestSuccess;

            base.OnExit();
        }

    }
}
