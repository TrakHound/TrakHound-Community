// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Windows;

using TrakHound;
using TrakHound.Logging;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {

        public bool DevConsole_Shown
        {
            get { return (bool)GetValue(DevConsole_ShownProperty); }
            set { SetValue(DevConsole_ShownProperty, value); }
        }

        double lastHeight = 400;

        public static readonly DependencyProperty DevConsole_ShownProperty =
            DependencyProperty.Register("DevConsole_Shown", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public GridLength DeveloperConsoleHeight
        {
            get { return (GridLength)GetValue(DeveloperConsoleHeightProperty); }
            set
            {
                SetValue(DeveloperConsoleHeightProperty, value);
            }
        }

        public static readonly DependencyProperty DeveloperConsoleHeightProperty =
            DependencyProperty.Register("DeveloperConsoleHeight", typeof(GridLength), typeof(MainWindow), new PropertyMetadata(new GridLength(0)));


        private void DeveloperConsole_ToolBarItem_Clicked(TrakHound_UI.Button bt)
        {
            //developerConsole.Shown = !developerConsole.Shown;
        }

        private void developerConsole_ShownChanged(bool shown)
        {
            DevConsole_Shown = shown;

            if (shown)
            {
                //developerConsole.ScrollLastIntoView();
                DeveloperConsoleHeight = new GridLength(lastHeight);
            }
            else
            {
                lastHeight = DeveloperConsoleHeight.Value;
                DeveloperConsoleHeight = new GridLength(0);
            }  
        }

        LogReader clientReader;
        LogReader serverReader;

        void Log_Initialize()
        {
            Logger.AppicationName = ApplicationNames.TRAKHOUND_DASHBOARD;

            StartLogReaders();
            LoggerConfigurationFileMonitor_Start();
        }

        void StartLogReaders()
        {
            clientReader = new LogReader(ApplicationNames.TRAKHOUND_DASHBOARD, DateTime.Now);
            clientReader.LineAdded += ClientReader_LineAdded;

            serverReader = new LogReader(ApplicationNames.TRAKHOUND_SERVER, DateTime.Now);
            serverReader.LineAdded += ServerReader_LineAdded;
        }

        private void LoggerConfigurationFileMonitor_Start()
        {
            string dir = FileLocations.TrakHound;
            string filename = LoggerConfiguration.CONFIG_FILENAME;

            var watcher = new FileSystemWatcher(dir, filename);
            watcher.Changed += LoggerConfigurationFileMonitor_Changed;
            watcher.Created += LoggerConfigurationFileMonitor_Changed;
            watcher.Deleted += LoggerConfigurationFileMonitor_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void LoggerConfigurationFileMonitor_Changed(object sender, FileSystemEventArgs e)
        {
            Logger.LoggerConfiguration = LoggerConfiguration.Read();
        }

        private void ClientReader_LineAdded(Line line)
        {
            Dispatcher.BeginInvoke(new Action<Line, string>(Log_Updated_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { line, ApplicationNames.TRAKHOUND_DASHBOARD });
        }

        private void ServerReader_LineAdded(Line line)
        {
            Dispatcher.BeginInvoke(new Action<Line, string>(Log_Updated_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { line, ApplicationNames.TRAKHOUND_SERVER });
        }

        void Log_Updated_GUI(Line line, string applicationName)
        {
            //if (developerConsole != null) developerConsole.AddLine(line, applicationName);
        }

    }
}
