using DemoApp.Services;
using DemoApp.Stores;
using DemoAppDatabase.Model;
using DemoAppDatabase.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DemoApp.ViewModels
{    
    public class CrudViewModel : ActivityViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IDatabaseService _databaseService;
        public ObservableCollection<AccountRecord> VisibleAccounts { get; set; }

        private DataGrid _accountGrid;
        private ScrollViewer _scrollViewer;
        private bool _loadingAccounts;
        private int _pageSize;
        private int _currentPage;
        private int _loadingData;
        private double _prevExtentHeight;
        private double _prevViewPortHeight;
        private double _prevVerticalOffset;

        private DispatcherTimer _backOffTimer;
        bool _backOff;

        #region ICommand

        //RelayCommands work like in Locker AFAIK
        //DelegateCommand is similar, but doesn't test command availabiltiy automatically.
        //Also easy usage of types with <T>

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new DelegateCommand<int>(Refresh);
                }
                return _refreshCommand;
            }
        }

        private ICommand _updateCommand;
        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new DelegateCommand<int>(Update);
                }
                return _updateCommand;
            }
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new DelegateCommand<int>(Delete);
                }
                return _deleteCommand;
            }
        }

        private ICommand _datagridLoadedCommand;
        public ICommand DatagridLoadedCommand
        {
            get
            {
                if (_datagridLoadedCommand == null)
                {
                    _datagridLoadedCommand = new DelegateCommand<RoutedEventArgs>(DatagridLoaded);
                }
                return _datagridLoadedCommand;
            }
        }

        #endregion


        public CrudViewModel(IActivityStore activityStore,
                             IDatabaseService databaseService) : base(activityStore)
        {
            _databaseService = databaseService;

            _pageSize = 20;
            _currentPage = 0;
            _loadingData = 0;

            VisibleAccounts = new ObservableCollection<AccountRecord>();

            _backOffTimer = new DispatcherTimer();
            _backOffTimer.Interval = TimeSpan.FromMilliseconds(50);
            _backOffTimer.Tick += _backOffTimer_Tick;
        }

        private void _backOffTimer_Tick(object sender, EventArgs e)
        {
            _backOffTimer.Stop();
            _backOff = false;
            _loadingData = 0;
        }

        public void GenerateAndAddAccounts(int n)
        {
            // Ensure _databaseService is initialized
            if (_databaseService == null)
            {
                throw new InvalidOperationException("_databaseService is not initialized.");
            }

            Parallel.For(1, n+1, (i) =>
            {
                // Create unique AccountName and Email
                string uniqueAccountName = $"ExampleAccount{i}";
                string uniqueEmail = $"user{i}@mail.net";

                // Create and add the account
                _databaseService.AddAccount(new AccountRecord
                {
                    AccountName = uniqueAccountName,
                    Email = uniqueEmail,
                    PasswordHash = "sameshit", // Example password hash
                });
            });
        }

        private ScrollViewer GetScrollViewer(DependencyObject obj)
        {
            if (obj is ScrollViewer)
            {
                return (ScrollViewer)obj;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void DatagridLoaded(RoutedEventArgs e)
        {
            _accountGrid = (DataGrid)e.Source;
            _scrollViewer = GetScrollViewer(e.Source as DependencyObject);
            _prevExtentHeight = _scrollViewer.ExtentHeight;
            _prevViewPortHeight = _scrollViewer.ViewportHeight;
            _prevVerticalOffset = _scrollViewer.VerticalOffset;

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {           
            if (_backOff)
            {
                _scrollViewer.ScrollToVerticalOffset(_prevVerticalOffset);
            }

            if ( e.VerticalOffset == e.ExtentHeight - e.ViewportHeight)
            {
                switch(_loadingData)
                {
                    case 0:
                        _loadingData = 1;

                        _prevExtentHeight = e.ExtentHeight;
                        _prevViewPortHeight = e.ViewportHeight;
                        _prevVerticalOffset = e.VerticalOffset;

                        _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                        await LoadMoreData();
                        _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;


                        break;
                    case 1:
                        _loadingData = 2;
                        _scrollViewer.ScrollToVerticalOffset(_prevVerticalOffset);
                        _backOff = true;
                        _backOffTimer.Start();
                        break;
                }
            }

           
        }

        private async Task<bool> LoadMoreData()
        {
            if (_loadingAccounts)
                return false;

            _loadingAccounts = true;
            List<AccountRecord> moreAccounts = null;

            try
            {
                moreAccounts = await _databaseService.GetAccountsAsync(_currentPage, _pageSize);


                foreach (var account in moreAccounts)
                {
                    VisibleAccounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            _currentPage++;
            _loadingAccounts = false;

            return moreAccounts != null && moreAccounts.Count > 0;
        }

        private void Refresh(int id)
        {
            var account = VisibleAccounts.FirstOrDefault(a => a.Id == id);
            int ind = VisibleAccounts.IndexOf(account);

            if (account != null)
            {
                account = _databaseService.GetAccountViaId(id);
                VisibleAccounts[ind] = account;

                account.OnPropertyChanged(nameof(VisibleAccounts));
            }
        }

        private async void Update(int id)
        {
            var account = VisibleAccounts.FirstOrDefault(a => a.Id == id);
            if (account != null)
            {
                await _databaseService.UpdateAccountSingleFast(account);
            }
        }

        private void Delete(int id)
        {
            var account = VisibleAccounts.FirstOrDefault(a => a.Id == id);

            if (account != null)
            {
                VisibleAccounts.Remove(account);
            }

            //_databaseService.DeleteAccountViaId(id);
        }

        public override void OnEnter()
        {
            //GenerateAndAddAccounts(100);
            //var t = Task.Run(() =>
            //{
            //    string pwHash = EncryptionService.ComputeSha256Hash("password123");
            //    _databaseService.AddAccountSingleFast(new AccountRecord
            //    {
            //        AccountName = "ExampleAccount1",
            //        Email = "johndoe@mail.net",
            //        PasswordHash = pwHash,
            //        FirstName = "John"
            //    });

            //});
            //t.Wait();


            //VisibleAccounts.Clear();
            //var tempAccounts = _databaseService.GetAllAccounts();

            //foreach (AccountRecord account in tempAccounts)
            //{
            //    VisibleAccounts.Add(account);
            //}
            //OnPropertyChanged(nameof(VisibleAccounts));

            if (VisibleAccounts.Count <= 0)
            {
                LoadMoreData();
            }

            base.OnEnter();
        }

        public override void OnExit()
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }

            base.OnExit();
        }
    }
}
