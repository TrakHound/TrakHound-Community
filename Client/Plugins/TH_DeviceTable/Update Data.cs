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

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;

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

                    //if (dd != null)
                    //{
                    //    // Connection
                    //    if (de_d.id.ToLower() == "statusdata_connection")
                    //    {
                    //        bool connected;
                    //        bool.TryParse(de_d.data02.ToString(), out connected);

                    //        if (dd.Connected != connected)
                    //        {
                    //            dd.Connected = connected;
                    //            dd.ComparisonGroup.row.Loading = false;
                    //        }
                    //    }

                    //    // Snapshot Table Data
                    //    if (de_d.id.ToLower() == "statusdata_snapshots")
                    //    {

                    //        // Production
                    //        this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Production), Priority_Context, new object[] { dd, de_d.data02 });

                    //        // Idle
                    //        this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Idle), Priority_Context, new object[] { dd, de_d.data02 });

                    //        // Alert
                    //        this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Alert), Priority_Context, new object[] { dd, de_d.data02 });


                    //        // Production Status
                    //        this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatus), Priority_Context, new object[] { dd, de_d.data02 });


                    //        //// Shift Info
                    //        //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_Snapshots), Priority_Context, new object[] { dd, de_d.data02 });

                    //        //// OEE Timeline / Histogram
                    //        //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                    //        //// Production Status Times
                    //        //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                    //        // Current Program
                    //        this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProgram_Current), Priority_Context, new object[] { dd, de_d.data02 });

                    //        // Previous Program
                    //        this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProgram_Previous), Priority_Context, new object[] { dd, de_d.data02 });


                    //        // Production Status Timeline
                    //        //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });
                    //    }

                    //    //// Shifts Table Data
                    //    //if (de_d.id.ToLower() == "statusdata_shiftdata")
                    //    //{
                    //    //    // Shift Info
                    //    //    //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                    //    //    // OEE Timeline / Histogram
                    //    //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                    //    //    // Production Status Times
                    //    //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });
                    //    //}

                    //    //// GenEvent Values
                    //    //if (de_d.id.ToLower() == "statusdata_geneventvalues")
                    //    //{
                    //    //    // Production Status Times
                    //    //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_GenEventValues), Priority_Context, new object[] { dd, de_d.data02 });

                    //    //    // Production Status Timeline
                    //    //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_GenEventValues), Priority_Context, new object[] { dd, de_d.data02 });
                    //    //}

                    //    // OEE Table Data
                    //    if (de_d.id.ToLower() == "statusdata_oee")
                    //    {
                    //        // OEE Average
                    //        this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Avg), Priority_Context, new object[] { dd, de_d.data02 });

                    //        // Current Segment OEE
                    //        this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Segment), Priority_Context, new object[] { dd, de_d.data02 });

                    //        // OEE Timeline / Histogram
                    //        //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_OEEData), Priority_Context, new object[] { dd, de_d.data02 });
                    //    }

                    //    //// Production Status (Generated Event) Table Data
                    //    //if (de_d.id.ToLower() == "statusdata_productionstatus")
                    //    //{
                    //    //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_ProductionStatusData), Priority_Context, new object[] { dd, de_d.data02 });
                    //    //}

                    //}
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
                this.Dispatcher.BeginInvoke(new Action<object, DeviceInfo>(OEEValues_Update), Priority_Context, new object[] { de_d.data02, info });

            }

        }

        void OEEValues_Update(object oeedata, DeviceInfo info)
        {
            var dt = oeedata as DataTable;
            if (dt != null)
            {
                var oeeData = OEEData.FromDataTable(dt);

                info.OEE = oeeData.Oee;
                info.Availability = oeeData.Availability;
                info.Performance = oeeData.Performance;

            }
        }
    }
}
