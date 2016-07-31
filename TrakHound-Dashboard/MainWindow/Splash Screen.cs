// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

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

            splsh.Version = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();
        }

        void Splash_Show() { this.Dispatcher.Invoke(new Action(Splash_Show_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { }); }

        void Splash_Show_GUI() { if (splsh != null) splsh.Show(); }

        void Splash_Close() { if (splsh != null) splsh.Close(); }


        void Splash_UpdateStatus(string status, double loadingProgress) { this.Dispatcher.Invoke(new Action<string, double>(Splash_UpdateStatus_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { status, loadingProgress }); }

        void Splash_UpdateStatus_GUI(string status, double loadingProgress) 
        {
            if (splsh != null)
            {
                splsh.Status3 = splsh.Status2;
                splsh.Status2 = splsh.Status1;
                splsh.Status1 = status;
                splsh.LoadingProgress = loadingProgress;
            }
        }

    }
}
