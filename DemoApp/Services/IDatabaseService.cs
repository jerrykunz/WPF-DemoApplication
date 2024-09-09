using DemoApp.Databases;
using DemoAppDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Services
{
    public interface IDatabaseService //: IDatabase
    {
        string Name { get; }
        T GetDatabase<T>();
        void AddOrUpdateEnergyMinAvg(DateTime timestamp, double average);
        IEnumerable<EnergyMinAvgRecord> GetEnergyMinAvg(DateTime start, DateTime end);
        void AddAccount(AccountRecord account);
        Task AddAccountSingleFast(AccountRecord account);

        void UpdateAccount(AccountRecord account);
        Task UpdateAccountSingleFast(AccountRecord account);


        void DeleteAccountViaId(int id);
        void DeleteAccountViaAccountNameHash(string accountNameHash);
        void DeleteAccountViaEmailHash(string emailHash);

        AccountRecord GetAccountViaId(int id);
        AccountRecord GetAccountViaAccountNameHash(string accountNameHash);
        AccountRecord GetAccountViaAccountEmailHash(string emailHash);
        IEnumerable<AccountRecord> GetAllAccounts();
        Task<List<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize);
    }
}
