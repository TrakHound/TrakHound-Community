using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;


namespace TH_Global.Functions
{
    public static class XML_Functions
    {

        public static bool SetInnerText(XmlDocument doc, string address, string text)
        {
            bool result = false;

            if (doc != null)
            {
                XmlElement element = doc.DocumentElement;

                XmlNode node = null;

                string[] names = address.Split('/');
                //foreach (string name in names)
                for (var x = 0; x <= names.Length - 1; x++)
                {
                    node = element.SelectSingleNode(names[x]);
                    if (node != null)
                    {
                        if (x == names.Length - 1)
                        {
                            node.InnerText = text;
                            result = true;
                            break;
                        }
                    }
                    else break;
                }
            }

            return result;
        }




    }
}
