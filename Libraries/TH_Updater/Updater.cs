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
            if (url != null)
            {
                // Create local path to download 'appinfo' file to
                string localFile = Tools.CreateLocalFileName() + ".zip";

                // Download File
                Tools.DownloadFile(url.ToString(), localFile);

                if (File.Exists(localFile))
                {
                    string unzipDirectory = UnZip(localFile);
                    if (unzipDirectory != null)
                    {
                        if (assembly != null)
                        {
                            string updateDirectory = Path.GetDirectoryName(assembly.Location);
                            Console.WriteLine("Update Directory = " + updateDirectory);
                        }
                    }
                }
            } 
        }

        string UnZip(string zipFilePath)
        {
            string Result = null;

            try
            {
                string path = Tools.CreateLocalFileName();
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (Directory.Exists(path))
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, path);
                    Result = path;
                }
            }
            catch (Exception ex) { Console.WriteLine("Update UnZip : Exception: " + ex.Message); }

            return Result;
        }

        //public void Update()
        //{
        //    Thread worker = new Thread(new ThreadStart(UpdateWorker));
        //    worker.Start();
        //}

        //void UpdateWorker()
        //{
        //    if (unzipDirectory != null && updateDirectory != null)
        //    {

        //        foreach (string file in Directory.GetFiles(unzipDirectory))
        //        {
        //            string filename = Path.GetFileName(file);

        //            File.Copy(file, updateDirectory + "\\" + filename, true);
        //        }
        //    }
        //}

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
