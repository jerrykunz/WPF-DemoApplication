using DemoApp.Services;
using DemoApp.Stores;
using DemoAppDatabase.Model;
using DemoAppDatabase.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.ViewModels
{    
    public class CrudViewModel : ActivityViewModelBase
    {
        IDatabaseService _databaseService;

        public ObservableCollection<AccountRecord> Accounts { get; set; }
        public CrudViewModel(IActivityStore activityStore,
                             IDatabaseService databaseService) : base(activityStore)
        {
            _databaseService = databaseService;

            Accounts = new ObservableCollection<AccountRecord>();
        }

        public override void OnEnter()
        {
            string pwHash = EncryptionService.ComputeSha256Hash("password123");
            _databaseService.AddAccount(new AccountRecord
            {
                AccountName = "ExampleAccount1",
                Email = "johndoe@mail.net",
                PasswordHash = pwHash,
                FirstName = "John"
            });

            Accounts.Clear();
            var tempAccounts = _databaseService.GetAllAccounts();

            foreach (AccountRecord account in tempAccounts)
            {
                Accounts.Add(account);
            }
            OnPropertyChanged(nameof(Accounts));

            base.OnEnter();
        }
    }
}
