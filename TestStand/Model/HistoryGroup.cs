using System.Collections.ObjectModel;

namespace TestStand.Model
{
    public class HistoryGroup
    {
        public string Title { get; set; }

        public ObservableCollection<HistoryDeviceEmployeeEntry> Items { get; set; }
    }
}
