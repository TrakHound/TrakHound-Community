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

using TH_Updater;

namespace TrakHound_Client.Options.Pages.Updates
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
            AutoUpdater_Initialize();
        }

        MainWindow mw;

        public string UpdateFileUrl { get; set; }


        void AutoUpdater_Initialize()
        {
            if (UpdateFileUrl != null)
            {
                UpdateCheck updater = new UpdateCheck();
                updater.AppInfoReceived += AutoUpdater_AppInfoReceived;
                updater.Start(UpdateFileUrl);
            }
        }

        void AutoUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        {
            // Print Auto Update info to Console
            Console.WriteLine("---- Auto-Update Info ----");
            //Console.WriteLine(PluginTitle);
            Console.WriteLine("Release Type : " + info.releaseType);
            Console.WriteLine("Version : " + info.version);
            Console.WriteLine("Build Date : " + info.buildDate);
            Console.WriteLine("Download URL : " + info.downloadUrl);
            Console.WriteLine("Update URL : " + info.updateUrl);
            Console.WriteLine("File Size : " + info.size);
            Console.WriteLine("--------------------------");

            this.Dispatcher.BeginInvoke(new Action<UpdateCheck.AppInfo>(AutoUpdater_AppInfoReceived_GUI), new object[] { info });
        }

        void AutoUpdater_AppInfoReceived_GUI(UpdateCheck.AppInfo info)
        {
            // Check if version is Up-to-date
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            Version latestVersion = null;
            Version.TryParse(info.version, out latestVersion);

            if (latestVersion != null)
            {
                if (version < latestVersion)
                {
                    Console.WriteLine("Update Available : " + latestVersion.ToString());

                    // Add Notification to Message Center
                    Message_Center.Message_Data mData = new Message_Center.Message_Data();
                    mData.title = "Version " + latestVersion.ToString() + " is Available";
                    mData.text = "Click to Update";

                    mw.messageCenter.AddNotification(mData);

                    UpdateCheckResult = "Version " + latestVersion.ToString() + " is available to Download";
                    UpdateCheckBrush = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    UpdateCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Arrow_Down_01.png"));
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

    }
}
