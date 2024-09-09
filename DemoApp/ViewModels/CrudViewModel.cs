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

namespace DemoApp.ViewModels
{    
    public class CrudViewModel : ActivityViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IDatabaseService _databaseService;
        public ObservableCollection<AccountRecord> VisibleAccounts { get; set; }

        private ScrollViewer _scrollViewer;
        private bool _loadingAccounts;
        private int _pageSize;
        private int _currentPage;


        #region ICommand

        //RelayCommands work like in Locker AFAIK
        //DelegateCommand is similar, but doesn't test command availabiltiy automatically.
        //Also easy usage of types with <T>

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

            VisibleAccounts = new ObservableCollection<AccountRecord>();
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
            _scrollViewer = GetScrollViewer(e.Source as DependencyObject);
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Check if vertical scroll is at the end
            if (e.VerticalOffset == e.ExtentHeight - e.ViewportHeight)
            {
                //only load more data if there is more data to load... get the max number of accounts from db with a new func

                var a = 3;
                //LoadMoreData();
            }
        }

        private async void LoadMoreData()
        {
            if (_loadingAccounts)
                return;

            _loadingAccounts = true;

            try
            {
                var moreAccounts = await _databaseService.GetAccountsAsync(_currentPage, _pageSize);
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

            LoadMoreData();

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
