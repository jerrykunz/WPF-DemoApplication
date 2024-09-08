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
        bool DeleteAccount(int id);
        bool UpdateAccount(AccountRecord account);
        IEnumerable<AccountRecord> GetAllAccounts();
    }
}
