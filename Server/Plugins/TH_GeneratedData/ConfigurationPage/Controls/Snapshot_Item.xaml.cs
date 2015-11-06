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

using System.Collections.ObjectModel;

namespace TH_GeneratedData.ConfigurationPage.Controls
{
    /// <summary>
    /// Interaction logic for Snapshot_Item.xaml
    /// </summary>
    public partial class Snapshot_Item : UserControl
    {
        public Snapshot_Item()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Name"

        public string NameText
        {
            get { return (string)GetValue(NameTextProperty); }
            set { SetValue(NameTextProperty, value); }
        }

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof(string), typeof(Snapshot_Item), new PropertyMetadata(null));

        #endregion

        #region "Type"

        public string SelectedType
        {
            get { return (string)GetValue(SelectedTypeProperty); }
            set 
            { 
                SetValue(SelectedTypeProperty, value);
                if (TypeChanged != null) TypeChanged(value, this);
            }
        }

        public static readonly DependencyProperty SelectedTypeProperty =
            DependencyProperty.Register("SelectedType", typeof(string), typeof(Snapshot_Item), new PropertyMetadata(null));


        ObservableCollection<string> typeitems;
        public ObservableCollection<string> TypeItems
        {
            get
            {
                if (typeitems == null)
                    typeitems = new ObservableCollection<string>();
                return typeitems;
            }

            set
            {
                typeitems = value;
            }
        }


        public delegate void TypeChanged_Handler(string type, Snapshot_Item item);
        public event TypeChanged_Handler TypeChanged;

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            SelectedType = combo.SelectedItem.ToString().ToLower();

            if (TypeChanged != null) TypeChanged(combo.SelectedItem.ToString(), this);
        }

        #endregion

        #region "Link"

        public string SelectedLink
        {
            get { return (string)GetValue(SelectedLinkProperty); }
            set { SetValue(SelectedLinkProperty, value); }
        }

        public static readonly DependencyProperty SelectedLinkProperty =
            DependencyProperty.Register("SelectedLink", typeof(string), typeof(Snapshot_Item), new PropertyMetadata(null));

        ObservableCollection<object> linkitems;
        public ObservableCollection<object> LinkItems
        {
            get
            {
                if (linkitems == null)
                    linkitems = new ObservableCollection<object>();
                return linkitems;
            }

            set
            {
                linkitems = value;
            }
        }


        private void Link_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion
 
    }
}
