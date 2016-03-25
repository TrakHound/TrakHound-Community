using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_Configuration;
using TH_Plugins;
using TH_Plugins.Client;

namespace TH_DevicePage
{
    public partial class DevicePage : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Device Page"; } }

        public string Description { get { return "View Current Detailed Information About a Single Device"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_DevicePage;component/Resources/About_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }

        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2016 Feenux LLC. All Rights Reserved"; } }

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

        public bool AcceptsPlugins { get { return false; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }

        #endregion

        #region "Events"

        public void GetSentData(EventData data) { Update(data); }

        public event SendData_Handler SendData;

        #endregion

        public ObservableCollection<Configuration> Devices { get; set; }

        public TH_Global.IPage Options { get; set; }

    }
}
