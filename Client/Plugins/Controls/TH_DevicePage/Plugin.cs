using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using System.Collections.ObjectModel;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_PlugIns_Client_Control;
using TH_UserManagement.Management;
using TH_WPF.TimeLine;

namespace TH_DevicePage
{
    public class Plugin : Control_PlugIn
    {

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Device Page"; } }

        public string Description { get { return "View Device Details"; } }

        public ImageSource Image { get { return null; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugIns { get { return false; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return false; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        //public void Update(ReturnData rd)
        //{
        //    this.Dispatcher.BeginInvoke(new Action<ReturnData>(Update_GUI), Priority_Background, new object[] { rd });
        //}

        public void Closing() { }

        //public void Show()
        //{
        //    if (ShowRequested != null)
        //    {
        //        PluginShowInfo info = new PluginShowInfo();
        //        info.Page = this;
        //        ShowRequested(info);
        //    }
        //}

        public void Show(object page, string title)
        {
            if (ShowRequested != null)
            {
                PluginShowInfo info = new PluginShowInfo();
                info.PageTitle = title;
                info.Page = page;
                ShowRequested(info);
            }
        }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                if (de_d.id.ToLower() == "deviceselected" && de_d.data01 != null)
                {
                    Configuration config = de_d.data01 as Configuration;

                    DevicePage page = Pages.ToList().Find(x => x.Device.UniqueId == config.UniqueId);
                    if (page == null)
                    {
                        page = new DevicePage();
                        page.ParentPlugin = this;
                        page.Device = config;
                        page.Load(config);
                        Pages.Add(page);
                    }

                    string header = config.Description.Description;
                    if (config.Description.Device_ID != null) header += " (" + config.Description.Device_ID + ")";

                    Show(page, header);
                }
                else
                {
                    foreach (DevicePage page in Pages)
                    {
                        page.Update(de_d);
                    }
                }

            }
        }

        public event DataEvent_Handler DataEvent;

        public event PlugInTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        ObservableCollection<Configuration> Devices = new ObservableCollection<Configuration>();

        public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("DeviceCompare :: Devices :: " + e.Action.ToString());

            if (e.NewItems != null)
            {
                foreach (Configuration newConfig in e.NewItems)
                {
                    Devices.Add(newConfig);
                }
            }

            if (e.OldItems != null)
            {
                foreach (Configuration oldConfig in e.OldItems)
                {
                    Devices.Add(oldConfig);
                }
            }
        }

        //List<Configuration> devices;
        //public List<Configuration> Devices
        //{
        //    get { return devices; }
        //    set
        //    {
        //        devices = value;

        //        //if (devices != null)
        //        //{
        //        //    DeviceDisplays = new List<DeviceDisplay>();
        //        //    ColumnHeaders.Clear();
        //        //    Columns.Clear();

        //        //    foreach (Configuration device in devices)
        //        //    {
        //        //        CreateDeviceDisplay(device);
        //        //    }
        //        //}
        //    }
        //}

        //private int lSelectedDeviceIndex;
        //public int SelectedDeviceIndex
        //{
        //    get { return lSelectedDeviceIndex; }

        //    set
        //    {
        //        lSelectedDeviceIndex = value;

        //        // Unselect other headers and columns
        //        for (int x = 0; x <= DeviceDisplays.Count - 1; x++) if (x != lSelectedDeviceIndex)
        //            {
        //                DeviceDisplays[x].ComparisonGroup.header.IsSelected = false;
        //                DeviceDisplays[x].ComparisonGroup.column.IsSelected = false;
        //            }

        //        // Select header and column at SelectedDeviceIndex
        //        DeviceDisplays[lSelectedDeviceIndex].ComparisonGroup.header.IsSelected = true;
        //        DeviceDisplays[lSelectedDeviceIndex].ComparisonGroup.column.IsSelected = true;

        //    }
        //}

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        #region "User"

        UserConfiguration currentuser = null;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                //if (currentuser != null) LoggedIn = true;
                //else LoggedIn = false;
            }
        }

        public Database_Settings UserDatabaseSettings { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        List<DevicePage> Pages = new List<DevicePage>();

    }
}
