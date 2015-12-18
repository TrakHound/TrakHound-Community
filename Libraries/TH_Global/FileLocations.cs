// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace TH_Global
{
    public static class FileLocations
    {

        public static string TrakHound = Environment.GetEnvironmentVariable("SystemDrive") + @"\TrakHound";

        public static string TrakHoundTemp = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TrakHound\\temp";

        public static string Plugins = TrakHound + @"\Plugins";


        #region "TempDirectory"

        public static void CreateTempDirectory()
        {
            string tempDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            tempDirectory = tempDirectory + "\\" + "TrakHound";

            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

            tempDirectory = tempDirectory + "\\" + "temp";

            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);
        }

        public static void CleanTempDirectory()
        {
            if (!Directory.Exists(TrakHoundTemp)) Directory.Delete(TrakHoundTemp, true);
        }

        #endregion

    }
}
