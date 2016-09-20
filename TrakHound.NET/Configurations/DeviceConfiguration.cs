// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;

using TrakHound.API;
using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound.Configurations
{

    public class DeviceConfiguration : IComparable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceConfiguration()
        {
            init();
        }

        void init()
        {
            Description = new Data.DescriptionInfo();
            CustomClasses = new List<object>();
        }

        #region "Properties"

        public Data.DescriptionInfo Description { get; set; }

        public Data.AgentInfo Agent { get; set; }

        public List<object> CustomClasses;

        public bool Enabled { get; set; }

        /// <summary>
        /// Used as a Globally Unique Id between any number of devices.
        /// Usually generated from a Guid.
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Used to identify when the configuration has been edited.
        /// This should be updated each time the configuration is changed.
        /// </summary>
        public string UpdateId { get; set; }

        /// <summary>
        /// Contains the original Xml configuration file
        /// (ex. Plugins use this to read their custom configuration classes)
        /// </summary>
        public XmlDocument Xml { get; set; }

        /// <summary>
        /// Used as a general index to make navigation between Configurations easier
        /// </summary>
        public int Index { get; set; }

        public string FilePath { get; set; }

        #endregion

        #region "Methods"

        public static string GenerateUniqueID()
        {
            return Guid.NewGuid().ToString("B");
        }

        /// <summary>
        /// Reads an XML file to parse configuration information.
        /// </summary>
        /// <param name="ConfigFilePath">Path to XML File</param>
        /// <returns>Returns a Machine_Settings object containing information found.</returns>
        public static DeviceConfiguration Read(string path)
        {
            DeviceConfiguration result = null;

            if (File.Exists(path))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);

                    result = Read(doc);

                    string filename = Path.GetFileNameWithoutExtension(path);
                    result.FilePath = filename;
                    XML_Functions.SetInnerText(result.Xml, "/FilePath", result.FilePath);

                    if (result.UniqueId == null)
                    {
                        result.UniqueId = GenerateUniqueID();
                        XML_Functions.SetInnerText(result.Xml, "/UniqueId", result.UniqueId);
                    }
                }
                catch (XmlException ex) { Logger.Log("XmlException :: " + ex.Message); }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
            }
            else
            {
                Logger.Log("Configuration File Not Found : " + path, LogLineType.Warning);
            }

            return result;
        }

        public static DeviceConfiguration Read(XmlDocument xml)
        {
            var result = new DeviceConfiguration();

            result.Xml = xml;

            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.InnerText != "")
                    {
                        switch (node.Name.ToLower())
                        {
                            case "description": result.Description = ProcessDescription(node); break;

                            case "agent": result.Agent = ProcessAgent(node); break;

                            default:

                                Type Settings = typeof(DeviceConfiguration);
                                PropertyInfo info = Settings.GetProperty(node.Name);

                                if (info != null)
                                {
                                    Type t = info.PropertyType;
                                    info.SetValue(result, Convert.ChangeType(node.InnerText, t), null);
                                }

                                break;
                        }
                    }
                }
            }

            return result;
        }

        public static DeviceConfiguration[] ReadAll(string path)
        {
            var result = new List<DeviceConfiguration>();

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    var config = Read(file);
                    if (config != null)
                    {
                        // Check to make sure Unique Id is not already used by another file.
                        // If so, generate a new one. This can happen if the file was manually copied
                        if (result.Exists(x => x.UniqueId == config.UniqueId))
                        {
                            config.UniqueId = GenerateUniqueID();

                            Save(config);
                        }

                        result.Add(config);
                    }
                }
            }
            else
            {
                Logger.Log("Configuration File Directory Not Found : " + path, LogLineType.Warning);
            }

            return result.ToArray();
        }

        private static Data.DescriptionInfo ProcessDescription(XmlNode Node)
        {
            var result = new Data.DescriptionInfo();

            foreach (XmlNode child in Node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText != "")
                    {
                        Type Settings = typeof(Data.DescriptionInfo);
                        PropertyInfo info = Settings.GetProperty(child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
                        }
                    }
                }
            }

            return result;
        }

        private static Data.AgentInfo ProcessAgent(XmlNode Node)
        {
            var result = new Data.AgentInfo();

            foreach (XmlNode child in Node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText != "")
                    {
                        Type Settings = typeof(Data.AgentInfo);
                        PropertyInfo info = Settings.GetProperty(child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
                        }
                    }
                }
            }

            return result;
        }


        public static bool Save(DeviceConfiguration config)
        {
            return Save(config, FileLocations.Devices);
        }

        public static bool Save(DeviceConfiguration config, string path)
        {
            bool result = false;

            result = Save(config.Xml, path);

            return result;
        }

        public static bool Save(DataTable dt)
        {
            return Save(dt, FileLocations.Devices);
        }

        public static bool Save(DataTable dt, string path)
        {
            bool result = false;

            XmlDocument xml = Converters.DeviceConfigurationConverter.TableToXML(dt);

            result = Save(xml, path);

            return result;
        }

        public static bool Save(XmlDocument xml)
        {
            return Save(xml, FileLocations.Devices);
        }

        public static bool Save(XmlDocument xml, string path)
        {
            bool result = false;

            if (xml != null)
            {
                try
                {
                    string filePath = XML_Functions.GetInnerText(xml, "FilePath");

                    if (filePath == null)
                    {
                        filePath = XML_Functions.GetInnerText(xml, "UniqueId");
                        XML_Functions.SetInnerText(xml, "FilePath", filePath);
                    }

                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    xml.Save(Path.Combine(path, Path.ChangeExtension(filePath, ".xml")));

                    result = true;
                }
                catch (Exception ex) { Logger.Log("Error during Configuration Xml Save : " + ex.Message, LogLineType.Warning); }
            }

            return result;
        }

        #endregion

        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as DeviceConfiguration;
            if (i != null)
            {
                if (i > this) return -1;
                else if (i < this) return 1;
                else return 0;
            }
            else return 1;
        }

        public override bool Equals(object obj)
        {

            var other = obj as DeviceConfiguration;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        #region "Private"

        static bool EqualTo(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.UniqueId == c2.UniqueId && c1.Index == c2.Index;
        }

        static bool NotEqualTo(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.UniqueId != c2.UniqueId || c1.Index != c2.Index;
        }

        static bool LessThan(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            if (c1.Index > c2.Index) return false;
            else return true;
        }

        static bool GreaterThan(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            if (c1.Index < c2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion

    }
    
    public static class Extensions
    {
        public static bool ChangeAndNotify<T>(this PropertyChangedEventHandler handler,
             ref T field, T value, Expression<Func<T>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Lambda must return a property.");
            }
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            var vmExpression = body.Expression as ConstantExpression;
            if (vmExpression != null)
            {
                LambdaExpression lambda = Expression.Lambda(vmExpression);
                Delegate vmFunc = lambda.Compile();
                object sender = vmFunc.DynamicInvoke();

                if (handler != null)
                {
                    handler(sender, new PropertyChangedEventArgs(body.Member.Name));
                }
            }

            field = value;
            return true;
        }
    }

}
