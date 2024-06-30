using DemoApp.Config;
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

        public event Action Changed;

        //whether new views and vms are created for each view change
        public bool MultiUseViewsAndVms;

        //load loose xamls or from inside the assembly
        public LooseViews LooseViews;
        private string _looseViewSubFolder;

        //all views and viewmodels by type
        public Dictionary<Type, UserControl> ViewByVM { get; private set; }
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

        //get view type by name
        public Dictionary<string, Type> ViewTypesByViewName { get; set; }

        public Type PreviousViewModelType { get; set; }
        public IViewModel CurrentViewModel { get; set; }
        public IViewModel PreviousViewModel { get; set; }
       
        public UserControl CurrentView { get; set; }
        public Type CurrentViewType { get; set; }
        public string CurrentViewName { get; set; }
        public UserControl PreviousView { get; set; }
        public Type PreviousViewType { get; set; }
        public string PreviousViewName { get; set; }

        #region Previous View Queue (Non-disposable views/vms)
        private readonly List<UserControl> _previousViews;
        private readonly int _previousViewsMaxSize;
        public IReadOnlyCollection<UserControl> PreviousViews => _previousViews;

        public void AddToPreviousViewQueue(UserControl view)
        {
            _previousViews.Add(view);

            // If the queue exceeds the maximum size, remove the oldest item
            if (_previousViewModels.Count > _previousViewsMaxSize)
            {
                _previousViews.RemoveAt(0);
            }
        }

        #endregion

        #region Previous View Type Queue (Disposable views/vms)
        private readonly List<Type> _previousViewTypes;
        private readonly int _previousViewTypesMaxSize;
        public IReadOnlyCollection<Type> PreviousViewTypes => _previousViewTypes;

        public void AddToPreviousViewTypeQueue(Type type)
        {
            _previousViewTypes.Add(type);

            // If the queue exceeds the maximum size, remove the oldest item
            if (_previousViewTypes.Count > _previousViewTypesMaxSize)
            {
                _previousViewTypes.RemoveAt(0);
            }
        }

        #endregion

        #region Previous View Name Queue (Disposable views/vms)
        private readonly List<string> _previousViewNames;
        private readonly int _previousViewNamesMaxSize;
        public IReadOnlyCollection<string> PreviousViewNames => _previousViewNames;

        public void AddToPreviousViewNameQueue(string name)
        {
            _previousViewNames.Add(name);

            // If the queue exceeds the maximum size, remove the oldest item
            if (_previousViewNames.Count > _previousViewNamesMaxSize)
            {
                _previousViewNames.RemoveAt(0);
            }
        }

        #endregion

        #region Previous ViewModel Queue (Non-disposable views/vms)
        private readonly List<IViewModel> _previousViewModels;
        private readonly int _previousVmsMaxSize;
        public IReadOnlyCollection<IViewModel> PreviousViewModels => _previousViewModels;

        public void AddToPreviousVmQueue(IViewModel viewModel)
        {
            _previousViewModels.Add(viewModel);

            // If the queue exceeds the maximum size, remove the oldest item
            if (_previousViewModels.Count > _previousVmsMaxSize)
            {
                _previousViewModels.RemoveAt(0);
            }
        }

        #endregion

        #region Previous ViewModel Type Queue (Disposable views/vms)
        private readonly List<Type> _previousViewModelTypes;
        private readonly int _previousVmTypesMaxSize;
        public IReadOnlyCollection<IViewModel> PreviousViewModelTypes => _previousViewModels;

        public void AddToPreviousVmTypeQueue(Type type)
        {
            _previousViewModelTypes.Add(type);

            // If the queue exceeds the maximum size, remove the oldest item
            if (_previousViewModelTypes.Count > _previousVmTypesMaxSize)
            {
                _previousViewModelTypes.RemoveAt(0);
            }
        }

        #endregion

        public NavigationStore()
        {
            MultiUseViewsAndVms = true;
            LooseViews = LooseViews.PerView; //true;
            _looseViewSubFolder = "default";

            //Used to get views/vm
            ViewByVM = new Dictionary<Type, UserControl>();
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

            _previousViewModels = new List<IViewModel>();
            _previousViewModelTypes = new List<Type>();

            _previousVmsMaxSize = 5; //TODO: replace with settings
            _previousVmTypesMaxSize = 5; //TODO: replace with settings

            _previousViews = new List<UserControl>();
            _previousViewTypes = new List<Type>();
            _previousViewsMaxSize = 5; //TODO: replace with settings
            _previousViewTypesMaxSize = 5; //TODO: replace with settings

            _previousViewNames = new List<string>();
            _previousViewNamesMaxSize = 5; //TODO: replace with settings

            ViewByName = new Dictionary<string, UserControl>();

            PopulateDictionaries();
        }

        public void RaiseChanged()
        {
            Changed?.Invoke();
        }

        //this seems outdated...
        public UserControl GetView(Type t)
        {
            if (ViewByVM.ContainsKey(t))
            {
                return ViewByVM[t];
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
                    ViewByVM.Add(t, view);
                }
                return view;
            }

            return null;
        }

        public UserControl GetViewByName(string name)
        {
            Type t = null;
            ViewTypesByViewName.TryGetValue(name, out t);

            if (ViewByName.ContainsKey(name))
            {
                return ViewByName[name];
            }

            UserControl view = null;

            if (LooseViews == LooseViews.All ||
                (LooseViews == LooseViews.PerView && 
                ViewPrefersLooseByName.Contains(name)))
            {
                FileStream s = new FileStream(App.MainDirectoryPath + "\\Views\\Loose\\" + _looseViewSubFolder + "\\" + name + ".xaml", FileMode.Open);
                view = XamlReader.Load(s) as UserControl;                
                s.Close();
            }

            if (t != null &&
                ViewModelTypeByViewType.ContainsKey(t))
            {
                view = ViewByVM[ViewModelTypeByViewType[t]];
            }

            if (view != null)
            {
                if (MultiUseViewsAndVms)
                {
                    ViewByName.Add(name, view);
                }
                return view;
            }

            return null;
        }

        //TODO: some views might be loose, some might not. instead of _looseViews, use a dict where each vm has a bool 'loose'
        public UserControl GetViewByVm(Type t)
        {
            object view = null;

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
                if (!ViewTypeByViewModelType.ContainsKey(t))
                {
                    return null;
                }

                Type viewType = ViewTypeByViewModelType[t];

                if (ViewByVM.ContainsKey(viewType))
                {
                    return ViewByVM[viewType];
                }

                //Views not present in AppServices, and don't need to be
                view = Activator.CreateInstance(viewType);
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
                //Don't know the exact reason for this, but let's not touch it for now
                if (ViewByVM.ContainsKey(t))
                    ViewByVM.Remove(t);

                if (MultiUseViewsAndVms)
                {
                    ViewByVM.Add(t, view as UserControl);
                }
                return view as UserControl;
            }

            return null;
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

        //Will be loaded one by one as they become necessary, so this will be unused
        public void LoadLayout(string layout)
        {
            try
            {
                FileStream s = null;
                UserControl view = null;

                ViewByVM.Clear();

                //alternate way of getting behaviour assembly working in xaml
                //Assembly assembly = Assembly.LoadFrom("Microsoft.Xaml.Behaviors.dll");

                #region Views

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\WaitForUserView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(WaitForUserView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\WaitForAdminView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(WaitForAdminView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\AdminMenuView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(AdminMenuView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\HelpView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(HelpView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\LanguageSelectionView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(LanguageSelectionView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\ReconnectDevicesView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(ReconnectDevicesView), view);

                ////s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\CheckedOutView.xaml", FileMode.Open);
                ////view = (UserControl)XamlReader.Load(s);
                ////s.Close();
                ////Views.Add(typeof(CheckedOutView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\CheckedOutStaffView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(CheckedOutStaffView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\WaitForCheckInView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(WaitForCheckInView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\CheckInView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(CheckInView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\ExpiredItemsView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(ExpiredItemsView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\LockerAdminView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(LockerAdminView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\CheckInMenuView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(CheckInMenuView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\ManualCheckOutView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(ManualCheckOutView), view);


                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\OutOfOrderView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(OutOfOrderView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\ReadErrorView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(ReadErrorView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\RemoveCardView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(RemoveCardView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\NoReservationsView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(NoReservationsView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\InvalidPatronView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(InvalidPatronView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\ReservationCheckInMenuView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(ReservationCheckInMenuView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\BrowseCheckInMenuView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(BrowseCheckInMenuView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\MixedCheckInMenuView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(MixedCheckInMenuView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\AcceptTermsView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(AcceptTermsView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\MainMenuView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(MainMenuView), view);

                ////This should not be commented out, unless the workaround fix for Bieberach is needed
                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\BrowseView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(BrowseView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\ReadPatronCardPinView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(ReadPatronCardPinView), view);

                //s = new FileStream(App.MainDirectoryPath + "\\Layouts\\" + layout + "\\OfflineTransactionsProcessingView.xaml", FileMode.Open);
                //view = (UserControl)XamlReader.Load(s);
                //s.Close();
                //Views.Add(typeof(OfflineTransactionsProcessingView), view);
                #endregion

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
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



            //LOOSE
            //get viewname by vm
            //it can be the case that 2 or more views share the same viewmodel, in this case when switching based on vm, only one can be chosen. that should be set here.
            ViewNamesByVM.Add(typeof(InitViewModel), "InitView");
            ViewNamesByVM.Add(typeof(TestFirstViewModel), "TestFirstView");
            ViewNamesByVM.Add(typeof(NavTest1ViewModel), "NavTest1View");
            ViewNamesByVM.Add(typeof(HubViewModel), "HubView");

            //get vm by view name
            //it can be the case that 2 or more views share the same viewmodel, in this case when switching based on view, all of these views should be set here
            ViewModelTypeByViewName.Add("InitView", typeof(InitViewModel));
            ViewModelTypeByViewName.Add("TestFirstView", typeof(TestFirstViewModel));
            ViewModelTypeByViewName.Add("NavTest1View", typeof(NavTest1ViewModel));
            ViewModelTypeByViewName.Add("NavTest2View", typeof(NavTest1ViewModel));
            ViewModelTypeByViewName.Add("HubView", typeof(HubViewModel));

            //whether to load as embedded or loose (LooseViews property in this class)
            //should match with ViewPrefersLoose
            //could remove ViewPrefersLoose completely, and just add them here with Type.Name instead.
            ViewPrefersLooseByName.Add("InitView");
            ViewPrefersLooseByName.Add("TestFirstView");
            ViewPrefersLooseByName.Add("NavTest1View");
            ViewPrefersLooseByName.Add("NavTest2View");
            ViewPrefersLooseByName.Add("HubView");


            //Used in view-based navigation
            ViewTypesByViewName.Add("InitView", typeof(InitView));
            ViewTypesByViewName.Add("TestFirstView", typeof(TestFirstView));
        }
    
    }
    
}
