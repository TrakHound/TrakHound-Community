// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrakHound_Client
{
    public partial class MainWindow
    {
        public void OpenBugReport()
        {
            Cursor = System.Windows.Input.Cursors.Wait;
            var bugReport = new Windows.BugReport();
            bugReport.ScreenshotPath = bugReport.GetScreenshot(this);
            Cursor = System.Windows.Input.Cursors.Wait;
            bugReport.ShowDialog();
        }

    }
}
