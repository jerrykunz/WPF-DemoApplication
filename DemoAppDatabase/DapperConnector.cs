using Dapper;
using DemoAppDatabase.Model;
using DemoAppDatabase.Services;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
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

            CheckStatsDatabase();

            CheckAccountssDatabase();
        }

        #region Stats

        public static string StatsDbFile
        {
            get { return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Database\\stats.sqlite"; }
        }

        public static SQLiteConnection StatsSqLiteDbConnection()
        {
            return new SQLiteConnection(string.Format("Data Source={0};Pooling=True;Max Pool Size=100;", StatsDbFile));
        }

        public static void CheckStatsDatabase()
        {
            if (!File.Exists(StatsDbFile))
            {
                string dir = Path.GetDirectoryName(StatsDbFile);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                SQLiteConnection.CreateFile(StatsDbFile);
            }

            string success, failed;
            bool? dbRecreateSuccess = CreateOrFixStatsDatabase(out success, out failed);

            if (dbRecreateSuccess == true)
            {
                log.Error("Stats database integrity check failed, the following tables/indices were recreated: " + success);
            }
            else if (dbRecreateSuccess == false)
            {
                if (string.IsNullOrWhiteSpace(failed))
                {
                    log.Error("Stats database integrity check failed, couldn't recreate any tables/indices");
                    throw new Exception("Stats database integrity check failed, couldn't recreate any tables/indices");
                }
                else
                {
                    log.Error("Stats database integrity check failed, couldn't recreate tables/indices: " + failed);
                    throw new Exception("Stats database integrity check failed, couldn't recreate tables/indices: " + failed);
                }
            }
        }

        public static bool? CreateOrFixStatsDatabase(out string recreatedTables, out string recreatedTablesFailed)
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

        public IEnumerable<EnergyMinAvgRecord> GetEnergyMinAvg(DateTime start, DateTime end)
        {
            var query = @"SELECT Time, Value
                        FROM EnergyMinAvg
                        WHERE Time >= @Start
                        AND TIME <= @End";

            using (var conn = StatsSqLiteDbConnection())
            {
                conn.Open();
                var parameters = new { Start = start, End = end };
                var result = conn.Query<EnergyMinAvgRecord>(query, parameters);
                return result;
            }
        }

        #endregion

        #region Accounts
        public static SQLiteConnection AccountsSqLiteDbConnection()
        {
            return new SQLiteConnection(string.Format("Data Source={0};Pooling=True;Max Pool Size=100;", AccountsDbFile));
        }

        public static string AccountsDbFile
        {
            get { return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Database\\accounts.sqlite"; }
        }

        public static void CheckAccountssDatabase()
        {
            if (!File.Exists(StatsDbFile))
            {
                string dir = Path.GetDirectoryName(AccountsDbFile);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                SQLiteConnection.CreateFile(AccountsDbFile);
            }

            string success, failed;
            bool? dbRecreateSuccess = CreateOrFixAccountsDatabase(out success, out failed);

            if (dbRecreateSuccess == true)
            {
                log.Error("Accounts database integrity check failed, the following tables/indices were recreated: " + success);
            }
            else if (dbRecreateSuccess == false)
            {
                if (string.IsNullOrWhiteSpace(failed))
                {
                    log.Error("Accounts database integrity check failed, couldn't recreate any tables/indices");
                    throw new Exception("Accounts database integrity check failed, couldn't recreate any tables/indices");
                }
                else
                {
                    log.Error("Accounts database integrity check failed, couldn't recreate tables/indices: " + failed);
                    throw new Exception("Accounts database integrity check failed, couldn't recreate tables/indices: " + failed);
                }
            }
        }

        public static bool? CreateOrFixAccountsDatabase(out string recreatedTables, out string recreatedTablesFailed)
        {
            bool? recreationSuccessful = null;

            recreatedTables = string.Empty;
            recreatedTablesFailed = string.Empty;

            using (var conn = AccountsSqLiteDbConnection())
            {
                bool Accounts_exists = false;

                try
                {
                    string getString = @"SELECT name FROM sqlite_master WHERE type='table' AND name='Accounts';";
                    SQLiteDataAdapter sda = new SQLiteDataAdapter(getString, conn);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);

                    Accounts_exists = ds.Tables[0].Rows.Count > 0;

                    sda.Dispose();
                    ds.Dispose();

                }
                catch (Exception ex)
                {
                    log.Error("Couldn't check sqlite db integrity", ex);
                    return false;
                }


                if (!Accounts_exists)
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

                    if (!Accounts_exists)
                    {
                        log.Error("Sqlite table 'Accounts' did not exist in db, creating...");

                        try
                        {
                            //conn.Execute(@"CREATE TABLE Accounts
                            //            (
                            //                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            //                AccountNameHash VARCHAR(1) UNIQUE NOT NULL,
                            //                AccountName VARCHAR(1) NOT NULL,
                            //                FirstName VARCHAR(1),
                            //                FamilyName VARCHAR(1),
                            //                EmailHash VARCHAR(1) UNIQUE NOT NULL,
                            //                Email VARCHAR(1) NOT NULL,
                            //                PhoneNumber VARCHAR(1),
                            //                Address VARCHAR(1),
                            //                Zipcode VARCHAR(1),
                            //                Country VARCHAR(1),
                            //                PasswordHash VARCHAR(1) NOT NULL
                            //            );"

                            conn.Execute(@"CREATE TABLE Accounts
                                        (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            AccountNameHash VARCHAR(1) UNIQUE NOT NULL,
                                            EmailHash VARCHAR(1) UNIQUE NOT NULL,
                                            Data BLOB NOT NULL
                                        );"

                                        );

                            recreatedTables += "Accounts, ";

                        }
                        catch (Exception ex)
                        {
                            recreationSuccessful = false;
                            recreatedTablesFailed += "Accounts, ";
                            log.Error("Couldn't recreate 'Accounts' table to db", ex);
                        }
                    }
                }
            }

            return recreationSuccessful;
        }


        public async Task AddAccountSingleFast(AccountRecord account)        {
            var messagePackAccountTask = Task.Run(() => MessagePackSerializer.Serialize(account));

            string pass = GetVariable();

            var accountNameHashTask = Task.Run(() => EncryptionService.ComputeSha256Hash(account.AccountName, true)); 
            var emailHashTask = Task.Run(() => EncryptionService.ComputeSha256Hash(account.Email, true));

            await Task.WhenAll(messagePackAccountTask);

            var dataTask = Task.Run(() => EncryptionService.Encrypt(messagePackAccountTask.Result, pass));

            await Task.WhenAll(accountNameHashTask, emailHashTask, dataTask);

            var p = new DynamicParameters();
            p.Add("@AccountNameHash", accountNameHashTask.Result);
            p.Add("@EmailHash", emailHashTask.Result);
            p.Add("@Data", dataTask.Result);
            ClearString(ref pass);

            var query = @"INSERT INTO Accounts
                ( AccountNameHash, EmailHash, Data ) VALUES
                ( @AccountNameHash, @EmailHash, @Data )";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }
        public async Task UpdateAccountSingleFast(AccountRecord account)
        {
            var messagePackAccountTask = Task.Run(() => MessagePackSerializer.Serialize(account));

            string pass = GetVariable();

            var accountNameHashTask = Task.Run(() => EncryptionService.ComputeSha256Hash(account.AccountName, true));
            var emailHashTask = Task.Run(() => EncryptionService.ComputeSha256Hash(account.Email, true));

            await Task.WhenAll(messagePackAccountTask);

            var dataTask = Task.Run(() => EncryptionService.Encrypt(messagePackAccountTask.Result, pass));

            await Task.WhenAll(accountNameHashTask, emailHashTask, dataTask);

            var p = new DynamicParameters();
            p.Add("@Id", account.Id);
            p.Add("@AccountNameHash", accountNameHashTask.Result);
            p.Add("@EmailHash", emailHashTask.Result);
            p.Add("@Data", dataTask.Result);
            ClearString(ref pass);




            // Define the query for updating the account
            var query = @"
                        UPDATE Accounts
                        SET AccountNameHash = @AccountNameHash,
                            EmailHash = @Emailhash,                            
                            Data = @Data
                        WHERE Id = @Id";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();

                int rowsAffected = conn.Execute(query, p);

                if (rowsAffected == 0)
                {
                    throw new Exception("Account to be updated not found");
                }
            }
        }
        public void AddAccount(AccountRecord account)
        {
            var messagePackAccount = MessagePackSerializer.Serialize(account);

            string pass = GetVariable();

            var accountNameHash = EncryptionService.ComputeSha256Hash(account.AccountName, true);
            var emailHash =  EncryptionService.ComputeSha256Hash(account.Email, true);
            var data = EncryptionService.Encrypt(messagePackAccount, pass);

            var p = new DynamicParameters();
            p.Add("@AccountNameHash", accountNameHash);
            p.Add("@EmailHash", emailHash);
            p.Add("@Data", data);
            ClearString(ref pass);

            var query = @"INSERT INTO Accounts
                ( AccountNameHash, EmailHash, Data ) VALUES
                ( @AccountNameHash, @EmailHash, @Data )";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }

        public void AddAccount(AccountRecordEncrypted account)
        {
            var p = new DynamicParameters();
            p.Add("@AccountNameHash", account.AccountNameHash);
            p.Add("@EmailHash", account.EmailHash);
            p.Add("@Data", account.Data);

            var query = @"INSERT INTO Accounts
                ( AccountNameHash, EmailHash, Data ) VALUES
                ( @AccountNameHash, @EmailHash, @Data )";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }

        public void UpdateAccount(AccountRecord account)
        {
            var messagePackAccount = MessagePackSerializer.Serialize(account);

            string pass = GetVariable();

            var accountNameHash = EncryptionService.ComputeSha256Hash(account.AccountName, true);
            var emailHash = EncryptionService.ComputeSha256Hash(account.Email, true);
            var dataTask = EncryptionService.Encrypt(messagePackAccount, pass);


            var p = new DynamicParameters();
            p.Add("@Id", account.Id);
            p.Add("@AccountNameHash", accountNameHash);
            p.Add("@EmailHash", emailHash);
            p.Add("@Data", dataTask);
            ClearString(ref pass);




            // Define the query for updating the account
            var query = @"
                        UPDATE Accounts
                        SET AccountNameHash = @AccountNameHash,                          
                            EmailHash = @Emailhash,
                            Data = @Data
                        WHERE Id = @Id";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();

                int rowsAffected = conn.Execute(query, p);

                if (rowsAffected == 0)
                {
                    throw new Exception("Account to be updated not found");
                }
            }
        }

        public void UpdateAccount(AccountRecordEncrypted account)
        {
            var p = new DynamicParameters();
            p.Add("@Id", account.Id);
            p.Add("@AccountNameHash", account.AccountNameHash);
            p.Add("@EmailHash", account.EmailHash);
            p.Add("@Data", account.Data);

            // Define the query for updating the account
            var query = @"
                        UPDATE Accounts
                        SET AccountNameHash = @AccountNameHash,                          
                            EmailHash = @Emailhash,
                            Data = @Data
                        WHERE Id = @Id";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();

                int rowsAffected = conn.Execute(query, p);

                if (rowsAffected == 0)
                {
                    throw new Exception("Account to be updated not found");
                }
            }
        }

        public void DeleteAccountViaId(int id)
        {
            var p = new DynamicParameters();
            p.Add("@Id", id);

            string query = @"DELETE FROM Accounts WHERE Id = @Id;";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }
        public void DeleteAccountViaAccountNameHash(string accountNameHash)
        {
            var p = new DynamicParameters();
            p.Add("@AccountNameHash", accountNameHash);

            string query = @"DELETE FROM Accounts WHERE AccountNameHash = @AccountNameHash;";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }
        public void DeleteAccountViaEmailHash(string emailHash)
        {
            var p = new DynamicParameters();
            p.Add("@EmailHash", emailHash);

            string query = @"DELETE FROM Accounts WHERE EmailHash = @EmailHash;";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }

        public AccountRecord GetAccountViaId(int id)
        {
            var p = new DynamicParameters();
            p.Add("@Id", id);

            string query = @"SELECT FROM Accounts WHERE Id = @Id;";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                var result = conn.Query<AccountRecordEncrypted>(query);

                foreach(var encryptedRecord in result)
                {
                    string pass = GetVariable();
                    var accRec= MessagePackSerializer.Deserialize<AccountRecord>(EncryptionService.Decrypt(encryptedRecord.Data, pass));
                    ClearString(ref pass);
                    return accRec;
                }
            }

            return null;
        }

        public AccountRecord GetAccountViaAccountNameHash(string accountNameHash)
        {
            var p = new DynamicParameters();
            p.Add("@AccountNameHash", accountNameHash);

            string query = @"SELECT FROM Accounts WHERE AccountNameHash = @AccountNameHash;";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                var result = conn.Query<AccountRecordEncrypted>(query);

                //return first
                foreach (var encryptedRecord in result)
                {
                    string pass = GetVariable();
                    var accRec = MessagePackSerializer.Deserialize<AccountRecord>(EncryptionService.Decrypt(encryptedRecord.Data, pass));
                    ClearString(ref pass);
                    accRec.Id = encryptedRecord.Id;
                    accRec.AccountNameHash = encryptedRecord.AccountNameHash;
                    accRec.EmailHash = encryptedRecord.EmailHash;
                    return accRec;
                }
            }

            return null;
        }

        public AccountRecord GetAccountViaAccountEmailHash(string emailHash)
        {
            var p = new DynamicParameters();
            p.Add("@EmailHash", emailHash);

            string query = @"SELECT FROM Accounts WHERE EmailHash = @EmailHash;";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                var result = conn.Query<AccountRecordEncrypted>(query);

                //return first
                foreach (var encryptedRecord in result)
                {
                    string pass = GetVariable();
                    var accRec = MessagePackSerializer.Deserialize<AccountRecord>(EncryptionService.Decrypt(encryptedRecord.Data, pass));
                    ClearString(ref pass);
                    accRec.Id = encryptedRecord.Id;
                    accRec.AccountNameHash = encryptedRecord.AccountNameHash;
                    accRec.EmailHash = encryptedRecord.EmailHash;
                    return accRec;
                }
            }

            return null;
        }


        public IEnumerable<AccountRecord> GetAllAccounts()
        {
            List<AccountRecord> records = new List<AccountRecord>();
            var query = @"SELECT * FROM Accounts";

            using (var conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                var result = conn.Query<AccountRecordEncrypted>(query).AsList();

                string pass = GetVariable();
                Parallel.For(0, result.Count, (i) =>
                {
                    var rec = MessagePackSerializer.Deserialize<AccountRecord>(EncryptionService.Decrypt(result[i].Data, pass));
                    rec.Id = result[i].Id;
                    rec.AccountNameHash = result[i].AccountNameHash;
                    rec.EmailHash = result[i].EmailHash;
                    records.Add(rec);
                });
                ClearString(ref pass);

                return records;
            }
        }

        public async Task<List<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize)
        {
            ConcurrentBag<AccountRecord> records = new ConcurrentBag<AccountRecord>();
            string query = @"SELECT * FROM Accounts 
                         LIMIT @PageSize OFFSET @PageOffset";

            using (var conn = AccountsSqLiteDbConnection())
            {
                var p = new DynamicParameters();
                p.Add("@PageSize", pageSize);
                p.Add("@PageOffset", pageNumber * pageSize);

               
                var result = await conn.QueryAsync<AccountRecordEncrypted>(query, p);
                var list = new ConcurrentBag<AccountRecordEncrypted>(result); // result.AsList()

                string pass = GetVariable();
                Parallel.ForEach(list, encryptedRecord =>
                {
                    var rec = MessagePackSerializer.Deserialize<AccountRecord>(EncryptionService.Decrypt(encryptedRecord.Data, pass));
                    rec.Id = encryptedRecord.Id;
                    rec.AccountNameHash = encryptedRecord.AccountNameHash;
                    rec.EmailHash = encryptedRecord.EmailHash;
                    records.Add(rec);
                });
                ClearString(ref pass);

                var returnList = new List<AccountRecord>(records);
                returnList.Sort((x, y) => x.Id.CompareTo(y.Id));
                return returnList; //new List<AccountRecord>(records);
            }
        }


        #endregion
    }
}
