using System;

namespace TestStand.Model
{
    public class DeviceEmployeeEntry
    {
        public Device Device { get; set; }

        public Employee Employee { get; set; }

        public DateTime? Date { get; set; }
    }
}