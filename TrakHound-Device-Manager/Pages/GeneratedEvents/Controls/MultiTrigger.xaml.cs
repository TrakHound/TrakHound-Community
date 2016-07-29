using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TrakHound_Device_Manager.Pages.GeneratedEvents.Controls
{
    /// <summary>
    /// Interaction logic for MultiTrigger.xaml
    /// </summary>
    public partial class MultiTrigger : UserControl
    {
        public MultiTrigger()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page.Event ParentEvent;
        public Page.Value ParentValue;
        public Page.MultiTrigger ParentMultiTrigger;

        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(MultiTrigger), new PropertyMetadata(null));


        public delegate void Clicked_Handler(MultiTrigger sender);
        public event Clicked_Handler AddTriggerClicked;
        public event Clicked_Handler RemoveClicked;

        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;


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

        private void AddTrigger_Clicked(TrakHound_UI.Button bt)
        {
            if (AddTriggerClicked != null) AddTriggerClicked(this);

            if (SettingChanged != null) SettingChanged();
        }

        private void Remove_Clicked(TrakHound_UI.Button bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);

            if (SettingChanged != null) SettingChanged();
        }
    }
}
