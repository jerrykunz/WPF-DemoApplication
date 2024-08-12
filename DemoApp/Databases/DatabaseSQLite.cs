using DemoApp.Id;
using DemoAppDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp.Databases
{
    public class DatabaseSQLite : IDatabaseSQLite
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Name { get { return DatabaseNames.SQLite; } }

        private Mutex _mutex;
        public DatabaseSQLite()
        {
            DapperConnector.CreateDatabase();
            _mutex = new Mutex();
        }

        public bool AddOrUpdateEnergyMinAvg(DateTime timestamp, double average)
        {
            _mutex.WaitOne();
            bool success = false;

            var db = new DapperConnector();

            try
            {
                db.AddOrUpdateEnergyMinAvg(timestamp, average);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't add or update EnergyMinAvg: " + timestamp.ToString() + " / " + average , ex);
            }

            _mutex.ReleaseMutex();
            return success;
        }
    }
}
