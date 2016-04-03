using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TH_DevicePage
{
    public static class Tools
    {

        public static string GetNode(string address, string name)
        {
            int index = address.ToLower().IndexOf(name);
            int length = (name).Length;

            length = address.IndexOf('/', index) - index;
            return address.Substring(index, length);
        }

        public static string GetId(string address, string name)
        {
            string node = GetNode(address, name);

            int index = node.IndexOf('[');
            if (index >= 0)
            {
                index = node.IndexOf('\'') + 1;
                int length = node.IndexOf('\'', index) - index;

                return node.Substring(index, length);
            }

            return null;
        }

    }
}
