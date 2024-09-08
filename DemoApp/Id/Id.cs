using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Id
{
    public class ServiceNames
    {
        private ServiceNames() { }
        public const string DatabaseService = "DatabaseService";
    }

    public class DatabaseNames
    {
        private DatabaseNames() { }
        public const string SQLite = "DatabaseSQLite";
    }

    public class Logs
    {
        public const string SysLogTcp = "SysLogTcp";
        public const string SysLogUdp = "SysLogUdp";
        public const string SysLogError = "SysLogError";
        public const string SysLogFileAppender = "SysLogFileAppender";
        public const string SysLogTestMessageId = "-§SysLogTest§-";
    }
}
