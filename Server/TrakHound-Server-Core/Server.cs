// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

using TH_Configuration;
using TH_Configuration.User;
using TH_Database;
using TH_Device_Server;
using TH_Global;

namespace TrakHound_Server_Core
{
    public class Server
    {

        List<Device_Server> Devices;

        public void Start()
        {

            DatabasePluginReader dpr = new DatabasePluginReader();

            Login();

            // Read Users and Login
            ReadUserManagementSettings();

            ReadConfigurations();

            if (Devices != null)
            {
                foreach (Device_Server device in Devices)
                {
                    device.Start(false);
                }
            }

        }

        public void Stop()
        {
            foreach (Device_Server device in Devices)
            {
                if (device != null) device.Close();
            }
        }


        bool rememberme;

        void Login()
        {
            // Remember Me
            UserConfiguration rememberUser = Management.GetRememberMe(Management.RememberMeType.Server);
            if (rememberUser != null)
            {
                if (rememberme) Management.SetRememberMe(rememberUser, Management.RememberMeType.Server);

                currentuser = rememberUser;

                Console.WriteLine("User Logged In!!");
            }
            else
            {
                Console.Write("Enter Username: ");
                string username = Console.ReadLine();

                string password = "";
                Console.Write("Enter your password: ");
                ConsoleKeyInfo key;

                do
                {
                    key = Console.ReadKey(true);

                    // Backspace Should Not Work
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        password += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                        {
                            password = password.Substring(0, (password.Length - 1));
                            Console.Write("\b \b");
                        }
                    }
                }
                // Stops Receving Keys Once Enter is Pressed
                while (key.Key != ConsoleKey.Enter);

                Console.WriteLine();
                Console.WriteLine("The Password You entered is : " + password);

                Console.Write("Remember Login (Y/N): ");
                string s = Console.ReadLine();
                if (s.Trim().ToLower() == "y") rememberme = true;
            }
        }


        UserConfiguration currentuser;

        Database_Settings userDatabaseSettings;

        List<Configuration> Configurations;

        void ReadConfigurations()
        {
            string configPath = null;

            Devices = new List<Device_Server>();

            if (currentuser != null)
            {
                if (userDatabaseSettings == null)
                {
                    Configurations = TH_Configuration.User.Management.GetConfigurationsForUser(currentuser);
                }
                else
                {
                    //Configurations = TH_Database.Tables.Users.GetConfigurationsForUser(currentuser, userDatabaseSettings);
                }
            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                Configurations = ReadConfigurationFile();
            }

            if (Configurations != null)
            {
                // Create DevicesList based on Configurations
                foreach (Configuration config in Configurations)
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

        void ReadUserManagementSettings()
        {
            DatabasePluginReader dpr = new DatabasePluginReader();

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

            string configPath;

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            Logger.Log(configPath);

            UserManagementSettings userSettings = UserManagementSettings.ReadConfiguration(configPath);

            if (userSettings != null)
            {
                if (userSettings.Databases.Databases.Count > 0)
                {
                    userDatabaseSettings = userSettings.Databases;
                    Global.Initialize(userDatabaseSettings);
                }
            }

        }

        //static List<Device_Server> ReadConfigurationFile()
        //{
        //    List<Device_Server> Result = new List<Device_Server>();

        //    string configPath;

        //    string localPath = AppDomain.CurrentDomain.BaseDirectory + "Configuration.Xml";
        //    string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "Configuration.Xml";

        //    // systemPath takes priority (easier for user to navigate to)
        //    if (File.Exists(systemPath)) configPath = systemPath;
        //    else configPath = localPath;

        //    Logger.Log(configPath);

        //    if (System.IO.File.Exists(configPath))
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load(configPath);

        //        int index = 0;

        //        foreach (XmlNode Node in doc.DocumentElement.ChildNodes)
        //        {
        //            if (Node.NodeType == XmlNodeType.Element)
        //            {
        //                switch (Node.Name.ToLower())
        //                {
        //                    case "devices":
        //                        foreach (XmlNode ChildNode in Node.ChildNodes)
        //                        {
        //                            if (ChildNode.NodeType == XmlNodeType.Element)
        //                            {
        //                                switch (ChildNode.Name.ToLower())
        //                                {
        //                                    case "device":

        //                                        Device_Server device = ProcessDevice(index, ChildNode);
        //                                        if (device != null)
        //                                        {
        //                                            Result.Add(device);
        //                                            index += 1;
        //                                        }
        //                                        break;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                }
        //            }  
        //        }

        //        Logger.Log("Configuration File Successfully Read From : " + configPath);
        //    }
        //    else Logger.Log("Configuration File Not Found : " + configPath);

        //    return Result;
        //}

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

    }
}
