using DemoApp.Databases;
using DemoApp.Id;
using DemoApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Factories
{
    internal class DatabaseServiceFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static IDatabaseService CreateDatabaseService()
        {
            Dictionary<string, IDatabase> databases = new Dictionary<string, IDatabase>();

            DatabaseSQLite dbSQLite = new DatabaseSQLite();
            databases.Add(DatabaseNames.SQLite,  dbSQLite);



            DatabaseService databaseService = new DatabaseService(databases);

            return databaseService;
        }

    }
}
