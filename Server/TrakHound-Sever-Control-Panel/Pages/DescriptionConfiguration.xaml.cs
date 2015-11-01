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
using System.Data;
using System.IO;

using TH_Configuration;
using TH_Configuration.User;
using TH_Global;
using TH_Global.Functions;
using TH_PlugIns_Server;

namespace TrakHound_Server_Control_Panel.Pages
{
    /// <summary>
    /// Interaction logic for DescriptionConfiguration.xaml
    /// </summary>
    public partial class DescriptionConfiguration : UserControl, ConfigurationPage
    {
        public DescriptionConfiguration()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Page Interface"

        public string PageName { get { return "Description"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Server-Control-Panel;component/Resources/About_01.png")); } }

        public UserConfiguration currentUser { get; set; }

        public event SaveRequest_Handler SaveRequest;

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            configurationTable = dt;

            // Load Description
            devicedescription_TXT.Text = TH_PlugIns_Server.Tools.GetTableValue(dprefix + "Description", dt);

            // Load Type
            devicetype_TXT.Text = TH_PlugIns_Server.Tools.GetTableValue(dprefix + "Device_Type", dt);

            // Load Manufacturer
            manufacturer_TXT.Text = TH_PlugIns_Server.Tools.GetTableValue(dprefix + "Manufacturer", dt);

            // Load Id
            deviceid_TXT.Text = TH_PlugIns_Server.Tools.GetTableValue(dprefix + "Device_ID", dt);

            // Load Model
            model_TXT.Text = TH_PlugIns_Server.Tools.GetTableValue(dprefix + "Model", dt);

            // Load Serial
            serial_TXT.Text = TH_PlugIns_Server.Tools.GetTableValue(dprefix + "Serial", dt);

            // Load Controller
            controller_TXT.Text = TH_PlugIns_Server.Tools.GetTableValue(dprefix + "Controller", dt);

            // Load Company
            company_TXT.Text = TH_PlugIns_Server.Tools.GetTableValue(dprefix + "Company", dt);

            // Load Manufacturer Logo
            manufacturerLogoFileName = TH_PlugIns_Server.Tools.GetTableValue(fprefix + "Manufacturer_Logo_Path", dt);
            LoadManufacturerLogo(manufacturerLogoFileName);

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            // Save Descritpion
            TH_PlugIns_Server.Tools.UpdateTableValue(devicedescription_TXT.Text, dprefix + "Description", dt);

            // Save Type
            TH_PlugIns_Server.Tools.UpdateTableValue(devicetype_TXT.Text, dprefix + "Device_Type", dt);

            // Save Manufacturer
            TH_PlugIns_Server.Tools.UpdateTableValue(manufacturer_TXT.Text, dprefix + "Manufacturer", dt);

            // Save Id
            TH_PlugIns_Server.Tools.UpdateTableValue(deviceid_TXT.Text, dprefix + "Device_ID", dt);

            // Save Model
            TH_PlugIns_Server.Tools.UpdateTableValue(model_TXT.Text, dprefix + "Model", dt);

            // Save Serial
            TH_PlugIns_Server.Tools.UpdateTableValue(serial_TXT.Text, dprefix + "Serial", dt);

            // Save Controller
            TH_PlugIns_Server.Tools.UpdateTableValue(controller_TXT.Text, dprefix + "Controller", dt);

            // Save Company
            TH_PlugIns_Server.Tools.UpdateTableValue(company_TXT.Text, dprefix + "Company", dt);

            // Save Manufacturer Logo
            TH_PlugIns_Server.Tools.UpdateTableValue(manufacturerLogoFileName, fprefix + "Manufacturer_Logo_Path", dt);
        }

        #endregion


        string dprefix = "/Description/";
        string fprefix = "/File_Locations/";

        DataTable configurationTable;


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(DescriptionConfiguration), new PropertyMetadata(false));

        
        void ChangeSetting(string name, string val)
        {
            if (!Loading)
            {
                string newVal = val;
                string oldVal = null;

                if (configurationTable != null)
                {
                    oldVal = TH_PlugIns_Server.Tools.GetTableValue(name, configurationTable);
                }

                if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
            }
        }

        private void devicedescription_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Description", ((TextBox)sender).Text); 
        }

        private void devicetype_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Device_Type", ((TextBox)sender).Text); 
        }

        private void manufacturer_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Manufactuter", ((TextBox)sender).Text); 
        }

        private void deviceid_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Device_ID", ((TextBox)sender).Text); 
        }

        private void model_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Model", ((TextBox)sender).Text); 
        }

        private void serial_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Serial", ((TextBox)sender).Text); 
        }

        private void controller_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Controller", ((TextBox)sender).Text); 
        }

        private void company_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Company", ((TextBox)sender).Text); 
        }

        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }

        private void manufacturerlogo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            manufacturerLogoFileName = UploadManufacturerLogo();

            LoadManufacturerLogo(manufacturerLogoFileName);

            ChangeSetting(fprefix + "Manufacturer_Logo_Path", manufacturerLogoFileName); 
        }

        private void deviceimage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }





        #region "Images"

        string manufacturerLogoFileName;

        public ImageSource ManufacturerLogo
        {
            get { return (ImageSource)GetValue(ManufacturerLogoProperty); }
            set { SetValue(ManufacturerLogoProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoProperty =
            DependencyProperty.Register("ManufacturerLogo", typeof(ImageSource), typeof(DescriptionConfiguration), new PropertyMetadata(null));



        public bool ManufacturerLogoSet
        {
            get { return (bool)GetValue(ManufacturerLogoSetProperty); }
            set { SetValue(ManufacturerLogoSetProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoSetProperty =
            DependencyProperty.Register("ManufacturerLogoSet", typeof(bool), typeof(DescriptionConfiguration), new PropertyMetadata(false));

        

        void LoadManufacturerLogo(string filename)
        {
            System.Drawing.Image img = Images.GetImage(filename);

            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                //ManufacturerLogo = bmpSource;

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    ManufacturerLogo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 180);
                }
                else
                {
                    ManufacturerLogo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 80);
                }

                ManufacturerLogoSet = true;
            }
            else
            {
                ManufacturerLogoSet = false;
            }  
        }

        string UploadManufacturerLogo()
        {
            string result = null;

            string imagePath = TH_Configuration.User.Images.OpenImageBrowse("Select a Manufacturer Logo");
            if (imagePath != null)
            {
                string filename = String_Functions.RandomString(20);

                string tempdir = FileLocations.TrakHound + @"\temp";
                if (!Directory.Exists(tempdir)) Directory.CreateDirectory(tempdir);

                string localPath = tempdir + @"\" + filename;

                File.Copy(imagePath, localPath);

                TH_Configuration.User.Images.UploadImage(localPath);

                result = filename;
            }

            return result;
        }

        #endregion


        




        //private void ToolTip_Closed(object sender, RoutedEventArgs e)
        //{
        //    ToolTip tt = (ToolTip)sender;
        //    tt.IsOpen = false;
        //}



    }
}
