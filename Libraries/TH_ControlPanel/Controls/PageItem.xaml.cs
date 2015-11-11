using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TH_DeviceManager.Controls
{
    /// <summary>
    /// Interaction logic for PageItem.xaml
    /// </summary>
    public partial class PageItem : UserControl
    {
        public PageItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public object Parent { get; set; }


        public bool AlternateStyle
        {
            get { return (bool)GetValue(AlternateStyleProperty); }
            set { SetValue(AlternateStyleProperty, value); }
        }

        public static readonly DependencyProperty AlternateStyleProperty =
            DependencyProperty.Register("AlternateStyle", typeof(bool), typeof(PageItem), new PropertyMetadata(false));

       

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(PageItem), new PropertyMetadata(null));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(PageItem), new PropertyMetadata(null));


        public object Data { get; set; }


        public delegate void Clicked_Handler(PageItem item);
        public event Clicked_Handler Clicked;

        private void root_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }

    }
}
