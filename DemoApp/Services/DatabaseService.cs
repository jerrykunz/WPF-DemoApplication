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

        public IEnumerable<EnergyMinAvgRecord> GetEnergyMinAvg(DateTime start, DateTime end)
        {
            if (Databases.ContainsKey(DatabaseNames.SQLite))
            {
                return Databases[DatabaseNames.SQLite].GetEnergyMinAvg(start, end);
            }

            return new List<EnergyMinAvgRecord>();
        }
    }
}
