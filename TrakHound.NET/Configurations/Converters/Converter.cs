// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Xml;
using System.Reflection;

using TrakHound.Logging;

namespace TrakHound.Configurations.Converters
{
    public static class DeviceConfigurationConverter
    {

        public static DataTable XMLToTable(XmlDocument xml)
        {
            DataTable result = null;

            try
            {
                List<XmlToTable.TableInfo> infos = XmlToTable.XMLToTable_CreateData(xml);

                result = XmlToTable.XMLToTable_CreateTable(infos);
            }
            catch (Exception ex)
            {
                Logger.Log("XMLToTable() :: Exception :: " + ex.Message, LogLineType.Warning);
            }

            return result;
        }

        public static XmlDocument TableToXML(DataTable table)
        {
            XmlDocument result = null;

            result = TableToXml.Create(table);

            return result;
        }

        public static string TableToXML(DataTable table, string savePath)
        {
            string result = null;

            XmlDocument xml = TableToXml.Create(table);
            if (xml != null)
            {
                TableToXml.Save(xml, savePath);
                result = savePath;
            }

            return result;
        }

    }
}
