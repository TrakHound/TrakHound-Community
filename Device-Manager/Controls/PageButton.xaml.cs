using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Device_Manager.Controls
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
        public TrakHound_UI.ListButton ParentButton { get; set; }

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



        public delegate void Clicked_Handler(TrakHound_UI.ListButton bt);
        public event Clicked_Handler Clicked;
        public event Clicked_Handler CloseClicked;

        private void CloseButton_Clicked(TrakHound_UI.Button bt)
        {
            if (CloseClicked != null) CloseClicked(ParentButton);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(ParentButton);
        }
    }
}
