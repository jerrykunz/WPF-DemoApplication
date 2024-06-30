using DemoApp.Services;
using DemoApp.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.ViewModels
{
    public class InitViewModel : ActivityViewModelBase
    {
        INavigator _navigator;
        public InitViewModel(INavigator navigator,
                            IActivityStore activityStore) :
            base(activityStore)
        {
            _navigator = navigator;
        }

        public override void OnEnterSoft()
        {

        }

        public override void OnExitSoft()
        {

        }

        public override void OnEnter()
        {
            ////Set UI language
            //var lang = Settings.Preferences.Default.LanguageSettings.Languages[0];
            //App.Instance.SwitchLanguage(lang.Code);

            ////Set style test
            //App.Instance.SwitchStyle("default");

            _navigator.ChangeViewModel<HubViewModel>(true, true);
        }

        public override void OnExit()
        {

        }

    }
}
