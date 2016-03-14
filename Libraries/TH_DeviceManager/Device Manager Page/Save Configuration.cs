using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Data;
using System.Xml;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Server;
using TH_UserManagement.Management;


namespace TH_DeviceManager
{
    public partial class DeviceManagerPage
    {

        Thread save_THREAD;

        public void Save(DataTable dt)
        {
            Saving = true;

            if (dt != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        page.SaveConfiguration(dt);
                    }
                }

                if (save_THREAD != null) save_THREAD.Abort();

                save_THREAD = new Thread(new ParameterizedThreadStart(Save_Worker));
                save_THREAD.Start(dt);
            }
        }

        void Save_Worker(object o)
        {
            bool success = false;

            DataTable dt = (DataTable)o;

            if (dt != null)
            {
                string tablename = null;

                if (dt != null)
                {
                    tablename = dt.TableName;

                    // Reset Update ID
                    if (SelectedManagerType == ManagementType.Client) Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/ClientUpdateId", dt);
                    else if (SelectedManagerType == ManagementType.Server) Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/ServerUpdateId", dt);

                    // Add Unique Id (ONLY if one not already set)
                    //if (Table_Functions.GetTableValue("/UniqueId", dt) == null) Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/UniqueId", dt);

                    if (currentuser != null)
                    {
                        // Create backup in temp directory
                        XmlDocument backupXml = Converter.TableToXML(dt);
                        if (backupXml != null)
                        {
                            string temp_filename = currentuser.username + String_Functions.RandomString(20) + ".xml";

                            FileLocations.CreateTempDirectory();

                            string localPath = FileLocations.TrakHoundTemp + @"\" + temp_filename;

                            try { backupXml.Save(localPath); }
                            catch (Exception ex) { Logger.Log("Error during Configuration Xml Backup"); }
                        }

                        success = Configurations.ClearConfigurationTable(tablename);
                        if (success) success = Configurations.UpdateConfigurationTable(tablename, dt);
                    }
                    // If not logged in Save to File in 'C:\TrakHound\'
                    else
                    {
                        success = SaveFileConfiguration(dt);
                    }
                }

                ConfigurationTable = dt.Copy();

                XmlDocument xml = Converter.TableToXML(dt);
                if (xml != null)
                {
                    Configuration = Configuration.Read(xml);
                    Configuration.TableName = tablename;

                    //if (SelectedDeviceButton != null)
                    //{
                    //    SelectedDeviceButton.Config = SelectedDevice;
                    //}
                }
            }

            this.Dispatcher.BeginInvoke(new Action<bool>(Save_Finished), PRIORITY_BACKGROUND, new object[] { success });
        }

        //void Save_GUI(ConfigurationPage page)
        //{
        //    page.SaveConfiguration(ConfigurationTable);
        //}

        void Save_Finished(bool success)
        {
            if (!success) TH_WPF.MessageBox.Show("Device did not save correctly. Try Again." + Environment.NewLine + @"A backup of the Device has been created in the 'C:\TrakHound\Temp directory'");

            if (Configuration != null) LoadConfiguration();

            SaveNeeded = false;
            Saving = false;

            // Raise DeviceUpdated Event
            var args = new DeviceUpdateArgs();
            args.Event = DeviceUpdateEvent.Changed;
            UpdateDevice(Configuration, args);
        }

        void UpdateDevice(Configuration config, DeviceUpdateArgs args)
        {
            if (DeviceUpdated != null) DeviceUpdated(config, args);
        }

        static bool SaveFileConfiguration(Configuration config)
        {
            bool result = false;

            result = SaveFileConfiguration(config.ConfigurationXML);

            return result;
        }

        static bool SaveFileConfiguration(DataTable dt)
        {
            bool result = false;

            XmlDocument xml = Converter.TableToXML(dt);

            result = SaveFileConfiguration(xml);

            return result;
        }

        static bool SaveFileConfiguration(XmlDocument xml)
        {
            bool result = false;

            if (xml != null)
            {
                try
                {
                    string uniqueId = XML_Functions.GetInnerText(xml, "UniqueId");

                    xml.Save(FileLocations.Devices + "\\" + uniqueId + ".xml");

                    result = true;
                }
                catch (Exception ex) { Logger.Log("Error during Configuration Xml Save : " + ex.Message); }
            }

            return result;
        }

    }
}
