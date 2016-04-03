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
using TH_Plugins;
using TH_Plugins.Client;
using TH_Global;
using TH_Global.Functions;
using System.Reflection;

namespace TH_DevicePage
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DevicePage : UserControl
    {
        public DevicePage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        public Configuration Configuration
        {
            get { return (Configuration)GetValue(ConfigurationProperty); }
            set
            {
                if (value != null) Logger.Log(value.Description.Description + " :: Loaded in DevicePage");
                else Logger.Log("Device Page Configuration set to null");

                SetValue(ConfigurationProperty, value);
            }
        }

        //public static readonly DependencyProperty ConfigurationProperty =
        //DependencyProperty.Register("Configuration", typeof(Configuration), typeof(DevicePage), new PropertyMetadata(null));

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(Configuration), typeof(DevicePage), new PropertyMetadata(null, new PropertyChangedCallback(Configuration_PropertyChanged)));

        private static void Configuration_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dp = obj as DevicePage;
            if (dp != null) dp.Configuration = (Configuration)e.NewValue;
        }



        public void Update(EventData data)
        {
            Update_Show(data);

            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(Configuration))
            {
                //Logger.Log("DevicePage :: " + data.id);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Configuration != null)
                    {
                        var config = (Configuration)data.Data01;

                        if (config.UniqueId == Configuration.UniqueId)
                        {
                            ((Controls.OeeStatusPanel)oeeStatus.Content).Update(data);



                            UpdateStatus(data);
                        }
                    }
                }));            
            }
        }

        bool createControls = true;

        #region "Status"



        #endregion

        private void UpdateStatus(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_status" && data.Data02.GetType() == typeof(DataTable))
            {
                var dt = (DataTable)data.Data02;

                

                if (createControls)
                {
                    CreateControllers(dt);


                }

                UpdateControllers(dt);

                createControls = false;

            }
        }

        private void Update_Show(EventData data)
        {
            if (data != null && data.Id != null && data.Data01 != null)
            {
                if (data.Id.ToLower() == "devicepage_show" && data.Data01.GetType() == typeof(Configuration))
                {
                    var config = (Configuration)data.Data01;

                    DevicePage page = createdPages.Find(x => x.Configuration.UniqueId == config.UniqueId);
                    if (page == null)
                    {
                        page = CreatePageInstance();
                        page.Configuration = config;
                        createdPages.Add(page);
                    }
                    
                    SendShowRequest(config, page);
                }
            }
        }

        private DevicePage CreatePageInstance()
        {
            ConstructorInfo ctor = typeof(DevicePage).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[] { }, null);

            Object_Functions.ObjectActivator<DevicePage> createdActivator = Object_Functions.GetActivator<DevicePage> (ctor);

            return createdActivator();
        }

        private static List<DevicePage> createdPages = new List<DevicePage>();


        private void SendShowRequest(Configuration config, IClientPlugin plugin)
        {
            var data = new EventData();
            data.Id = "show";
            data.Data01 = config;
            data.Data02 = plugin;
            data.Data03 = "Test Title";
            data.Data04 = config.UniqueId;
            if (SendData != null) SendData(data);
        }

    }
}
