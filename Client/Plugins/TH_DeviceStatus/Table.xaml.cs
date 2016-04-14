using System;
using System.Collections.Specialized;
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
using System.ComponentModel;

using TH_Global.Functions;

namespace TH_StatusTable
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class StatusTable : UserControl
    {
        public StatusTable()
        {
            InitializeComponent();

            

            root.DataContext = this;

            CreateColumns(0, 24);

        }

        private void CreateColumns(int first, int count)
        {
            var columns = new List<DataGridColumn>();


            // Define Cell Template
            //var cell = new FrameworkElementFactory(typeof(Controls.Cell));
            //cell.SetBinding(Controls.Cell.HourDataProperty, new Binding("HourDatas[" + x.ToString() + "]"));

            //cell.SetBinding(Controls.Cell.Data00Property, new Binding("SegmentDatas[" + x.ToString() + "-0]"));
            //cell.SetBinding(Controls.Cell.Data01Property, new Binding("SegmentDatas[" + x.ToString() + "-1]"));
            //cell.SetBinding(Controls.Cell.Data02Property, new Binding("SegmentDatas[" + x.ToString() + "-2]"));
            //cell.SetBinding(Controls.Cell.Data03Property, new Binding("SegmentDatas[" + x.ToString() + "-3]"));
            //cell.SetBinding(Controls.Cell.Data04Property, new Binding("SegmentDatas[" + x.ToString() + "-4]"));
            //cell.SetBinding(Controls.Cell.Data05Property, new Binding("SegmentDatas[" + x.ToString() + "-5]"));
            //cell.SetBinding(Controls.Cell.Data06Property, new Binding("SegmentDatas[" + x.ToString() + "-6]"));
            //cell.SetBinding(Controls.Cell.Data07Property, new Binding("SegmentDatas[" + x.ToString() + "-7]"));
            //cell.SetBinding(Controls.Cell.Data08Property, new Binding("SegmentDatas[" + x.ToString() + "-8]"));
            //cell.SetBinding(Controls.Cell.Data09Property, new Binding("SegmentDatas[" + x.ToString() + "-9]"));
            //cell.SetBinding(Controls.Cell.Data10Property, new Binding("SegmentDatas[" + x.ToString() + "-10]"));
            //cell.SetBinding(Controls.Cell.Data11Property, new Binding("SegmentDatas[" + x.ToString() + "-11]"));

            //var template = new DataTemplate();
            //template.VisualTree = cell;


            int last = first + count;
            for (var x = first; x <= last - 1; x++)
            {
                //Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    var stpw = new System.Diagnostics.Stopwatch();
                //stpw.Start();

                var column = new DataGridTemplateColumn();
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                //column.Width = 100;

                // Header
                var header = new Controls.Header();

                int hour = x;

                DateTime start = new DateTime(1, 1, 1, hour, 0, 0);
                hour += 1;
                if (hour > 23) hour -= 24;
                DateTime end = new DateTime(1, 1, 1, hour, 0, 0);

                header.Text = start.ToShortTimeString();
                header.TooltipHeader = start.ToShortTimeString();
                header.TooltipText = start.ToShortTimeString() + " - " + end.ToShortTimeString();
                column.Header = header;

                // Add Cell
                var cell = new FrameworkElementFactory(typeof(Controls.Cell));
                cell.SetBinding(Controls.Cell.HourDataProperty, new Binding("HourDatas[" + x.ToString() + "]"));

                //cell.SetBinding(Controls.Cell.Data00Property, new Binding("SegmentDatas[" + x.ToString() + "-0]"));
                //cell.SetBinding(Controls.Cell.Data01Property, new Binding("SegmentDatas[" + x.ToString() + "-1]"));
                //cell.SetBinding(Controls.Cell.Data02Property, new Binding("SegmentDatas[" + x.ToString() + "-2]"));
                //cell.SetBinding(Controls.Cell.Data03Property, new Binding("SegmentDatas[" + x.ToString() + "-3]"));
                //cell.SetBinding(Controls.Cell.Data04Property, new Binding("SegmentDatas[" + x.ToString() + "-4]"));
                //cell.SetBinding(Controls.Cell.Data05Property, new Binding("SegmentDatas[" + x.ToString() + "-5]"));
                //cell.SetBinding(Controls.Cell.Data06Property, new Binding("SegmentDatas[" + x.ToString() + "-6]"));
                //cell.SetBinding(Controls.Cell.Data07Property, new Binding("SegmentDatas[" + x.ToString() + "-7]"));
                //cell.SetBinding(Controls.Cell.Data08Property, new Binding("SegmentDatas[" + x.ToString() + "-8]"));
                //cell.SetBinding(Controls.Cell.Data09Property, new Binding("SegmentDatas[" + x.ToString() + "-9]"));
                //cell.SetBinding(Controls.Cell.Data10Property, new Binding("SegmentDatas[" + x.ToString() + "-10]"));
                //cell.SetBinding(Controls.Cell.Data11Property, new Binding("SegmentDatas[" + x.ToString() + "-11]"));

                // Set Template
                var template = new DataTemplate();
                template.VisualTree = cell;
                column.CellTemplate = template;

                columns.Add(column);

                //Devices_DG.Columns.Add(column);

                //stpw.Stop();
                //TH_Global.Logger.Log("CreateColumns() :: " + stpw.ElapsedMilliseconds + "ms");

                //}), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }

            //Devices_DG.EnableColumnVirtualization = true;
            //Devices_DG.MaxColumnWidth = 0;

            //Devices_DG.ItemsSource = null;
            //DeviceInfos.SupressNotification = true;

            //System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Loaded;

            foreach (var column in columns)
            {
                Devices_DG.Columns.Add(column);
            }




            //var delay = new ParameterTimer();
            ////delay.DataObject = column;

            //var info = new TimerInfo();
            //info.Index = 0;
            //info.Columns = columns;
            //delay.DataObject = info;

            //delay.Interval = 1000;
            //delay.Elapsed += Delay_Elapsed;
            //delay.Enabled = true;




            //    Dispatcher.BeginInvoke(new Action(() =>
            //{
            //Devices_DG.Columns.Add(column);

            //}), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            //}

            //foreach (var info in DeviceInfos)
            //{

            //    Devices_DG.Items.Add(info);


            //}

            //    Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    //Devices_DG.SetBinding(DataGrid.ItemsSourceProperty, "DeviceInfos");
            //}), priority, new object[] { });

            //DeviceInfos.SupressNotification = false;

            //Devices_DG.MaxColumnWidth = double.MaxValue;

        }

        private class TimerInfo
        {
            public int Index { get; set; }
            public List<DataGridColumn> Columns { get; set; }
        }

        private class ParameterTimer : System.Timers.Timer
        {
            public object DataObject { get; set; }
        }

        private void Delay_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (ParameterTimer)sender;
            timer.Enabled = false;

            var info = (TimerInfo)timer.DataObject;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                int index = info.Index;

                var column = info.Columns[index];

                if (!Devices_DG.Columns.Contains(column)) Devices_DG.Columns.Add(column);

            }), new object[] { });

            info.Index += 1;
            if (info.Index >= info.Columns.Count - 1) timer.Enabled = false;
            else timer.Enabled = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //CreateColumns();
        }

        private void Devices_DG_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e)
        {

        }
    }

    /// <summary>
    /// This class is a bindable encapsulation of a 2D array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableTwoDArray<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string property)
        {
            var pc = PropertyChanged;
            if (pc != null)
                pc(this, new PropertyChangedEventArgs(property));
        }

        T[,] data;

        public T this[int c1, int c2]
        {
            get { return data[c1, c2]; }
            set
            {
                data[c1, c2] = value;
                Notify(Binding.IndexerName);
            }
        }

        public string GetStringIndex(int c1, int c2)
        {
            return c1.ToString() + "-" + c2.ToString();
        }

        private void SplitIndex(string index, out int c1, out int c2)
        {
            var parts = index.Split('-');
            if (parts.Length != 2)
                throw new ArgumentException("The provided index is not valid");

            c1 = int.Parse(parts[0]);
            c2 = int.Parse(parts[1]);
        }

        public T this[string index]
        {
            get
            {
                int c1, c2;
                SplitIndex(index, out c1, out c2);
                return data[c1, c2];
            }
            set
            {
                int c1, c2;
                SplitIndex(index, out c1, out c2);
                data[c1, c2] = value;
                Notify(Binding.IndexerName);
            }
        }

        public BindableTwoDArray(int size1, int size2)
        {
            data = new T[size1, size2];
        }

        public static implicit operator T[,] (BindableTwoDArray<T> a)
        {
            return a.data;
        }
    }
}
