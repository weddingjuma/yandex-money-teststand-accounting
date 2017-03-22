using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TestStand.ViewModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TestStand.Controls.Barcode
{
    public sealed partial class QrCaptureAreaOverlayControl : UserControl
    {
        public static readonly DependencyProperty ScanningStateProperty = DependencyProperty.Register(
            "ScanningState", typeof(ScanningState), typeof(QrCaptureAreaOverlayControl), new PropertyMetadata(default(ScanningState), OnScanningStatePropertyChanged));

        private static void OnScanningStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QrCaptureAreaOverlayControl)d;

            switch ((ScanningState)e.NewValue)
            {
                case ScanningState.Waiting:
                    VisualStateManager.GoToState(control, "Waiting", true);
                    break;
                case ScanningState.DeviceInBucket:
                    VisualStateManager.GoToState(control, "Success", true);
                    break;
                case ScanningState.Scanning:
                    VisualStateManager.GoToState(control, "Scanning", true);
                    break;
                case ScanningState.Error:
                    VisualStateManager.GoToState(control, "Error", true);
                    break;
            }
        }

        public ScanningState ScanningState
        {
            get { return (ScanningState)GetValue(ScanningStateProperty); }
            set { SetValue(ScanningStateProperty, value); }
        }

        public static readonly DependencyProperty MaxAnimationOffsetProperty = DependencyProperty.Register(
            "MaxAnimationOffset", typeof(double), typeof(QrCaptureAreaOverlayControl), new PropertyMetadata(120d));

        /// <summary>
        /// Смещение для анимации сканера, по умолчанию (для QR сканера) 120
        /// </summary>
        public double MaxAnimationOffset
        {
            get { return (double)GetValue(MaxAnimationOffsetProperty); }
            set { SetValue(MaxAnimationOffsetProperty, value); }
        }

        public QrCaptureAreaOverlayControl()
        {
            this.InitializeComponent();
        }
    }
}
