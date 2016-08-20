// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Reflection;

using TrakHound.Logging;

namespace TrakHound.Servers.DataProcessing
{
    public partial class ProcessingServer
    {

        void PrintHeader()
        {
            Logger.Log("--------------------------------------------------", LogLineType.Console);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TrakHound.Servers.DataProcessing.Header.txt";

            try
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    string trakhoundlogo = reader.ReadToEnd();

                    if (trakhoundlogo.Contains("[v]")) trakhoundlogo = trakhoundlogo.Replace("[v]", GetVersion());

                    Logger.Log(trakhoundlogo, LogLineType.Console);
                }
            }
            catch (Exception ex) { }

            Logger.Log("--------------------------------------------------", LogLineType.Console);
        }

        static string GetVersion()
        {
            // Build Information
            var assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            return "v" + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();
        }

    }
}
