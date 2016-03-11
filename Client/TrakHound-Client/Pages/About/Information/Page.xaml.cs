// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;

using TH_Global;
using TH_Global.Functions;

namespace TrakHound_Client.Pages.About.Information
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, TH_Global.IPage
    {

        public Page()
        {
            InitializeComponent();

            DataContext = this;

            PageContent = this;

            // Build Information
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            Build_Version = "v" + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();

            //// Usage Information
            //Usage_TIMER = new System.Timers.Timer();
            //Usage_TIMER.Interval = 100;
            //Usage_TIMER.Elapsed += Usage_TIMER_Elapsed;
            //Usage_TIMER.Enabled = true;

        }

        //void Usage_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{

        //    Usage_TIMER.Interval = 3000;

        //    this.Dispatcher.BeginInvoke(new Action(Usage_TIMER_Elapsed_GUI));

        //}

        //void Usage_TIMER_Elapsed_GUI()
        //{

        //    Usage_MemoryUsed = GetMemoryUsed();

        //    Usage_MostMemoryUsed_Value = String_Functions.FileSizeSuffix(Properties.Settings.Default.Usage_MemoryUsed_Value);

        //    Usage_MostMemoryUsed_Date = Properties.Settings.Default.Usage_MemoryUsed_Date.ToString();

        //}

        public string PageName { get { return "Information"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Pages/About/Information/Information_01.png")); } }

        public event EventHandler PageOpened;
        public event CancelEventHandler PageOpening;

        public event EventHandler PageClosed;
        public event CancelEventHandler PageClosing;


        public object PageContent { get; set; }

        #region "Build Information"

        public string Build_Version
        {
            get { return (string)GetValue(Build_VersionProperty); }
            set { SetValue(Build_VersionProperty, value); }
        }

        public static readonly DependencyProperty Build_VersionProperty =
            DependencyProperty.Register("Build_Version", typeof(string), typeof(Page), new PropertyMetadata(null));

        #endregion

        #region "Usage Information"

        //System.Timers.Timer Usage_TIMER;

        //public string Usage_MemoryUsed
        //{
        //    get { return (string)GetValue(Usage_MemoryUsedProperty); }
        //    set { SetValue(Usage_MemoryUsedProperty, value); }
        //}

        //public static readonly DependencyProperty Usage_MemoryUsedProperty =
        //    DependencyProperty.Register("Usage_MemoryUsed", typeof(string), typeof(Page), new PropertyMetadata(null));

        //public string Usage_MostMemoryUsed_Value
        //{
        //    get { return (string)GetValue(Usage_MostMemoryUsed_ValueProperty); }
        //    set { SetValue(Usage_MostMemoryUsed_ValueProperty, value); }
        //}

        //public static readonly DependencyProperty Usage_MostMemoryUsed_ValueProperty =
        //    DependencyProperty.Register("Usage_MostMemoryUsed_Value", typeof(string), typeof(Page), new PropertyMetadata(null));

        //public string Usage_MostMemoryUsed_Date
        //{
        //    get { return (string)GetValue(Usage_MostMemoryUsed_DateProperty); }
        //    set { SetValue(Usage_MostMemoryUsed_DateProperty, value); }
        //}

        //public static readonly DependencyProperty Usage_MostMemoryUsed_DateProperty =
        //    DependencyProperty.Register("Usage_MostMemoryUsed_Date", typeof(string), typeof(Page), new PropertyMetadata(null));

        
        //string GetMemoryUsed()
        //{

        //    Process proc = Process.GetCurrentProcess();

        //    Int64 memory = proc.PrivateMemorySize64;

        //    if (memory > Properties.Settings.Default.Usage_MemoryUsed_Value)
        //    {
        //        Properties.Settings.Default.Usage_MemoryUsed_Value = memory;
        //        Properties.Settings.Default.Usage_MemoryUsed_Date = DateTime.Now;
        //        Properties.Settings.Default.Save();
        //    }

        //    return String_Functions.FileSizeSuffix(memory);

        //}

        #endregion

    }
}
