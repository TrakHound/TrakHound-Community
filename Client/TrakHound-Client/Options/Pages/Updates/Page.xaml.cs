// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

using TH_PlugIns_Client_Control;
using TH_Updater;

namespace TrakHound_Client.Options.Pages.Updates
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class Page : UserControl, OptionsPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;

            ProcessUpdateBehavior();

            AutoUpdater_Initialize();
        }

        MainWindow mw;

        public string PageName { get { return "Updates"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Arrow_Up_01.png")); } }


        void AutoUpdater_Initialize()
        {
            UpdateCheck updateCheck = new UpdateCheck();
            updateCheck.AppInfoReceived += AutoUpdater_AppInfoReceived;
            updateCheck.Start("http://www.feenux.com/trakhound/appinfo/th/client-appinfo.txt");
        }


        void AutoUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        {
            // Print Auto Update info to Console
            Console.WriteLine("---- Auto-Update Info ----");
            Console.WriteLine("TrakHound - Client");
            Console.WriteLine("Release Type : " + info.releaseType);
            Console.WriteLine("Version : " + info.version);
            Console.WriteLine("Build Date : " + info.buildDate);
            Console.WriteLine("Download URL : " + info.downloadUrl);
            Console.WriteLine("Update URL : " + info.updateUrl);
            Console.WriteLine("File Size : " + info.size);
            Console.WriteLine("--------------------------");

            // $$ $  DEBUG  $ $$$
            Updater updater = new Updater();
            updater.assembly = Assembly.GetExecutingAssembly();
            updater.Start(info.updateUrl);

            this.Dispatcher.BeginInvoke(new Action<UpdateCheck.AppInfo>(AutoUpdater_AppInfoReceived_GUI), new object[] { info });
        }

        void AutoUpdater_AppInfoReceived_GUI(UpdateCheck.AppInfo info)
        {

            // Check if version is Up-to-date
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            ClientVersion = "v" + version.ToString();

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

                    ClientCheckResult = "Version " + latestVersion.ToString() + " is available to Download";
                    ClientCheckBrush = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Arrow_Down_01.png"));
                }
                else
                {
                    ClientCheckResult = "Up to Date";
                    ClientCheckBrush = new SolidColorBrush(Colors.Green);
                    ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/CheckMark_01.png"));
                }
            }
            else
            {
                ClientCheckResult = "Error during Update Check";
                ClientCheckBrush = new SolidColorBrush(Colors.Red);
                ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/X_01.png"));
            }
        }


        public void LoadPluginConfigurations()
        {
            Plugin_STACK.Children.Clear();

            // Load Page Plugin Configurations
            if (Properties.Settings.Default.PagePlugIn_Configurations != null)
            {
                List<PlugInConfiguration> configs = Properties.Settings.Default.PagePlugIn_Configurations.ToList();

                foreach (PlugInConfiguration config in configs)
                {
                    if (mw != null)
                    {
                        if (mw.PagePlugIns != null)
                        {
                            try
                            {
                                Lazy<Control_PlugIn> LCP = mw.PagePlugIns.Find(x => x.Value.Title.ToUpper() == config.name.ToUpper());
                                if (LCP != null)
                                {
                                    Control_PlugIn CP = LCP.Value;

                                    UpdateItem ui = new UpdateItem();
                                    ui.PluginTitle = config.name;
                                    ui.PluginImage = CP.Image;

                                    // Build Information
                                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                                    Version version = assembly.GetName().Version;

                                    ui.PluginVersion = "v" + version.ToString();

                                    // Author Info
                                    ui.PluginAuthor = CP.Author;
                                    ui.PluginAuthorInfo = CP.AuthorText;

                                    // Update Info
                                    ui.UpdateFileUrl = CP.UpdateFileURL;

                                    ui.CheckUpdateVersion();

                                    Plugin_STACK.Children.Add(ui);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }


        public string ClientVersion
        {
            get { return (string)GetValue(ClientVersionProperty); }
            set { SetValue(ClientVersionProperty, value); }
        }

        public static readonly DependencyProperty ClientVersionProperty =
            DependencyProperty.Register("ClientVersion", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string ClientCheckResult
        {
            get { return (string)GetValue(ClientCheckResultProperty); }
            set { SetValue(ClientCheckResultProperty, value); }
        }

        public static readonly DependencyProperty ClientCheckResultProperty =
            DependencyProperty.Register("ClientCheckResult", typeof(string), typeof(Page), new PropertyMetadata(null));


        public SolidColorBrush ClientCheckBrush
        {
            get { return (SolidColorBrush)GetValue(ClientCheckBrushProperty); }
            set { SetValue(ClientCheckBrushProperty, value); }
        }

        public static readonly DependencyProperty ClientCheckBrushProperty =
            DependencyProperty.Register("ClientCheckBrush", typeof(SolidColorBrush), typeof(Page), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public ImageSource ClientCheckImage
        {
            get { return (ImageSource)GetValue(ClientCheckImageProperty); }
            set { SetValue(ClientCheckImageProperty, value); }
        }

        public static readonly DependencyProperty ClientCheckImageProperty =
            DependencyProperty.Register("ClientCheckImage", typeof(ImageSource), typeof(Page), new PropertyMetadata(null));


        #region "Update Behavior"

        void ProcessUpdateBehavior()
        {
            switch (Properties.Settings.Default.UpdateBehavior)
            {
                case 1: SemiAuto_RADIO.IsChecked = true; break;

                case 2: Manual_RADIO.IsChecked = true; break;

                default: Auto_RADIO.IsChecked = true; break;
            }
        }

        private void Auto_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UpdateBehavior = 0;
            Properties.Settings.Default.Save();
        }

        private void SemiAuto_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UpdateBehavior = 1;
            Properties.Settings.Default.Save();
        }

        private void Manual_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UpdateBehavior = 2;
            Properties.Settings.Default.Save();
        }

        #endregion

        private void CheckForUpdates_Button_Clicked(Controls.TH_Button bt)
        {
            AutoUpdater_Initialize();

            foreach (UpdateItem ui in Plugin_STACK.Children.OfType<UpdateItem>().ToList())
            {
                ui.CheckUpdateVersion();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPluginConfigurations();
        }

    }

}
