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
        void ChangeView(Type viewType, bool onEnter, bool onExit);
        void ChangeView<T>(bool onEnter, bool onExit);
        void ChangeView(string viewName, bool onEnter, bool onExit);
        void PreviousView(bool onEnter, bool onExit);
        void ChangeViewModelDispose(Type viewModelType, bool onEnter, bool onExit);
        void ChangeViewModelDispose<T>(bool onEnter, bool onExit) where T : class, IViewModel;
        void PreviousViewModelDispose(bool onEnter, bool onExit);
        void ChangeViewDispose(Type viewType, bool onEnter, bool onExit);
        void ChangeViewDispose<T>(bool onEnter, bool onExit);
        void ChangeViewDispose(string viewName, bool onEnter, bool onExit);
        void PreviousViewDispose(bool onEnter, bool onExit);
    }
}
