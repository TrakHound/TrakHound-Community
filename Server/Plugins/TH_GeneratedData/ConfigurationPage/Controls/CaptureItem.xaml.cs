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

namespace TH_GeneratedData.ConfigurationPage.Controls
{
    /// <summary>
    /// Interaction logic for CaptureItem.xaml
    /// </summary>
    public partial class CaptureItem : UserControl
    {
        public CaptureItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page.Event ParentEvent;
        public Page.CaptureItem ParentCaptureItem;


        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(CaptureItem), new PropertyMetadata(null));

        


        public object CaptureName
        {
            get { return (object)GetValue(CaptureNameProperty); }
            set { SetValue(CaptureNameProperty, value); }
        }

        public static readonly DependencyProperty CaptureNameProperty =
            DependencyProperty.Register("CaptureName", typeof(object), typeof(CaptureItem), new PropertyMetadata(null));


        public object SelectedLink
        {
            get { return (object)GetValue(SelectedLinkProperty); }
            set { SetValue(SelectedLinkProperty, value); }
        }

        public static readonly DependencyProperty SelectedLinkProperty =
            DependencyProperty.Register("SelectedLink", typeof(object), typeof(CaptureItem), new PropertyMetadata(null));


        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        private void name_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentCaptureItem != null) ParentCaptureItem.name = txt.Text;

            if (txt.IsKeyboardFocused) if (SettingChanged != null) SettingChanged();
        }

        private void link_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                if (ParentCaptureItem != null) ParentCaptureItem.link = combo.SelectedItem.ToString();
            }

            if (combo.IsKeyboardFocused || combo.IsMouseCaptured) if (SettingChanged != null) SettingChanged();
            
        }

        private void link_COMBO_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                if (ParentCaptureItem != null) ParentCaptureItem.link = combo.SelectedItem.ToString();
            }

            if (combo.IsKeyboardFocused || combo.IsMouseCaptured) if (SettingChanged != null) SettingChanged();
        }

        public delegate void RemoveClicked_Handler(CaptureItem ci);
        public event RemoveClicked_Handler RemoveClicked;

        private void Remove_Clicked(TH_WPF.Button_04 bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }
    }
}
