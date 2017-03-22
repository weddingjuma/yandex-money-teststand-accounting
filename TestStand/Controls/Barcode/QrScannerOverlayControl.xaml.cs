using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TestStand.Controls.Barcode
{
    [ContentProperty(Name = "OverlayContent")]
    public sealed partial class QrScannerOverlayControl : UserControl
    {
        public static GridLength DefaultWidth = new GridLength(400.0);

        public static GridLength DefaultHeight = new GridLength(400.0);

        public static readonly DependencyProperty OverlayContentProperty = DependencyProperty.Register(
            "OverlayContent", typeof(object), typeof(QrScannerOverlayControl), new PropertyMetadata(default(object)));

        public object OverlayContent
        {
            get { return (object)GetValue(OverlayContentProperty); }
            set { SetValue(OverlayContentProperty, value); }
        }

        public static readonly DependencyProperty OverlayContentWidthProperty = DependencyProperty.Register(
            "OverlayContentWidth", typeof(GridLength), typeof(QrScannerOverlayControl),
            new PropertyMetadata(DefaultWidth, (s, a) =>
            {
                var sender = s as QrScannerOverlayControl;

                if (sender == null)
                    return;

                sender.grid.ColumnDefinitions[1].Width = (GridLength)a.NewValue;
            }));

        public GridLength OverlayContentWidth
        {
            get { return (GridLength)GetValue(OverlayContentWidthProperty); }
            set { SetValue(OverlayContentWidthProperty, value); }
        }

        public static readonly DependencyProperty OverlayContentHeightProperty = DependencyProperty.Register(
            "OverlayContentHeight", typeof(GridLength), typeof(QrScannerOverlayControl),
            new PropertyMetadata(DefaultHeight, (s, a) =>
            {
                var sender = s as QrScannerOverlayControl;

                if (sender == null)
                    return;

                sender.grid.RowDefinitions[1].Height = (GridLength)a.NewValue;
            }));

        public GridLength OverlayContentHeight
        {
            get { return (GridLength)GetValue(OverlayContentHeightProperty); }
            set { SetValue(OverlayContentHeightProperty, value); }
        }

        public static readonly DependencyProperty OverlayContentTemplateProperty = DependencyProperty.Register(
            "OverlayContentTemplate", typeof(DataTemplate), typeof(QrScannerOverlayControl), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate OverlayContentTemplate
        {
            get { return (DataTemplate)GetValue(OverlayContentTemplateProperty); }
            set { SetValue(OverlayContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty CaptureAreaOverlayContentProperty = DependencyProperty.Register(
            "CaptureAreaOverlayContent", typeof(object), typeof(QrScannerOverlayControl), new PropertyMetadata(default(object)));

        public object CaptureAreaOverlayContent
        {
            get { return (object)GetValue(CaptureAreaOverlayContentProperty); }
            set { SetValue(CaptureAreaOverlayContentProperty, value); }
        }

        public static readonly DependencyProperty CaptureAreaOverlayContentTemplateProperty = DependencyProperty.Register(
            "CaptureAreaOverlayContentTemplate", typeof(DataTemplate), typeof(QrScannerOverlayControl), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate CaptureAreaOverlayContentTemplate
        {
            get { return (DataTemplate)GetValue(CaptureAreaOverlayContentTemplateProperty); }
            set { SetValue(CaptureAreaOverlayContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty CaptureAreaOverlayContentTemplateSelectorProperty = DependencyProperty.Register(
            "CaptureAreaOverlayContentTemplateSelector", typeof(DataTemplateSelector), typeof(QrScannerOverlayControl), new PropertyMetadata(default(DataTemplateSelector)));

        public DataTemplateSelector CaptureAreaOverlayContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(CaptureAreaOverlayContentTemplateSelectorProperty); }
            set { SetValue(CaptureAreaOverlayContentTemplateSelectorProperty, value); }
        }

        public QrScannerOverlayControl()
        {
            InitializeComponent();
        }
    }
}
