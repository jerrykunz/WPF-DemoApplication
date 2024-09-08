using DemoApp.Model;
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
        int CurrentIndex { get; }
        int NextIndex { get; set; }
        int Slots { get; }

        //Dictionaries
        Dictionary<Type, UserControl> ViewByViewModelType { get; }
        Dictionary<Type, ViewModelBase> ViewModelByType { get; }
        Dictionary<Type, Type> ViewTypeByViewModelType { get; }
        Dictionary<Type, string> ViewNamesByVM { get; }
        Dictionary<Type, Type> ViewModelTypeByViewType { get; }
        Dictionary<string, Type> ViewModelTypeByViewName { get; }
        Dictionary<string, Type> ViewTypesByViewName { get; }
        Dictionary<Type, string> ViewNamesByViewType { get; set; }
        HashSet<string> ViewPrefersLooseByName { get; }
        HashSet<Type> ViewPrefersLooseByType { get; }

        //Lists
        List<IViewModel> ViewModelList { get; }
        List<Type> PreviousViewModelTypes { get; }
        List<UserControl> ViewList { get; }
        List<Type> ViewTypeList { get; }
        List<string> ViewNameList { get; }
        List<NavigationFunctionRecord> FunctionList { get;  }

        void AddtoViewModelList(IViewModel viewModel);
        void AddToViewModelTypeList(Type type);
        void AddToViewList(UserControl view);
        void AddToViewTypeList(Type type);
        void AddToViewNameList(string name);


        //Event
        event Action Changed;
        void RaiseChanged();

        //Previous 
        //When old vms are not disposed, we can save the actual ref
        IViewModel PreviousViewModel { get; set; }
        //When old vms are disposed, we don't want any refs to remain so we only get the type
        Type PreviousViewModelType { get; set; }       
        //When old views are not disposed, we can save the actual ref
        UserControl PreviousView { get; set; }
        //When old views are disposed, we don't want any refs to remain so we only get the type
        Type PreviousViewType { get; set; }
        string PreviousViewName{ get; set; }

        //Current
        IViewModel CurrentViewModel { get; set; }
        Type CurrentViewModelType { get; set; }
        UserControl CurrentView { get; set; }
        Type CurrentViewType { get; set; }
        string CurrentViewName { get; set; }

        //Get and load       
        UserControl GetView(Type t);
        UserControl GetViewByName(string name);
        UserControl GetViewByViewModelType(Type t);
        ViewModelBase GetViewModel(Type t);
        ViewModelBase GetViewModelByView(Type t);
        ViewModelBase GetViewModelByViewName(string name);
        UserControl LoadDialogView(string viewName, Type viewType);


       

    }
}
