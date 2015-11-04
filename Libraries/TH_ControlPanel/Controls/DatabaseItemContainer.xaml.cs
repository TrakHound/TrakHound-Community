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

using TH_WPF;

namespace TH_DeviceManager.Controls
{
    /// <summary>
    /// Interaction logic for DatabaseItemContainer.xaml
    /// </summary>
    public partial class DatabaseItemContainer : UserControl
    {
        public DatabaseItemContainer()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string prefix;

        public CollapseButton collapseButton;

        public object ItemContent
        {
            get { return (object)GetValue(ItemContentProperty); }
            set { SetValue(ItemContentProperty, value); }
        }

        public delegate void Clicked_Handler(DatabaseItemContainer item);
        public event Clicked_Handler RemoveClicked;


        public static readonly DependencyProperty ItemContentProperty =
            DependencyProperty.Register("ItemContent", typeof(object), typeof(DatabaseItemContainer), new PropertyMetadata(null));

        private void Remove_Clicked(Button bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }

    }
}
