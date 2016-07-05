using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using System.Data;

using TH_Global.TrakHound.Configurations;
using TH_Global.Functions;

using MTConnect.Application.Components;

namespace TH_DeviceManager
{
    class AutoGenerate
    {

        //private void UpdateAgentConfiguration(Configuration config)
        //{
        //    // Save IP Address
        //    config.Agent.Address = info.IPAddress;
        //    XML_Functions.SetInnerText(config.ConfigurationXML, "/Agent/Address", info.IPAddress);

        //    // Save Port
        //    config.Agent.Port = info.Port;
        //    XML_Functions.SetInnerText(config.ConfigurationXML, "/Agent/Port", info.Port.ToString());

        //    // Save DeviceName
        //    config.Agent.DeviceName = info.Device.Name;
        //    XML_Functions.SetInnerText(config.ConfigurationXML, "/Agent/DeviceName", info.Device.Name);

        //    // Save Heartbeat
        //    config.Agent.Heartbeat = 5000;
        //    XML_Functions.SetInnerText(config.ConfigurationXML, "/Agent/Heartbeat", "5000");
        //}

        //private void UpdateDatabaseConfiguration(Configuration config)
        //{
        //    // If NO databases are configured then add a SQLite database for both client and server
        //    if (config.Databases_Client.Databases.Count == 0 && config.Databases_Server.Databases.Count == 0)
        //    {
        //        config.DatabaseId = Configuration.GenerateDatabaseId();

        //        AddDatabaseConfiguration("/Databases_Client", config);
        //        AddDatabaseConfiguration("/Databases_Server", config);
        //    }
        //}

        //private void AddDatabaseConfiguration(string prefix, Configuration config)
        //{
        //    XML_Functions.AddNode(config.ConfigurationXML, prefix + "/SQLite");
        //    XML_Functions.SetAttribute(config.ConfigurationXML, prefix + "/SQLite", "id", "00");

        //    string path = FileLocations.Databases + "\\TrakHound.db";

        //    XML_Functions.SetInnerText(config.ConfigurationXML, prefix + "/SQLite/DatabasePath", path);
        //}

        //private void UpdateDescriptionConfiguration(DeviceInfo info, Configuration config)
        //{
        //    // Save Device Description
        //    string val = info.Device.Description.CDATA;
        //    if (val != null) val.Trim();
        //    config.Description.Description = val;
        //    XML_Functions.SetInnerText(config.ConfigurationXML, "/Description/Description", val);

        //    // Save Serial Number
        //    val = info.Device.Description.SerialNumber;
        //    if (val != null) val.Trim();
        //    config.Description.Serial = val;
        //    XML_Functions.SetInnerText(config.ConfigurationXML, "/Description/Serial", val);

        //    // Save Manufacturer
        //    val = info.Device.Description.Manufacturer;
        //    if (val != null) val.Trim();
        //    config.Description.Manufacturer = val;
        //    XML_Functions.SetInnerText(config.ConfigurationXML, "/Description/Manufacturer", val);
        //}


        //private void SetInstanceData(XmlDocument doc, List<DataItem> probeItems)
        //{
        //    // Add root node
        //    XML_Functions.AddNode(doc, "/InstanceTable");

        //    // Add DataItems node
        //    XML_Functions.AddNode(doc, "/InstanceTable/DataItems");

        //    // Set Table Defaults
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Conditions", "False");
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Events", "False");
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Samples", "False");

        //    foreach (var item in probeItems)
        //    {
        //        if (item.Category == DataItemCategory.SAMPLE || item.Type == "LINE" || item.Type == "BLOCK")
        //        {
        //            XML_Functions.AddNode(doc, "/InstanceTable/DataItems/Omit/" + item.Id);
        //        }
        //    }
        //}

        private void SetInstanceData(DataTable dt, List<DataItem> probeItems)
        {
            // Set Table Defaults
            DataTable_Functions.UpdateTableValue(dt, "address", "/InstanceTable/DataItems/Conditions", "value", "False");
            DataTable_Functions.UpdateTableValue(dt, "address", "/InstanceTable/DataItems/Events", "value", "False");
            DataTable_Functions.UpdateTableValue(dt, "address", "/InstanceTable/DataItems/Samples", "value", "False");

            // Set Omit Items
            foreach (var item in probeItems)
            {
                if (item.Category == DataItemCategory.SAMPLE || item.Type == "LINE" || item.Type == "BLOCK")
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", "/InstanceTable/DataItems/Omit/" + item.Id, null, null);
                }
            }
        }

