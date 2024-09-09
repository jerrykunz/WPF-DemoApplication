using DemoAppDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Databases
{
    public interface IDatabase
    {
        string Name { get; }
        bool AddOrUpdateEnergyMinAvg(DateTime timestamp, double average);
        IEnumerable<EnergyMinAvgRecord> GetEnergyMinAvg(DateTime start, DateTime end);



        bool AddAccount(AccountRecord account);
        Task<bool> AddAccountSingleFast(AccountRecord account);

        bool UpdateAccount(AccountRecord account);
        Task<bool> UpdateAccountSingleFast(AccountRecord account);


        bool DeleteAccountViaId(int id);
        bool DeleteAccountViaAccountNameHash(string accountNameHash);
        bool DeleteAccountViaEmailHash(string emailHash);

        AccountRecord GetAccountViaId(int id);
        AccountRecord GetAccountViaAccountNameHash(string accountNameHash);
        AccountRecord GetAccountViaAccountEmailHash(string emailHash);

        IEnumerable<AccountRecord> GetAllAccounts();
        Task<List<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize);
    }
}
