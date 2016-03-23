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

using System.Reflection;

using TH_Global;
using TH_Updater;

namespace TrakHound_Client.Pages.Options.Updates
{
    /// <summary>
    /// Interaction logic for UpdateItem.xaml
    /// </summary>
    public partial class UpdateItem : UserControl
    {
        public UpdateItem()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;
        }

        public void CheckUpdateVersion()
        {
            AutoUpdater_Start();
        }

        MainWindow mw;

        public Assembly assembly { get; set; }

        public string UpdateFileUrl { get; set; }


        void AutoUpdater_Start()
        {
            if (UpdateFileUrl != null)
            {
                UpdateCheck updateCheck = new UpdateCheck();
                updateCheck.AppInfoReceived += AutoUpdater_AppInfoReceived;
                updateCheck.Start(UpdateFileUrl);
            }
            else
            {
                UpdateCheckResult = "Update Not Compatible";
            }
        }

        void AutoUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        {
            if (info != null)
            {
                // Print Auto Update info to Console
                Logger.Log("---- Auto-Update Info ----");
                //Logger.Log("TrakHound - Client");
                Logger.Log("Release Type : " + info.releaseType);
                Logger.Log("Version : " + info.version);
                Logger.Log("Build Date : " + info.buildDate);
                Logger.Log("Download URL : " + info.downloadUrl);
                Logger.Log("Update URL : " + info.updateUrl);
                Logger.Log("File Size : " + info.size);
                Logger.Log("--------------------------");

                this.Dispatcher.BeginInvoke(new Action<UpdateCheck.AppInfo>(AutoUpdater_AppInfoReceived_GUI), new object[] { info });
            }
        }

        void AutoUpdater_AppInfoReceived_GUI(UpdateCheck.AppInfo info)
        {
            Version latestVersion = null;
            Version.TryParse(info.version, out latestVersion);

            PluginUpdateShown = false;

            if (latestVersion != null && version != null)
            {
                if (version < latestVersion)
                {
                    // Run Updater
                    Updater updater = new Updater();
                    updater.assembly = assembly;
                    updater.Start(info.updateUrl);

                    Logger.Log("Update Available : " + latestVersion.ToString());

                    //// Add Notification to Message Center
                    //Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                    //mData.Title = "Version " + latestVersion.ToString() + " is Available";
                    //mData.Text = "Reopen TrakHound to apply update";

                    //mw.messageCenter.AddMessage(mData);

                    UpdateCheckResult = "Version " + latestVersion.ToString() + " is available";
                    UpdateCheckBrush = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    UpdateCheckImage = null;

                    PluginUpdateShown = true;
                }
                else
                {
                    UpdateCheckResult = "Up to Date";
                    UpdateCheckBrush = new SolidColorBrush(Colors.Green);
                    UpdateCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/CheckMark_01.png"));
                }
            }
            else
            {
                UpdateCheckResult = "Error during Update Check";
                UpdateCheckBrush = new SolidColorBrush(Colors.Red);
                UpdateCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/X_01.png"));
            }
        }


        UpdateCheck.AppInfo appInfo;

        void ManualUpdater_Start()
        {
            UpdateCheck updateCheck = new UpdateCheck();
            updateCheck.AppInfoReceived += ManualUpdater_AppInfoReceived;
            updateCheck.Start(UpdateFileUrl);
        }

        void ManualUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        {
            appInfo = info;

            if (info != null)
            {
                // Print Auto Update info to Console
                Logger.Log("---- Manual-Update Info ----");
                Logger.Log("Release Type : " + info.releaseType);
                Logger.Log("Version : " + info.version);
                Logger.Log("Build Date : " + info.buildDate);
                Logger.Log("Download URL : " + info.downloadUrl);
                Logger.Log("Update URL : " + info.updateUrl);
                Logger.Log("File Size : " + info.size);
                Logger.Log("--------------------------");

                this.Dispatcher.BeginInvoke(new Action<UpdateCheck.AppInfo>(ManualUpdater_AppInfoReceived_GUI), new object[] { info });
            }
        }

