using Dapper;
using DemoAppDatabase.Model;
using DemoAppDatabase.Services;
using SQLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DemoAppDatabase
{
    public class DapperConnector
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static SecureString _variable;

        private static void SetVariable(string variable)
        {
            if (_variable != null)
            {
                _variable.Clear();
            }

            _variable = new SecureString();
            foreach (char c in variable)
            {
                _variable.AppendChar(c);
            }
            _variable.MakeReadOnly();
            ClearString(ref variable);
        }

        public static string GetVariable()
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                // Decrypt the SecureString into an unmanaged memory string
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(_variable);

                // Convert unmanaged memory to managed string
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                // Ensure unmanaged memory is freed and zeroed out
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static void ClearString(ref string str)
        {
            if (str != null)
            {
                unsafe
                {
                    fixed (char* ptr = str)
                    {
                        for (int i = 0; i < str.Length; i++)
                        {
                            ptr[i] = '\0'; // Overwrite each character with null
                        }
                    }
                }
                str = null; // Set the reference to null
            }
        }

        public static void CreateDatabase()
        {
            SetVariable("password123");
            CreateOrFixDatabase(typeof(EnergyMinAvgRecord), StatsSqLiteDbConnection());
            CreateOrFixDatabase(typeof(AccountRecord), AccountsSqLiteDbConnection());
        }


        public static void CreateOrFixDatabase(Type type, SQLiteAsyncConnection conn)
        {
            try
            {
                //var conn = AccountsSqLiteDbConnection();
                string tableName = type.Name;

                bool found = false;
                bool correct = true;
                string missingColumns = string.Empty;
                string missingAttributes = string.Empty;
                foreach (var table in conn.TableMappings)
                {
                    if (table.TableName == type.Name)
                    {
                        tableName = table.TableName;
                        found = true;

                        var classProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var tableColumns = table.Columns.Select(c => c.Name).ToList();

                        //check each class property
                        foreach (var property in classProperties)
                        {
                            //class property is ignored, don't check this one
                            if (Attribute.IsDefined(property, typeof(IgnoreAttribute)))
                                continue;

                            //column tags
                            bool foundColumn = false;
                            bool isPrimaryKey = false;
                            bool isNullable = false;
                            bool isUnique = false;

                            //check table for column corresponding to class property
                            foreach (var column in table.Columns)
                            {
                                //no correspondence
                                if (property.Name != column.Name)
                                    continue;

                                foundColumn = true;

                                //check column tags
                                isPrimaryKey = column.IsPK;
                                isNullable = column.IsNullable;
                                isUnique = column.Indices != null && column.Indices.Any(index => index.Unique);
                            }

                            //column not found, check next class property
                            if (!foundColumn)
                            {
                                correct = false;
                                missingColumns += property.Name + ", ";
                                continue;
                            }

                            //check if column tags match with class property
                            bool propIsPrimaryKey = Attribute.IsDefined(property, typeof(PrimaryKeyAttribute));
                            bool propIsUnique = Attribute.IsDefined(property, typeof(UniqueAttribute));
                            bool propIsNotNull = Attribute.IsDefined(property, typeof(NotNullAttribute));

                            if (isPrimaryKey != propIsPrimaryKey)
                            {
                                missingAttributes += table.TableName + " table column " + property.Name + " mismatching primary key attribute with value " + isPrimaryKey + "\n";
                                correct = false;
                            }

                            if (isUnique != propIsUnique)
                            {
                                missingAttributes += table.TableName + " table column " + property.Name + " mismatching unique attribute with value " + isUnique + "\n";
                                correct = false;
                            }

                            if (isNullable == propIsNotNull)
                            {
                                missingAttributes += table.TableName + " table column " + property.Name + " mismatching nullable attribute with value " + isNullable + "\n";
                                correct = false;
                            }
                        }
                    }

                    //found table, exit loop
                    if (found)
                    {
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(missingColumns))
                {
                    string missingColumnsLogString = "Sqlite table '" + tableName + "' was missing columns: ";
                    missingColumnsLogString += missingColumns.Substring(0, missingColumns.Length - 2);
                    log.Error(missingColumnsLogString);
                }
                else if (!string.IsNullOrWhiteSpace(missingAttributes))
                {
                    var lines = missingAttributes.Split('\n');
                    foreach (var line in lines)
                    {
                        log.Error(line);
                    }
                }

                if (!found)
                {
                    conn.CreateTableAsync(type);
                    log.Error("Sqlite table '" + tableName + "' did not exist in db, creating...");
                }
                else if (!correct)
                {
                    conn.CreateTableAsync(type);
                    log.Error("Sqlite table '" + tableName + "' was incorrect, recreating...");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error checking or recreating stats database", ex);
                throw new Exception("Stats database integrity check failed");
            }
        }

        #region Stats

        public static string StatsDbFile
        {
            get { return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Database\\stats.sqlite"; }
        }

        private static SQLiteAsyncConnection _statsSqliteDbConnection;
        public static SQLiteAsyncConnection StatsSqLiteDbConnection()
        {
            if (_statsSqliteDbConnection == null)
            {
                string dir = Path.GetDirectoryName(StatsDbFile);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                };

                var options = new SQLiteConnectionString(StatsDbFile, true);
                _statsSqliteDbConnection =  new SQLiteAsyncConnection(options);
            }

            return _statsSqliteDbConnection;
        }

        public async Task AddOrUpdateEnergyMinAvg(DateTime timestamp, double average)
        {
            var conn = StatsSqLiteDbConnection();
            await conn.InsertAsync(new EnergyMinAvgRecord
            {
                Time = timestamp,
                Value = average
            });
        }

        public async Task<IEnumerable<EnergyMinAvgRecord>> GetEnergyMinAvg(DateTime start, DateTime end)
        {
            var query = @"SELECT Time, Value
                        FROM EnergyMinAvg
                        WHERE Time >= ?
                        AND TIME <= ?";

            return await StatsSqLiteDbConnection().QueryAsync<EnergyMinAvgRecord>(query, start, end);
        }

        #endregion

        #region Accounts

        private static SQLiteAsyncConnection _accountsSqLiteDbConnection;
        public static SQLiteAsyncConnection AccountsSqLiteDbConnection()
        {
            if (_accountsSqLiteDbConnection == null)
            {
                var options = new SQLiteConnectionString(AccountsDbFile, true,
                            key: "password",
                            preKeyAction: db => db.Execute("PRAGMA cipher_default_use_hmac = ON; PRAGMA page_size = 4096; PRAGMA cipher_page_size = 4096; PRAGMA cache_size = -2000;"),
                            postKeyAction: db => db.Execute("PRAGMA kdf_iter = 128000; PRAGMA kdf_salt = 'your_salt_here';"));
                _accountsSqLiteDbConnection = new SQLiteAsyncConnection(options);
            }

            return _accountsSqLiteDbConnection;
        }

        public static string AccountsDbFile
        {
            get { return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Database\\accounts.sqlite"; }
        }
        public async Task AddAccount(AccountRecord account)
        {
            var conn = AccountsSqLiteDbConnection();
            await conn.InsertAsync(account);
        }

        public async Task UpdateAccount(AccountRecord account)
        {
            var conn = AccountsSqLiteDbConnection();
            await conn.UpdateAsync(account);
        }

        public async Task DeleteAccountViaId(int id)
        {
            var conn = AccountsSqLiteDbConnection();
            await conn.DeleteAsync<AccountRecord>(id);
        }

        public async Task DeleteAccount(AccountRecord record)
        {
            var conn = AccountsSqLiteDbConnection();
            await conn.DeleteAsync<AccountRecord>(record);
        }

        public async Task<AccountRecord> GetAccountViaId(int id)
        {
            var conn = AccountsSqLiteDbConnection();
            return await conn.GetAsync<AccountRecord>(id);
        }

        public async Task<AccountRecord> GetAccount(AccountRecord record)
        {
            var conn = AccountsSqLiteDbConnection();
            return await conn.GetAsync<AccountRecord>(record);
        }

        public async Task<IEnumerable<AccountRecord>> GetAllAccounts()
        {
            var conn = AccountsSqLiteDbConnection();
            return await conn.Table<AccountRecord>().ToListAsync();
        }

        public async Task<IEnumerable<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize)
        {
            var conn = AccountsSqLiteDbConnection();

            var query = @"SELECT * FROM Accounts 
                  LIMIT ? OFFSET ?";

            int pageOffset = pageNumber * pageSize;
            return await conn.QueryAsync<AccountRecord>(query, pageSize, pageOffset);
        }

        public async Task<int> GetAccountsCountAsync()
        {
            var conn = AccountsSqLiteDbConnection();
            var query = @"SELECT COUNT(*) FROM Accounts";
            return await conn.ExecuteScalarAsync<int>(query);
        }

        #endregion
    }
}
