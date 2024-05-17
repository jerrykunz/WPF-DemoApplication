using DemoApp.Services;
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
    public class NavTest1ViewModel : ActivityViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        INavigator _navigator;


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

        public NavTest1ViewModel(INavigator navigator,
                                  IActivityStore activityStore) :
            base(activityStore)
        {
            _navigator = navigator;
        }

      

        private void Test(string text)
        {
            switch (App.Instance.MainVm.CurrentViewName)
            {
                case "NavTest1View":
                    _navigator.ChangeView("NavTest2View", false, false);
                    break;
                case "NavTest2View":
                    _navigator.ChangeView("NavTest1View", false, false);
                    break;
            }           
        }

        private void EmptyTest2()
        {

        }

        private void Test2(object text)
        {
            _navigator.PreviousView(true, true);
        }

        private bool CanTest2Empty()
        {
            return true;
        }

        private bool CanTest2(object o)
        {
            return true;
        }

        public override void OnEnterSoft()
        {
            _testCommand2.RaiseCanExecuteChanged();
        }

        public override void OnExitSoft()
        {

        }

        public override void OnEnter()
        {
            int a = 3;
            //_testCommand2.RaiseCanExecuteChanged();
        }

        public override void OnExit()
        {

        }

    }
}
