using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using TH_DevicePage.Controls.Controller;

namespace TH_DevicePage.Data.Controller
{
    public static class Manager
    {

        static bool ContainsController(string address)
        {
            return address.ToLower().Contains("controller");
        }

        static string GetControllerNode(string address)
        {
            int index = address.ToLower().IndexOf("controller");
            int length = ("controller").Length;

            length = address.IndexOf('/', index) - index;
            return address.Substring(index, length);
        }

        static string GetControllerId(string address)
        {
            string node = GetControllerNode(address);

            int index = node.IndexOf('[');
            if (index >= 0)
            {
                index = node.IndexOf('\'') + 1;
                int length = node.IndexOf('\'', index) - index;

                return node.Substring(index, length);
            }

            return null;
        }

        public static List<Panel> CreateControllers(DataTable dt)
        {
            var result = new List<Panel>();

            var dv = dt.AsDataView();
            dv.RowFilter = "address LIKE '%controller%'";
            var temp_dt = dv.ToTable();

            var ids = new List<string>();

            foreach (DataRow row in temp_dt.Rows)
            {
                string address = row["address"].ToString();

                string id = GetControllerId(address);
                if (id != null && !ids.Exists(x => x == id)) ids.Add(id);
            }

            foreach (var id in ids)
            {
                var control = new Panel();
                control.Id = id;
                result.Add(control);
            }

            return result;
        }

        public static void CreateControllers(string[] addresses)
        {
            var controllerAddresses = addresses.ToList().FindAll(x => ContainsController(x));

            foreach (var address in controllerAddresses)
            {
                Console.WriteLine("Controller Address : " + address);
            }
        }

        public static void CreateController(string[] addresses)
        {
            var controllerAddresses = addresses.ToList().FindAll(x => ContainsController(x));

            foreach (var address in controllerAddresses)
            {
                Console.WriteLine("Controller Address : " + address);
            }
        }

        static void UpdateControllers()
        {




        }


    }
}
