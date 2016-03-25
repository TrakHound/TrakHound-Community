// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using TH_Global.Functions;

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

namespace TH_Global
{
    public class LogWriter : TextWriter
    {
        public delegate void Updated_Handler(string newline);
        public event Updated_Handler Updated;

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }

        public override void Write(char value)
        {
            if (Updated != null) Updated(value.ToString());
        }

        public override void Write(string value)
        {
            if (Updated != null) Updated(value);
        }

        public override void WriteLine(string value)
        {
            if (Updated != null) Updated(value);
        }
    }

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
        public static void Log(string text, [CallerFilePath] string filename = "", [CallerMemberName] string member = "", [CallerLineNumber] int lineNumber = 0)
        {
            string[] lines = text.Split(new string[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                //string assembly = "";
                //if (file != "") assembly = Path.GetFileName(Path.GetDirectoryName(file)).Replace(' ', '_');
                //file = Path.GetFileName(file);
                //member = member.Replace('.', '_');

                var queueLine = new Line();
                queueLine.Text = line;

                queueLine.Timestamp = DateTime.Now;

                var assembly = System.Reflection.Assembly.GetCallingAssembly();

                queueLine.Assembly = assembly.FullName;
                queueLine.Filename = filename;
                queueLine.Member = member;
                queueLine.LineNumber = lineNumber;

                logQueue.AddLineToQueue(queueLine);
                Console.WriteLine(line);
            }  
        }

        public static void ReadOutputLog()
        {



        }

        public static string ReadOutputLogText(string applicationName, DateTime timestamp)
        {
            string result = null;

            XmlNode[] nodes = ReadOutputLogXml(applicationName, timestamp);
            if (nodes != null)
            {
                result = "";

                foreach (XmlNode lineNode in nodes)
                {
                    string t = XML_Functions.GetAttributeValue(lineNode, null, "timestamp");
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

        public static XmlNode[] ReadOutputLogXml(string applicationName, DateTime timestamp)
        {
            XmlNode[] result = null;

            string path = FileLocations.Logs + @"\Log-" + FormatDate(DateTime.Now) + ".xml";

            try
            {
                var doc = new XmlDocument();
                doc.Load(path);

                if (doc.DocumentElement != null)
                {
                    var node = doc.DocumentElement.SelectSingleNode("//" + applicationName);
                    if (node != null)
                    {
                        var nodes = new List<XmlNode>();

                        foreach (XmlNode lineNode in node.ChildNodes)
                        {
                            string t = XML_Functions.GetAttributeValue(lineNode, null, "timestamp");
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
            catch (Exception ex)
            {
                Log(ex.Message);
            }

            return result;
        }

        public class Line
        {
            public Int64 Row { get; set; }

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
                DateTime.TryParse(XML_Functions.GetAttributeValue(lineNode, null, "timestamp"), out ts);

                line.Timestamp = ts;
                line.Assembly = XML_Functions.GetAttributeValue(lineNode, null, "assembly");
                line.Filename = XML_Functions.GetAttributeValue(lineNode, null, "filename");
                line.Member = XML_Functions.GetAttributeValue(lineNode, null, "member");

                string lineNumber = XML_Functions.GetAttributeValue(lineNode, null, "line");
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

            private DateTime previousTimestamp = DateTime.MinValue;

            public LogReader(string applicationName, DateTime StartTimestamp)
            {
                previousTimestamp = StartTimestamp;

                Init(applicationName);
            }

            public LogReader(string applicationName)
            {
                Init(applicationName);
            }

            private void Init(string applicationName)
            {
                AppicationName = applicationName;

                var watcher = new FileSystemWatcher(FileLocations.Logs);
                watcher.Changed += Watcher_Changed;
                watcher.EnableRaisingEvents = true;
            }

            private void Watcher_Changed(object sender, FileSystemEventArgs e)
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    var nodes = ReadOutputLogXml(AppicationName, previousTimestamp);
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            var line = Line.FromXmlNode(node);
                            previousTimestamp = line.Timestamp;
                            if (LineAdded != null) LineAdded(line);
                        }
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
            //return String.Format(@"yyyy-MM-ddTHH\:mm\:ss.fffffffzzz", date);
            return date.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffffzzz");
            //return date.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }
        

        /// <summary>
        /// Queue for writing log data to an Xml file
        /// </summary>
        private class LogQueue
        {
            System.Timers.Timer queue_TIMER;

            private List<Line> queue;

            public LogQueue()
            {
                queue = new List<Line>();

                queue_TIMER = new System.Timers.Timer();
                queue_TIMER.Interval = 5000;
                queue_TIMER.Elapsed += queue_TIMER_Elapsed;
                queue_TIMER.Enabled = true;
            }

            public void AddLineToQueue(Line line) { queue.Add(line); }

            void queue_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                if (queue != null && queue.Count > 0)
                {
                    try
                    {
                        FileLocations.CreateLogsDirectory();

                        string path = FileLocations.Logs + @"\Log-" + FormatDate(DateTime.Now) + ".xml";

                        // Create Log (XmlDocument)
                        XmlDocument doc = CreateDocument(path);

                        Line[] lines = queue.ToArray();
                        foreach (Line line in lines)
                        {
                            AddToLog(doc, line);
                        }

                        WriteDocument(doc, path);

                        queue.Clear();
                    }
                    catch (Exception ex) { Console.WriteLine("Logger.queue_TIMER_Elapsed() :: Exception :: " + ex.Message); }
                }
            }

            void AddToLog(XmlDocument doc, Line line)
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

            static XmlNode CreateLineNode(XmlDocument doc, Line line)
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

            static void WriteDocument(XmlDocument doc, string path)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                try
                {
                    using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    {
                        using (XmlWriter writer = XmlWriter.Create(fs, settings))
                        {
                            doc.Save(writer);
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Logger.WriteDocument() :: Exception :: " + ex.Message); }
            }

            static XmlDocument CreateDocument(string LogFile)
            {
                XmlDocument Result = new XmlDocument();

                if (!File.Exists(LogFile))
                {
                    XmlNode docNode = Result.CreateXmlDeclaration("1.0", "UTF-8", null);
                    Result.AppendChild(docNode);

                    FileLocations.CreateLogsDirectory();
                }
                else
                {
                    try
                    {
                        Result.Load(LogFile);
                    }
                    catch (Exception ex) { Console.WriteLine("Logger.CreateDocument() :: Exception :: " + ex.Message); }
                }

                return Result;
            }

            static XmlNode CreateRoot(XmlDocument doc)
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

        }

    }

}
