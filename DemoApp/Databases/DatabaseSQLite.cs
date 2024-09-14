using DemoApp.Id;
using DemoAppDatabase;
using DemoAppDatabase.Model;
using DemoAppDatabase.Services;
using MessagePack;
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

        public IEnumerable<EnergyMinAvgRecord> GetEnergyMinAvg(DateTime start, DateTime end)
        {
            _mutex.WaitOne();

            var db = new DapperConnector();

            List<EnergyMinAvgRecord> records = new List<EnergyMinAvgRecord>();
            try
            {
                records = db.GetEnergyMinAvg(start, end).ToList();
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get EnergyMinAvg between: " + start.ToString() + " and " + end.ToString(), ex);
            }

            _mutex.ReleaseMutex();
            return records;
        }

        //public bool AddAccount(AccountRecord account)
        //{
        //    _mutex.WaitOne();
        //    bool success = false;

        //    var db = new DapperConnector();

        //    try
        //    {
        //        db.AddAccount(account);
        //        success = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Couldn't add Account with name " + account.AccountName + ":" , ex);
        //    }
        //    finally
        //    {
        //        _mutex.ReleaseMutex();
        //    }
        //    return success;
        //}

        public bool AddAccount(AccountRecord account)
        {
            bool success = false;
            try
            {
                var messagePackAccount = MessagePackSerializer.Serialize(account);

                var db = new DapperConnector();

                string pass = DapperConnector.GetVariable();
                AccountRecordEncrypted encryptedAccount = new AccountRecordEncrypted()
                {
                    AccountNameHash = EncryptionService.ComputeSha256Hash(account.AccountName, true),
                    EmailHash = EncryptionService.ComputeSha256Hash(account.Email, true),
                    Data = EncryptionService.Encrypt(messagePackAccount, pass)
                };
                DapperConnector.ClearString(ref pass);

                _mutex.WaitOne();
                db.AddAccount(encryptedAccount);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't add Account with name " + account.AccountName + ":", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return success;
        }

        public async Task<bool> AddAccountSingleFast(AccountRecord account)
        {
            _mutex.WaitOne();
            bool success = false;

            var db = new DapperConnector();

            try
            {
                await db.AddAccountSingleFast(account);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't add Account with name " + account.AccountName + ":", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }

            return success;
        }

        public bool DeleteAccountViaId(int id)
        {
            _mutex.WaitOne();
            bool success = false;

            var db = new DapperConnector();

            try
            {
                db.DeleteAccountViaId(id);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't delete Account with id " + id + ":", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }

            return success;
        }

        public bool DeleteAccountViaAccountNameHash(string accountNameHash)
        {
            _mutex.WaitOne();
            bool success = false;

            var db = new DapperConnector();

            try
            {
                db.DeleteAccountViaAccountNameHash(accountNameHash);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't delete Account with AccountNameHash " + accountNameHash + ":", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return success;
        }

        public bool DeleteAccountViaEmailHash(string emailHash)
        {
            _mutex.WaitOne();
            bool success = false;

            var db = new DapperConnector();

            try
            {
                db.DeleteAccountViaEmailHash(emailHash);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't delete Account with EmailHash " + emailHash + ":", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return success;
        }


        public bool UpdateAccount(AccountRecord account)
        {
            bool success = false;
            try
            {
                var messagePackAccount = MessagePackSerializer.Serialize(account);

                var db = new DapperConnector();

                string pass = DapperConnector.GetVariable();
                AccountRecordEncrypted encryptedAccount = new AccountRecordEncrypted()
                {
                    AccountNameHash = EncryptionService.ComputeSha256Hash(account.AccountName, true),
                    EmailHash = EncryptionService.ComputeSha256Hash(account.Email, true),
                    Data = EncryptionService.Encrypt(messagePackAccount, pass)
                };
                DapperConnector.ClearString(ref pass);

                _mutex.WaitOne();
                db.UpdateAccount(encryptedAccount);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't add Account with name " + account.AccountName + ":", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return success;
        }

        //public bool UpdateAccount(AccountRecord account)
        //{
        //    _mutex.WaitOne();
        //    bool success = false;

        //    var db = new DapperConnector();

        //    try
        //    {
        //        db.UpdateAccount(account);
        //        success = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Couldn't update Account with id " + account.Id + ":", ex);
        //    }
        //    finally
        //    {
        //        _mutex.ReleaseMutex();
        //    }
        //    return success;
        //}

        public async Task<bool> UpdateAccountSingleFast(AccountRecord account)
        {
            _mutex.WaitOne();
            bool success = false;

            var db = new DapperConnector();

            try
            {
                await db.UpdateAccountSingleFast(account);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't update Account with id " + account.Id + ":", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return success;
        }

        public IEnumerable<AccountRecord> GetAllAccounts()
        {
            _mutex.WaitOne();

            var db = new DapperConnector();

            List<AccountRecord> records = new List<AccountRecord>();
            try
            {
                records = db.GetAllAccounts().ToList();
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get Accounts: ",  ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return records;
        }

        public AccountRecord GetAccountViaId(int id)
        {
            _mutex.WaitOne();

            var db = new DapperConnector();

            AccountRecord record = null;
            try
            {
                record = db.GetAccountViaId(id);
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get Account id " + id + ": ", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return record;
        }

        public AccountRecord GetAccountViaAccountNameHash(string accountNameHash)
        {
            _mutex.WaitOne();

            var db = new DapperConnector();

            AccountRecord record = null;
            try
            {
                record = db.GetAccountViaAccountNameHash(accountNameHash);
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get Account with AccountNameHash " + accountNameHash + ": ", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return record;
        }

        public AccountRecord GetAccountViaAccountEmailHash(string emailHash)
        {
            _mutex.WaitOne();

            var db = new DapperConnector();

            AccountRecord record = null;
            try
            {
                record = db.GetAccountViaAccountEmailHash(emailHash);
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get Account with EmailHash " + emailHash + ": ", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return record;
        }

        public async Task<List<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize)
        {
            _mutex.WaitOne();

            var db = new DapperConnector();

            List<AccountRecord> records = new List<AccountRecord>(); 
            try
            {
                records = await db.GetAccountsAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get Accounts with pageNum: " + pageNumber + ", pageSize: " + pageSize, ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return records;
        }

        public async Task<int> GetAccountsCountAsync()
        {
            _mutex.WaitOne();

            var db = new DapperConnector();

            int count = -1;
            try
            {
                count = await db.GetAccountsCountAsync();
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get Accounts count", ex);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return count;
        }
    }
}
