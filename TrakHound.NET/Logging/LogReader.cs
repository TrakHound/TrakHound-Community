//  Copyright 2016 Feenux LLC
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.IO;

namespace TrakHound.Logging
{
    public class LogReader
    {
        public string AppicationName { get; set; }

        private DateTime lastDebugTimestamp = DateTime.MinValue;
        private DateTime lastErrorTimestamp = DateTime.MinValue;
        private DateTime lastNotificationTimestamp = DateTime.MinValue;
        private DateTime lastWarningTimestamp = DateTime.MinValue;

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
                    if (LineAdded != null) LineAdded(line);
                }
            }
        }

        public delegate void LineAdded_Handler(Line line);
        public event LineAdded_Handler LineAdded;
    }
}
