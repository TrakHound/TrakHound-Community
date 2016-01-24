using System;

using TH_Plugins_Client;

namespace TrakHound_Client
{
    public partial class MainWindow
    {
        Splash.Screen SPLSH;

        System.Timers.Timer Splash_TIMER;

        void Splash_Initialize()
        {

            SPLSH = new Splash.Screen();
            Splash_Show();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            Version version = assembly.GetName().Version;

            SPLSH.Version = "Version " + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();

            Splash_TIMER = new System.Timers.Timer();
            Splash_TIMER.Interval = 4000;
            Splash_TIMER.Elapsed += Splash_TIMER_Elapsed;
            Splash_TIMER.Enabled = true;

        }

        void Splash_Show() { this.Dispatcher.Invoke(new Action(Splash_Show_GUI), Priority, new object[] { }); }

        void Splash_Show_GUI() { SPLSH.Show(); }

        void Splash_Close() { if (SPLSH != null) SPLSH.Close(); }

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        void Splash_UpdateStatus(string Status) { this.Dispatcher.Invoke(new Action<string>(Splash_UpdateStatus_GUI), Priority, new object[] { Status }); }

        void Splash_UpdateStatus_GUI(string Status) { SPLSH.Status = Status; }

        void Splash_AddPlugin(IClientPlugin plugin) { this.Dispatcher.Invoke(new Action<IClientPlugin>(Splash_AddPlugin_GUI), Priority, new object[] { plugin }); }

        void Splash_AddPlugin_GUI(IClientPlugin plugin) { SPLSH.AddPlugin(plugin); }

        bool SplashWait = true;

        void Splash_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Splash_TIMER.Enabled = false;
            SplashWait = false;
        }

    }
}
