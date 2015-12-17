// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;
using System.Collections.Generic;

using System.Runtime.CompilerServices;

using System.Xml;
using System.IO;

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

        static LogQueue logQueue = new LogQueue();

        public static void Log(string text,
                        string file = "",
                        string member = "",
                        int line = 0)
        {

            LogQueue.Line queueLine = new LogQueue.Line();
            queueLine.text = text;
            queueLine.file = file;
            queueLine.member = member;
            queueLine.lineNumber = line;

            logQueue.LineQueue.Add(queueLine);

            Console.WriteLine(text);

        }

        //public static void Log(string text,
        //                [CallerFilePath] string file = "",
        //                [CallerMemberName] string member = "",
        //                [CallerLineNumber] int line = 0)
        //{

        //    LogQueue.Line queueLine = new LogQueue.Line();
        //    queueLine.text = text;
        //    queueLine.file = file;
        //    queueLine.member = member;
        //    queueLine.lineNumber = line;

        //    logQueue.LineQueue.Add(queueLine);

        //    Console.WriteLine(text);

        //}
    }

    class LogQueue
    {

        System.Timers.Timer queue_TIMER;

        public List<Line> LineQueue;

        public class Line
        {
            public string text { get; set; }

            public string file { get; set; }
            public string member { get; set; }
            public int lineNumber { get; set; }
        }

        public LogQueue()
        {

            LineQueue = new List<Line>();

            queue_TIMER = new System.Timers.Timer();
            queue_TIMER.Interval = 5000;
            queue_TIMER.Elapsed += queue_TIMER_Elapsed;
            queue_TIMER.Enabled = true;

        }

        void queue_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(FileLocations.TrakHound)) Directory.CreateDirectory(FileLocations.TrakHound);

                string LogDirectory = FileLocations.TrakHound + @"\Logs";

                if (!Directory.Exists(LogDirectory)) Directory.CreateDirectory(LogDirectory);

                string LogFile = LogDirectory + @"\Log-" + FormatDate(DateTime.Now) + ".xml";

                // Create Log (XmlDocument)
                XmlDocument doc = CreateDocument(LogFile);

                Line[] lines = LineQueue.ToArray();

                foreach (Line line in lines)
                {
                    AddToLog(doc, line);
                }

                WriteDocument(doc, LogFile);

                LineQueue.Clear();
            }
            catch { }
        }

        void AddToLog(XmlDocument doc, Line line)
        {

            string text = line.text;
            string file = line.file;
            string member = line.member;
            int lineNumber = line.lineNumber;

            string assembly = "";
            if (file != "") assembly = Path.GetFileName(Path.GetDirectoryName(file));
            file = Path.GetFileName(file);
            member = member.Replace('.', '_');

            // Create Document Root
            XmlNode rootNode = CreateRoot(doc);

            //Assembly
            XmlNode assemblyNode = rootNode.SelectSingleNode("//" + assembly);

            if (assemblyNode == null)
            {
                XmlNode xn = doc.CreateElement(assembly);
                rootNode.AppendChild(xn);
                assemblyNode = xn;
            }

            //File
            XmlNode fileNode = assemblyNode.SelectSingleNode("//" + file);

            if (fileNode == null)
            {
                XmlNode xn = doc.CreateElement(file);
                assemblyNode.AppendChild(xn);
                fileNode = xn;
            }

            //Member
            XmlNode memberNode = fileNode.SelectSingleNode("//" + member);

            if (memberNode == null)
            {
                XmlNode xn = doc.CreateElement(member);
                fileNode.AppendChild(xn);
                memberNode = xn;
            }

            //Item
            XmlNode itemNode = doc.CreateElement("Item");

            XmlAttribute timestampAttribute = doc.CreateAttribute("timestamp");
            timestampAttribute.Value = DateTime.Now.ToString();
            itemNode.Attributes.Append(timestampAttribute);

            XmlAttribute lineAttribute = doc.CreateAttribute("line");
            lineAttribute.Value = lineNumber.ToString();
            itemNode.Attributes.Append(lineAttribute);

            XmlAttribute textAttribute = doc.CreateAttribute("text");
            textAttribute.Value = text;
            itemNode.Attributes.Append(textAttribute);

            memberNode.AppendChild(itemNode);

        }

        static void WriteDocument(XmlDocument doc, string LogFile)
        {

            XmlWriterSettings settings = new XmlWriterSettings();
            //settings.Async = true;
            settings.Indent = true;

            try
            {
                using (XmlWriter writer = XmlWriter.Create(LogFile, settings))
                {
                    doc.Save(writer);
                }
            }
            catch { }

        }

        static XmlDocument CreateDocument(string LogFile)
        {

            XmlDocument Result = new XmlDocument();

            if (!File.Exists(LogFile))
            {
                XmlNode docNode = Result.CreateXmlDeclaration("1.0", "UTF-8", null);
                Result.AppendChild(docNode);

                string directory = Path.GetDirectoryName(LogFile);

                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            }
            else
            {
                try
                {
                    Result.Load(LogFile);
                }
                catch { }             
            }

            return Result;

        }

        static XmlNode CreateRoot(XmlDocument doc)
        {

            XmlNode Result;

            if (doc.DocumentElement == null)
            {
                Result = doc.CreateElement("Root");
                doc.AppendChild(Result);
            }
            else Result = doc.DocumentElement;

            return Result;

        }

        static string FormatDate(DateTime date)
        {

            return date.Year.ToString() + "-" + date.Month.ToString() + "-" + date.Day.ToString();

        }

    }
}
