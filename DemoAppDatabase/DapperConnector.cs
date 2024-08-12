using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace DemoAppDatabase
{
    public class DapperConnector
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public void AddOrUpdateEnergyMinAvg(DateTime timestamp, double average)
        {
            var p = new DynamicParameters();
            p.Add("@Time", timestamp);
            p.Add("@Value", average);

            var query = @"INSERT INTO EnergyMinAvg
                ( Time, Value ) VALUES
                ( @Time, @Value )
                ON CONFLICT (Time)
                DO UPDATE SET Value = @Value";

            using (SQLiteConnection conn = StatsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }





        public static string StatsDbFile
        {
            get { return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Database\\stats.sqlite"; }
        }

        public static SQLiteConnection StatsSqLiteDbConnection()
        {
            return new SQLiteConnection(string.Format("Data Source={0};Pooling=True;Max Pool Size=100;", StatsDbFile));
        }

        public static void CreateDatabase()
        {
            if (!File.Exists(StatsDbFile))
            {
                string dir = Path.GetDirectoryName(StatsDbFile);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                SQLiteConnection.CreateFile(StatsDbFile);
            }
            
            string success, failed;
            bool? dbRecreateSuccess = CheckAndFixStatsDatabase(out success, out failed);

            if (dbRecreateSuccess == true)
            {
                log.Error("Database integrity check failed, the following tables/indices were recreated: " + success);
            }
            else if (dbRecreateSuccess == false)
            {
                if (string.IsNullOrWhiteSpace(failed))
                {
                    log.Error("Database integrity check failed, couldn't recreate any tables/indices");
                    throw new Exception("Database integrity check failed, couldn't recreate any tables/indices");
                }
                else
                {
                    log.Error("Database integrity check failed, couldn't recreate tables/indices: " + failed);
                    throw new Exception("Database integrity check failed, couldn't recreate tables/indices: " + failed);
                }
            }
        }

        public static bool? CheckAndFixStatsDatabase(out string recreatedTables, out string recreatedTablesFailed)
        {
            bool? recreationSuccessful = null;

            recreatedTables = string.Empty;
            recreatedTablesFailed = string.Empty;

            using (var conn = StatsSqLiteDbConnection())
            {
                bool EnergyMinAvg_exists = false;

                try
                {
                    string getString = @"SELECT name FROM sqlite_master WHERE type='table' AND name='EnergyMinAvg';";
                    SQLiteDataAdapter sda = new SQLiteDataAdapter(getString, conn);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);

                    EnergyMinAvg_exists = ds.Tables[0].Rows.Count > 0;

                    sda.Dispose();
                    ds.Dispose();

                }
                catch (Exception ex)
                {
                    log.Error("Couldn't check sqlite db integrity", ex);
                    return false;
                }


                if (!EnergyMinAvg_exists)
                {
                    recreationSuccessful = true;

                    try
                    {
                        conn.Open();
                    }
                    catch
                    {
                        recreationSuccessful = false;
                    }

                    if (!EnergyMinAvg_exists)
                    {
                        log.Error("Sqlite table 'EnergyMinAvg' did not exist in db, creating...");

                        try
                        {
                            conn.Execute(@"CREATE TABLE EnergyMinAvg
                                        (
                                            Id INTEGER PRIMARY KEY,
                                            Time DATETIME UNIQUE NOT NULL,
                                            Value REAL NOT NULL
                                        );"
                                        );

                            recreatedTables += "EnergyMinAvg, ";

                        }
                        catch (Exception ex)
                        {
                            recreationSuccessful = false;
                            recreatedTablesFailed += "EnergyMinAvg, ";
                            log.Error("Couldn't recreate 'EnergyMinAvg' table to db", ex);
                        }
                    }
                }
            }

            return recreationSuccessful;
        }

    }
}
