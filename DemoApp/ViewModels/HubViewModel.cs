﻿using DemoApp.Services;
using DemoApp.Stores;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DemoApp.ViewModels
{
    public class HubViewModel : ActivityViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        INavigator _navigator;

        private bool _test;

        public string TestText { get { return "TEST_TEXT1"; } }

        #region ICommand

        //RelayCommands work like in Locker AFAIK
        //DelegateCommand is similar, but doesn't test command availabiltiy automatically.
        //Also easy usage of types with <T>

        private ICommand _testCommand;
        public ICommand TestCommand
        {
            get
            {
                if (_testCommand == null)
                {
                    _testCommand = new DelegateCommand<string>(Test);
                }
                return _testCommand;
            }
        }

        private DelegateCommand<object> _testCommand2;
        public ICommand TestCommand2
        {
            get
            {
                if (_testCommand2 == null)
                {
                    _testCommand2 = new DelegateCommand<object>(Test2, CanTest2Empty);
                }
                return _testCommand2;
            }
        }

        #endregion

        public HubViewModel(INavigator navigator,
                                  IActivityStore activityStore) :
            base(activityStore)
        {
            _navigator = navigator;
        }



        private void Test(string text)
        {
            App.Instance.ZoomStyle();
            _test = true;

            //does CanTest2Empty(), changes availability of button in UI
            _testCommand2.RaiseCanExecuteChanged();
        }

        private void EmptyTest2()
        {
            int a = 3;
        }

        private void Test2(object text)
        {
            int a = 3;

            _navigator.ChangeViewModel<NavTest1ViewModel>(true, true);
        }

        private bool CanTest2Empty()
        {
            return _test;
        }

        private bool CanTest2(object o)
        {
            return _test;
        }

        public override void OnEnterSoft()
        {

        }

        public override void OnExitSoft()
        {

        }

        public override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }

    }
}
