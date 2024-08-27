using DemoApp.Config;
using DemoApp.Model;
using DemoApp.Services;
using DemoApp.ViewModels;
using DemoApp.Views.Embedded;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DemoApp.Stores
{       
    public class NavigationStore : INavigationStore
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int NextIndex { get; set; }
        public int CurrentIndex { get { return NextIndex - 1; } }

        public int Slots { get; private set; }

        public event Action Changed;

        //whether new views and vms are created for each view change
        public bool MultiUseViewsAndVms;

        //load loose xamls or from inside the assembly
        public LooseViews LooseViews;
        private string _looseViewSubFolder;

        //all views and viewmodels by type
        public Dictionary<Type, UserControl> ViewByViewModelType { get; private set; }
        public Dictionary<string, UserControl> ViewByName { get; private set; }
        public Dictionary<Type, ViewModelBase> ViewModelByType { get; private set; }

        //viewmodels by view type or name
        public Dictionary<Type, Type> ViewModelTypeByViewType { get; private set; }
        public Dictionary<string, Type> ViewModelTypeByViewName { get; private set; }

        //whether to use loose or builtin xamls, by view type or name
        public HashSet<string> ViewPrefersLooseByName { get; private set; }
        public HashSet<Type> ViewPrefersLooseByType { get; private set; }

        //get the view or view names by vm
        public Dictionary<Type, Type> ViewTypeByViewModelType { get; set; }
        public Dictionary<Type, string> ViewNamesByVM { get; set; }
        public Dictionary<Type, string> ViewNamesByViewType { get; set; }

        //get view type by name
        public Dictionary<string, Type> ViewTypesByViewName { get; set; }

        public Type PreviousViewModelType { get; set; }
        public IViewModel CurrentViewModel { get; set; }
        public Type CurrentViewModelType { get; set; }
        public IViewModel PreviousViewModel { get; set; }
       
        public UserControl CurrentView { get; set; }
        public Type CurrentViewType { get; set; }
        public string CurrentViewName { get; set; }
        public UserControl PreviousView { get; set; }
        public Type PreviousViewType { get; set; }
        public string PreviousViewName { get; set; }

        public List<NavigationFunctionRecord> FunctionList { get; private set; }


        #region Previous View Queue (Non-disposable views/vms)
        public List<UserControl> ViewList { get; private set; }

        public void AddToViewList(UserControl view)
        {
            ViewList.Add(view);

            // If the queue exceeds the maximum size, remove the oldest item
            if (ViewList.Count > Slots)
            {
                ViewList.RemoveAt(0);
            }
        }

        #endregion

        #region Previous ViewModel Queue (Non-disposable views/vms)
        public List<IViewModel> ViewModelList { get; private set; }

        public void AddtoViewModelList(IViewModel viewModel)
        {
            ViewModelList.Add(viewModel);

            // If the queue exceeds the maximum size, remove the oldest item
            if (ViewModelList.Count > Slots)
            {
                ViewModelList.RemoveAt(0);
            }
        }

        #endregion


        #region Previous View Type Queue (Disposable views/vms)
        public List<Type> ViewTypeList { get; private set; }

        public void AddToViewTypeList(Type type)
        {
            ViewTypeList.Add(type);

            // If the queue exceeds the maximum size, remove the oldest item
            if (ViewTypeList.Count > Slots)
            {
                ViewTypeList.RemoveAt(0);
            }
        }

        #endregion

        #region Previous View Name Queue (Disposable views/vms)
        public List<string> ViewNameList { get; private set; }

        public void AddToViewNameList(string name)
        {
            ViewNameList.Add(name);

            // If the queue exceeds the maximum size, remove the oldest item
            if (ViewNameList.Count > Slots)
            {
                ViewNameList.RemoveAt(0);
            }
        }

        #endregion

        #region Previous ViewModel Type Queue (Disposable views/vms)
        public List<Type> PreviousViewModelTypes { get; private set; }


        public void AddToViewModelTypeList(Type type)
        {
            PreviousViewModelTypes.Add(type);

            // If the queue exceeds the maximum size, remove the oldest item
            if (PreviousViewModelTypes.Count > Slots)
            {
                PreviousViewModelTypes.RemoveAt(0);
            }
        }

        #endregion

        public NavigationStore()
        {
            NextIndex = 0;
            Slots = 5;

            MultiUseViewsAndVms = true;
            LooseViews = LooseViews.PerView; //true;
            _looseViewSubFolder = "default";

            //Used to get views/vm
            ViewByViewModelType = new Dictionary<Type, UserControl>();
            ViewModelByType = new Dictionary<Type, ViewModelBase>();

            //Used to bind views to vms
            ViewTypeByViewModelType = new Dictionary<Type, Type>();
            ViewNamesByVM = new Dictionary<Type, string>();

            //Used to bind vms to views
            ViewModelTypeByViewType = new Dictionary<Type, Type>();
            ViewModelTypeByViewName = new Dictionary<string, Type>();

            //for per view loose status checking
            ViewPrefersLooseByType = new HashSet<Type>();
            ViewPrefersLooseByName = new HashSet<string>();

            ViewTypesByViewName = new Dictionary<string, Type>();
            ViewNamesByViewType = new Dictionary<Type, string>();

            ViewModelList = new List<IViewModel>();
            PreviousViewModelTypes = new List<Type>();

            ViewList = new List<UserControl>();
            ViewTypeList = new List<Type>();

            ViewNameList = new List<string>();

            ViewByName = new Dictionary<string, UserControl>();

            FunctionList = new List<NavigationFunctionRecord>();

            PopulateDictionaries();
        }

        public void RaiseChanged()
        {
            Changed?.Invoke();
        }

        public UserControl GetView(Type t)
        {
            if (ViewByViewModelType.ContainsKey(t))
            {
                return ViewByViewModelType[t];
            }

            UserControl view = null;

            if (LooseViews == LooseViews.All ||
                (LooseViews == LooseViews.PerView && ViewPrefersLooseByType.Contains(t)))
            {
                if (!ViewNamesByVM.ContainsKey(t))
                {
                    return null;
                }

                FileStream s = new FileStream(App.MainDirectoryPath + "\\Views\\Loose\\" + _looseViewSubFolder + "\\" + ViewNamesByVM[t] +  ".xaml", FileMode.Open);
                view = XamlReader.Load(s) as UserControl;
                s.Close();
            }
            else //_looseViews == LooseViews.None || _looseViews == LooseViews.PerView && !ViewIsLoose.Contains(t))
            {
                //Views not present in AppServices, and don't need to be
                view = Activator.CreateInstance(t) as UserControl; 
            }

            if (view != null)
            {
                if (MultiUseViewsAndVms)
                {
                    ViewByViewModelType.Add(t, view);
                }
                return view;
            }

            return null;
        }
        public UserControl GetViewByName(string name)
        {
            Type t = null;
            ViewTypesByViewName.TryGetValue(name, out t);

            //already in multi use store? just take it from there
            if (ViewByName.ContainsKey(name))
            {
                return ViewByName[name];
            }

            UserControl view = null;

            //get loose version
            if (LooseViews == LooseViews.All ||
                (LooseViews == LooseViews.PerView && 
                ViewPrefersLooseByName.Contains(name)))
            {
                FileStream s = new FileStream(App.MainDirectoryPath + "\\Views\\Loose\\" + _looseViewSubFolder + "\\" + name + ".xaml", FileMode.Open);
                view = XamlReader.Load(s) as UserControl;                
                s.Close();
            }

            //get embedded version 
            //if (view == null &&
            //    t != null &&
            //    ViewModelTypeByViewType.ContainsKey(t))
            //{
            //    view = ViewByVM[ViewModelTypeByViewType[t]];
            //}

            if (view == null)
            {
                //get embedded from dict
                //really necessary?
                //if (t != null &&
                //    ViewModelTypeByViewType.ContainsKey(t))
                //{
                //    view = ViewByVM[ViewModelTypeByViewType[t]];
                //}
                ////create new view
                //else
                {
                    view = Activator.CreateInstance(t) as UserControl;
                }
            }


            if (view != null)
            {
                if (MultiUseViewsAndVms)
                {
                    ViewByName.Add(name, view);
                }
                //return view;
            }

            //return null;

            return view;
        }
        public UserControl GetViewByViewModelType(Type t)
        {
            if (ViewByViewModelType.ContainsKey(t))
            {
                return ViewByViewModelType[t];
            }

            UserControl view = null;

            //A view can have both built-in and loose versions, here we determine which is the preferred as per settings
            bool preferLoose = false;
            if (LooseViews == LooseViews.PerView)
            {
                bool looseFound = ViewNamesByVM.ContainsKey(t);
                bool builtInFound = ViewTypeByViewModelType.ContainsKey(t);

                if (builtInFound)
                {
                    Type viewType = ViewTypeByViewModelType[t];
                    preferLoose = ViewPrefersLooseByType.Contains(viewType);
                }
                else if (looseFound)
                {
                    string viewType = ViewNamesByVM[t];
                    preferLoose = ViewPrefersLooseByName.Contains(viewType);
                }
                else
                {
                    return null;
                }
            }

            if (LooseViews == LooseViews.None ||
                (LooseViews == LooseViews.PerView && !preferLoose))
            {
                //get view type from viewmodeltype, then create that type
                if (!ViewTypeByViewModelType.ContainsKey(t))
                {
                    return null;
                }
                Type viewType = ViewTypeByViewModelType[t];

                view = Activator.CreateInstance(viewType) as UserControl;
            }
            else //LooseViews.All || (LooseViews.PerView && isLoose)
            {
                if (!ViewNamesByVM.ContainsKey(t))
                {
                    return null;
                }

                FileStream s = new FileStream(App.MainDirectoryPath + "\\Views\\Loose\\" + _looseViewSubFolder + "\\" + ViewNamesByVM[t] + ".xaml", FileMode.Open);
                view = XamlReader.Load(s) as UserControl;
                s.Close();

                //you could do this, but the events won't work
                //UserControl view2 = Activator.CreateInstance(viewType) as UserControl;
                //view2.Content = view;
            }

            if (view != null)
            {
                if (MultiUseViewsAndVms)
                {
                    ViewByViewModelType.Add(t, view);
                }                
            }

            return view;
        }

        public ViewModelBase GetViewModel(Type t)
        {
            if (ViewModelByType.ContainsKey(t))
            {
                return ViewModelByType[t];
            }

            ViewModelBase vm = AppServices.Instance.GetService(t) as ViewModelBase;

            if (vm != null)
            {
                if (MultiUseViewsAndVms)
                {
                    ViewModelByType.Add(t, vm);
                }
                return vm;
            }

            return null;
        }
        public ViewModelBase GetViewModelByView(Type t)
        {
            if (!ViewModelTypeByViewType.ContainsKey(t))
                return null;

            return GetViewModel(ViewModelTypeByViewType[t]);
        }
        public ViewModelBase GetViewModelByViewName(string name)
        {
            if (!ViewModelTypeByViewName.ContainsKey(name))
                return null;

            return GetViewModel(ViewModelTypeByViewName[name]);
        }



        public UserControl LoadDialogView(string viewName, Type viewType)
        {
            //if (!Settings.Preferences.Default.ThemesUseCustom)
            //{
            //    return (UserControl)Activator.CreateInstance(viewType);
            //}

            //var theme = Settings.Preferences.Default.ThemeSettings.Themes.FirstOrDefault(x => x.InUse);

            //if (theme == null)
            //    return (UserControl)Activator.CreateInstance(viewType);

            //try
            //{
            //    FileStream s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + theme.Layout + "\\dialogs\\" + viewName + ".xaml", FileMode.Open);
            //    UserControl view = (UserControl)XamlReader.Load(s);
            //    s.Close();
            //    return view;
            //}
            //catch (Exception ex)
            //{
            //    string path = App.MainDirectoryPath + "Layouts\\" + theme.Layout + "\\dialogs\\" + viewName + ".xaml";
            //    log.Error("Could not load view from path " + path, ex);

            //    MessageBox.Show(App.Instance.MainWindow,
            //                    string.Format("Could not load view from path " + path, ex.ToString()),
            //                    "XAML Loading from file",
            //                    MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            return null;
        }


        private void PopulateDictionaries()
        {
            //BUILT IN
            //get view by vm
            //it can be the case that 2 or more views share the same viewmodel, in this case when switching based on vm, only one can be chosen. that should be set here.
            ViewTypeByViewModelType.Add(typeof(InitViewModel), typeof(InitView));
            ViewTypeByViewModelType.Add(typeof(TestFirstViewModel), typeof(TestFirstView));

            //get vm by view
            //it can be the case that 2 or more views share the same viewmodel, in this case when switching based on view, all of these views should be set here
            ViewModelTypeByViewType.Add(typeof(InitView), typeof(InitViewModel));
            ViewModelTypeByViewType.Add(typeof(TestFirstView), typeof(TestFirstViewModel));

            //whether to load as embedded or loose (LooseViews property in this class)
            //should match with ViewPrefersLooseByName
            ViewPrefersLooseByType.Add(typeof(InitView));
            ViewPrefersLooseByType.Add(typeof(TestFirstView));

            //Used in view-based navigation (built-in only I guess?)
            ViewTypesByViewName.Add("InitView", typeof(InitView));
            ViewTypesByViewName.Add("TestFirstView", typeof(TestFirstView));

            //when changing view by type, and we want to set name also
            ViewNamesByViewType.Add(typeof(InitView), "InitView");
            ViewNamesByViewType.Add(typeof(TestFirstView), "TestFirstView");


            //LOOSE
            //get viewname by vm
            //it can be the case that 2 or more views share the same viewmodel, in this case when switching based on vm, only one can be chosen. that should be set here.
            ViewNamesByVM.Add(typeof(InitViewModel), "InitView");
            ViewNamesByVM.Add(typeof(TestFirstViewModel), "TestFirstView");
            ViewNamesByVM.Add(typeof(NavTest1ViewModel), "NavTest1View");
            ViewNamesByVM.Add(typeof(HubViewModel), "HubView");
            ViewNamesByVM.Add(typeof(ChartViewModel), "ChartView");
            ViewNamesByVM.Add(typeof(IntroductionViewModel), "IntroductionView");
            ViewNamesByVM.Add(typeof(LogViewModel), "LogView");

            //get vm by view name
            //it can be the case that 2 or more views share the same viewmodel, in this case when switching based on view, all of these views should be set here
            ViewModelTypeByViewName.Add("InitView", typeof(InitViewModel));
            ViewModelTypeByViewName.Add("TestFirstView", typeof(TestFirstViewModel));
            ViewModelTypeByViewName.Add("NavTest1View", typeof(NavTest1ViewModel));
            ViewModelTypeByViewName.Add("NavTest2View", typeof(NavTest1ViewModel));
            ViewModelTypeByViewName.Add("HubView", typeof(HubViewModel));
            ViewModelTypeByViewName.Add("ChartView", typeof(ChartViewModel));
            ViewModelTypeByViewName.Add("IntroductionView", typeof(IntroductionViewModel));
            ViewModelTypeByViewName.Add("LogView", typeof(LogViewModel));

            //whether to load as embedded or loose (LooseViews property in this class)
            //should match with ViewPrefersLoose
            //could remove ViewPrefersLoose completely, and just add them here with Type.Name instead.
            ViewPrefersLooseByName.Add("InitView");
            ViewPrefersLooseByName.Add("TestFirstView");
            ViewPrefersLooseByName.Add("NavTest1View");
            ViewPrefersLooseByName.Add("NavTest2View");
            ViewPrefersLooseByName.Add("HubView");
            ViewPrefersLooseByName.Add("ChartView");
            ViewPrefersLooseByName.Add("IntroductionView");
            ViewPrefersLooseByName.Add("LogView");


        }
    
    }
    
}
