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

                        UpdateProductionStatus(de_d, info);

                        UpdateCNCControllerStatus(de_d, info);
                    }

                    Devices_DG.UpdateLayout();
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
            if (de_d.id.ToLower() == "statusdata_oee")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var dt = de_d.data02 as DataTable;
                    if (dt != null)
                    {
                        var oeeData = OEEData.FromDataTable(dt);

                        info.Oee = oeeData.Oee;
                        info.Availability = oeeData.Availability;
                        info.Performance = oeeData.Performance;
                    }
                }), Priority_Background, new object[] { });
            }

        }

        private void UpdateProductionStatus(DataEvent_Data de_d, DeviceInfo info)
        {
            if (de_d.id.ToLower() == "statusdata_snapshots")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    info.ProductionStatus = GetTableValue(de_d.data02, "Production Status");
                }), Priority_Background, new object[] { });
            }
        }

        private void UpdateCNCControllerStatus(DataEvent_Data de_d, DeviceInfo info)
        {
            if (de_d.id.ToLower() == "statusdata_snapshots")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    info.EmergencyStop = GetTableValue(de_d.data02, "Emergency Stop");
                    info.ControllerMode = GetTableValue(de_d.data02, "Controller Mode");
                    info.ExecutionMode = GetTableValue(de_d.data02, "Execution Mode");
                    info.Alarm = GetTableValue(de_d.data02, "Alarm");
                    info.PartCount = GetTableValue(de_d.data02, "PartCount");
                }), Priority_Background, new object[] { });
            }
        }

        private string GetTableValue(object obj, string key)
        {
            var dt = obj as DataTable;
            if (dt != null)
            {
                return DataTable_Functions.GetTableValue(dt, "name", key, "value");
            }
            return null;
        }

    }
}
