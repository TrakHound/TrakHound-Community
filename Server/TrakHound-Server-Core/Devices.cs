using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml;

using TH_Configuration;
using TH_Database;
using TH_Device_Server;
using TH_Global;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TrakHound_Server_Core
{
    public partial class Server
    {

        List<Configuration> configurations;

        void LoadDevices()
        {

            string configPath = null;

            Devices = new List<Device_Server>();

            if (currentuser != null)
            {
                configurations = Configurations.GetConfigurationsForUser(currentuser, userDatabaseSettings);
            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                configurations = ReadConfigurationFile();
            }

            if (configurations != null)
            {
                // Create DevicesList based on Configurations
                foreach (Configuration config in configurations)
                {
                    if (config.Remote) StartMonitor(config);

                    if (config.Enabled)
                    {
                        Device_Server server = new Device_Server(config);
                        server.configurationPath = configPath;
                        server.updateConfigurationFile = false;

                        // Initialize Database Configurations
                        Global.Initialize(server.configuration.Databases);

                        Devices.Add(server);
                    }
                }
            }

        }

        void StartMonitor(Configuration config)
        {
            ConfigurationMonitor monitor = new ConfigurationMonitor();
            monitor.CurrentConfiguration = config;
            monitor.Interval = 10000;
            monitor.TableName = config.TableName;
            monitor.ConfigurationChanged += monitor_ConfigurationChanged;

            monitor.Start();
        }

        void monitor_ConfigurationChanged(Configuration config)
        {
            Console.WriteLine("Configuration Changed!");
        }

        #region "Xml File"

        List<Configuration> ReadConfigurationFile()
        {
            List<Configuration> result = new List<Configuration>();

            string configPath;

            string localPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + "Configuration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "Configuration.Xml";

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            if (System.IO.File.Exists(configPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);

                foreach (XmlNode Node in doc.DocumentElement.ChildNodes)
                {
                    if (Node.NodeType == XmlNodeType.Element)
                    {
                        switch (Node.Name.ToLower())
                        {
                            case "devices":
                                foreach (XmlNode ChildNode in Node.ChildNodes)
                                {
                                    if (ChildNode.NodeType == XmlNodeType.Element)
                                    {
                                        switch (ChildNode.Name.ToLower())
                                        {
                                            case "device":

                                                Configuration config = GetSettingsFromNode(ChildNode);
                                                if (config != null) result.Add(config);

                                                break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            return result;
        }

        private Configuration GetSettingsFromNode(XmlNode Node)
        {

            Configuration Result = null;

            string configPath = null;

            foreach (XmlNode ChildNode in Node.ChildNodes)
            {
                switch (ChildNode.Name.ToLower())
                {
                    case "configuration_path": configPath = ChildNode.InnerText; break;
                }
            }

            if (configPath != null)
            {
                configPath = GetConfigurationPath(configPath);

                Result = Configuration.ReadConfigFile(configPath);
            }

            return Result;

        }

        static Device_Server ProcessDevice(int index, XmlNode node)
        {
            Device_Server Result = null;

            string configPath = null;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    if (childNode.Name.ToLower() == "configuration_path")
                    {
                        configPath = childNode.InnerText;
                    }
                }
            }

            if (configPath != null)
            {
                configPath = GetConfigurationPath(configPath);

                Logger.Log("Reading Device Configuration File @ '" + configPath + "'");

                if (File.Exists(configPath))
                {
                    Configuration config = new Configuration();
                    config = Configuration.ReadConfigFile(configPath);

                    if (config != null)
                    {
                        Console.WriteLine("Device Congifuration Read Successfully!");

                        config.Index = index;

                        Device_Server server = new Device_Server(config);
                        server.configurationPath = configPath;
                        server.updateConfigurationFile = false;

                        // Initialize Database Configurations
                        Global.Initialize(server.configuration.Databases);

                        Result = server;
                    }
                    else Logger.Log("Error Occurred While Reading : " + configPath);
                }
                else Logger.Log("Can't find Device Configuration file @ " + configPath);
            }
            else Logger.Log("No Device Congifuration found");

            return Result;

        }

        static string GetConfigurationPath(string path)
        {
            // If not full path, try System Dir ('C:\TrakHound\') and then local App Dir
            if (!System.IO.Path.IsPathRooted(path))
            {
                // Remove initial Backslash if contained in "configuration_path"
                if (path[0] == '\\' && path.Length > 1) path.Substring(1);

                string original = path;

                // Check System Path
                path = TH_Global.FileLocations.TrakHound + "\\Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Logger.Log(path + " Not Found");


                // Check local app Path
                path = AppDomain.CurrentDomain.BaseDirectory + "Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Logger.Log(path + " Not Found");

                // if no files exist return null
                return null;
            }
            else return path;
        }

        #endregion

    }
}
