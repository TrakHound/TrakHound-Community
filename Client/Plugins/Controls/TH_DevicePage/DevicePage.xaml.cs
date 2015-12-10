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

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_PlugIns_Client_Control;
using TH_UserManagement.Management;
using TH_WPF.TimeLine;

namespace TH_DevicePage
{
    /// <summary>
    /// Interaction logic for DevicePage.xaml
    /// </summary>
    public partial class DevicePage : UserControl
    {
        public DevicePage()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Device Page"

        public Configuration Device { get; set; }
        

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;

        public void Load(Configuration device)
        {

            Device_Description = device.Description.Description;




        }

        public void Update(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                Configuration config = de_d.data01 as Configuration;
                if (config != null)
                {
                    if (Device != null)
                    {
                        if (config.UniqueId == Device.UniqueId)
                        {
                            // Connection
                            if (de_d.id.ToLower() == "statusdata_connection")
                            {
                                bool connected;
                                bool.TryParse(de_d.data02.ToString(), out connected);

                                if (Connected != connected)
                                {
                                    Connected = connected;

                                    Loading = false;
                                }
                            }

                            //// Snapshot Table Data
                            //if (de_d.id.ToLower() == "statusdata_snapshots")
                            //{

                            //    // Production
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Production), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Idle
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Idle), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Alert
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Alert), Priority_Context, new object[] { dd, de_d.data02 });



                            //    // Shift Info
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_Snapshots), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // OEE Timeline / Histogram
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Production Status Times
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Production Status Timeline
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });
                            //}

                            //// Shifts Table Data
                            //if (de_d.id.ToLower() == "statusdata_shiftdata")
                            //{
                            //    // Shift Info
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // OEE Timeline / Histogram
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Production Status Times
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });
                            //}

                            //// GenEvent Values
                            //if (de_d.id.ToLower() == "statusdata_geneventvalues")
                            //{
                            //    // Production Status Times
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_GenEventValues), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Production Status Timeline
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_GenEventValues), Priority_Context, new object[] { dd, de_d.data02 });
                            //}

                            //// OEE Table Data
                            //if (de_d.id.ToLower() == "statusdata_oee")
                            //{
                            //    // OEE Average
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Avg), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Current Segment OEE
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Segment), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // OEE Timeline / Histogram
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_OEEData), Priority_Context, new object[] { dd, de_d.data02 });
                            //}

                            //// Production Status (Generated Event) Table Data
                            //if (de_d.id.ToLower() == "statusdata_productionstatus")
                            //{
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_ProductionStatusData), Priority_Context, new object[] { dd, de_d.data02 });
                            //}

                        }
                    }
                }
            }
        }

        #region "Properties"

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(DevicePage), new PropertyMetadata(false));


        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(DevicePage), new PropertyMetadata(false));



        public string Device_Description
        {
            get { return (string)GetValue(Device_DescriptionProperty); }
            set { SetValue(Device_DescriptionProperty, value); }
        }

        public static readonly DependencyProperty Device_DescriptionProperty =
            DependencyProperty.Register("Device_Description", typeof(string), typeof(DevicePage), new PropertyMetadata(null));

        
        

        #endregion

        #endregion

    }
}
