using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Client;

namespace TH_StatusData
{
    public partial class StatusData : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Status Data"; } }

        public string Description { get { return "Retrieve Data from database(s) related to device status"; } }

        public ImageSource Image { get { return null; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/statusdata-appinfo.json"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugins { get { return false; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Closing() { Update_Stop(); }

        bool closing = false;

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        public event TH_Plugins_Client.PluginTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Devices"

        List<Configuration> devices;
        public List<Configuration> Devices
        {
            get { return devices; }
            set
            {
                devices = value;

                Update_Start();
            }
        }

        #endregion

        #region "Options"

        public Page Options { get; set; }

        #endregion

    }
}
