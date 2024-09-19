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
        Task<IEnumerable<EnergyMinAvgRecord>> GetEnergyMinAvg(DateTime start, DateTime end);
        void AddAccount(AccountRecord account);

        void UpdateAccount(AccountRecord account);

        void DeleteAccountViaId(int id);
        Task<AccountRecord> GetAccountViaId(int id);
        Task<IEnumerable<AccountRecord>> GetAllAccounts();
        Task<IEnumerable<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize);
        Task<int> GetAccountsCountAsync();
    }
}
