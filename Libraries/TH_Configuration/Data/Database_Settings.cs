using System;
using System.Collections.Generic;
using System.Xml;

using TH_Global.Functions;

namespace TH_Configuration
{
    public class Database_Settings
    {
        public Database_Settings()
        {
            Databases = new List<Database_Configuration>();
        }

        public List<Database_Configuration> Databases;
    }

    public class Database_Configuration
    {
        public string Type { get; set; }

        public bool Primary { get; set; }

        public object Configuration { get; set; }

        public XmlNode Node { get; set; }

        public string DatabaseName { get; set; }
    }
}
