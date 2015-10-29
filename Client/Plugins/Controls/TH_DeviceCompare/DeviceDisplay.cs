using System;
using System.Collections.Generic;
using System.Windows.Media;

using TH_Configuration;

using TH_DeviceCompare.Components;

namespace TH_DeviceCompare
{
    class DeviceDisplay
    {
        public DeviceDisplay()
        {
            Init();
        }
        public DeviceDisplay(Configuration config)
        {
            configuration = config;
            Init();
        }

        void Init()
        {
            DataItems_Init();
            ComparisonGroup = new Comparison_Group();
        }

        private bool connected;
        public bool Connected
        {
            get { return connected; }
            set
            {
                connected = value;
                if (ConnectedChanged != null) ConnectedChanged(this);
            }
        }
        public delegate void ConnectedChanged_Handler(DeviceDisplay dd);
        public event ConnectedChanged_Handler ConnectedChanged;


        private string connectionstatus;
        public string ConnectionStatus
        {
            get { return connectionstatus; }
            set
            {
                connectionstatus = value;
                if (ConnectionStatus != null) ConnectionStatusChanged(this);
            }
        }
        public event ConnectedChanged_Handler ConnectionStatusChanged;
        

        public int connectionAttempts { get; set; }
        public const int maxConnectionAttempts = 5;

        public Configuration configuration;

        public class DataItem
        {
            public string id { get; set; }

            public string header { get; set; }

            public object data { get; set; }

            public int rowHeight { get; set; }
        }

        public class Comparison_Group
        {
            public Header header;

            public Column column;
        }

        public List<DataItem> DataItems;

        void DataItems_Init()
        {
            DataItems = new List<DataItem>();

            DataItem dd;

            dd = new DataItem();
            dd.id = "ShiftInfo";
            dd.header = "Shift Info";
            DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "OEEAverage";
            dd.header = "Average Shift OEE";
            DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "OEESegment";
            dd.header = "Shift Segment OEE";
            DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "OEETimeLine";
            dd.header = "OEE Timeline";
            DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "productionstatustimes";
            dd.header = "Production Status Times";
            DataItems.Add(dd);

            //dd = new DataItem();
            //dd.id = "CNCInfo";
            //dd.header = "CNC Info";
            //DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "ProductionStatusTimeline";
            dd.header = "Production Status Timeline";
            DataItems.Add(dd);

        }

        public Comparison_Group ComparisonGroup;

        public ImageSource Logo { get; set; }

        public bool Alert { get; set; }
        public bool Break { get; set; }
        public int Index { get; set; }
        public bool IsConnected { get; set; }
    }
}
