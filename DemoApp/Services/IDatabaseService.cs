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
        void DeleteAccount(int id);
        void UpdateAccount(AccountRecord account);
        IEnumerable<AccountRecord> GetAllAccounts();
    }
}
