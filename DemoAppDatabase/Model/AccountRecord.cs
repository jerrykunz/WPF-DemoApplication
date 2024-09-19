using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAppDatabase.Model
{
    [Table("Accounts")]
    public class AccountRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        [NotNull]
        public string AccountName { get; set; }
        [Unique]
        [NotNull]
        public string Email { get; set; }
        [NotNull]
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string FamilyName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
