using DemoApp.Services;
using DemoApp.Stores;
using DemoAppDatabase.Model;
using DemoAppDatabase.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DemoApp.ViewModels
{    
    public class CrudViewModel : ActivityViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IDatabaseService _databaseService;
        public ObservableCollection<AccountRecord> Accounts { get; set; }
        public ICollectionView VisibleAccounts { get; set; }

        private DataGrid _accountGrid;
        private ScrollViewer _scrollViewer;
        private bool _loadingAccounts;
        private int _pageSize;
        private int _currentPage;

        private bool _loadingData;
        public bool LoadingData
        {
            get { return _loadingData; }
            set { _loadingData = value; OnPropertyChanged(nameof(LoadingData)); }
        }

        private string _idFilter;
        public string IdFilter
        {
            get { return _idFilter; }
            set { _idFilter = value; UpdateFilter(); }
        }


        private string _accountNameFilter;
        public string AccountNameFilter
        {
            get { return _accountNameFilter; }
            set { _accountNameFilter = value; UpdateFilter(); OnPropertyChanged(nameof(AccountNameFilter)); }
        }

        private string _emailFilter;
        public string EmailFilter
        {
            get { return _emailFilter; }
            set { _emailFilter = value; UpdateFilter(); }
        }

        private string _firstNameFilter;
        public string FirstNameFilter
        {
            get { return _firstNameFilter; }
            set { _firstNameFilter = value; UpdateFilter(); }
        }

        private string _familyNameFilter;
        public string FamilyNameFilter
        {
            get { return _familyNameFilter; }
            set { _familyNameFilter = value; UpdateFilter(); }
        }

        private string _phoneNumberFilter;
        public string PhoneNumberFilter
        {
            get { return _phoneNumberFilter; }
            set { _phoneNumberFilter = value; UpdateFilter(); }
        }

        private string _addressFilter;
        public string AddressFilter
        {
            get { return _addressFilter; }
            set { _addressFilter = value; UpdateFilter(); }
        }

        private string _zipcodeFilter;
        public string ZipcodeFilter
        {
            get { return _zipcodeFilter; }
            set { _zipcodeFilter = value; UpdateFilter(); }
        }

        private string _countryFilter;
        public string CountryFilter
        {
            get { return _countryFilter; }
            set { _countryFilter = value; UpdateFilter(); }
        }

        public bool FilteringData
        {
            get 
            {
                return !string.IsNullOrWhiteSpace(IdFilter) ||
                       !string.IsNullOrWhiteSpace(AccountNameFilter) ||
                       !string.IsNullOrWhiteSpace(EmailFilter) ||
                       !string.IsNullOrWhiteSpace(FirstNameFilter) ||
                       !string.IsNullOrWhiteSpace(FamilyNameFilter) ||
                       !string.IsNullOrWhiteSpace(PhoneNumberFilter) ||
                       !string.IsNullOrWhiteSpace(AddressFilter) ||
                       !string.IsNullOrWhiteSpace(ZipcodeFilter) ||
                       !string.IsNullOrWhiteSpace(CountryFilter);
            }
        }


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
                    _refreshCommand = new AsyncCommand<int>(Refresh);
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

        private ICommand _windowDragBlockCommand;
        public ICommand WindowDragBlockCommand
        {
            get
            {
                if (_windowDragBlockCommand == null)
                {
                    _windowDragBlockCommand = new RelayCommand(param => WindowDragBlock(param as ManipulationBoundaryFeedbackEventArgs),
                                                            param => (true));
                }
                return _windowDragBlockCommand;
            }
        }

        private ICommand _gridColumnsUpdateCommand;
        public ICommand GridColumnsUpdateCommand
        {
            get
            {
                if (_gridColumnsUpdateCommand == null)
                {
                    _gridColumnsUpdateCommand = new RelayCommand(param => UpdateGridColumns(param),
                                                            param => (true));
                }
                return _gridColumnsUpdateCommand;
            }
        }

        #endregion


        public CrudViewModel(IActivityStore activityStore,
                             IDatabaseService databaseService) : base(activityStore)
        {
            _databaseService = databaseService;

            _pageSize = 20;
            _currentPage = 0;
            LoadingData = false;

            Accounts = new ObservableCollection<AccountRecord>();
            VisibleAccounts = CollectionViewSource.GetDefaultView(Accounts);
        }

        private void UpdateFilter()
        {
            VisibleAccounts.Filter = FilterAccounts;
        }

        private bool FilterAccounts(object item)
        {
            if (item is AccountRecord account)
            {                

                if (!string.IsNullOrWhiteSpace(IdFilter) && !MatchesWildcardOrExact(account.AccountName, IdFilter))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(AccountNameFilter) && !MatchesWildcardOrExact(account.AccountName, AccountNameFilter))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(EmailFilter) && !MatchesWildcardOrExact(account.Email, EmailFilter))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(FirstNameFilter) && !MatchesWildcardOrExact(account.FirstName, FirstNameFilter))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(FamilyNameFilter) && !MatchesWildcardOrExact(account.FamilyName, FamilyNameFilter))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(PhoneNumberFilter) && !MatchesWildcardOrExact(account.FamilyName, FamilyNameFilter))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(AddressFilter) && !MatchesWildcardOrExact(account.FamilyName, FamilyNameFilter))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(ZipcodeFilter) && !MatchesWildcardOrExact(account.FamilyName, FamilyNameFilter))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(CountryFilter) && !MatchesWildcardOrExact(account.FamilyName, FamilyNameFilter))
                {
                    return false;
                }

            }
            return true;
        }

        private bool MatchesWildcardOrExact(string input, string pattern)
        {
            if (pattern.Contains('*'))
            {
                string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
            }
            else
            {
                return string.Equals(input, pattern, StringComparison.OrdinalIgnoreCase);
            }
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

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }

            UpdateGridColumns(e.Source);
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //don't load data while filtering
            if (FilteringData)
                return;


            if (e.VerticalOffset == e.ExtentHeight - e.ViewportHeight)
            {
                switch(LoadingData)
                {
                    case false:

                        int count = await _databaseService.GetAccountsCountAsync();
                        if (Accounts.Count >= count)
                            return;

                        LoadingData = true;
                        await LoadMoreData();
                        LoadingData = false;
                        break;
                }
            }

           
        }

        private async Task<bool> LoadMoreData()
        {
            if (_loadingAccounts)
                return false;

            _loadingAccounts = true;
            IEnumerable<AccountRecord> moreAccounts = null;

            try
            {
                moreAccounts = await Task.Run(() => _databaseService.GetAccountsAsync(_currentPage, _pageSize));


                foreach (var account in moreAccounts)
                {
                    Accounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            _currentPage++;
            _loadingAccounts = false;

            return moreAccounts != null && moreAccounts.Count() > 0;
        }

        private async Task Refresh(int id)
        {
            var account = Accounts.FirstOrDefault(a => a.Id == id);
            int ind = Accounts.IndexOf(account);

            if (account != null)
            {
                account = await _databaseService.GetAccountViaId(id);
                Accounts[ind] = account;

                account.OnPropertyChanged(nameof(Accounts));
            }
        }

        private void Update(int id)
        {
            var account = Accounts.FirstOrDefault(a => a.Id == id);
            if (account != null)
            {
                _databaseService.UpdateAccount(account);
            }
        }

        private void Delete(int id)
        {
            var account = Accounts.FirstOrDefault(a => a.Id == id);

            if (account != null)
            {
                Accounts.Remove(account);
            }

            _databaseService.DeleteAccountViaId(id);
        }

        private void WindowDragBlock(ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        public void UpdateGridColumns(object o)
        {
            if (o == null)
                return;
            
            foreach (var col in ((DataGrid)o).Columns)
            {
                //if (col.Header as string == "Account")
                //{
                //    col.Width = DataGridLength.SizeToHeader;
                //    col.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                //    continue;
                //}

                col.Width = DataGridLength.SizeToHeader;
                col.Width = DataGridLength.Auto;
            }
        }

        public async override void OnEnter()
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

            if (Accounts.Count <= 0)
            {
                LoadingData = true;
                await LoadMoreData();
                LoadingData = false;
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
