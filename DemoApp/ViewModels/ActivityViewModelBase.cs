using DemoApp.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.ViewModels
{
    public class ActivityViewModelBase : ViewModelBase
    {
        protected IActivityStore Activity { get; private set; }

        public ActivityViewModelBase(IActivityStore activity)
        {
            Activity = activity;
            Activity.ActivityUpdated += ActivityUpdated;

            Activity.UpdateActivity();
        }



        protected virtual void ActivityUpdated(object sender, EventArgs e)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
