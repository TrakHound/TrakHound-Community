// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;

using TrakHound.Tools.XML;

namespace TrakHound.Logging
{
    public static class Logger
    {
        public static string OutputLogPath = FileLocations.Logs;

        public const string OUTPUT_DIRECTORY_DEBUG = "Debug";
        public const string OUTPUT_DIRECTORY_ERROR = "Error";
        public const string OUTPUT_DIRECTORY_NOTIFICATION = "Notification";
        public const string OUTPUT_DIRECTORY_WARNING = "Warning";

        public static string AppicationName { get; set; }

        private static LoggerConfiguration _loggerConfiguration;
        public static LoggerConfiguration LoggerConfiguration
        {
            get { return _loggerConfiguration; }
            set
            {
                _loggerConfiguration = value;
                if (logQueue != null) logQueue.configuration = _loggerConfiguration;
            }
        }

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
            try
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
            catch (Exception ex) { }         
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
                    string t = Attributes.Get(lineNode, "timestamp");
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

            string logDir = null;

            switch (type)
            {
                case LogLineType.Debug: logDir = OUTPUT_DIRECTORY_DEBUG; break;
                case LogLineType.Error: logDir = OUTPUT_DIRECTORY_ERROR; break;
                case LogLineType.Notification: logDir = OUTPUT_DIRECTORY_NOTIFICATION; break;
                case LogLineType.Warning: logDir = OUTPUT_DIRECTORY_WARNING; break;
            }

            if (!string.IsNullOrEmpty(logDir))
            {
                // Create filename based on current DateTime
                string filename = "Log-" + FormatDate(DateTime.Now) + ".xml";

                // Create full filepath
                string path = Path.Combine(OutputLogPath, logDir, filename);

                // Create temp file to read from (to help prevent file corruption when Logger is writing to the file)
                string tmpFileName = Path.ChangeExtension(Guid.NewGuid().ToString(), ".tmp");
                string tmpPath = Path.Combine(OutputLogPath, logDir, tmpFileName);

                if (File.Exists(path))
                {
                    File.Copy(path, tmpPath);

                    var doc = Files.ReadDocument(tmpPath);
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
                                    string t = Attributes.Get(lineNode, "timestamp");
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

                    if (File.Exists(tmpPath)) File.Delete(tmpPath);
                }
            }

            return result;
        }

        #endregion

        internal static string FormatDate(DateTime date)
        {
            return date.Year.ToString() + "-" + date.Month.ToString() + "-" + date.Day.ToString();
        }

    }
}
