using Dapper;
using DemoAppDatabase.Model;
using DemoAppDatabase.Services;
using System;
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

        private static string GetVariable()
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

        static void ClearString(ref string str)
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
                            conn.Execute(@"CREATE TABLE Accounts
                                        (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            AccountNameHash VARCHAR(1) UNIQUE NOT NULL,
                                            AccountName VARCHAR(1) NOT NULL,
                                            FirstName VARCHAR(1),
                                            FamilyName VARCHAR(1),
                                            EmailHash VARCHAR(1) UNIQUE NOT NULL,
                                            Email VARCHAR(1) NOT NULL,
                                            PhoneNumber VARCHAR(1),
                                            Address VARCHAR(1),
                                            Zipcode VARCHAR(1),
                                            Country VARCHAR(1),
                                            PasswordHash VARCHAR(1) NOT NULL
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


        public async void AddAccount(AccountRecord account)
        {
            string pass = GetVariable();

            var accountNameTask = Task.Run(() => EncryptionService.Encrypt(account.AccountName, pass));
            var accountNameHashTask = Task.Run(() => EncryptionService.ComputeSha256Hash(account.AccountName, true));
            var emailTask = Task.Run(() => EncryptionService.Encrypt(account.Email, pass));
            var emailHashTask = Task.Run(() => EncryptionService.ComputeSha256Hash(account.Email, true));
            var passwordHashTask = Task.Run(() => EncryptionService.Encrypt(account.PasswordHash, pass));
            var firstNameTask = Task.Run(() => EncryptionService.Encrypt(account.FirstName, pass));       
            var familyNameTask = Task.Run(() => EncryptionService.Encrypt(account.FamilyName, pass));
            var phoneNumberTask = Task.Run(() =>  EncryptionService.Encrypt(account.PhoneNumber, pass));
            var addressTask = Task.Run(() => EncryptionService.Encrypt(account.Address, pass));
            var zipcodeTask = Task.Run(() => EncryptionService.Encrypt(account.Zipcode, pass));
            var countryTask = Task.Run(() => EncryptionService.Encrypt(account.Country, pass));

            await Task.WhenAll(accountNameTask, accountNameHashTask, emailTask, emailHashTask, passwordHashTask, firstNameTask, familyNameTask, phoneNumberTask, addressTask, zipcodeTask, countryTask);



            var p = new DynamicParameters();
            p.Add("@AccountName", accountNameTask.Result);
            p.Add("@AccountNameHash", accountNameHashTask.Result);
            p.Add("@Email", emailTask.Result);
            p.Add("@EmailHash", emailHashTask.Result);
            p.Add("@PasswordHash", passwordHashTask.Result);
            p.Add("@FirstName", firstNameTask.Result);
            p.Add("@FamilyName", familyNameTask.Result);
            p.Add("@PhoneNumber", phoneNumberTask.Result);
            p.Add("@Address", addressTask.Result);
            p.Add("@Zipcode", zipcodeTask.Result);
            p.Add("@Country", countryTask.Result);
            ClearString(ref pass);

            var query = @"INSERT INTO Accounts
                ( AccountName, AccountNameHash, Email, EmailHash, PasswordHash, FirstName, FamilyName, PhoneNumber, Address, Zipcode, Country ) VALUES
                ( @AccountName, @AccountNameHash, @Email, @EmailHash, @PasswordHash, @FirstName, @FamilyName, @PhoneNumber, @Address, @Zipcode, @Country )";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }

        public void AddAccount2(AccountRecord account)
        {
            string pass = GetVariable();
            var p = new DynamicParameters();
            p.Add("@AccountName", EncryptionService.Encrypt(account.AccountName, pass));
            p.Add("@AccountNameHash", EncryptionService.ComputeSha256Hash(account.AccountName, true));
            p.Add("@Email", EncryptionService.Encrypt(account.Email, pass));
            p.Add("@EmailHash", EncryptionService.ComputeSha256Hash(account.Email, true));
            p.Add("@PasswordHash", EncryptionService.Encrypt(account.PasswordHash, pass));


            if (!string.IsNullOrWhiteSpace(account.FirstName))
                p.Add("@FirstName", EncryptionService.Encrypt(account.FirstName, pass));
            else
                p.Add("@Firstname", string.Empty);

            if (!string.IsNullOrWhiteSpace(account.FamilyName))
                p.Add("@FamilyName", EncryptionService.Encrypt(account.FamilyName, pass));
            else
                p.Add("@FamilyName", string.Empty);

            if (!string.IsNullOrWhiteSpace(account.PhoneNumber))
                p.Add("@PhoneNumber", EncryptionService.Encrypt(account.PhoneNumber, pass));
            else
                p.Add("@PhoneNumber", string.Empty);

            if (!string.IsNullOrWhiteSpace(account.Address))
                p.Add("@Address", EncryptionService.Encrypt(account.Address, pass));
            else
                p.Add("@Address", string.Empty);

            if (!string.IsNullOrWhiteSpace(account.Zipcode))
                p.Add("@Zipcode", EncryptionService.Encrypt(account.Zipcode, pass));
            else
                p.Add("@Zipcode", string.Empty);

            if (!string.IsNullOrWhiteSpace(account.Country))
                p.Add("@Country", EncryptionService.Encrypt(account.Country, pass));
            else
                p.Add("@Country", string.Empty);
            ClearString(ref pass);

            var query = @"INSERT INTO Accounts
                ( AccountName, AccountNameHash, Email, EmailHash, PasswordHash, FirstName, FamilyName, PhoneNumber, Address, Zipcode, Country ) VALUES
                ( @AccountName, @AccountNameHash, @Email, @EmailHash, @PasswordHash, @FirstName, @FamilyName, @PhoneNumber, @Address, @Zipcode, @Country )";

            using (SQLiteConnection conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                conn.Execute(query, p);
            }
        }

        public void DeleteAccount(int id)
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

        public async void UpdateAccount(AccountRecord account)
        {
            string pass = GetVariable();

            var accountNameTask = Task.Run(() => EncryptionService.Encrypt(account.AccountName, pass));
            var accountNameHashTask = Task.Run(() => EncryptionService.ComputeSha256Hash(account.AccountName, true));
            var emailTask = Task.Run(() => EncryptionService.Encrypt(account.Email, pass));
            var emailHashTask = Task.Run(() => EncryptionService.ComputeSha256Hash(account.Email, true));
            var passwordHashTask = Task.Run(() => EncryptionService.Encrypt(account.PasswordHash, pass));
            var firstNameTask = Task.Run(() => EncryptionService.Encrypt(account.FirstName, pass));
            var familyNameTask = Task.Run(() => EncryptionService.Encrypt(account.FamilyName, pass));
            var phoneNumberTask = Task.Run(() => EncryptionService.Encrypt(account.PhoneNumber, pass));
            var addressTask = Task.Run(() => EncryptionService.Encrypt(account.Address, pass));
            var zipcodeTask = Task.Run(() => EncryptionService.Encrypt(account.Zipcode, pass));
            var countryTask = Task.Run(() => EncryptionService.Encrypt(account.Country, pass));

            await Task.WhenAll(accountNameTask, accountNameHashTask, emailTask, emailHashTask, passwordHashTask, firstNameTask, familyNameTask, phoneNumberTask, addressTask, zipcodeTask, countryTask);

            var p = new DynamicParameters();
            p.Add("@Id", account.Id);
            p.Add("@AccountName", accountNameTask.Result);
            p.Add("@AccountNameHash", accountNameHashTask.Result);
            p.Add("@Email", emailTask.Result);
            p.Add("@EmailHash", emailHashTask.Result);
            p.Add("@PasswordHash", passwordHashTask.Result);
            p.Add("@FirstName", firstNameTask.Result);
            p.Add("@FamilyName", familyNameTask.Result);
            p.Add("@PhoneNumber", phoneNumberTask.Result);
            p.Add("@Address", addressTask.Result);
            p.Add("@Zipcode", zipcodeTask.Result);
            p.Add("@Country", countryTask.Result);
            ClearString(ref pass);




            // Define the query for updating the account
            var query = @"
                        UPDATE Accounts
                        SET AccountName = @AccountName,
                            AccountNameHash = @AccountNameHash,
                            Email = @Email,
                            EmailHash = @Emailhash,
                            PasswordHash = @PasswordHash,
                            FirstName = @FirstName,
                            FamilyName = @FamilyName,
                            PhoneNumber = @PhoneNumber,
                            Address = @Address,
                            Zipcode = @Zipcode,
                            Country = @Country
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

        public IEnumerable<AccountRecord> GetAllAccounts()
        {
            var query = @"SELECT Id, AccountName, Email, PasswordHash, FirstName, FamilyName, PhoneNumber, Address, Zipcode, Country
                  FROM Accounts";

            using (var conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                var result = conn.Query<AccountRecord>(query);

                //TODO: multiple tasks and parallel for
                string pass = GetVariable();
                foreach (AccountRecord rec in result)
                {
                    rec.AccountName = EncryptionService.Decrypt(rec.AccountName, pass);
                    rec.Email = EncryptionService.Decrypt(rec.Email, pass);
                    rec.PasswordHash = EncryptionService.Decrypt(rec.PasswordHash, pass);
                    rec.FirstName = EncryptionService.Decrypt(rec.FirstName, pass);
                    rec.FamilyName = EncryptionService.Decrypt(rec.FamilyName, pass);
                    rec.PhoneNumber = EncryptionService.Decrypt(rec.PhoneNumber, pass);
                    rec.Address = EncryptionService.Decrypt(rec.Address, pass);
                    rec.Zipcode = EncryptionService.Decrypt(rec.Zipcode, pass);
                    rec.Country = EncryptionService.Decrypt(rec.Country, pass);
                }
                ClearString(ref pass);

                return result;
            }
        }

        public IEnumerable<AccountRecord> GetAllAccounts2()
        {
            var query = @"SELECT Id, AccountName, Email, PasswordHash, FirstName, FamilyName, PhoneNumber, Address, Zipcode, Country
                  FROM Accounts";

            using (var conn = AccountsSqLiteDbConnection())
            {
                conn.Open();
                List<AccountRecord> accounts = conn.Query<AccountRecord>(query).AsList<AccountRecord>();
                //TODO: multiple tasks and parallel for
                string pass = GetVariable();

                Parallel.For(0, accounts.Count, (i) =>
                {
                    AccountRecord rec = accounts[i];

                    rec.AccountName = EncryptionService.Decrypt(rec.AccountName, pass);
                    rec.Email = EncryptionService.Decrypt(rec.Email, pass);
                    rec.PasswordHash = EncryptionService.Decrypt(rec.PasswordHash, pass);
                    rec.FirstName = EncryptionService.Decrypt(rec.FirstName, pass);
                    rec.FamilyName = EncryptionService.Decrypt(rec.FamilyName, pass);
                    rec.PhoneNumber = EncryptionService.Decrypt(rec.PhoneNumber, pass);
                    rec.Address = EncryptionService.Decrypt(rec.Address, pass);
                    rec.Zipcode = EncryptionService.Decrypt(rec.Zipcode, pass);
                    rec.Country = EncryptionService.Decrypt(rec.Country, pass);
                });

                foreach (AccountRecord rec in accounts)
                {
                    rec.AccountName = EncryptionService.Decrypt(rec.AccountName, pass);
                    rec.Email = EncryptionService.Decrypt(rec.Email, pass);
                    rec.PasswordHash = EncryptionService.Decrypt(rec.PasswordHash, pass);
                    rec.FirstName = EncryptionService.Decrypt(rec.FirstName, pass);
                    rec.FamilyName = EncryptionService.Decrypt(rec.FamilyName, pass);
                    rec.PhoneNumber = EncryptionService.Decrypt(rec.PhoneNumber, pass);
                    rec.Address = EncryptionService.Decrypt(rec.Address, pass);
                    rec.Zipcode = EncryptionService.Decrypt(rec.Zipcode, pass);
                    rec.Country = EncryptionService.Decrypt(rec.Country, pass);
                }
                ClearString(ref pass);

                return accounts;
            }
        }


        #endregion
    }
}
