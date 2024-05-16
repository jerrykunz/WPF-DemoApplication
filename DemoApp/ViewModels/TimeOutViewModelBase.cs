using DemoApp.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DemoApp.ViewModels
{
    public class TimeOutViewModelBase : ActivityViewModelBase
    {
        public bool IsBusy { get; set; }
        public bool TimedOut { get; set; }

        protected bool IsAdminViewModel { get; set; }
        protected DispatcherTimer TimeOutTimer;
        public TimeOutViewModelBase(IActivityStore activityStore)
            : base(activityStore)
        {
            TimeOutTimer = new DispatcherTimer();
            TimeOutTimer.Interval = TimeSpan.FromSeconds(30);
        }

        protected void SetTimeOut(TimeSpan span)
        {
            if (TimeOutTimer != null)
                TimeOutTimer.Interval = span;
        }

        protected void StartTimeOutTimer()
        {
            if (TimeOutTimer != null)
                TimeOutTimer.Start();
        }

        protected void StopTimeOutTimer()
        {
            if (TimeOutTimer != null)
                TimeOutTimer.Stop();
        }

        protected void _clearTimer_Tick(object sender, EventArgs e)
        {          
            if (IsBusy)
            {
                //Wait until no longer busy, then timeout
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (IsBusy)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        break;
                    }

                    //but only if there was no activity
                    TimeSpan timeSinceLastActivity = (DateTime.Now - Activity.LastActivity);
                    if (timeSinceLastActivity > TimeOutTimer.Interval)
                    {
                        TimedOut = true;
                        TimeOut();
                    }                  
                    //otherwise restart timeout timer
                    else
                    {
                        StartTimeOutTimer();
                    }
                });
                return;
            }

            //Normal timeout
            TimedOut = true;
            TimeOut();
        }

        //Returns true when IsBusy was already on
        protected bool SetBusyOn()
        {
            if (IsBusy)
                return true;

            IsBusy = true;
            return false;
        }

        protected virtual void TimeOut()
        {

        }

        protected override void ActivityUpdated(object sender, EventArgs e)
        {
            //this should probably be here, but let's not yet
            //if (App.Instance.MainVm.CurrentViewModel != this)
            //    return;

            //restart timer
            if (TimeOutTimer != null)
            {
                TimeOutTimer.Stop();
                TimeOutTimer.Start();
            }
        }

        public override void OnEnter()
        {
            IsBusy = false;
            TimedOut = false;

            base.OnEnter();

            if (IsAdminViewModel &&
                Settings.Preferences.Default.AdminExitTimerEnabled)
            {
                TimeOutTimer.Tick += _clearTimer_Tick;
                TimeOutTimer.Start();
            }
            else if (Settings.Preferences.Default.UserExitTimerEnabled)
            {
                TimeOutTimer.Tick += _clearTimer_Tick;
                TimeOutTimer.Start();
            }
        }

        public override void OnExit()
        {
            IsBusy = false;

            if (IsAdminViewModel &&
                Settings.Preferences.Default.AdminExitTimerEnabled)
            {
                TimeOutTimer.Stop();
                TimeOutTimer.Tick -= _clearTimer_Tick;
            }
            else if (Settings.Preferences.Default.UserExitTimerEnabled)
            {
                TimeOutTimer.Stop();
                TimeOutTimer.Tick -= _clearTimer_Tick;
            }

            base.OnExit();
        }
    }
}
