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

namespace TrakHound_Dashboard.Controls
{
    /// <summary>
    /// Interaction logic for TabManager.xaml
    /// </summary>
    public partial class TabManager : UserControl
    {
        public TabManager()
        {
            InitializeComponent();
            DataContext = this;
        }

        List<TabItem> items;
        public List<TabItem> Items
        {
            get { return items; }
            set { items = value; }
        }

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(TabManager), new PropertyMetadata(null));
       

        public class TabItem
        {
            public string Title { get; set; }
        }
    }
}
