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

namespace TrakHound_Server_Control_Panel.Controls
{
    /// <summary>
    /// Interaction logic for MessageItem.xaml
    /// </summary>
    public partial class MessageItem : UserControl
    {
        public MessageItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string URL
        {
            get { return (string)GetValue(URLProperty); }
            set { SetValue(URLProperty, value); }
        }

        public static readonly DependencyProperty URLProperty =
            DependencyProperty.Register("URL", typeof(string), typeof(MessageItem), new PropertyMetadata(null));

    }
}
