using TestStand.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TestStand.Controls
{
    public sealed partial class DeviceItemControl : UserControl
    {
        // Using a DependencyProperty as the backing store for Device.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(Device), typeof(DeviceItemControl), new PropertyMetadata(null));

        public Device Device
        {
            get { return (Device)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowEmployee.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowEmployeeProperty =
            DependencyProperty.Register("ShowEmployee", typeof(bool), typeof(DeviceItemControl), new PropertyMetadata(true));


        public bool ShowEmployee
        {
            get { return (bool)GetValue(ShowEmployeeProperty); }
            set { SetValue(ShowEmployeeProperty, value); }
        }


        public DeviceItemControl()
        {
            this.InitializeComponent();
        }
    }
}
