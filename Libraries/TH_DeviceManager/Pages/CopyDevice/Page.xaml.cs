using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Data;

using TH_Configuration;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TH_DeviceManager.Pages.CopyDevice
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        Configuration configuration;

        public DataTable configurationtable;

        public UserConfiguration currentuser;

        public Database_Settings userDatabaseSettings;

        public DeviceManager devicemanager;

        public void LoadConfiguration(Configuration config)
        {
            configuration = config;
        }

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Page), new PropertyMetadata(false));




        public string Copies
        {
            get { return (string)GetValue(CopiesProperty); }
            set { SetValue(CopiesProperty, value); }
        }

        public static readonly DependencyProperty CopiesProperty =
            DependencyProperty.Register("Copies", typeof(string), typeof(Page), new PropertyMetadata("1"));


        public string DeviceIds
        {
            get { return (string)GetValue(DeviceIdsProperty); }
            set { SetValue(DeviceIdsProperty, value); }
        }

        public static readonly DependencyProperty DeviceIdsProperty =
            DependencyProperty.Register("DeviceIds", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Find
        {
            get { return (string)GetValue(FindProperty); }
            set { SetValue(FindProperty, value); }
        }

        public static readonly DependencyProperty FindProperty =
            DependencyProperty.Register("Find", typeof(string), typeof(Page), new PropertyMetadata(null));



        public string Replace
        {
            get { return (string)GetValue(ReplaceProperty); }
            set { SetValue(ReplaceProperty, value); }
        }

        public static readonly DependencyProperty ReplaceProperty =
            DependencyProperty.Register("Replace", typeof(string), typeof(Page), new PropertyMetadata(null));



        private void Copy_Clicked(TH_WPF.Button bt)
        {
            if (configuration != null) CopyDevice(configuration);
        }

        Thread CopyDevice_THREAD;

        class CopyDevice_Info
        {
            public Configuration Configuration { get; set; }
            public int Copies { get; set; }
            public string[] DeviceIds { get; set; }
            public string Find { get; set; }
            public string[] Replace { get; set; }
        }

        void CopyDevice(Configuration config)
        {

            if (Copies != null)
            {
                int copies = 0;
                if (int.TryParse(Copies, out copies))
                {
                    if (copies > 0)
                    {
                        var info = new CopyDevice_Info();
                        info.Configuration = config;
                        info.Copies = copies;

                        if (DeviceIds != null)
                        {
                            info.DeviceIds = DeviceIds.Split(',');
                        }
                        
                        // Create Replace Array
                        if (Find != null && Replace != null)
                        {
                            info.Find = Find;

                            string[] replace = Replace.Split(',');

                            info.Replace = replace;
                        }

                        if (CopyDevice_THREAD != null) CopyDevice_THREAD.Abort();

                        CopyDevice_THREAD = new Thread(new ParameterizedThreadStart(CopyDevice_Worker));
                        CopyDevice_THREAD.Start(info);
                    }
                }
            }

           
        }

        void CopyDevice_Worker(object o)
        {
            bool success = false;

            if (o != null)
            {
                var info = (CopyDevice_Info)o;

                //var config = info.Configuration;

                if (currentuser != null)
                {
                    // Iterate through copies
                    for (var x = 0; x <= info.Copies - 1; x++)
                    {
                        Configuration copyConfig = Configuration.ReadConfigFile(info.Configuration.ConfigurationXML);

                        if (info.Replace != null)
                        {
                            if (x <= info.Replace.Length - 1)
                            {
                                if (info.Replace[x] != null)
                                {
                                    string xmlText = XML_Functions.XmlDocumentToString(copyConfig.ConfigurationXML);
                                    xmlText = xmlText.Replace(info.Find, info.Replace[x]);

                                    var xml = XML_Functions.StringToXmlDocument(xmlText);

                                    copyConfig = Configuration.ReadConfigFile(xml);
                                }
                            }                            
                        }

                        if (info.DeviceIds != null)
                        {
                            if (x <= info.DeviceIds.Length - 1)
                            {
                                if (info.DeviceIds[x] != null)
                                {
                                    copyConfig.Description.Device_ID = info.DeviceIds[x];
                                    XML_Functions.SetInnerText(copyConfig.ConfigurationXML, "Description/Device_ID", info.DeviceIds[x]);
                                }
                            }
                        }

                        success = Configurations.AddConfigurationToUser(currentuser, copyConfig, userDatabaseSettings);
                    } 
                }
                else
                {
                    success = false;
                }

                //this.Dispatcher.BeginInvoke(new Action<bool, Configuration>(CopyDevice_GUI), priority, new object[] { success, config });
            }
        }

        //void CopyDevice_GUI(bool success, Configuration config)
        //{
        //    if (success) AddDeviceButton(config);
        //    else
        //    {
        //        TH_WPF.MessageBox.Show("Error during Device Copy. Please try again", "Device Copy Error", MessageBoxButtons.Ok);
        //    }
        //}







        




        //private void Share_Clicked(TH_WPF.Button bt)
        //{
        //    Shared.SharedListItem item = new Shared.SharedListItem();

        //    item.description = Description;
        //    item.device_type = Type;
        //    item.manufacturer = Manufacturer;
        //    item.model = Model;
        //    item.controller = Controller;
        //    item.author = Author;

        //    item.upload_date = DateTime.Now;

        //    item.version = "1.0.0.0";

        //    item.image_url = imageFileName;

        //    item.version = Version;
        //    item.tags = Tags;
        //    item.dependencies = Dependencies;

        //    if (currentuser != null && configuration != null && configurationtable != null)
        //    {
        //        item.id = configuration.UniqueId;

        //        Share(item);
        //    }
        //}


        //class Share_Info
        //{
        //    public Shared.SharedListItem item { get; set; }
        //    public DataTable dt { get; set; }
        //}

        //Thread share_THREAD;

        //public void Share(Shared.SharedListItem item)
        //{
        //    Loading = true;

        //    DataTable dt = configurationtable.Copy();

        //    // Set Export Options
        //    if (export_agent_CHK.IsChecked != true)
        //    {
        //        string agentprefix = "/Agent/";

        //        // Clear all generated event rows first (so that Ids can be sequentially assigned)
        //        string filter = "address LIKE '" + agentprefix + "*'";
        //        DataView dv = dt.AsDataView();
        //        dv.RowFilter = filter;
        //        DataTable temp_dt = dv.ToTable();
        //        foreach (DataRow row in temp_dt.Rows)
        //        {
        //            DataRow dbRow = dt.Rows.Find(row["address"]);
        //            if (dbRow != null) dt.Rows.Remove(dbRow);
        //        }
        //    }


        //    if (export_databases_CHK.IsChecked != true)
        //    {
        //        string databaseprefix1 = "/Databases/";
        //        string databaseprefix2 = "/Databases_Server/";
        //        string databaseprefix3 = "/Databases_Client/";

        //        // Clear all generated event rows first (so that Ids can be sequentially assigned)
        //        string filter = "address LIKE '" + databaseprefix1 + "*' OR address LIKE '" + databaseprefix2 + "*' OR address LIKE '" + databaseprefix3 + "*'";
        //        DataView dv = dt.AsDataView();
        //        dv.RowFilter = filter;
        //        DataTable temp_dt = dv.ToTable();
        //        foreach (DataRow row in temp_dt.Rows)
        //        {
        //            DataRow dbRow = dt.Rows.Find(row["address"]);
        //            if (dbRow != null) dt.Rows.Remove(dbRow);
        //        }
        //    }

        //    Share_Info info = new Share_Info();
        //    info.item = item;
        //    info.dt = dt;

        //    if (share_THREAD != null) share_THREAD.Abort();

        //    share_THREAD = new Thread(new ParameterizedThreadStart(Share_Worker));
        //    share_THREAD.Start(info);
        //}

        //void Share_Worker(object o)
        //{
        //    bool success = false;

        //    if (o != null)
        //    {
        //        Share_Info info = (Share_Info)o;

        //        string tablename = "shared_" + String_Functions.RandomString(20);

        //        // Save Shared Tablename
        //        Table_Functions.UpdateTableValue(tablename, "/SharedTableName", info.dt);

        //        success = Shared.CreateSharedConfiguration(currentuser, tablename, info.dt, info.item);

        //    }

        //    this.Dispatcher.BeginInvoke(new Action<bool>(Share_Finished), priority, new object[] { success });
        //}

        //void Share_Finished(bool success)
        //{
        //    Loading = false;
        //}

    }
}
