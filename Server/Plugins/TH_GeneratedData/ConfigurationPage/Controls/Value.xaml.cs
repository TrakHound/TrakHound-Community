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
    /// Interaction logic for Value.xaml
    /// </summary>
    public partial class Value : UserControl
    {
        public Value()
        {
            InitializeComponent();
            DataContext = this;
        }

        public TH_GeneratedData.ConfigurationPage.Page.Event ParentEvent;
        public TH_GeneratedData.ConfigurationPage.Page.Value ParentValue;

        ObservableCollection<Trigger> triggers;
        public ObservableCollection<Trigger> Triggers
        {
            get
            {
                if (triggers == null)
                    triggers = new ObservableCollection<Trigger>();
                return triggers;
            }

            set
            {
                triggers = value;
            }
        }


        public string ValueName
        {
            get { return (string)GetValue(ValueNameProperty); }
            set { SetValue(ValueNameProperty, value); }
        }

        public static readonly DependencyProperty ValueNameProperty =
            DependencyProperty.Register("ValueName", typeof(string), typeof(Value), new PropertyMetadata(null));


        public string TriggerCount
        {
            get { return (string)GetValue(TriggerCountProperty); }
            set { SetValue(TriggerCountProperty, value); }
        }

        public static readonly DependencyProperty TriggerCountProperty =
            DependencyProperty.Register("TriggerCount", typeof(string), typeof(Value), new PropertyMetadata(null));

        #region "Help"

        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }


        #endregion

        private void CHK_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CHK_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        public delegate void Clicked_Handler(Value val);
        public event Clicked_Handler AddTriggerClicked;
        public event Clicked_Handler RemoveClicked;

        private void AddTrigger_Clicked(TH_WPF.Button_04 bt)
        {
            if (AddTriggerClicked != null) AddTriggerClicked(this);

            //if (ParentValue != null)
            //{
            //    Page.Trigger t = new Page.Trigger();
            //    ParentValue.triggers.Add(t);

            //    Trigger trigger = new Trigger();
            //    trigger.ParentTrigger = t;
            //    Triggers.Add(trigger);
            //}
        }

        private void Remove_Clicked(TH_WPF.Button_02 bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }

        private void ValueName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ParentValue != null) if (ParentValue.result != null) ParentValue.result.value = ((TextBox)sender).Text;
        }

        private void EditValue_Clicked(TH_WPF.Button_02 bt)
        {

        }

    }
}
