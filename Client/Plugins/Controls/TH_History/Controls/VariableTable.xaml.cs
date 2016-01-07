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
    public partial class VariableTable : UserControl
    {
        public VariableTable()
        {
            InitializeComponent();
            DataContext = this;
        }

        private ObservableCollection<RowData> data;
        public ObservableCollection<RowData> Data
        {
            get
            {
                if (data == null)
                    data = new ObservableCollection<RowData>();

                return data;
            }

            set
            {
                data = value;
            }
        }

        public class RowData
        {
            public string Variable { get; set; }
            public string Value { get; set; }
        }

        public void LoadData(RowData rowData)
        {
            Data.Add(rowData);
        }

    }


}
