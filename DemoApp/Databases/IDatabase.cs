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
        Task<bool> AddOrUpdateEnergyMinAvg(DateTime timestamp, double average);
        Task<IEnumerable<EnergyMinAvgRecord>> GetEnergyMinAvg(DateTime start, DateTime end);



        Task<bool> AddAccount(AccountRecord account);

        Task<bool>UpdateAccount(AccountRecord account);


        Task<bool> DeleteAccountViaId(int id);

        Task<AccountRecord> GetAccountViaId(int id);

        Task<IEnumerable<AccountRecord>> GetAllAccounts();
        Task<IEnumerable<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize);
        Task<int> GetAccountsCountAsync();
    }
}
