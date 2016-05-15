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

using TH_Global;

namespace TrakHound_Server_DeviceManager.Controls
{
    /// <summary>
    /// Interaction logic for PageButton.xaml
    /// </summary>
    public partial class PageButton : UserControl
    {
        public PageButton()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        //public IPage Page { get; set; }
        public TH_WPF.ListButton ParentButton { get; set; }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PageButton), new PropertyMetadata(null));


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(PageButton), new PropertyMetadata(null));



        public delegate void Clicked_Handler(TH_WPF.ListButton bt);
        public event Clicked_Handler Clicked;
        public event Clicked_Handler CloseClicked;

        private void CloseButton_Clicked(TH_WPF.Button bt)
        {
            if (CloseClicked != null) CloseClicked(ParentButton);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(ParentButton);
        }
    }
}
