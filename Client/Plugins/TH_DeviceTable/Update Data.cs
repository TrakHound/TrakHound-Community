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

using System.IO;
using System.Data;
using System.Collections.ObjectModel;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Client;

namespace TH_DeviceTable
{
    public partial class DeviceTable
    {

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        void Update(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                Configuration config = de_d.data01 as Configuration;
                if (config != null)
                {
                    int index = DeviceInfos.ToList().FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                    if (index >= 0)
                    {
                        DeviceInfo info = DeviceInfos[index];

                        UpdateConnected(de_d, info);

                        UpdateOEE(de_d, info);
                    }
                }
            }
        }

        private void UpdateConnected(DataEvent_Data de_d, DeviceInfo info)
        {
            if (de_d.id.ToLower() == "statusdata_connection")
            {
                bool connected;
                bool.TryParse(de_d.data02.ToString(), out connected);

                info.Connected = connected;
            }
        }

        private void UpdateOEE(DataEvent_Data de_d, DeviceInfo info)
        {
            // OEE Table Data
            if (de_d.id.ToLower() == "statusdata_oee")
            {
                // OEE Average
                Dispatcher.BeginInvoke(new Action<object, DeviceInfo>(OEEValues_Update), Priority_Background, new object[] { de_d.data02, info });
                //OEEValues_Update(de_d.data02, info);
            }

        }

        void OEEValues_Update(object oeedata, DeviceInfo info)
        {
            var dt = oeedata as DataTable;
            if (dt != null)
            {
                var oeeData = OEEData.FromDataTable(dt);

                info.Oee = oeeData.Oee;
                info.Availability = oeeData.Availability;
                info.Performance = oeeData.Performance;
            }
        }


        private void RefreshTimer_Initialize()
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 5000;
            timer.Elapsed += RefreshTimer_Elapsed;
            timer.Enabled = true;
        }

        private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(Refresh_Timer_GUI), Priority_Background, new object[] { });
        }

        private void Refresh_Timer_GUI()
        {
            Devices_DG.Items.Refresh();
        }
    }
}
