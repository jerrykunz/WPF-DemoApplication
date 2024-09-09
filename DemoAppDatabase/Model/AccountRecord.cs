using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAppDatabase.Model
{
    [MessagePackObject]
    public class AccountRecord
    {
        [IgnoreMember]
        public int Id { get; set; }
        [IgnoreMember]
        public string AccountNameHash { get; set; }
        [IgnoreMember]
        public string EmailHash { get; set; }

        [Key(0)]
        public string AccountName { get; set; }
        [Key(1)]
        public string Email { get; set; }
        [Key(2)]
        public string PasswordHash { get; set; }
        [Key(3)]
        public string FirstName { get; set; }
        [Key(4)]
        public string FamilyName { get; set; }
        [Key(5)]
        public string PhoneNumber { get; set; }
        [Key(6)]
        public string Address { get; set; }
        [Key(7)]
        public string Zipcode { get; set; }
        [Key(8)]
        public string Country { get; set; }
    }
}
