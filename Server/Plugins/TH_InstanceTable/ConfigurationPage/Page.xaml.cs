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

using System.Data;
using System.Collections.ObjectModel;

using TH_Configuration;
using TH_Configuration.User;
using TH_PlugIns_Server;
using TH_MTC_Data.Components;
using TH_MTC_Requests;

using TH_InstanceTable.ConfigurationPage;

namespace TH_InstanceTable.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, TH_PlugIns_Server.ConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string PageName { get { return "Instance Table"; } }

        public ImageSource Image { get { return null; } }

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            ConditionItems.Clear();
            EventItems.Clear();
            SampleItems.Clear();

            configurationTable = dt;

            LoadAgentSettings(dt);

            configurationTable = dt;

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {

            string prefix = "/InstanceTable/DataItems/";

            // Save Conditions
            Table_Functions.UpdateTableValue(conditions_CHK.IsChecked.ToString(), prefix + "Conditions", dt);

            // Save Events
            Table_Functions.UpdateTableValue(events_CHK.IsChecked.ToString(), prefix + "Events", dt);

            // Save Samples
            Table_Functions.UpdateTableValue(samples_CHK.IsChecked.ToString(), prefix + "Samples", dt);


            prefix = "/InstanceTable/DataItems/Omit/";

            foreach (Controls.CheckBox chk in ConditionItems)
            {
                if (chk.IsChecked == false) Table_Functions.UpdateTableValue(null, prefix + chk.id, dt);
                else Table_Functions.RemoveTableRow(prefix + chk.id, dt);
            }

            foreach (Controls.CheckBox chk in EventItems)
            {
                if (chk.IsChecked == false) Table_Functions.UpdateTableValue(null, prefix + chk.id, dt);
                else Table_Functions.RemoveTableRow(prefix + chk.id, dt);
            }

            foreach (Controls.CheckBox chk in SampleItems)
            {
                if (chk.IsChecked == false) Table_Functions.UpdateTableValue(null, prefix + chk.id, dt);
                else Table_Functions.RemoveTableRow(prefix + chk.id, dt);
            }

        }


        DataTable configurationTable;

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

        bool Loading;

        void ChangeSetting(string address, string name, string val)
        {
            if (!Loading)
            {
                string newVal = val;
                string oldVal = null;

                if (configurationTable != null)
                {
                    oldVal = Table_Functions.GetTableValue(address, configurationTable);
                }

                if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
            }
        }

        private void events_CHK_Checked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Events", "Events", "True");
        }

        private void events_CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Events", "Events", "False");
        }

        private void samples_CHK_Checked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Samples", "Samples", "True");
        }

        private void samples_CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Samples", "Samples", "False");
        }

        private void conditions_CHK_Checked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Conditions", "Conditions", "True");
        }

        private void conditions_CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Conditions", "Conditions", "False");
        }

        #region "Omit Data Items"

        public bool OmitItemsLoading
        {
            get { return (bool)GetValue(OmitItemsLoadingProperty); }
            set { SetValue(OmitItemsLoadingProperty, value); }
        }

        public static readonly DependencyProperty OmitItemsLoadingProperty =
            DependencyProperty.Register("OmitItemsLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));
        

        ObservableCollection<Controls.CheckBox> eventitems;
        public ObservableCollection<Controls.CheckBox> EventItems
        {
            get
            {
                if (eventitems == null)
                    eventitems = new ObservableCollection<Controls.CheckBox>();
                return eventitems;
            }

            set
            {
                eventitems = value;
            }
        }

        ObservableCollection<Controls.CheckBox> sampleitems;
        public ObservableCollection<Controls.CheckBox> SampleItems
        {
            get
            {
                if (sampleitems == null)
                    sampleitems = new ObservableCollection<Controls.CheckBox>();
                return sampleitems;
            }

            set
            {
                sampleitems = value;
            }
        }

        ObservableCollection<Controls.CheckBox> conditionitems;
        public ObservableCollection<Controls.CheckBox> ConditionItems
        {
            get
            {
                if (conditionitems == null)
                    conditionitems = new ObservableCollection<Controls.CheckBox>();
                return conditionitems;
            }

            set
            {
                conditionitems = value;
            }
        }


        void AddConditionItem(DataItem dataItem)
        {
            Controls.CheckBox chk = new Controls.CheckBox();

            // Set text
            if (dataItem.name == null) chk.Content = dataItem.id;
            else chk.Content = dataItem.id + " : " + dataItem.name;

            chk.id = dataItem.id;
            chk.name = dataItem.name;

            // Set Checked based on whether it is found in 'Omit' list
            string prefix = "/InstanceTable/DataItems/Omit/";
            string s = Table_Functions.GetTableValue(prefix + dataItem.id, configurationTable);
            if (s != null) chk.IsChecked = false;
            else chk.IsChecked = true;

            chk.Checked += Omit_Checked;
            chk.Unchecked += Omit_Unchecked;

            chk.Style = (Style)TryFindResource("Android_CheckBoxStyle");

            ConditionItems.Add(chk);
        }

        void AddEventItem(DataItem dataItem)
        {
            Controls.CheckBox chk = new Controls.CheckBox();

            // Set text
            if (dataItem.name == null) chk.Content = dataItem.id;
            else chk.Content = dataItem.id + " : " + dataItem.name;

            chk.id = dataItem.id;
            chk.name = dataItem.name;

            // Set Checked based on whether it is found in 'Omit' list
            string prefix = "/InstanceTable/DataItems/Omit/";
            string s = Table_Functions.GetTableValue(prefix + dataItem.id, configurationTable);
            if (s != null) chk.IsChecked = false;
            else chk.IsChecked = true;

            chk.Checked += Omit_Checked;
            chk.Unchecked += Omit_Unchecked;

            chk.Style = (Style)TryFindResource("Android_CheckBoxStyle");

            EventItems.Add(chk);
        }

        void AddSampleItem(DataItem dataItem)
        {
            Controls.CheckBox chk = new Controls.CheckBox();

            // Set text
            if (dataItem.name == null) chk.Content = dataItem.id;
            else chk.Content = dataItem.id + " : " + dataItem.name;

            chk.id = dataItem.id;
            chk.name = dataItem.name;

            // Set Checked based on whether it is found in 'Omit' list
            string prefix = "/InstanceTable/DataItems/Omit/";
            string s = Table_Functions.GetTableValue(prefix + dataItem.id, configurationTable);
            if (s != null) chk.IsChecked = false;
            else chk.IsChecked = true;

            chk.Checked += Omit_Checked;
            chk.Unchecked += Omit_Unchecked;

            chk.Style = (Style)TryFindResource("Android_CheckBoxStyle");

            SampleItems.Add(chk);
        }


        void Omit_Checked(object sender, RoutedEventArgs e)
        {
            Controls.CheckBox chk = (Controls.CheckBox)sender;
            ChangeSetting("/InstanceTable/DataItems/Omit/" + chk.id, chk.id, "True");
        }

        void Omit_Unchecked(object sender, RoutedEventArgs e)
        {
            Controls.CheckBox chk = (Controls.CheckBox)sender;
            ChangeSetting("/InstanceTable/DataItems/Omit/" + chk.id, chk.id, "False");
        }


        void LoadAgentSettings(DataTable dt)
        {
            OmitItemsLoading = true;

            string prefix = "/Agent/";

            string ip = Table_Functions.GetTableValue(prefix + "IP_Address", dt);
            string p = Table_Functions.GetTableValue(prefix + "Port", dt);
            string devicename = Table_Functions.GetTableValue(prefix + "Device_Name", dt);

            int port;
            int.TryParse(p, out port);

            RunProbe(ip, port, devicename);
        }

        void RunProbe(string url, int port, string deviceName)
        {
            // Create Configuration with agent settings
            Configuration config = new Configuration();
            Agent_Settings agentSettings = new Agent_Settings();

            agentSettings.IP_Address = url;
            agentSettings.Port = port;
            agentSettings.Device_Name = deviceName;

            config.Agent = agentSettings;

            // Run a Probe request
            Probe probe = new Probe();
            probe.configuration = config;
            probe.ProbeFinished += probe_ProbeFinished;
            probe.ProbeError += probe_ProbeError;
            probe.Start();
        }

        void probe_ProbeFinished(ReturnData returnData, Probe sender)
        {
            if (returnData != null)
            {
                foreach (Device device in returnData.devices)
                {
                    DataItemCollection dataItems = Tools.GetDataItemsFromDevice(device);

                    // Conditions
                    foreach (DataItem dataItem in dataItems.Conditions) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddConditionItem), new object[] { dataItem });

                    // Events
                    foreach (DataItem dataItem in dataItems.Events) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddEventItem), new object[] { dataItem });

                    // Samples
                    foreach (DataItem dataItem in dataItems.Samples) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddSampleItem), new object[] { dataItem });
                }
            }

            // Set 'Loading' to false
            this.Dispatcher.BeginInvoke(new Action(OmitProbeFinished));
        }

        void OmitProbeFinished()
        {
            OmitItemsLoading = false;
        }

        
        void probe_ProbeError(Probe.ErrorData errorData)
        {

            // Set 'Loading' to false
            this.Dispatcher.BeginInvoke(new Action(OmitProbeFinished));

        }

        #endregion

    }
}
