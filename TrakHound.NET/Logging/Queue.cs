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
using System.Collections.Generic;
using System.IO;
using System.Xml;

using TrakHound.Tools.XML;

namespace TrakHound.Logging
{
    /// <summary>
    /// Queue for writing log data to an Xml file
    /// </summary>
    internal class LogQueue
    {
        System.Timers.Timer queueTimer;

        private List<Line> queue;

        internal LoggerConfiguration configuration;

        public LogQueue()
        {
            configuration = LoggerConfiguration.Read();

            queue = new List<Line>();

            queueTimer = new System.Timers.Timer();
            queueTimer.Interval = Math.Max(500, configuration.QueueWriteInterval);
            queueTimer.Elapsed += queue_TIMER_Elapsed;
            queueTimer.Enabled = true;
        }

        private void queue_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ProcessQueue();
        }


        public void AddLineToQueue(Line line)
        {
            bool add = false;
            switch (line.Type)
            {
                case LogLineType.Debug: add = configuration.Debug; break;
                case LogLineType.Error: add = configuration.Error; break;
                case LogLineType.Notification: add = configuration.Notification; break;
                case LogLineType.Warning: add = configuration.Warning; break;
            }

            if (queue != null && add) queue.Add(line);
        }

        private void AddToLog(XmlDocument doc, Line line)
        {
            string appName = Logger.AppicationName;
            if (appName == null) appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);

            // Create Document Root
            XmlNode rootNode = CreateRoot(doc);

            // Application Name Node
            XmlNode appNameNode = rootNode.SelectSingleNode("//" + appName);
            if (appNameNode == null)
            {
                XmlNode node = doc.CreateElement(appName);
                rootNode.AppendChild(node);
                appNameNode = node;
            }