        void ManualUpdater_AppInfoReceived_GUI(UpdateCheck.AppInfo info)
        {
            Version latestVersion = null;
            Version.TryParse(info.version, out latestVersion);

            PluginUpdateShown = false;

            if (latestVersion != null && version != null)
            {
                if (version < latestVersion)
                {
                    Logger.Log("Update Available : " + latestVersion.ToString());

                    // Add Notification to Message Center
                    Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                    mData.Title = "Version " + latestVersion.ToString() + " is Available";
                    mData.Text = "Click to Update";

                    mw.messageCenter.AddMessage(mData);


                    UpdateCheckResult = "Version " + latestVersion.ToString() + " is available";
                    UpdateCheckBrush = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    UpdateCheckImage = null;

                    PluginUpdateShown = true;
                }
                else
                {
                    UpdateCheckResult = "Up to Date";
                    UpdateCheckBrush = new SolidColorBrush(Colors.Green);
                    UpdateCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/CheckMark_01.png"));
                }
            }
            else
            {
                UpdateCheckResult = "Error during Update Check";
                UpdateCheckBrush = new SolidColorBrush(Colors.Red);
                UpdateCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/X_01.png"));
            }
        }

        public ImageSource PluginImage
        {
            get { return (ImageSource)GetValue(PluginImageProperty); }
            set { SetValue(PluginImageProperty, value); }
        }

        public static readonly DependencyProperty PluginImageProperty =
            DependencyProperty.Register("PluginImage", typeof(ImageSource), typeof(UpdateItem), new PropertyMetadata(null));


        public Version version { get; set; }

        public string PluginVersion
        {
            get { return (string)GetValue(PluginVersionProperty); }
            set { SetValue(PluginVersionProperty, value); }
        }

        public static readonly DependencyProperty PluginVersionProperty =
            DependencyProperty.Register("PluginVersion", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));


        public string PluginTitle
        {
            get { return (string)GetValue(PluginTitleProperty); }
            set { SetValue(PluginTitleProperty, value); }
        }

        public static readonly DependencyProperty PluginTitleProperty =
            DependencyProperty.Register("PluginTitle", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));


        public string PluginAuthor
        {
            get { return (string)GetValue(PluginAuthorProperty); }
            set { SetValue(PluginAuthorProperty, value); }
        }

        public static readonly DependencyProperty PluginAuthorProperty =
            DependencyProperty.Register("PluginAuthor", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));


        public string PluginAuthorInfo
        {
            get { return (string)GetValue(PluginAuthorInfoProperty); }
            set { SetValue(PluginAuthorInfoProperty, value); }
        }

        public static readonly DependencyProperty PluginAuthorInfoProperty =
            DependencyProperty.Register("PluginAuthorInfo", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));



        public string UpdateCheckResult
        {
            get { return (string)GetValue(UpdateCheckResultProperty); }
            set { SetValue(UpdateCheckResultProperty, value); }
        }

        public static readonly DependencyProperty UpdateCheckResultProperty =
            DependencyProperty.Register("UpdateCheckResult", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));



        public ImageSource UpdateCheckImage
        {
            get { return (ImageSource)GetValue(UpdateCheckImageProperty); }
            set { SetValue(UpdateCheckImageProperty, value); }
        }

        public static readonly DependencyProperty UpdateCheckImageProperty =
            DependencyProperty.Register("UpdateCheckImage", typeof(ImageSource), typeof(UpdateItem), new PropertyMetadata(null));


        public SolidColorBrush UpdateCheckBrush
        {
            get { return (SolidColorBrush)GetValue(UpdateCheckBrushProperty); }
            set { SetValue(UpdateCheckBrushProperty, value); }
        }

        public static readonly DependencyProperty UpdateCheckBrushProperty =
            DependencyProperty.Register("UpdateCheckBrush", typeof(SolidColorBrush), typeof(UpdateItem), new PropertyMetadata(new SolidColorBrush(Colors.Black)));



        public bool PluginUpdateShown
        {
            get { return (bool)GetValue(PluginUpdateShownProperty); }
            set { SetValue(PluginUpdateShownProperty, value); }
        }

        public static readonly DependencyProperty PluginUpdateShownProperty =
            DependencyProperty.Register("PluginUpdateShown", typeof(bool), typeof(UpdateItem), new PropertyMetadata(false));
        

    }
}
