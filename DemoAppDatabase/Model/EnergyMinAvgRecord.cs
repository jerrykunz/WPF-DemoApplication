using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAppDatabase.Model
{
    [Table("EnergyMinAvg")]
    public class EnergyMinAvgRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        [NotNull]
        public DateTime Time { get; set; }
        [NotNull]
        public double Value { get; set; }
    }
}
