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

using TH_Configuration;
using TH_Database;
using TH_ShiftTable;

namespace TH_History.Controls
{
    /// <summary>
    /// Interaction logic for ShiftComparison.xaml
    /// </summary>
    public partial class ShiftComparison : UserControl
    {
        public ShiftComparison()
        {
            init();
        }

        public ShiftComparison(Configuration config)
        {
            init();

            configuration = config;
            LoadTable();
        }

        void init()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Configuration configuration;

        #region "OEE Comparison"

        void LoadTable()
        {

            if (configuration != null)
            {

                ShiftConfiguration sc = ShiftConfiguration.ReadXML(configuration.ConfigurationXML);
                if (sc != null)
                {
                    DataTable table = new DataTable();
                    table.Columns.Add("Shift Name");
                    table.Columns.Add("Average OEE");
                    table.Columns.Add("Median OEE");
                    table.Columns.Add("Max OEE");
                    table.Columns.Add("Min OEE");

                    // Get entire table (may need to be sorted using user input)
                    DataTable oeeTable = TH_Database.Table.Get(configuration.Databases, "oee");

                    if (oeeTable != null)
                    {
                        foreach (Shift shift in sc.shifts)
                        {

                            DataView dv = oeeTable.AsDataView();
                            string filter = "Shift_Id LIKE '*_" + shift.id.ToString("00") + "_*'";
                            //string filter = "Shift_Id LIKE '%00%'";
                            dv.RowFilter = filter;
                            oeeTable = dv.ToTable();

                            double[] valsOEE = new double[oeeTable.Rows.Count];

                            if (oeeTable.Rows.Count > 0)
                            {
                                for (int x = 0; x <= oeeTable.Rows.Count - 1; x++)
                                {
                                    object o = oeeTable.Rows[x]["OEE"];
                                    double.TryParse(o.ToString(), out valsOEE[x]);
                                }

                                string shiftName = shift.name;
                                double averageOEE = 0;
                                double medianOEE = 0;
                                double maxOEE = 0;
                                double minOEE = 0;

                                averageOEE = valsOEE.Average();
                                medianOEE = TH_Global.Functions.Math_Functions.GetMedian(valsOEE);
                                maxOEE = valsOEE.Max();
                                minOEE = valsOEE.Min();

                                DataRow newRow = table.NewRow();
                                newRow["Shift Name"] = shiftName;
                                newRow["Average OEE"] = averageOEE.ToString("P2");
                                newRow["Median OEE"] = medianOEE.ToString("P2");
                                newRow["Max OEE"] = maxOEE.ToString("P2");
                                newRow["Min OEE"] = minOEE.ToString("P2");

                                table.Rows.Add(newRow);
                            }

                        }
                    }

                    if (table != null)
                    {
                        OEEComparisonDataView = table.AsDataView();
                    }

                }

            }

        }


        public DataView OEEComparisonDataView
        {
            get { return (DataView)GetValue(OEEComparisonDataViewProperty); }
            set { SetValue(OEEComparisonDataViewProperty, value); }
        }

        public static readonly DependencyProperty OEEComparisonDataViewProperty =
            DependencyProperty.Register("OEEComparisonDataView", typeof(DataView), typeof(ShiftComparison), new PropertyMetadata(null));

        

        #endregion
    }
}
