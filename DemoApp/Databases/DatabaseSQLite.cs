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

        private SemaphoreSlim _semaphore;
        public DatabaseSQLite()
        {
            DapperConnector.CreateDatabase();
            _semaphore = new SemaphoreSlim(1,1);
        }

        public async Task<bool> AddOrUpdateEnergyMinAvg(DateTime timestamp, double average)
        {
            await _semaphore.WaitAsync();
            bool success = false;

            var db = new DapperConnector();

            try
            {
                await db.AddOrUpdateEnergyMinAvg(timestamp, average);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't add or update EnergyMinAvg: " + timestamp.ToString() + " / " + average , ex);
            }

            _semaphore.Release();
            return success;
        }

        public async Task<IEnumerable<EnergyMinAvgRecord>> GetEnergyMinAvg(DateTime start, DateTime end)
        {
            await _semaphore.WaitAsync();

            var db = new DapperConnector();

            IEnumerable<EnergyMinAvgRecord> records = null;
            try
            {
                var result = await db.GetEnergyMinAvg(start, end);
                records = result;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get EnergyMinAvg between: " + start.ToString() + " and " + end.ToString(), ex);
            }

            _semaphore.Release();
            return records ??  new List<EnergyMinAvgRecord>();
        }

        public async Task<bool> AddAccount(AccountRecord account)
        {
            bool success = false;
            try
            {
                var db = new DapperConnector();

                await _semaphore.WaitAsync();
                await db.AddAccount(account);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't add Account with name " + account.AccountName + ":", ex);
            }
            finally
            {
                _semaphore.Release();
            }
            return success;
        }


        public async Task<bool> DeleteAccountViaId(int id)
        {
            await _semaphore.WaitAsync();
            bool success = false;

            var db = new DapperConnector();

            try
            {
                await db.DeleteAccountViaId(id);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't delete Account with id " + id + ":", ex);
            }
            finally
            {
                _semaphore.Release();
            }

            return success;
        }

        public async Task<bool> UpdateAccount(AccountRecord account)
        {
            bool success = false;
            try
            {
                var db = new DapperConnector();

                await _semaphore.WaitAsync();
               await db.UpdateAccount(account);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Couldn't add Account with name " + account.AccountName + ":", ex);
            }
            finally
            {
                _semaphore.Release();
            }
            return success;
        }

        public async Task<IEnumerable<AccountRecord>> GetAllAccounts()
        {
            await _semaphore.WaitAsync();

            var db = new DapperConnector();

            IEnumerable<AccountRecord> records = null;
            try
            {
                records = await db.GetAllAccounts();
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get Accounts: ",  ex);
            }
            finally
            {
                _semaphore.Release();
            }
            return records ?? new List<AccountRecord>();
        }

        public async Task<AccountRecord> GetAccountViaId(int id)
        {
            await _semaphore.WaitAsync();

            var db = new DapperConnector();

            AccountRecord record = null;
            try
            {
                record = await db.GetAccountViaId(id);
            }
            catch (Exception ex)
            {
                log.Error("Couldn't get Account id " + id + ": ", ex);
            }
            finally
            {
                _semaphore.Release();
            }
            return record;
        }

        public async Task<IEnumerable<AccountRecord>> GetAccountsAsync(int pageNumber, int pageSize)
        {
            await _semaphore.WaitAsync();

            var db = new DapperConnector();

            IEnumerable<AccountRecord> records = null; 
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
                _semaphore.Release();
            }
            return records ?? new List<AccountRecord>();
        }

        public async Task<int> GetAccountsCountAsync()
        {
            await _semaphore.WaitAsync();

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
                _semaphore.Release();
            }
            return count;
        }
    }
}
