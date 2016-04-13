using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

using System.Data;

using TH_DevicePage.Controls.Controller;

namespace TH_DevicePage
{
    public partial class DevicePage
    {

        ObservableCollection<Controls.CategoryPanel> _controllerPanels;
        public ObservableCollection<Controls.CategoryPanel> ControllerPanels
        {
            get
            {
                if (_controllerPanels == null)
                    _controllerPanels = new ObservableCollection<Controls.CategoryPanel>();
                return _controllerPanels;
            }

            set
            {
                _controllerPanels = value;
            }
        }

        List<Panel> controllerPanels = new List<Panel>();

        public void CreateControllers(DataTable dt)
        {
            var dv = dt.AsDataView();
            dv.RowFilter = "address LIKE '%controller%'";
            var temp_dt = dv.ToTable();

            var ids = new List<string>();

            foreach (DataRow row in temp_dt.Rows)
            {
                string address = row["address"].ToString();

                string id = Tools.GetId(address, "controller");
                if (id != null && !ids.Exists(x => x == id)) ids.Add(id);
            }

            foreach (var id in ids)
            {
                var control = new Panel();
                control.Id = id;
                controllerPanels.Add(control);

                var catPanel = new Controls.CategoryPanel();
                catPanel.Id = id;
                catPanel.Title = "Controller Status";
                catPanel.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DevicePage;component/Resources/Warning_01.png"));
                catPanel.Content = control;
                ControllerPanels.Add(catPanel);
            }
        }

        public void UpdateControllers(DataTable dt)
        {
            var dv = dt.AsDataView();
            dv.RowFilter = "address LIKE '%controller%'";
            var temp_dt = dv.ToTable();

            foreach (DataRow row in temp_dt.Rows)
            {
                string address = row["address"].ToString();

                string id = Tools.GetId(address, "controller");
                if (id != null)
                {
                    int index = controllerPanels.ToList().FindIndex(x => x.Id == id);
                    if (index >= 0) UpdateController(controllerPanels[index], row);
                }
            }
        }

        private void UpdateController(Panel panel, DataRow row)
        {
            string type = row["type"].ToString();

            switch (type.ToLower())
            {
                case "controller_mode": UpdateControllerMode(panel, row); break;
                case "emergency_stop": UpdateEmergencyStop(panel, row); break;
                case "availability": UpdateAvailabilty(panel, row); break;
                case "message": UpdateMessage(panel, row); break;

            }


        }

        private void UpdateControllerMode(Panel panel, DataRow row)
        {
            string value = row["Value"].ToString();
            panel.ControllerMode = value;
        }

        private void UpdateEmergencyStop(Panel panel, DataRow row)
        {
            string value = row["Value"].ToString();
            panel.EmergencyStop = value;
        }

        private void UpdateAvailabilty(Panel panel, DataRow row)
        {
            string value = row["Value"].ToString();
            panel.Availability = value;
        }

        private void UpdateMessage(Panel panel, DataRow row)
        {
            string value = row["Value"].ToString();
            panel.Message = value;
        }

        //private void UpdateMessage(Panel panel, DataRow row)
        //{
        //    string value = row["Value"].ToString();
        //    panel.Message = value;
        //}


    }
}
