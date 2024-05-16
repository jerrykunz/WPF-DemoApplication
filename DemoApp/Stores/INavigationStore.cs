using DemoApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DemoApp.Stores
{
    public interface INavigationStore
    {
        Dictionary<Type, UserControl> Views { get; }
        Dictionary<Type, ViewModelBase> ViewModels { get; }
        Dictionary<Type, Type> ViewsByVM { get; }
        Dictionary<Type, string> ViewNamesByVM { get; }
        Dictionary<Type, Type> ViewModelsByView { get; }
        Dictionary<string, Type> ViewModelsByViewName { get; }
        Dictionary<string, Type> ViewTypesByViewName { get; }

        IReadOnlyCollection<IViewModel> PreviousViewModels { get; }
        IReadOnlyCollection<IViewModel> PreviousViewModelTypes { get; }
        IReadOnlyCollection<UserControl> PreviousViews { get; }
        IReadOnlyCollection<Type> PreviousViewTypes { get; }

        event Action Changed;
        //When old vms are not disposed, we can save the actual ref
        IViewModel PreviousViewModel { get; set; }
        //When old vms are disposed, we don't want any refs to remain so we only get the type
        Type PreviousViewModelType { get; set; }       
        //When old views are not disposed, we can save the actual ref
        UserControl PreviousView { get; set; }
        //When old views are disposed, we don't want any refs to remain so we only get the type
        Type PreviousViewType { get; set; }
        IViewModel CurrentViewModel { get; set; }
        UserControl CurrentView { get; set; }
        Type CurrentViewType { get; set; }
        void RaiseChanged();
        UserControl GetView(Type t);
        UserControl GetViewByVm(Type t);
        ViewModelBase GetViewModel(Type t);
        ViewModelBase GetViewModelByView(Type t);
        ViewModelBase GetViewModelByViewName(string name);
        UserControl LoadDialogView(string viewName, Type viewType);
        void LoadLayout(string layout);
        void AddToPreviousVmQueue(IViewModel viewModel);
        void AddToPreviousVmTypeQueue(Type type);
        void AddToPreviousViewQueue(UserControl view);
        void AddToPreviousViewTypeQueue(Type type);

    }
}
