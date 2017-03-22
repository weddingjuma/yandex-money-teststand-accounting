using TestStand.Controls;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestStand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        public MainView()
        {
            this.InitializeComponent();
        }

        private void SearchBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Popup.Current == null)
            {
                Popup.Show(new SearchView());
            }
        }
    }
}