        // Use for separate event for (Full vs Limited Production)
        private static void AddFullProductionValue(string eventPrefix, DataTable dt, List<DataItem> probeData)
        {
            if (probeData.Exists(x => x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType != "JOG"))
            {
                // Add Value
                string valuePrefix = eventPrefix + "/Value||00";
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||00;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = probeData.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
                if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||AVAILABLE;");

                item = probeData.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
                if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||01", "attributes", "id||01;link||" + item.Id + ";link_type||ID;value||ARMED;");

                item = probeData.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "SYSTEM" && x.FullAddress.Contains("controller"));
                if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||02", "attributes", "id||02;link||" + item.Id + ";link_type||ID;modifier||NOT;value||Fault;");

                item = probeData.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||03", "attributes", "id||03;link||" + item.Id + ";link_type||ID;value||PRODUCTION;");

                // Add Feedrate Overrides as MultiTriggers requiring that the value be >= 100
                var feedrateItems = probeData.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType != "JOG");
                if (feedrateItems != null)
                {
                    for (var x = 0; x < feedrateItems.Count; x++)
                    {
                        DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/MultiTrigger||" + x.ToString("00"), "attributes", "id||" + x.ToString("00") + ";");
                        DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/MultiTrigger||" + x.ToString("00") + "/Trigger||00", "attributes", "id||00;link||" + feedrateItems[x].Id + ";link_type||ID;modifier||greater_than;value||100;");
                        DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/MultiTrigger||" + x.ToString("00") + "/Trigger||01", "attributes", "id||01;link||" + feedrateItems[x].Id + ";link_type||ID;value||100;");
                    }
                }

                // Add Result
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Full Production");
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||2;");
            }
        }

        //private void SetGeneratedEvents(DataTable dt, List<DataItem> probeItems)

        //private void SetGeneratedEvents(DataTable dt, List<DataItem> probeItems)
        //{
        //    // Add Production Event
        //    string eventPrefix = "/GeneratedData/GeneratedEvents/Event||00";
        //    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix, "attributes", "id||00;name||production_status;");



        //    // Add Default
        //    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "value", "Production");
        //    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "attributes", "numval||2;");


        //    // Add root node
        //    XML_Functions.AddNode(doc, "/GeneratedData/GeneratedEvents");

        //    // Add Production Event
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event", "id", "00");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event", "name", "production_status");

        //    // Add Production Value
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value", "id", "00");
        //    XML_Functions.AddNode(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "id", "00");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link", "AVAILABILITY");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link_type", "TYPE");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "value", "AVAILABLE");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "id", "01");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link", "EMERGENCY_STOP");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link_type", "TYPE");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "value", "ARMED");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Result", "numval", "2");
        //    XML_Functions.SetInnerText(doc, "/GeneratedData/GeneratedEvents/Event/Value/Result", "Production");

        //    // Add Idle Value
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value", "id", "00");
        //    XML_Functions.AddNode(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "id", "00");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link", "AVAILABILITY");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link_type", "TYPE");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "value", "AVAILABLE");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "id", "01");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link", "EMERGENCY_STOP");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link_type", "TYPE");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "value", "ARMED");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Result", "numval", "2");
        //    XML_Functions.SetInnerText(doc, "/GeneratedData/GeneratedEvents/Event/Value/Result", "Production");







        //    XML_Functions.AddNode(doc, "/InstanceTable/DataItems");

        //    // Set Table Defaults
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Conditions", "False");
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Events", "False");
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Samples", "False");

        //    foreach (var item in probeItems)
        //    {
        //        if (item.Category == DataItemCategory.SAMPLE || item.Type == "LINE" || item.Type == "BLOCK")
        //        {
        //            XML_Functions.AddNode(doc, "/InstanceTable/DataItems/Omit/" + item.Id);
        //        }
        //    }
        //}

        //private void SetGeneratedEvents(XmlDocument doc, List<DataItem> probeItems)
        //{
        //    // Add root node
        //    XML_Functions.AddNode(doc, "/GeneratedData/GeneratedEvents");

        //    // Add Production Event
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event", "id", "00");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event", "name", "production_status");

        //    // Add Production Value
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value", "id", "00");
        //    XML_Functions.AddNode(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "id", "00");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link", "AVAILABILITY");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link_type", "TYPE");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "value", "AVAILABLE");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "id", "01");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link", "EMERGENCY_STOP");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link_type", "TYPE");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "value", "ARMED");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Result", "numval", "2");
        //    XML_Functions.SetInnerText(doc, "/GeneratedData/GeneratedEvents/Event/Value/Result", "Production");

        //    // Add Idle Value
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value", "id", "00");
        //    XML_Functions.AddNode(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "id", "00");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link", "AVAILABILITY");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link_type", "TYPE");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "value", "AVAILABLE");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "id", "01");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link", "EMERGENCY_STOP");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "link_type", "TYPE");
        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Triggers/Trigger", "value", "ARMED");

        //    XML_Functions.SetAttribute(doc, "/GeneratedData/GeneratedEvents/Event/Value/Result", "numval", "2");
        //    XML_Functions.SetInnerText(doc, "/GeneratedData/GeneratedEvents/Event/Value/Result", "Production");







        //    XML_Functions.AddNode(doc, "/InstanceTable/DataItems");

        //    // Set Table Defaults
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Conditions", "False");
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Events", "False");
        //    XML_Functions.SetInnerText(doc, "/InstanceTable/DataItems/Samples", "False");

        //    foreach (var item in probeItems)
        //    {
        //        if (item.Category == DataItemCategory.SAMPLE || item.Type == "LINE" || item.Type == "BLOCK")
        //        {
        //            XML_Functions.AddNode(doc, "/InstanceTable/DataItems/Omit/" + item.Id);
        //        }
        //    }
        //}


    }
}
