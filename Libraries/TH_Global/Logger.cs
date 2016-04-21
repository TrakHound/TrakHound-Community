// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;

using TH_Global.Functions;

namespace TH_Global
{

    public static class Logger
    {
        public static string OutputLogPath = FileLocations.Logs;

        public static string AppicationName { get; set; }

        static LogQueue logQueue = new LogQueue();

        #region "Public"

        /// <summary>
        /// Add line to Log
        /// </summary>
        /// <param name="text"></param>
        /// <param name="file"></param>
        /// <param name="member"></param>
        /// <param name="lineNumber"></param>
        public static void Log(string text, LogLineType type = LogLineType.Notification, [CallerFilePath] string filename = "", [CallerMemberName] string member = "", [CallerLineNumber] int lineNumber = 0)
        {
            string[] lines = text.Split(new string[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var queueLine = new Line();
                queueLine.Text = line;
                queueLine.Type = type;
                queueLine.Timestamp = DateTime.Now;

                var assembly = Assembly.GetCallingAssembly();

                queueLine.Assembly = assembly.FullName;
                queueLine.Filename = filename;
                queueLine.Member = member;
                queueLine.LineNumber = lineNumber;

                logQueue.AddLineToQueue(queueLine);
                Console.WriteLine(line);
            }  
        }

        public static string ReadOutputLogText(string applicationName, DateTime timestamp)
        {
            string result = null;

            result += ReadOutputLogText(LogLineType.Debug, applicationName, timestamp);
            result += ReadOutputLogText(LogLineType.Error, applicationName, timestamp);
            result += ReadOutputLogText(LogLineType.Notification, applicationName, timestamp);
            result += ReadOutputLogText(LogLineType.Warning, applicationName, timestamp);

            return result;
        }

        private static string ReadOutputLogText(LogLineType type, string applicationName, DateTime timestamp)
        {
            string result = null;

            XmlNode[] nodes = ReadOutputLogXml(type, applicationName, timestamp);
            if (nodes != null)
            {
                result = "";

                foreach (XmlNode lineNode in nodes)
                {
                    string t = XML_Functions.GetAttributeValue(lineNode, "timestamp");
                    if (t != null)
                    {
                        DateTime date = DateTime.MinValue;
                        if (DateTime.TryParse(t, out date))
                        {
                            if (date > timestamp)
                            {
                                result += lineNode.InnerText + Environment.NewLine;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static XmlNode[] ReadOutputLogXml(LogLineType type, string applicationName, DateTime timestamp)
        {
            XmlNode[] result = null;

            string path = @"\Log-" + FormatDate(DateTime.Now) + ".xml";

            switch (type)
            {
                case LogLineType.Debug: path = FileLocations.DebugLogs + path; break;
                case LogLineType.Error: path = FileLocations.ErrorLogs + path; break;
                case LogLineType.Notification: path = FileLocations.NotificationLogs + path; break;
                case LogLineType.Warning: path = FileLocations.WarningLogs + path; break;
            }

            var doc = XML_Functions.ReadDocument(path);
            if (doc != null)
            {
                if (doc.DocumentElement != null)
                {
                    var node = doc.DocumentElement.SelectSingleNode("//" + applicationName);
                    if (node != null)
                    {
                        var nodes = new List<XmlNode>();

                        foreach (XmlNode lineNode in node.ChildNodes)
                        {
                            string t = XML_Functions.GetAttributeValue(lineNode, "timestamp");
                            if (t != null)
                            {
                                DateTime date = DateTime.MinValue;
                                if (DateTime.TryParse(t, out date))
                                {
                                    if (date > timestamp)
                                    {
                                        nodes.Add(lineNode);
                                    }
                                }
                            }
                        }

                        result = nodes.ToArray();
                    }
                }
            }

            //try
            //{
            //    var doc = new XmlDocument();
            //    doc.Load(path);

            //    if (doc.DocumentElement != null)
            //    {
            //        var node = doc.DocumentElement.SelectSingleNode("//" + applicationName);
            //        if (node != null)
            //        {
            //            var nodes = new List<XmlNode>();

            //            foreach (XmlNode lineNode in node.ChildNodes)
            //            {
            //                string t = XML_Functions.GetAttributeValue(lineNode, "timestamp");
            //                if (t != null)
            //                {
            //                    DateTime date = DateTime.MinValue;
            //                    if (DateTime.TryParse(t, out date))
            //                    {
            //                        if (date > timestamp)
            //                        {
            //                            nodes.Add(lineNode);
            //                        }
            //                    }
            //                }
            //            }

            //            result = nodes.ToArray();
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log(ex.Message);
            //}

            return result;
        }

        public enum LogLineType
        {
            /// <summary>
            /// Used for debugging only, Shows detailed information
            /// </summary>
            Debug,

            /// <summary>
            /// Used to show warninig information (ex. 'Could not find file' when method continues anyways)
            /// </summary>
            Warning,

            /// <summary>
            /// Used to show error information (ex. a feature will not work because of this error)
            /// </summary>
            Error,

            /// <summary>
            /// Used to show notification information (ex. a feature has started)
            /// </summary>
            Notification,

            /// <summary>
            /// Used to only write to the console
            /// </summary>
            Console,
        }

        public class Line
        {
            public Int64 Row { get; set; }

            public LogLineType Type { get; set; }

            public string Text { get; set; }
            public DateTime Timestamp { get; set; }

            public string Assembly { get; set; }
            public string Filename { get; set; }
            public string Member { get; set; }
            public int LineNumber { get; set; }

            public static Line FromXmlNode(XmlNode lineNode)
            {
                var line = new Line();

                line.Text = lineNode.InnerText;

                DateTime ts = DateTime.MinValue;
                DateTime.TryParse(XML_Functions.GetAttributeValue(lineNode, "timestamp"), out ts);

                line.Timestamp = ts;
                line.Assembly = XML_Functions.GetAttributeValue(lineNode, "assembly");
                line.Filename = XML_Functions.GetAttributeValue(lineNode, "filename");
                line.Member = XML_Functions.GetAttributeValue(lineNode, "member");

                string lineNumber = XML_Functions.GetAttributeValue(lineNode, "line");
                if (lineNumber != null)
                {
                    int n = -1;
                    int.TryParse(lineNumber, out n);
                    line.LineNumber = n;
                }

                return line;
            }
        }

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

                FileLocations.CreateLogsDirectory();

                // Debug Watcher
                var debugWatcher = new FileSystemWatcher(FileLocations.DebugLogs);
                debugWatcher.Changed += DebugWatcher_Changed;
                debugWatcher.EnableRaisingEvents = true;

                // Error Watcher
                var errorWatcher = new FileSystemWatcher(FileLocations.ErrorLogs);
                errorWatcher.Changed += ErrorWatcher_Changed;
                errorWatcher.EnableRaisingEvents = true;

                // Notification Watcher
                var notificationWatcher = new FileSystemWatcher(FileLocations.NotificationLogs);
                notificationWatcher.Changed += NotificationWatcher_Changed;
                notificationWatcher.EnableRaisingEvents = true;

                // Warning Watcher
                var warningWatcher = new FileSystemWatcher(FileLocations.WarningLogs);
                warningWatcher.Changed += WarningWatcher_Changed;
                warningWatcher.EnableRaisingEvents = true;
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

                var nodes = ReadOutputLogXml(type, AppicationName, time);
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

        #endregion

        static string FormatDate(DateTime date)
        {
            return date.Year.ToString() + "-" + date.Month.ToString() + "-" + date.Day.ToString();
        }

        static string FormatTimestamp(DateTime date)
        {
            return date.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffffzzz");
        }
        

        /// <summary>
        /// Queue for writing log data to an Xml file
        /// </summary>
        private class LogQueue
        {
            System.Timers.Timer queue_TIMER;

            private List<Line> queue;

            private LoggerConfiguration configuration;

            public LogQueue()
            {
                configuration = LoggerConfiguration.Read();

                queue = new List<Line>();

                queue_TIMER = new System.Timers.Timer();
                queue_TIMER.Interval = Math.Max(500, configuration.QueueWriteInterval);
                queue_TIMER.Elapsed += queue_TIMER_Elapsed;
                queue_TIMER.Enabled = true;
            }

            private void queue_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                ProcessQueue();
            }


            public void AddLineToQueue(Line line)
            {
                bool add = false;
                switch(line.Type)
                {
                    case LogLineType.Debug: add = configuration.Debug; break;
                    case LogLineType.Error: add = configuration.Error; break;
                    case LogLineType.Notification: add = configuration.Notification; break;
                    case LogLineType.Warning: add = configuration.Warning; break;
                }

                if (add) queue.Add(line);
            }

            private void AddToLog(XmlDocument doc, Line line)
            {
                string appName = Logger.AppicationName;
                if (appName == null) appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);

                // Create Document Root
                XmlNode rootNode = CreateRoot(doc);

                //Application Name Node
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
                        FileLocations.CreateLogsDirectory();

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
                        FileLocations.CreateLogsDirectory();

                        string path = @"\Log-" + FormatDate(DateTime.Now) + ".xml";

                        switch (type)
                        {
                            case LogLineType.Debug: path = FileLocations.DebugLogs + path; break;
                            case LogLineType.Error: path = FileLocations.ErrorLogs + path; break;
                            case LogLineType.Notification: path = FileLocations.NotificationLogs + path; break;
                            case LogLineType.Warning: path = FileLocations.WarningLogs + path; break;
                        }

                        // Create Log (XmlDocument)
                        XmlDocument doc = CreateDocument(path);

                        foreach (Line line in lines)
                        {
                            AddToLog(doc, line);
                        }

                        XML_Functions.WriteDocument(doc, path);

                        //WriteDocument(doc, path);
                    }
                    catch (Exception ex) { Console.WriteLine("ProcessQueue(LogLineType) :: Exception :: " + type.ToString() + " :: " + ex.Message); }
                }
            }

            //private static void WriteDocument(XmlDocument doc, string path)
            //{
            //    XmlWriterSettings settings = new XmlWriterSettings();
            //    settings.Indent = true;

            //    try
            //    {
            //        using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            //        {
            //            using (XmlWriter writer = XmlWriter.Create(fs, settings))
            //            {
            //                doc.Save(writer);
            //            }
            //        }
            //    }
            //    catch (Exception ex) { Console.WriteLine("Logger.WriteDocument() :: Exception :: " + ex.Message); }
            //}

            private static XmlDocument CreateDocument(string LogFile)
            {
                var result = new XmlDocument();

                if (!File.Exists(LogFile))
                {
                    XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
                    result.AppendChild(docNode);

                    FileLocations.CreateLogsDirectory();
                }
                else
                {
                    try
                    {
                        result.Load(LogFile);
                    }
                    catch (Exception ex) { Console.WriteLine("Logger.CreateDocument() :: Exception :: " + ex.Message); }
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
                    string timestamp = DateTime.Now.ToLongDateString() + " ";
                    timestamp += DateTime.Now.ToLongTimeString();
                    created.Value = timestamp;
                    result.Attributes.Append(created);
                   
                    doc.AppendChild(result);
                }
                else result = doc.DocumentElement;

                return result;
            }


            private void StartConfigurationFileWatcher()
            {
                var watcher = new FileSystemWatcher(FileLocations.TrakHound, LoggerConfiguration.CONFIG_FILENAME);
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
                    case LogLineType.Debug: path = FileLocations.DebugLogs; break;
                    case LogLineType.Error: path = FileLocations.ErrorLogs; break;
                    case LogLineType.Notification: path = FileLocations.NotificationLogs; break;
                    case LogLineType.Warning: path = FileLocations.WarningLogs; break;
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
        }

        public class LoggerConfiguration
        {
            public LoggerConfiguration()
            {
                QueueWriteInterval = 5000;

                Debug = false;
                Error = true;
                Notification = true;
                Warning = true;

                DebugRecycleDays = 7;
                ErrorRecycleDays = 7;
                NotificationRecycleDays = 1;
                WarningRecycleDays = 1;
            }

            /// <summary>
            /// Sets the interval (in ms) that the queue writes to the log file (Minimum is 500ms)
            /// </summary>
            public int QueueWriteInterval { get; set; }


            /// <summary>
            /// Toggle Debug data collection On/Off
            /// </summary>
            public bool Debug { get; set; }

            /// <summary>
            /// Toggle Error data collection On/Off
            /// </summary>
            public bool Error { get; set; }

            /// <summary>
            /// Toggle Notification data collection On/Off
            /// </summary>
            public bool Notification { get; set; }

            /// <summary>
            /// Toggle Warning data collection On/Off
            /// </summary>
            public bool Warning { get; set; }


            /// <summary>
            /// Set number of days to keep Debug files
            /// </summary>
            public int DebugRecycleDays { get; set; }

            /// <summary>
            /// Set number of days to keep Error files
            /// </summary>
            public int ErrorRecycleDays { get; set; }

            /// <summary>
            /// Set number of days to keep Notification files
            /// </summary>
            public int NotificationRecycleDays { get; set; }

            /// <summary>
            /// Set number of days to keep Warning files
            /// </summary>
            public int WarningRecycleDays { get; set; }


            #region "Configuration File"

            public const string CONFIG_FILENAME = "logger_config.xml";
            public static string CONFIG_FILEPATH = FileLocations.TrakHound + @"\" + CONFIG_FILENAME;


            public static bool Create(LoggerConfiguration config)
            {
                bool result = false;

                Remove();

                if (config != null)
                {
                    var xml = CreateDocument(config);
                    WriteDocument(xml, CONFIG_FILEPATH);
                }

                return result;
            }

            public static LoggerConfiguration Read()
            {
                var result = new LoggerConfiguration();

                if (File.Exists(CONFIG_FILEPATH))
                {
                    try
                    {
                        var xml = new XmlDocument();
                        xml.Load(CONFIG_FILEPATH);

                        foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                        {
                            if (node.NodeType == XmlNodeType.Element)
                            {
                                if (node.InnerText != "")
                                {
                                    Type Settings = typeof(LoggerConfiguration);
                                    PropertyInfo info = Settings.GetProperty(node.Name);

                                    if (info != null)
                                    {
                                        Type t = info.PropertyType;
                                        info.SetValue(result, Convert.ChangeType(node.InnerText, t), null);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
                }

                return result;
            }

            public static void Remove()
            {
                if (File.Exists(CONFIG_FILEPATH)) File.Delete(CONFIG_FILEPATH);
            }


            private static XmlDocument CreateDocument(LoggerConfiguration config)
            {
                var result = new XmlDocument();

                XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
                result.AppendChild(docNode);

                XmlNode root = result.CreateElement("LoggerConfiguration");
                result.AppendChild(root);

                foreach (var info in typeof(LoggerConfiguration).GetProperties())
                {
                    XmlNode node = result.CreateElement(info.Name);
                    var val = info.GetValue(config, new object[] { });
                    if (val != null) node.InnerText = val.ToString();
                    root.AppendChild(node);
                }

                return result;
            }

            private static void WriteDocument(XmlDocument doc, string path)
            {
                var settings = new XmlWriterSettings();
                settings.Indent = true;

                try
                {
                    using (var writer = XmlWriter.Create(path, settings))
                    {
                        doc.Save(writer);
                    }
                }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
            }

            #endregion

        }

    }

}

// Used to enable autofilling parameter for CallerMemberName, CallerFilePath, and CallerLineNumber in .NET 4.0 (built in for .NET 4.5)
namespace System.Runtime.CompilerServices
{
    [AttributeUsageAttribute(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerMemberNameAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerFilePathAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerLineNumberAttribute : Attribute
    {
    }
}
