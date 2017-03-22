using System.Collections.ObjectModel;

namespace TestStand.Model
{
    public class DeviceGroup
    {
        public string Title { get; set; }

        public ObservableCollection<Device> Items { get; set; }
    }
}
