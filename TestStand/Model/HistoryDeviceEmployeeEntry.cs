using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestStand.Model
{
    public class HistoryDeviceEmployeeEntry
    {
        public Device Device { get; set; }

        public Employee Employee { get; set; }

        public HistoryEntry HistoryEntry { get; set; }
    }
}
