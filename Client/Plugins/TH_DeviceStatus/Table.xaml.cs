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

            CreateColumns();
        }

        private const double MINIMUM_COLUMN_WIDTH = 40;

        private void CreateColumns()
        {
            for (var x = 0; x <= 23; x++)
            {
                var column = new DataGridTemplateColumn();

                // Header
                var header = new Controls.Header();
                header.Text = x.ToString();
                header.TooltipHeader = x.ToString();
                header.TooltipText = x.ToString() + " Text";
                column.Header = header;

                // Cell Content
                //var cell = new Controls.Cell();
                //cell.SetBinding(Controls.Cell.StatusLevelProperty, "StatusLevel");

                //var factory = new FrameworkElementFactory(typeof(Controls.Cell));

                var cellHolder = new FrameworkElementFactory(typeof(Controls.Cell));
                cellHolder.SetBinding(Controls.Cell.StatusLevelProperty, new Binding("StatusLevel[" + x.ToString() + "]"));
                //factory.AppendChild(cellHolder);





                //var template = new DataTemplate();
                //template.VisualTree = cell;
                var template = new DataTemplate();
                template.VisualTree = cellHolder;
                //template.Resources.Add("cell", cell);

                column.CellTemplate = template;
                column.MinWidth = MINIMUM_COLUMN_WIDTH;

                Devices_DG.Columns.Add(column);

            }
        }
    }
}
