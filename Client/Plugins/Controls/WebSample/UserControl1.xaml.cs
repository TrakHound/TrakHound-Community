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

using System.IO;

using Awesomium.Core;

using CRI;

using TH_Configuration;
using TH_Device_Client;
using TH_Global;
using TH_PlugIns_Client_Control;

namespace WebSample
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, Control_PlugIn
    {

        private CustomResourceInterceptor cri;

        public UserControl1()
        {



            InitializeComponent();

            //browser.Navigate("http://www.google.com");

            //string postData_str = "servername=localhost" + "&" +
            //    "username=feenuxco_reader" + "&" +
            //    "password=ethan123" + "&" +
            //    "database=feenuxco_customerlocal_cnc_mazatrol_mazak_01";

            //byte[] postData = Encoding.ASCII.GetBytes(postData_str);

            //browser.Navigate("http://trakhound.org/demo/MYSQL_TEST_02.php", "", postData, "Content-Type: application/x-www-form-urlencoded");

            //Browser_Initialize();

            //Browser_Navigate();

            browser.Source = new Uri("http://trakhound.org/demo/MYSQL_TEST_02.php");

        }




        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Web Sample"; } }

        public string Description { get { return "Sample of a Web Based plugin"; } }

        public ImageSource Image { get { return null; ; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_TableManager;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Plugin Properties/Options"

        /// <summary>
        /// Name of the "preferred" plugin that this plugin is supposed to be a child too (ex. Dashboard)
        /// </summary>
        public string DefaultParent { get { return null; } }

        /// <summary>
        /// Name of the Category inside the parent plugin (ex. Pages)
        /// </summary>
        public string DefaultParentCategory { get { return null; } }

        /// <summary>
        /// Used to turn on/off whether or not this plugin accepts child plugins
        /// </summary>
        public bool AcceptsPlugIns { get { return true; } }

        /// <summary>
        /// Used to determine whether this plugin's page is created immediately upon being enabled
        /// </summary>
        public bool OpenOnStartUp { get { return true; } }

        /// <summary>
        /// List of SubCatogories for this plugin to use for child plugins (ex. Pages for Dashboard)
        /// </summary>
        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        /// <summary>
        /// List of child plugins
        /// </summary>
        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        /// <summary>
        /// Method called whenever the plugin is first loaded or enabled
        /// </summary>
        public void Initialize() 
        {
            //Browser_Initialize();
        }

        /// <summary>
        /// Method called every 5 seconds (can be changed under General Options page)
        /// </summary>
        /// <param name="rd"></param>
        public void Update(ReturnData rd) { Console.WriteLine(Title + " : Update()"); }

        /// <summary>
        /// Method called while TrakHound Client is closing
        /// </summary>
        public void Closing() { Console.WriteLine(Title + " : Closing()"); }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        #endregion

        #region "Device Properties"

        /// <summary>
        /// List of Device_Client objects for each Device
        /// </summary>
        public List<Device_Client> Devices { get; set; }

        #endregion

        #region "Options"

        /// <summary>
        /// OptionsPage UserControl object for any options associated with this plugin
        /// </summary>
        public OptionsPage Options { get; set; }

        #endregion

        #endregion

        #region "Browser"

        //Awesomium.Windows.Controls.WebControl browser;

        //void Browser_Initialize()
        //{
        //    browser = new Awesomium.Windows.Controls.WebControl();

        //    Root_GRID.Children.Add(browser);

        //    //browser.Source = new Uri("http://www.bing.com/");

            


        //    //WebCore.Started += WebCore_Started;
        //    //cri = new CustomResourceInterceptor();
        //}

        void WebCore_Started(object sender, CoreStartEventArgs e)
        {
            WebCore.ResourceInterceptor = cri;
        }

        class Browser_Info
        {
            public string url { get; set; }

            // POST Data
            public string servername { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string database { get; set; }
        }

        void Browser_Navigate()
        {

            WebCore.CreateWebView(500, 500);

            //cri.RequestEnabled = true;
            //cri.Parameters = String.Format("login={0}&password={1}", login, pw);

            //browser.Source = new Uri("http://www.feenux.com/trakhound/demo/MYSQL_DEMO_02.php");

        }

        #endregion

        public Uri WebSource
        {
            get { return (Uri)GetValue(WebSourceProperty); }
            set { SetValue(WebSourceProperty, value); }
        }

        public static readonly DependencyProperty WebSourceProperty =
            DependencyProperty.Register("WebSource", typeof(Uri), typeof(UserControl1), new PropertyMetadata(null));

        private void Refresh_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //browser.Reload(false);
        }

    }
}
