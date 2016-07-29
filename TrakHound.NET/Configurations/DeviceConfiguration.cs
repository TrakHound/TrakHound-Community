using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;

using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound.Configurations
{

    public class DeviceConfiguration : IComparable, INotifyPropertyChanged
    {
        /// <summary>
        /// *** IN PROGRESS :: 4-9-16 *** NOT FULLY IMPLENETED YET
        /// 
        /// Each property must also change the ConfigurationXML to match the change.
        /// This will keep the xml and object synced
        /// 
        /// Use the format below:
        /// 
        /// private string _propertyName;
        /// public string PropertyName
        /// {
        ///     get { return _propertyName; }
        ///     set
        ///     {
        ///         _propertyName = value;
        ///         TH_Configuration.UpdateConfigurationXML("PropertyName", _propertyName);
        ///     }
        /// }
        /// </summary>

        public Database_Settings Databases_Client;
        public Database_Settings Databases_Server;
        public FileLocation_Settings FileLocations;

        // Implemented INotifyPropertyChanged to Bind in WPF
        private Description_Settings _description;
        public Description_Settings Description
        {
            get { return _description; }
            set { PropertyChanged.ChangeAndNotify<Description_Settings>(ref _description, value, () => Description); }
        }
        public List<object> CustomClasses;

        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceConfiguration()
        {
            init();
        }

        void init()
        {
            FileLocations = new FileLocation_Settings();
            Description = new Description_Settings();
            Databases_Client = new Database_Settings();
            Databases_Server = new Database_Settings();
            CustomClasses = new List<object>();
        }

        public void UpdateConfigurationXML(string xpath, string value)
        {
            if (ConfigurationXML != null)
            {
                XML_Functions.SetInnerText(ConfigurationXML, xpath, value);
            }
        }

        #region "Properties"

        private bool _clientEnabled;
        public bool ClientEnabled
        {
            get { return _clientEnabled; }
            set
            {
                _clientEnabled = value;
                UpdateConfigurationXML("ClientEnabled", _clientEnabled.ToString());
            }
        }

        private bool _serverEnabled;
        public bool ServerEnabled
        {
            get { return _serverEnabled; }
            set
            {
                _serverEnabled = value;
                UpdateConfigurationXML("ServerEnabled", _serverEnabled.ToString());
            }
        }

        #region "Remote Configurations"

        //private bool _useTrakHoundCloud;
        //public bool UseTrakHoundCloud
        //{
        //    get { return _useTrakHoundCloud; }
        //    set
        //    {
        //        _useTrakHoundCloud = value;
        //        UpdateConfigurationXML("UseTrakHoundCloud", _useTrakHoundCloud.ToString());
        //    }
        //}

        //private bool _remote;
        //public bool Remote
        //{
        //    get { return _remote; }
        //    set
        //    {
        //        _remote = value;
        //        UpdateConfigurationXML("Remote", _remote.ToString());
        //    }
        //}

        private string _clientUpdateId;
        public string ClientUpdateId
        {
            get { return _clientUpdateId; }
            set
            {
                _clientUpdateId = value;
                UpdateConfigurationXML("ClientUpdateId", _clientUpdateId);
            }
        }

        private string _serverUpdateId;
        public string ServerUpdateId
        {
            get { return _serverUpdateId; }
            set
            {
                _serverUpdateId = value;
                UpdateConfigurationXML("ServerUpdateId", _serverUpdateId);
            }
        }

        private string _tablename;
        public string TableName
        {
            get { return _tablename; }
            set
            {
                _tablename = value;
                UpdateConfigurationXML("TableName", _tablename);
            }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                UpdateConfigurationXML("Version", _version);
            }
        }

        //private string _sharedId;
        //public string SharedId
        //{
        //    get { return _sharedId; }
        //    set
        //    {
        //        _sharedId = value;
        //        UpdateConfigurationXML("SharedId", _sharedId);
        //    }
        //}

        //private string _sharedLinkTag;
        //public string SharedLinkTag
        //{
        //    get { return _sharedLinkTag; }
        //    set
        //    {
        //        _sharedLinkTag = value;
        //        UpdateConfigurationXML("SharedLinkTag", _sharedLinkTag);
        //    }
        //}

        //private string _sharedTableName;
        //public string SharedTableName
        //{
        //    get { return _sharedTableName; }
        //    set
        //    {
        //        _sharedTableName = value;
        //        UpdateConfigurationXML("SharedTableName", _sharedTableName);
        //    }
        //}

        #endregion

        #region "Images"

        public Image Manufacturer_Logo { get; set; }

        public Image Device_Image { get; set; }

        #endregion

        /// <summary>
        /// Used as a UniqueId between any number of devices.
        /// Usually generated from random characters
        /// </summary>
        private string _uniqueId;
        public string UniqueId
        {
            get { return _uniqueId; }
            set
            {
                _uniqueId = value;
                UpdateConfigurationXML("UniqueId", _uniqueId);
            }
        }

        /// <summary>
        /// Used to link this device to tables in a database. This is typically used as the prefix for a table name.
        /// </summary>
        private string _databaseId;
        public string DatabaseId
        {
            get { return _databaseId; }
            set
            {
                _databaseId = value;
                UpdateConfigurationXML("DatabaseId", _databaseId);
            }
        }


        /// <summary>
        /// Used as a general index to make navigation between Configurations easier
        /// </summary>
        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                UpdateConfigurationXML("Index", _index.ToString());
            }
        }

        /// <summary>
        /// Contains the original XML configuration file
        /// (ex. Plugins use this to read their custom configuration classes)
        /// </summary>
        public XmlDocument ConfigurationXML { get; set; }

        public string FilePath { get; set; }

        /// <summary>
        /// The full path of the configuration file
        /// </summary>
        public string SettingsRootPath { get; set; }

        #endregion

        #region "Methods"

        public static string GenerateUniqueID()
        {
            return Guid.NewGuid().ToString("B");
        }

        public static string GenerateDatabaseId()
        {
            return String_Functions.RandomString(10);
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
                    XML_Functions.SetInnerText(result.ConfigurationXML, "/FilePath", result.FilePath);

                    if (result.UniqueId == null)
                    {
                        result.UniqueId = GenerateUniqueID();
                        XML_Functions.SetInnerText(result.ConfigurationXML, "/UniqueId", result.UniqueId);
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

        public static DeviceConfiguration Read(XmlDocument xml)
        {
            var result = new DeviceConfiguration();

            result.ConfigurationXML = xml;

            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.InnerText != "")
                    {
                        switch (node.Name.ToLower())
                        {
                            case "databases_client": result.Databases_Client = Process_Databases(node); break;
                            case "databases_server": result.Databases_Server = Process_Databases(node); break;
                            case "description": result.Description = Process_Description(node); break;
                            case "file_locations": result.FileLocations = Process_File_Locations(node, result.SettingsRootPath); break;

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

        private static Database_Settings Process_Databases(XmlNode node)
        {
            Database_Settings result = new Database_Settings();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    Database_Configuration db = new Database_Configuration();
                    db.Type = child.Name;
                    db.Node = child;
                    result.Databases.Add(db);
                }
            }

            return result;
        }

        private static Description_Settings Process_Description(XmlNode Node)
        {

            Description_Settings Result = new Description_Settings();

            List<Tuple<string, string>> Custom = new List<Tuple<string, string>>();

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {
                        Type Settings = typeof(Description_Settings);
                        PropertyInfo info = Settings.GetProperty(Child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                        }
                        else
                        {
                            // Allow for category nodes
                            CreateAddInfoAddress(Child, Child.Name, Custom);
                        }
                    }
                }
            }

            Result.Custom = Custom;

            return Result;

        }

        static void CreateAddInfoAddress(XmlNode Node, string address, List<Tuple<string, string>> List)
        {

            // Had to check for ChildNode count == 1 and then see if it is #text (wierd??!!??)
            if (Node.ChildNodes.Count > 1)
            {
                foreach (XmlNode CustomChild in Node.ChildNodes)
                {
                    if (CustomChild.NodeType == XmlNodeType.Element)
                    {
                        string New_address = address + "-" + CustomChild.Name;
                        CreateAddInfoAddress(CustomChild, New_address, List);
                    }
                }
            }
            else if (Node.ChildNodes.Count == 1)
            {
                if (Node.ChildNodes[0].NodeType == XmlNodeType.Text)
                {
                    List.Add(new Tuple<string, string>(address, Node.InnerText));
                }
                else
                {
                    List.Add(new Tuple<string, string>(address, Node.InnerText));
                }
            }
        }


        private static FileLocation_Settings Process_File_Locations(XmlNode Node, string rootPath)
        {

            FileLocation_Settings Result = new FileLocation_Settings();

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {

                        Type Settings = typeof(FileLocation_Settings);
                        PropertyInfo info = Settings.GetProperty(Child.Name);

                        string filepath = Child.InnerText;
                        if (!System.IO.Path.IsPathRooted(filepath)) filepath = rootPath + filepath;

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(Result, Convert.ChangeType(filepath, t), null);
                        }

                    }
                }
            }

            return Result;
        }

        public void Process_IConfiguration<T>(IDeviceConfiguration config, XmlNode Node)
        {
            config.ConfigurationPropertyChanged += Config_ConfigurationPropertyChanged;

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {
                        Type Setting = typeof(T);
                        PropertyInfo info = Setting.GetProperty(Child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(config, Convert.ChangeType(Child.InnerText, t), null);
                        }
                    }
                }
            }

        }

        private void Config_ConfigurationPropertyChanged(string xPath, string value)
        {
            //UpdateConfigurationXML(xPath, value);
        }

        public static void Process_CustomClass<T>(object customClass, XmlNode Node)
        {

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {

                        Type Setting = typeof(T);
                        PropertyInfo info = Setting.GetProperty(Child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(customClass, Convert.ChangeType(Child.InnerText, t), null);
                        }

                    }
                }
            }

        }


        #region "Static"

        public static bool Save(DeviceConfiguration config)
        {
            bool result = false;

            result = Save(config.ConfigurationXML);

            return result;
        }

        public static bool Save(DataTable dt)
        {
            bool result = false;

            XmlDocument xml = Converters.DeviceConfigurationConverter.TableToXML(dt);

            result = Save(xml);

            return result;
        }

        public static bool Save(XmlDocument xml)
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

                    if (!Directory.Exists(TrakHound.FileLocations.Devices)) Directory.CreateDirectory(TrakHound.FileLocations.Devices);

                    xml.Save(Path.Combine(TrakHound.FileLocations.Devices, Path.ChangeExtension(filePath, ".xml")));

                    result = true;
                }
                catch (Exception ex) { Logger.Log("Error during Configuration Xml Save : " + ex.Message, LogLineType.Warning); }
            }

            return result;
        }

        #endregion

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
