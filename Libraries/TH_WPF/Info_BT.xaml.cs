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
    /// Interaction logic for Info_BT.xaml
    /// </summary>
    public partial class Info_BT : UserControl
        {
        public Info_BT()
            {
            InitializeComponent();
            Data_Link = "";
            }

        public string ItemName
            {
            get { return Item_TXT.Text; }
            set
                {

                Item_TXT.Text = value;

                if (Item_TXT.Text != "")
                    {
                    Item_TXT.Visibility = System.Windows.Visibility.Visible;
                    Image_IMG.Margin = new Thickness(10, 0, 0, 0);
                    Image_IMG.Opacity = 0.25;
                    }
                else
                    {
                    Item_TXT.Visibility = System.Windows.Visibility.Collapsed;
                    Image_IMG.Margin = new Thickness(10, 0, 10, 0);
                    Image_IMG.Opacity = 0.75;
                    }

                }
            }

        public object Data_Link { get; set; }


        public ImageSource Image
            {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
            }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(Info_BT), new PropertyMetadata(null));


        public bool IsSelected
            {
            get { return (bool)this.GetValue(IsSelectedProperty); }
            set { this.SetValue(IsSelectedProperty, value); }
            }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected", typeof(bool), typeof(Info_BT), new PropertyMetadata(false));


        public delegate void Clicked_Handler(Info_BT BT, object Data_Link);
        public event Clicked_Handler Clicked;

        private void Main_GRID_MouseDown(object sender, MouseButtonEventArgs e)
            {

            Clicked_Handler handler = Clicked;
            if (handler != null) Clicked(this, Data_Link);

            }
        
        }
    }
