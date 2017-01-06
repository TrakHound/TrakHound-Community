// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {
        public void OpenBugReport(Exception ex)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrakHound-Bug-Reporter.exe");

            if (File.Exists(path))
            {
                string subject = "\"Bug Report : [Exception] : " + DateTime.Now.ToString() + "\"";

                string type = "Exception";
                string exMessage = ex.Message;
                string exSource = ex.Source;
                string exHelplink = ex.HelpLink;
                string exTargetSite = ex.TargetSite.Name;

                string stackTrace = ex.StackTrace;

                string n = Environment.NewLine;

                string format =
                    "\"Bug Type : {0}" + n + n +
                    "Message : {1}" + n + n +

                    "Source : {2}" + n +
                    "Help Link : {3}" + n +
                    "Target Site : {4}" + n + n +

                    "Stack Trace : {5}\"";

                string message = string.Format(
                    format,
                    type,
                    exMessage,
                    exSource,
                    exHelplink,
                    exTargetSite,
                    stackTrace
                    );

                string args = string.Format(format, subject, message);

                Process.Start(path, args);
            }
        }

        public void OpenBugReport()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrakHound-Bug-Reporter.exe");

            if (File.Exists(path))
            {
                Process.Start(path);
            }
        }
    }
}
