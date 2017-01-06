// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace TrakHound.Logging
{
    public class LogReader
    {
        public string AppicationName { get; set; }

        public delegate void LineAdded_Handler(Line line);
        public event LineAdded_Handler LineAdded;

        private DateTime lastDebugTimestamp = DateTime.MinValue;
        private DateTime lastErrorTimestamp = DateTime.MinValue;
        private DateTime lastNotificationTimestamp = DateTime.MinValue;
        private DateTime lastWarningTimestamp = DateTime.MinValue;

        private System.Timers.Timer debugDelayTimer;
        private System.Timers.Timer errorDelayTimer;
        private System.Timers.Timer notificationDelayTimer;
        private System.Timers.Timer warningDelayTimer;

        private const int READ_DELAY = 2000;

        public LogReader(string applicationName, DateTime StartTimestamp)
        {
            lastDebugTimestamp = StartTimestamp;
            lastErrorTimestamp = StartTimestamp;
            lastNotificationTimestamp = StartTimestamp;
            lastWarningTimestamp = StartTimestamp;

            Init(applicationName);
        }

        public LogReader(string applicationName)
        {
            Init(applicationName);
        }

        private void Init(string applicationName)
        {
            AppicationName = applicationName;

            try
            {
                // Debug Watcher
                var debugWatcher = new FileSystemWatcher(Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_DEBUG));
                debugWatcher.Changed += DebugWatcher_Changed;
                debugWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex) { Console.WriteLine("Error Starting LogReader :: Exception :: " + ex.Message); }

            try
            {
                // Error Watcher
                var errorWatcher = new FileSystemWatcher(Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_ERROR));
                errorWatcher.Changed += ErrorWatcher_Changed;
                errorWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex) { Console.WriteLine("Error Starting LogReader :: Exception :: " + ex.Message); }

            try
            {
                // Notification Watcher
                var notificationWatcher = new FileSystemWatcher(Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_NOTIFICATION));
                notificationWatcher.Changed += NotificationWatcher_Changed;
                notificationWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex) { Console.WriteLine("Error Starting LogReader :: Exception :: " + ex.Message); }

            try
            {
                // Warning Watcher
                var warningWatcher = new FileSystemWatcher(Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_WARNING));
                warningWatcher.Changed += WarningWatcher_Changed;
                warningWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex) { Console.WriteLine("Error Starting LogReader :: Exception :: " + ex.Message); }
        }

        private void DebugWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed) ReadChanged(LogLineType.Debug);
        }

        private void ErrorWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed) ReadChanged(LogLineType.Error);
        }

        private void NotificationWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed) ReadChanged(LogLineType.Notification);
        }

        private void WarningWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed) ReadChanged(LogLineType.Warning);
        }

        private void ReadChanged(LogLineType type)
        {
            switch (type)
            {
                case LogLineType.Debug:

                    if (debugDelayTimer != null) debugDelayTimer.Enabled = false;

                    debugDelayTimer = new System.Timers.Timer();
                    debugDelayTimer.Interval = READ_DELAY;
                    debugDelayTimer.Elapsed += DebugDelayTimer_Elapsed;
                    debugDelayTimer.Enabled = true;
                    break;

                case LogLineType.Error:

                    if (errorDelayTimer != null) errorDelayTimer.Enabled = false;

                    errorDelayTimer = new System.Timers.Timer();
                    errorDelayTimer.Interval = READ_DELAY;
                    errorDelayTimer.Elapsed += ErrorDelayTimer_Elapsed;
                    errorDelayTimer.Enabled = true;
                    break;

                case LogLineType.Notification:

                    if (notificationDelayTimer != null) notificationDelayTimer.Stop();
                    else
                    {
                        notificationDelayTimer = new System.Timers.Timer();
                        notificationDelayTimer.Interval = READ_DELAY;
                        notificationDelayTimer.Elapsed += NotificationDelayTimer_Elapsed;
                    }

                    notificationDelayTimer.Start();

                    break;

                case LogLineType.Warning:

                    if (warningDelayTimer != null) warningDelayTimer.Enabled = false;

                    warningDelayTimer = new System.Timers.Timer();
                    warningDelayTimer.Interval = READ_DELAY;
                    warningDelayTimer.Elapsed += WarningDelayTimer_Elapsed;
                    warningDelayTimer.Enabled = true;
                    break;
            }
        }

        private void DebugDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) { CloseTimer(sender); ReadLog(LogLineType.Debug); }

        private void ErrorDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) { CloseTimer(sender); ReadLog(LogLineType.Error); }

        private void NotificationDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) { CloseTimer(sender); ReadLog(LogLineType.Notification); }

        private void WarningDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) { CloseTimer(sender); ReadLog(LogLineType.Warning); }

        private void CloseTimer(object sender)
        {
            var timer = (System.Timers.Timer)sender;
            timer.Stop();
        }


        private void ReadLog(LogLineType type)
        {
            DateTime time = DateTime.MinValue;

            switch (type)
            {
                case LogLineType.Debug: time = lastDebugTimestamp; break;
                case LogLineType.Error: time = lastErrorTimestamp; break;
                case LogLineType.Notification: time = lastNotificationTimestamp; break;
                case LogLineType.Warning: time = lastWarningTimestamp; break;
            }

            var nodes = Logger.ReadOutputLogXml(type, AppicationName, time);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var line = Line.FromXmlNode(node);

                    switch (type)
                    {
                        case LogLineType.Debug: lastDebugTimestamp = line.Timestamp; break;
                        case LogLineType.Error: time = lastErrorTimestamp = line.Timestamp; break;
                        case LogLineType.Notification: time = lastNotificationTimestamp = line.Timestamp; break;
                        case LogLineType.Warning: time = lastWarningTimestamp = line.Timestamp; break;
                    }

                    line.Type = type;
                    LineAdded?.Invoke(line);
                }
            }
        }

    }
}
