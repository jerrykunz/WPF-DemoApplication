using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Stores
{
    public class ActivityStore : IActivityStore
    {
        public DateTime LastActivity { get; private set; }

        public event EventHandler<EventArgs> ActivityUpdated;

        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
            Debug.WriteLine("User activity detected at time " + LastActivity.ToString());
            OnActivityUpdated();
        }

        public void OnActivityUpdated()
        {
            ActivityUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
