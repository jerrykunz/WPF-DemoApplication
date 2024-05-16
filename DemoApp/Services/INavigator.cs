using DemoApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Services
{
    public interface INavigator
    {
        void ChangeViewModel(Type viewModelType, bool onEnter, bool onExit);
        void ChangeViewModel<T>(bool onEnter, bool onExit) where T : class, IViewModel;
        void PreviousViewModel(bool onEnter, bool onExit);
    }
}
