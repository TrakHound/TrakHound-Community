// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Reflection;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    static class Tools
    {

        public static string GetValue(object obj, string prop)
        {
            if (obj != null)
            {
                foreach (PropertyInfo info in obj.GetType().GetProperties())
                {
                    if (info.Name == prop)
                    {
                        object val = info.GetValue(obj, null);
                        if (val != null)
                        {
                            return val.ToString().ToLower();
                        }
                    }
                }
            }
            return "";
        }

    }
}
