using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Data;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;

namespace TH_DeviceManager
{
    public partial class DeviceManagerList
    {
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
