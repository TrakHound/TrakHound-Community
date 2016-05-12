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


        public static bool IsFileLocked(string filepath)
        {
            FileStream stream = null;

            try
            {
                if (File.Exists(filepath))
                {
                    var file = new FileInfo(filepath);
                    if (file != null)
                    {
                        stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

    }
}
