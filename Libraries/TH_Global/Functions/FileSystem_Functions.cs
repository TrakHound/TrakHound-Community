using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace TH_Global.Functions
{
    public static class FileSystem_Functions
    {


        /// <summary>
        /// Delete directory and all of its contents without sending to Recycle Bin
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteDirectory(string path)
        {
            var root = new DirectoryInfo(path);

            foreach (var file in root.GetFiles())
            {
                File.Delete(file.FullName);
            }
            foreach (var dir in root.GetDirectories())
            {
                DeleteDirectory(dir.FullName);
            }

            Directory.Delete(root.FullName);
        }

    }
}
