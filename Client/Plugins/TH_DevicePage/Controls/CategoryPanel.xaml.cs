using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for CategoryPanel.xaml
    /// </summary>
    public partial class CategoryPanel : UserControl
    {
        public CategoryPanel()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CategoryPanel), new PropertyMetadata(null));


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(CategoryPanel), new PropertyMetadata(null));


        public object PanelContent
        {
            get { return (object)GetValue(PanelContentProperty); }
            set { SetValue(PanelContentProperty, value); }
        }

        public static readonly DependencyProperty PanelContentProperty =
            DependencyProperty.Register("PanelContent", typeof(object), typeof(CategoryPanel), new PropertyMetadata(null));


    }
}
