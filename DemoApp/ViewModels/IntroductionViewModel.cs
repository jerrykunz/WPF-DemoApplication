using DemoApp.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.ViewModels
{
    public class IntroductionViewModel : ActivityViewModelBase
    {
        private ITextStore _textStore;
        public string HtmlText { get; private set; }

        public IntroductionViewModel(IActivityStore activityStore, ITextStore textStore) : base(activityStore)
        {
            _textStore = textStore;
            HtmlText = _textStore.GetString("IntroductionView.html");

        }

        public override void OnLanguageChange()
        {
            HtmlText = _textStore.GetString("IntroductionView.html");
        }
    }
}
