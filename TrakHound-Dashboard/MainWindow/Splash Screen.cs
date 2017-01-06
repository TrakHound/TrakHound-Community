// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TrakHound.Tools;

using TrakHound_Dashboard.Windows;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {
        Splash splsh;

        void Splash_Initialize()
        {
            splsh = new Splash();
            Splash_Show();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            Version version = assembly.GetName().Version;

            splsh.Version = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();
        }

        void Splash_Show() { this.Dispatcher.Invoke(new Action(Splash_Show_GUI), System.Windows.Threading.DispatcherPriority.Background, new object[] { }); }

        void Splash_Show_GUI() { if (splsh != null) splsh.Show(); }

        void Splash_Close() { if (splsh != null) splsh.Close(); }


        void Splash_UpdateStatus(string status, double loadingProgress) { this.Dispatcher.Invoke(new Action<string, double>(Splash_UpdateStatus_GUI), System.Windows.Threading.DispatcherPriority.Background, new object[] { status, loadingProgress }); }

        void Splash_UpdateStatus_GUI(string status, double loadingProgress) 
        {
            if (splsh != null)
            {
                splsh.Status1 = status;
                splsh.LoadingProgress = loadingProgress;
            }
        }

    }
}
