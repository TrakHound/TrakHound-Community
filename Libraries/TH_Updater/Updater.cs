using System;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace TH_Updater
{
    public class Updater
    {

        public Assembly assembly;

        public void Start(string url)
        {
            Thread worker = new Thread(new ParameterizedThreadStart(Prepare));
            worker.Start(url);
        }

        void Prepare(object url)
        {
            if (url != null && assembly != null)
            {
                string tempDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                tempDirectory = tempDirectory + "\\" + "TrakHound";
                if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);
                tempDirectory = tempDirectory + "\\" + "temp";
                if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

                // Get assembly directory
                string localDirectory = Path.GetDirectoryName(assembly.Location);

                // Create local path to download 'appinfo' file to
                string localFile = Tools.CreateLocalFileName(tempDirectory);

                // Download File
                Tools.DownloadFile(url.ToString(), localFile);

                if (File.Exists(localFile))
                {
                    string unzipDirectory = UnZip(localFile, tempDirectory);
                    if (unzipDirectory != null)
                    {
                        string keyName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));

                        string keyValue = unzipDirectory + ";" + localDirectory;

                        Tools.SetRegistryKey(keyName, keyValue);

                        Console.WriteLine("Update Registry Key :: " + keyName + " :: " + localDirectory + " :: " + unzipDirectory);

                        if (Finished != null) Finished();
                    }
                }
            }
        }

        string UnZip(string zipFilePath, string destinationPath)
        {
            string Result = null;

            try
            {
                string path = Tools.CreateLocalFileName(destinationPath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (Directory.Exists(path))
                {
                    //System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, path);
                    Result = path;
                }
            }
            catch (Exception ex) { Console.WriteLine("Update UnZip : Exception: " + ex.Message); }

            return Result;
        }

        public delegate void Finished_Handler();
        public event Finished_Handler Finished;

    }

    public class UpdatePerformer
    {

        string updateDirectory;
        string unzipDirectory;

        public void Update()
        {
            Thread worker = new Thread(new ThreadStart(UpdateWorker));
            worker.Start();
        }

        void UpdateWorker()
        {
            if (unzipDirectory != null && updateDirectory != null)
            {
                foreach (string file in Directory.GetFiles(unzipDirectory))
                {
                    string filename = Path.GetFileName(file);

                    File.Copy(file, updateDirectory + "\\" + filename, true);
                }
            }
        }

    }

}
