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
using TH_Plugins;
using TH_Plugins.Client;

namespace TH_DeviceTable
{
    public partial class DeviceTable
    {

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        void Update(EventData data)
        {
            if (data != null)
            {
                Configuration config = data.data01 as Configuration;
                if (config != null)
                {
                    int index = DeviceInfos.ToList().FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                    if (index >= 0)
                    {
                        DeviceInfo info = DeviceInfos[index];

                        UpdateConnected(data, info);

                        UpdateOEE(data, info);

                        UpdateProductionStatus(data, info);

                        UpdateCNCControllerStatus(data, info);

                        if (Devices_DG.Items.NeedsRefresh) Devices_DG.Items.Refresh();
                    }

                    //Devices_DG.UpdateLayout();
                }
            }
        }

        private void UpdateConnected(EventData data, DeviceInfo info)
        {
            if (data.id.ToLower() == "statusdata_connection")
            {
                bool connected;
                bool.TryParse(data.data02.ToString(), out connected);

                info.Connected = connected;
            }
        }

        private void UpdateOEE(EventData data, DeviceInfo info)
        {
            if (data.id.ToLower() == "statusdata_oee")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var dt = data.data02 as DataTable;
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

        private void UpdateProductionStatus(EventData data, DeviceInfo info)
        {
            if (data.id.ToLower() == "statusdata_snapshots")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    info.ProductionStatus = GetTableValue(data.data02, "Production Status");
                }), Priority_Background, new object[] { });
            }
        }

        private void UpdateCNCControllerStatus(EventData data, DeviceInfo info)
        {
            if (data.id.ToLower() == "statusdata_snapshots")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    info.EmergencyStop = GetTableValue(data.data02, "Emergency Stop");
                    info.ControllerMode = GetTableValue(data.data02, "Controller Mode");
                    info.ExecutionMode = GetTableValue(data.data02, "Execution Mode");
                    info.Alarm = GetTableValue(data.data02, "Alarm");
                    info.PartCount = GetTableValue(data.data02, "PartCount");
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
