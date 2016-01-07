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

using System.Collections.ObjectModel;

namespace TH_History.Controls
{
    /// <summary>
    /// Interaction logic for ShiftInfo.xaml
    /// </summary>
    public partial class ShiftInfo : UserControl
    {
        public ShiftInfo()
        {
            InitializeComponent();
            DataContext = this;
        }

        private ObservableCollection<Controls.VariableTable> tables;
        public ObservableCollection<Controls.VariableTable> Tables
        {
            get
            {
                if (tables == null)
                    tables = new ObservableCollection<Controls.VariableTable>();

                return tables;
            }

            set
            {
                tables = value;
            }
        }

        public void LoadData(Data data)
        {
            var vt = new Controls.VariableTable();
            Controls.VariableTable.RowData rowData;

            rowData = new VariableTable.RowData();
            rowData.Variable = "Average OEE";
            rowData.Value = data.OEEAverage.ToString();
            vt.LoadData(rowData);

            rowData = new VariableTable.RowData();
            rowData.Variable = "Median OEE";
            rowData.Value = data.OEEMedian.ToString();
            vt.LoadData(rowData);

            rowData = new VariableTable.RowData();
            rowData.Variable = "Standard Deviation OEE";
            rowData.Value = data.OEEStandardDeviation.ToString();
            vt.LoadData(rowData);

            Tables.Add(vt);
        }

        public void AddShift(ShiftData shiftData)
        {
            var vt = new Controls.VariableTable();

            Controls.VariableTable.RowData rowData;

            rowData = new VariableTable.RowData();
            rowData.Variable = "Average OEE";
            rowData.Value = shiftData.OEEAverage.ToString();
            vt.LoadData(rowData);

            rowData = new VariableTable.RowData();
            rowData.Variable = "Median OEE";
            rowData.Value = shiftData.OEEMedian.ToString();
            vt.LoadData(rowData);

            rowData = new VariableTable.RowData();
            rowData.Variable = "Standard Deviation OEE";
            rowData.Value = shiftData.OEEStandardDeviation.ToString();
            vt.LoadData(rowData);

            Tables.Add(vt);
        }


        public class Data
        {
            // OEE
            public double OEEAverage { get; set; }
            public double OEEMedian { get; set; }
            public double OEEStandardDeviation { get; set; }

            public List<ShiftData> shiftData { get; set; }
        }

        public class ShiftData
        {
            public string ShiftName { get; set; }
            public string ShiftTimes { get; set; }

            // OEE
            public double OEEAverage { get; set; }
            public double OEEMedian { get; set; }
            public double OEEStandardDeviation { get; set; }
        }

    }
}
