using System;

using TH_Plugins_Client;

namespace TrakHound_Client
{
    public partial class MainWindow
    {
        Splash.Screen splsh;

        System.Timers.Timer splash_TIMER;

        void Splash_Initialize()
        {

            splsh = new Splash.Screen();
            Splash_Show();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            Version version = assembly.GetName().Version;

            splsh.Version = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();

            //splash_TIMER = new System.Timers.Timer();
            //splash_TIMER.Interval = 4000;
            //splash_TIMER.Elapsed += splash_TIMER_Elapsed;
            //splash_TIMER.Enabled = true;

        }

        void Splash_Show() { this.Dispatcher.Invoke(new Action(Splash_Show_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { }); }

        void Splash_Show_GUI() { if (splsh != null) splsh.Show(); }

        void Splash_Close() { if (splsh != null) splsh.Close(); }


        void Splash_UpdateStatus(string status, double loadingProgress) { this.Dispatcher.Invoke(new Action<string, double>(Splash_UpdateStatus_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { status, loadingProgress }); }

        void Splash_UpdateStatus_GUI(string status, double loadingProgress) 
        {
            splsh.Status3 = splsh.Status2;
            splsh.Status2 = splsh.Status1;
            splsh.Status1 = status;
            splsh.LoadingProgress = loadingProgress;
        }

        bool splashWait = true;

        void splash_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            splash_TIMER.Enabled = false;
            splashWait = false;
        }

    }
}
