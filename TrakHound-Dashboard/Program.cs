// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace TrakHound_Dashboard
{
    public static class Program
    {
        public static bool CloseApp = false;

        static Mutex mutex = new Mutex(true, "{8C1E94A7-0783-41A5-8F80-6463BC0524C6}");
        [STAThread]
        static void Main()
        {
            var app = new TrakHound_Dashboard.App();
            app.InitializeComponent();
            app.Run();

            //if (!CloseApp)
            //{
            //    if (mutex.WaitOne(TimeSpan.Zero, true))
            //    {
            //        var app = new TrakHound_Dashboard.App();
            //        app.InitializeComponent();
            //        app.Run();
            //    }
            //    else
            //    {
            //        // send our Win32 message to make the currently running instance
            //        // jump on top of all the other windows
            //        NativeMethods.PostMessage(
            //            (IntPtr)NativeMethods.HWND_BROADCAST,
            //            NativeMethods.WM_SHOWME,
            //            IntPtr.Zero,
            //            IntPtr.Zero);
            //    }
            //}
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
