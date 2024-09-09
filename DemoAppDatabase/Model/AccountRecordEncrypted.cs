using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAppDatabase.Model
{
    public class AccountRecordEncrypted
    {
        public int Id { get; set; }
        public string AccountNameHash { get; set; }
        public string EmailHash { get; set; }
        public byte[] Data { get; set; }
    }
}
