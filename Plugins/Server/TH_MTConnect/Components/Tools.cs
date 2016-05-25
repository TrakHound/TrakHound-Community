// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using System.Xml;

namespace TH_MTConnect.Components
{
    public static class Tools
    {
        public static string GetFullAddress(XmlNode node)
        {
            string result = "";

            do
            {
                string name = node.Name;

                string address = name;

                string id = XML.GetAttribute(node, "id");
                if (!String.IsNullOrEmpty(id))
                {
                    address = name + "[@id='" + id + "']";
                }

                result = address + "/" + result;
                node = node.ParentNode;

                if (node == null) break;

            } while (node.Name != "Device");

            if (result.Length > 0)
            {
                if (result[0] != Convert.ToChar("/")) result = "/" + result;
                if (result.Length > 1)
                {
                    if (result[result.Length - 1] == Convert.ToChar("/")) result = result.Remove(result.Length - 1);
                }
            }

            return result;
        }
    }
}
