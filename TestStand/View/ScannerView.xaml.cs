using TestStand.Controls;
using TestStand.ViewModel;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TestStand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScannerView : Page
    {
        public ScannerViewModel ViewModel => (ScannerViewModel)DataContext;

        public ScannerView()
        {
            this.InitializeComponent();
        }

        private void CaptureElement_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SetCaptureElement(CaptureElement);
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
