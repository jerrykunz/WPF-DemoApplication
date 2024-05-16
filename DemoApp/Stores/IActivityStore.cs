using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Stores
{
    public interface IActivityStore
    { 
        event EventHandler<EventArgs> ActivityUpdated;
        DateTime LastActivity { get; }
        void UpdateActivity();
    }
}
