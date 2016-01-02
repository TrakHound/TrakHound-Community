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

namespace TH_InstanceTable
{
    /// <summary>
    /// Interaction logic for ConfigPage.xaml
    /// </summary>
    public partial class Configuration_Page : UserControl, ConfigurationPage
    {
        public Configuration_Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string PageName { get { return "Instance Table"; } }

        public ImageSource Image { get { return null; } }

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            configurationTable = dt;

            LoadAgentSettings(dt);
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

            foreach (CheckBox chk in ConditionItems)
            {
                if (chk.IsChecked == false) Table_Functions.UpdateTableValue(null, prefix + "chk", dt);
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

        private void events_CHK_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void events_CHK_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void samples_CHK_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void samples_CHK_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void conditions_CHK_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void conditions_CHK_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        #region "Omit Data Items"

        ObservableCollection<CheckBox> eventitems;
        public ObservableCollection<CheckBox> EventItems
        {
            get
            {
                if (eventitems == null)
                    eventitems = new ObservableCollection<CheckBox>();
                return eventitems;
            }

            set
            {
                eventitems = value;
            }
        }

        ObservableCollection<CheckBox> sampleitems;
        public ObservableCollection<CheckBox> SampleItems
        {
            get
            {
                if (sampleitems == null)
                    sampleitems = new ObservableCollection<CheckBox>();
                return sampleitems;
            }

            set
            {
                sampleitems = value;
            }
        }

        ObservableCollection<CheckBox> conditionitems;
        public ObservableCollection<CheckBox> ConditionItems
        {
            get
            {
                if (conditionitems == null)
                    conditionitems = new ObservableCollection<CheckBox>();
                return conditionitems;
            }

            set
            {
                conditionitems = value;
            }
        }


        void LoadAgentSettings(DataTable dt)
        {
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
        }

        void AddEventItem(DataItem dataItem)
        {
            CheckBox chk = new CheckBox();

            // Set text
            if (dataItem.name == null) chk.Content = dataItem.id;
            else chk.Content = dataItem.id + " : " + dataItem.name;

            // Set Checked based on whether it is found in 'Omit' list
            string prefix = "/InstanceTable/DataItems/Omit/";
            string s = Table_Functions.GetTableValue(prefix + dataItem.id, configurationTable);
            if (s != null) chk.IsChecked = false;
            else chk.IsChecked = true;
        
            chk.Style = (Style)TryFindResource("Android_CheckBoxStyle");

            EventItems.Add(chk);
        }

        void AddSampleItem(DataItem dataItem)
        {
            CheckBox chk = new CheckBox();

            // Set text
            if (dataItem.name == null) chk.Content = dataItem.id;
            else chk.Content = dataItem.id + " : " + dataItem.name;

            // Set Checked based on whether it is found in 'Omit' list
            string prefix = "/InstanceTable/DataItems/Omit/";
            string s = Table_Functions.GetTableValue(prefix + dataItem.id, configurationTable);
            if (s != null) chk.IsChecked = false;
            else chk.IsChecked = true;

            chk.Style = (Style)TryFindResource("Android_CheckBoxStyle");
        
            SampleItems.Add(chk);
        }

        void AddConditionItem(DataItem dataItem)
        {
            CheckBox chk = new CheckBox();

            // Set text
            if (dataItem.name == null) chk.Content = dataItem.id;
            else chk.Content = dataItem.id + " : " + dataItem.name;

            // Set Checked based on whether it is found in 'Omit' list
            string prefix = "/InstanceTable/DataItems/Omit/";
            string s = Table_Functions.GetTableValue(prefix + dataItem.id, configurationTable);
            if (s != null) chk.IsChecked = false;
            else chk.IsChecked = true;

            chk.Style = (Style)TryFindResource("Android_CheckBoxStyle");
        
            ConditionItems.Add(chk);
        }

        void probe_ProbeError(Probe.ErrorData errorData)
        {

        }

        #endregion

    }
}
