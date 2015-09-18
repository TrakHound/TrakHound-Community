// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

using TH_Configuration;
using TH_Device_Server;
using TH_Global;

namespace TrakHound_Server_Core
{
    public class Server
    {

        List<Device_Server> Devices;

        public void Start()
        {

            Devices = ReadDevices();

            foreach (Device_Server device in Devices)
            {
                device.Start(false);
            }
        }

        public void Stop()
        {
            foreach (Device_Server device in Devices)
            {
                if (device != null) device.Close();
            }
        }

        static List<Device_Server> ReadDevices()
        {
            List<Device_Server> Result = new List<Device_Server>();

            string path = AppDomain.CurrentDomain.BaseDirectory + "Devices.Xml";

            Logger.Log(path);

            if (System.IO.File.Exists(path))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                int index = 0;

                if (doc.DocumentElement.Name.ToLower() == "devices")
                {
                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            if (node.Name.ToLower() == "device")
                            {
                                Device_Server device = ProcessDevice(index, node);
                                if (device != null)
                                {
                                    Result.Add(device);
                                    index += 1;
                                }
                            }
                        }
                    }
                    Logger.Log("Devices File Successfully Read From : " + path);
                }
            }
            else Logger.Log("Devices File Not Found : " + path);

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

    }
}
