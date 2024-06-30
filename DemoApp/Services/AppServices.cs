using DemoApp.Stores;
using DemoApp.ViewModels;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Services
{

    public interface IComponentContext
    {
        object GetService(Type t);
        T GetService<T>();

        T GetService<T>(bool isRequired);
    }
    public class AppServices : IDisposable, IComponentContext
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static AppServices _instance;
        private static readonly object _instanceLock = new object();
        private static AppServices GetInstance()
        {
            lock (_instanceLock)
            {
                return _instance ?? (_instance = new AppServices());
            }
        }

        private IServiceScope _serviceScope = null;

        public static AppServices Instance => _instance ?? GetInstance();
        private static IServiceProvider _serviceProvider;

        private AppServices()
        {
            _serviceProvider = Configure();
            _serviceScope = _serviceProvider.CreateScope();
        }

        private IServiceProvider Configure()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IComponentContext>(this);

            //Main Window
            services.AddTransient<MainWindow>();
            services.AddTransient<FlatUIWindow>();

            //Viewmodels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<InitViewModel>();
            services.AddTransient<TestFirstViewModel>();
            services.AddTransient<NavTest1ViewModel>();
            services.AddTransient<HubViewModel>();

            //Stores
            services.AddSingleton<IActivityStore, ActivityStore>();
            services.AddSingleton<INavigationStore, NavigationStore>();

            //Services
            services.AddSingleton<INavigator, Navigator>();


            //services.AddTransient<AdminMenuViewModel>();
            //services.AddTransient<CheckedOutStaffViewModel>();
            ////services.AddTransient<CheckedOutViewModel>();
            //services.AddTransient<CheckInMenuViewModel>();
            //services.AddTransient<CheckInViewModel>();
            //services.AddTransient<ExpiredItemsViewModel>();

            //services.AddTransient<HelpViewModel>();
            //services.AddTransient<InitViewModel>();
            //services.AddTransient<LanguageSelectionViewModel>();
            //services.AddTransient<LoadingViewModel>();
            //services.AddTransient<LockerAdminViewModel>();
            //services.AddTransient<ManualCheckOutViewModel>();
            //services.AddTransient<ReconnectDevicesViewModel>();
            //services.AddTransient<WaitForAdminViewModel>();
            //services.AddTransient<WaitForCheckInViewModel>();
            //services.AddTransient<WaitForUserViewModel>();

            //services.AddTransient<OutOfOrderViewModel>();

            //services.AddTransient<RemoveCardViewModel>();
            //services.AddTransient<ReadErrorViewModel>();
            //services.AddTransient<NoReservationsViewModel>();
            //services.AddTransient<InvalidPatronViewModel>();

            //services.AddTransient<ReservationCheckInMenuViewModel>();
            //services.AddTransient<BrowseCheckInMenuViewModel>();
            //services.AddTransient<MixedCheckInMenuViewModel>();

            //services.AddTransient<AcceptTermsViewModel>();
            //services.AddTransient<MainMenuViewModel>();
            //services.AddTransient<BrowseViewModel>();
            //services.AddTransient<ReadPatronCardPinViewModel>();
            //services.AddTransient<OfflineTransactionsProcessingViewModel>();

            //services.AddTransient<DesignationSetViewModel>();
            //services.AddTransient<DesignationLockerViewModel>();
            //services.AddTransient<DesignationLockerSelectViewModel>();

            ////separate window
            //services.AddTransient<OfflineAdminViewModel>();



            ////Stores
            //IStateStore stateStore = new StateStore();
            //services.AddSingleton<IStateStore>(stateStore);

            //services.AddSingleton<IPatronStore, PatronStore>();
            //services.AddSingleton<ILockerStore, LockerStore>();
            //services.AddSingleton<IActivityStore, ActivityStore>();
            //services.AddSingleton<INavigationStore, NavigationStore>();
            //services.AddSingleton<ITagStore, TagStore>();
            //services.AddSingleton<ITextStore, TextStore>();
            //services.AddSingleton<IAdminStore, AdminStore>();
            //services.AddSingleton<IDesignationStore, DesignationStore>();

            ////Devices

            ////Rfid reader
            //if (Settings.Devices.Default.ElatecInUse)
            //    services.AddSingleton<IRfidReader, ElatecReader>();
            //else
            //    services.AddSingleton<IRfidReader, RfidReaderNull>();

            ////Barcode reader
            //if (Settings.Devices.Default.NewlandInUse)
            //    services.AddSingleton<IBarcodeReader, NewlandScanner>();
            //else
            //    services.AddSingleton<IBarcodeReader, BarcodeReaderNull>();

            ////Smartcard reader
            //if (Settings.Devices.Default.Iso7816Settings.Read)
            //{
            //    try
            //    {
            //        var smartCardService = SmartCardHandlerServiceFactory.CreateSmartCardHandlerService(Settings.Devices.Default);
            //        var smartCardReader = new DeviceSmartCardEx(DeviceNames.SmartCardReader, smartCardService);
            //        services.AddSingleton<ISmartCardReader>(smartCardReader);
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Error(ex);
            //    }
            //}
            //else
            //{
            //    services.AddSingleton<ISmartCardReader, DeviceSmartCardNull>();
            //}

            ////Printer
            //if (Settings.Devices.Default.PrinterInUse)
            //    services.AddSingleton<IPrinter, Printer>();
            //else
            //    services.AddSingleton<IPrinter, DevicePrinterNull>();

            ////Lsu
            //services.AddSingleton<ILsuController, LsuController>();

            ////Tagmanager+rfid readers
            //if (Settings.Devices.Default.RfidReaderInUse)
            //    services.AddSingleton<IRfidReaderTm>(RfidReaderFactory.CreateRfidReader());
            //else
            //    services.AddSingleton<IRfidReaderTm>(new RfidReaderTmNull());

            //services.AddSingleton<IBinReaderController>(new BinReaderController());

            ////Services

            ////Navigation service
            //services.AddSingleton<INavigator, Navigator>();

            ////Primary services
            //services.AddSingleton<IAuthenticationService, AuthenticationService>();
            //services.AddSingleton<ICheckInService, CheckInService>();
            //services.AddSingleton<ICheckOutService, CheckOutService>();

            ////Secondary services
            //services.AddSingleton<ILockerService, LockerService>();
            //services.AddSingleton<IReaderService, ReaderService>();
            //services.AddSingleton<IPrintService, PrintService>();

            //var bmaClient = BmaClientFactory.GetClient();
            //services.AddSingleton<IServmanagerClient>(bmaClient);

            //var statusSenderList = new List<ISystemStatusSender>();

            //if (Settings.Data.Default.BmaSendSystemStatus)
            //{
            //    var bmaStatusSender = new BmaSystemStatusSender(bmaClient,
            //                                                    new ServmanagerCredential(Settings.Data.Default.BmaDeviceId,
            //                                                                              Settings.Data.Default.BmaApiKey));
            //    statusSenderList.Add(bmaStatusSender);
            //}

            //services.AddSingleton(l => { return Factories.LoggingFactory.CreateAndAddLog4NetAppenders(); });
            //services.AddSingleton<LogSender>();

            //if (Settings.Devices.Default.SysLogInUse)
            //{
            //    var sysLogStatusSender = new SysLogStatusSender();
            //    statusSenderList.Add(sysLogStatusSender);
            //}

            //services.AddSingleton<IEnumerable<ISystemStatusSender>>(statusSenderList);

            //services.AddSingleton<IStatusService, StatusService>();

            //if (Settings.Devices.Default.EmailUseSmtp)
            //{
            //    services.AddSingleton<IMailService, MailService>();
            //    log.Info("Using SMTP mailing service");
            //}
            //else
            //{
            //    //TODO: not sure if necessary?
            //    services.AddSingleton<IMailService, MailServiceNull>();
            //    log.Info("Using Null SMTP mailing service");
            //}

            ////Databases
            //IDatabaseService databaseService = DatabaseServiceFactory.CreateDatabaseService(bmaClient);
            //services.AddSingleton<IDatabaseService>(databaseService);

            //IDatabaseLocal localDatabase = databaseService.GetDb<IDatabaseLocal>();
            //services.AddSingleton<IDatabaseLocal>(localDatabase);

            //IDatabaseBma bmaDatabase = databaseService.GetDb<IDatabaseBma>();
            //services.AddSingleton<IDatabaseBma>(bmaDatabase);

            ////Circulation / LMS
            //var circulationServices = (ICirculationServices)CirculationServiceFactory.CreateCirculationService(databaseService,
            //                                                                                                   stateStore,
            //                                                                                                   Settings.Devices.Default.SipClient == SipConnectionType.Dummy);
            //services.AddSingleton<ICirculationServices>(circulationServices);

            //bool dummySierra = Settings.Devices.Default.SierraMode == SierraConnectionType.Dummy;
            //services.AddSingleton<ISierraApi>(SierraFactory.GetSierra(dummySierra));
            //log.Info("Using" + (dummySierra ? "dummy" : "normal") + "Sierra");

            //services.AddSingleton<IResourceManager>(r => { return Factories.ResourceManagerFactory.CreateResourceManager(!Settings.Data.Default.BmaSettings); });









            return services.BuildServiceProvider();
        }

        public object GetService(Type t)
        {
            return _serviceScope.ServiceProvider.GetRequiredService(t);
        }
        public T GetService<T>()
        {
            return GetService<T>(true);
        }

        public T GetService<T>(bool isRequired)
        {
            if (isRequired)
            {
                return _serviceScope.ServiceProvider.GetRequiredService<T>();
            }
            return _serviceScope.ServiceProvider.GetService<T>();
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_serviceScope != null)
                {
                    _serviceScope.Dispose();
                }
                _instance = null;
            }
        }
        #endregion
    }
}
