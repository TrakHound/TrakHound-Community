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

namespace TH_WPF
{
    /// <summary>
    /// Interaction logic for Button_01.xaml
    /// </summary>
    public partial class Button_01 : UserControl
    {
        public Button_01()
        {
            InitializeComponent();
            DataContext = this;
        }

        public object DataObject { get; set; }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Button_01), new PropertyMetadata(false));


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(Button_01), new PropertyMetadata(null));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Button_01), new PropertyMetadata(null));


        public bool AlternateStyle
        {
            get { return (bool)GetValue(AlternateStyleProperty); }
            set 
            { 
                SetValue(AlternateStyleProperty, value);         
            }
        }

        public static readonly DependencyProperty AlternateStyleProperty =
            DependencyProperty.Register("AlternateStyle", typeof(bool), typeof(Button_01), new PropertyMetadata(false));

        public Style MainBorderStyle
        {
            get { return (Style)GetValue(MainBorderStyleProperty); }
            set { SetValue(MainBorderStyleProperty, value); }
        }

        public static readonly DependencyProperty MainBorderStyleProperty =
            DependencyProperty.Register("MainBorderStyle", typeof(Style), typeof(Button_01), new PropertyMetadata(null));


        public Style ImageRectangleStyle
        {
            get { return (Style)GetValue(ImageRectangleStyleProperty); }
            set { SetValue(ImageRectangleStyleProperty, value); }
        }

        public static readonly DependencyProperty ImageRectangleStyleProperty =
            DependencyProperty.Register("ImageRectangleStyle", typeof(Style), typeof(Button_01), new PropertyMetadata(null));


        public Style TextLabelStyle
        {
            get { return (Style)GetValue(TextLabelStyleProperty); }
            set { SetValue(TextLabelStyleProperty, value); }
        }

        public static readonly DependencyProperty TextLabelStyleProperty =
            DependencyProperty.Register("TextLabelStyle", typeof(Style), typeof(Button_01), new PropertyMetadata(null));

        
        public delegate void Clicked_Handler(Button_01 bt);
        public event Clicked_Handler Clicked;

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }   

    }
}
