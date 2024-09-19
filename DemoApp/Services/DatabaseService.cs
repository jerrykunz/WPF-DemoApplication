using DemoApp.Databases;
using DemoApp.Id;
using DemoAppDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Services
{
    public class DatabaseService : IDatabaseService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Name { get { return ServiceNames.DatabaseService; } }

        private Dictionary<string, IDatabase> _databases { get; set; }
        public IReadOnlyDictionary<string, IDatabase> Databases
        {
            get { return _databases; }
        }

        public DatabaseService(Dictionary<string, IDatabase> databases)
        {
            _databases = databases;
        }

        public T GetDatabase<T>()
        {
            foreach (IDatabase d in Databases.Values)
            {
                if (d is T)
                    return (T)d;
            }

            return default;
        }

        public void AddOrUpdateEnergyMinAvg(DateTime timestamp, double average)
        {
            try
            {
                foreach (IDatabase db in _databases.Values)
                {
                    Task.Run(() =>
                    {
                        db.AddOrUpdateEnergyMinAvg(timestamp, average);
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public async Task<IEnumerable<EnergyMinAvgRecord>> GetEnergyMinAvg(DateTime start, DateTime end)
        {
            if (Databases.ContainsKey(DatabaseNames.SQLite))
            {
                return await Databases[DatabaseNames.SQLite].GetEnergyMinAvg(start, end);
            }

            return new List<EnergyMinAvgRecord>();
        }

        public void AddAccount(AccountRecord account)
        {
            try
            {
                foreach (IDatabase db in _databases.Values)
                {
                    Task.Run(() =>
                    {
                        db.AddAccount(account);
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DeleteAccountViaId(int id)
        {
            try
            {
                foreach (IDatabase db in _databases.Values)
                {
                    Task.Run(() =>
                    {
                        db.DeleteAccountViaId(id);
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

       

        public void UpdateAccount(AccountRecord account)
        {
            try
            {
                foreach (IDatabase db in _databases.Values)
                {
                    Task.Run(() =>
                    {
                        db.UpdateAccount(account);
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public async Task<IEnumerable<AccountRecord>> GetAllAccounts()
        {
            if (Databases.ContainsKey(DatabaseNames.SQLite))
            {
                return await Databases[DatabaseNames.SQLite].GetAllAccounts();
            }

            return new List<AccountRecord>();
        }

        public async Task<AccountRecord> GetAccountViaId(int id)
        {
            if (Databases.ContainsKey(DatabaseNames.SQLite))
            {
                return await Databases[DatabaseNames.SQLite].GetAccountViaId(id);
            }

            return null;
        }

        
        public async Task<IEnumerable<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize)
        {
            if (Databases.ContainsKey(DatabaseNames.SQLite))
            {
                return await Databases[DatabaseNames.SQLite].GetAccountsAsync(pageNumber, pageSize);
            }

            return new List<AccountRecord>();
        }

        public async Task<int> GetAccountsCountAsync()
        {
            if (Databases.ContainsKey(DatabaseNames.SQLite))
            {
                return await Databases[DatabaseNames.SQLite].GetAccountsCountAsync();
            }

            return -1;
        }
    }
}