            XmlNode lineNode = CreateLineNode(doc, line);
            appNameNode.AppendChild(lineNode);
        }

        private static XmlNode CreateLineNode(XmlDocument doc, Line line)
        {
            XmlNode itemNode = doc.CreateElement("Line");

            XmlAttribute assemblyAttribute = doc.CreateAttribute("assembly");
            assemblyAttribute.Value = line.Assembly;
            itemNode.Attributes.Append(assemblyAttribute);

            XmlAttribute filenameAttribute = doc.CreateAttribute("filename");
            filenameAttribute.Value = line.Filename;
            itemNode.Attributes.Append(filenameAttribute);

            XmlAttribute memberAttribute = doc.CreateAttribute("member");
            memberAttribute.Value = line.Member;
            itemNode.Attributes.Append(memberAttribute);

            XmlAttribute timestampAttribute = doc.CreateAttribute("timestamp");
            timestampAttribute.Value = FormatTimestamp(line.Timestamp);
            itemNode.Attributes.Append(timestampAttribute);

            XmlAttribute lineAttribute = doc.CreateAttribute("line");
            lineAttribute.Value = line.LineNumber.ToString();
            itemNode.Attributes.Append(lineAttribute);

            itemNode.InnerText = line.Text;

            return itemNode;
        }


        private void ProcessQueue()
        {
            if (queue != null && queue.Count > 0)
            {
                try
                {
                    ProcessQueue(LogLineType.Debug);
                    ProcessQueue(LogLineType.Error);
                    ProcessQueue(LogLineType.Notification);
                    ProcessQueue(LogLineType.Warning);

                    queue.Clear();
                }
                catch (Exception ex) { Console.WriteLine("ProcessQueue() :: Exception :: " + ex.Message); }
            }
        }

        private void ProcessQueue(LogLineType type)
        {
            var lines = queue.FindAll(x => x.Type == type);
            if (lines != null && lines.Count > 0)
            {
                try
                {
                    if (!Directory.Exists(Logger.OutputLogPath)) Directory.CreateDirectory(Logger.OutputLogPath);

                    string filename = "Log-" + Logger.FormatDate(DateTime.Now) + ".log";

                    string path = null;

                    switch (type)
                    {
                        case LogLineType.Debug: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_DEBUG, filename); break;
                        case LogLineType.Error: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_ERROR, filename); break;
                        case LogLineType.Notification: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_NOTIFICATION, filename); break;
                        case LogLineType.Warning: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_WARNING, filename); break;
                    }

                    // Write to Log File
                    using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            foreach (Line line in lines)
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("ProcessQueue(LogLineType) :: Exception :: " + type.ToString() + " :: " + ex.Message); }
            }
        }

        //private void ProcessQueue(LogLineType type)
        //{
        //    var lines = queue.FindAll(x => x.Type == type);
        //    if (lines != null && lines.Count > 0)
        //    {
        //        try
        //        {
        //            if (!Directory.Exists(Logger.OutputLogPath)) Directory.CreateDirectory(Logger.OutputLogPath);

        //            string filename = "Log-" + Logger.FormatDate(DateTime.Now) + ".xml";

        //            string path = null;

        //            switch (type)
        //            {
        //                case LogLineType.Debug: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_DEBUG, filename); break;
        //                case LogLineType.Error: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_ERROR, filename); break;
        //                case LogLineType.Notification: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_NOTIFICATION, filename); break;
        //                case LogLineType.Warning: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_WARNING, filename); break;
        //            }

        //            // Create Log (XmlDocument)
        //            XmlDocument doc = CreateDocument(path);

        //            foreach (Line line in lines)
        //            {
        //                AddToLog(doc, line);
        //            }

        //            Files.WriteDocument(doc, path);
        //        }
        //        catch (Exception ex) { Console.WriteLine("ProcessQueue(LogLineType) :: Exception :: " + type.ToString() + " :: " + ex.Message); }
        //    }
        //}

        private static XmlDocument CreateDocument(string path)
        {
            var result = new XmlDocument();

            if (!File.Exists(path))
            {
                XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
                result.AppendChild(docNode);
            }
            else
            {
                var xml = Files.ReadDocument(path);
                if (xml != null) result = xml;
            }

            return result;
        }

        private static XmlNode CreateRoot(XmlDocument doc)
        {
            XmlNode result;

            if (doc.DocumentElement == null)
            {
                result = doc.CreateElement("Log");

                // Add Created Timestamp Attribute
                var created = doc.CreateAttribute("created");
                string timestamp = DateTime.Now.ToString("o");
                created.Value = timestamp;
                result.Attributes.Append(created);

                doc.AppendChild(result);
            }
            else result = doc.DocumentElement;

            return result;
        }


        private void StartConfigurationFileWatcher()
        {
            var watcher = new FileSystemWatcher(LoggerConfiguration.ConfigFilePath);
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            configuration = LoggerConfiguration.Read();
        }


        public void CleanFiles()
        {
            if (configuration != null)
            {
                CleanFiles(configuration.DebugRecycleDays, LogLineType.Debug);
                CleanFiles(configuration.ErrorRecycleDays, LogLineType.Error);
                CleanFiles(configuration.NotificationRecycleDays, LogLineType.Notification);
                CleanFiles(configuration.WarningRecycleDays, LogLineType.Warning);
            }
        }

        private static void CleanFiles(int days, LogLineType type)
        {
            string path = null;
            switch (type)
            {
                case LogLineType.Debug: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_DEBUG); break;
                case LogLineType.Error: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_ERROR); break;
                case LogLineType.Notification: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_NOTIFICATION); break;
                case LogLineType.Warning: path = Path.Combine(Logger.OutputLogPath, Logger.OUTPUT_DIRECTORY_WARNING); break;
            }

            if (days == 0)
            {
                // Delete everything
                if (path != null && !Directory.Exists(path)) Directory.Delete(path, true);
            }
            else
            {
                // Only delete files older than the days set in parameter
                DateTime threshold = DateTime.Now - TimeSpan.FromDays(days);

                if (path != null && Directory.Exists(path))
                {
                    string[] filePaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    if (filePaths.Length > 0)
                    {
                        foreach (var filePath in filePaths)
                        {
                            var fileInfo = new FileInfo(filePath);
                            if (fileInfo != null)
                            {
                                if (fileInfo.LastWriteTime < threshold) File.Delete(filePath);
                            }
                        }
                    }
                }
            }
        }

        private static string FormatTimestamp(DateTime date)
        {
            return date.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffffzzz");
        }
    }
}
