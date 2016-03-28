using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for ImageDisplay.xaml
    /// </summary>
    public partial class ImageDisplay : UserControl
    {
        public ImageDisplay()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageDisplay), new PropertyMetadata(null));


    }
}
