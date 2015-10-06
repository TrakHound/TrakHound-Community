using System;
using System.Collections.Generic;
using System.Xml;

namespace TH_Configuration
{
    public class Database_Settings
    {
        public Database_Settings()
        {
            Databases = new List<object>();
            Nodes = new List<XmlNode>();
        }

        public List<object> Databases;

        public List<XmlNode> Nodes;

    }
}
