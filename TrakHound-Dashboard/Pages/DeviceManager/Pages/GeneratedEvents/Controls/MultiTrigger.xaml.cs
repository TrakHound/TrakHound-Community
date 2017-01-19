// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.GeneratedEvents.Controls
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
            AddTriggerClicked?.Invoke(this);

            SettingChanged?.Invoke();
        }

        private void Remove_Clicked(TrakHound_UI.Button bt)
        {
            RemoveClicked?.Invoke(this);

            SettingChanged?.Invoke();
        }
    }
}
