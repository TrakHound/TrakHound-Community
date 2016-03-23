using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Global.Functions;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        public bool ServerInstalled
        {
            get { return (bool)GetValue(ServerInstalledProperty); }
            set { SetValue(ServerInstalledProperty, value); }
        }

        public static readonly DependencyProperty ServerInstalledProperty =
            DependencyProperty.Register("ServerInstalled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public ServiceStatus ServerStatus
        {
            get { return (ServiceStatus)GetValue(ServerStatusProperty); }
            set { SetValue(ServerStatusProperty, value); }
        }

        public static readonly DependencyProperty ServerStatusProperty =
            DependencyProperty.Register("ServerStatus", typeof(ServiceStatus), typeof(MainWindow), new PropertyMetadata(ServiceStatus.Unavailable));


        public bool ServerRunning
        {
            get { return (bool)GetValue(ServerRunningProperty); }
            set { SetValue(ServerRunningProperty, value); }
        }

        public static readonly DependencyProperty ServerRunningProperty =
            DependencyProperty.Register("ServerRunning", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        private void ServerMonitor_Initialize()
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 2000;
            timer.Elapsed += ServerMonitor_Timer_Elapsed;
            timer.Enabled = true;
        }

        private void ServerMonitor_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(ServerMonitor_GUI), PRIORITY_BACKGROUND, new object[] { });
        }

        const string SERVER_SERVICE_NAME = "TrakHound Server";

        private void ServerMonitor_GUI()
        {
            ServerStatus = Service_Functions.GetServiceStatus(SERVER_SERVICE_NAME);
            ServerRunning = Service_Functions.IsServiceRunning(SERVER_SERVICE_NAME);
            ServerInstalled = Service_Functions.IsServiceInstalled(SERVER_SERVICE_NAME);
        }
    }
}
