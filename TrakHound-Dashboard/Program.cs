// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

using TrakHound;
using TrakHound.API;
using TrakHound_UI.Windows;

namespace TrakHound_Dashboard
{
    public static class Program
    {
        public static bool CloseApp = false;

        static Mutex mutex = new Mutex(true, "{8C1E94A7-0783-41A5-8F80-6463BC0524C6}");

        //private static App app;

        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                RunApp();
            }
            else
            {
                // send our Win32 message to make the currently running instance
                // jump on top of all the other windows
                NativeMethods.PostMessage(
                    (IntPtr)NativeMethods.HWND_BROADCAST,
                    NativeMethods.WM_SHOWME,
                    IntPtr.Zero,
                    IntPtr.Zero);
            }
        }

        private static void RunApp()
        {
#if DEBUG
            var app = new TrakHound_Dashboard.App();
            app.InitializeComponent();
            app.Run();
#else
            try
            {
                var app = new TrakHound_Dashboard.App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                if (SendBugReport(ex))
                {
                    var window = new BugReportSent();
                    var restart = window.ShowDialog();
                    if (restart == true)
                    {
                        // Restart Application
                        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    }
                }

                //Shutdown Current Application
                Application.Current.Shutdown();
            }
#endif
        }

        private static bool SendBugReport(Exception ex)
        {
            var bugInfo = new Bugs.BugInfo(ex);
            bugInfo.Application = ApplicationNames.TRAKHOUND_DASHBOARD;
            bugInfo.Type = 0;

            var bugInfos = new List<Bugs.BugInfo>();
            bugInfos.Add(bugInfo);

            return Bugs.Send(null, bugInfos);
        }
    }

    // this class just wraps some Win32 stuff that we're going to use
    internal class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }

}
