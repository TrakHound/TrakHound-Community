using System;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Reflection;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using TH_Global;

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
                FileLocations.CreateTempDirectory();

                // Get assembly directory
                string localDirectory = Path.GetDirectoryName(assembly.Location);

                // Create local path to download 'appinfo' file to
                string localFile = Tools.CreateLocalFileName(FileLocations.TrakHoundTemp) + ".zip";

                // Download File
                Tools.DownloadFile(url.ToString(), localFile);

                if (File.Exists(localFile))
                {
                    string unzipDirectory = UnZip(localFile, FileLocations.TrakHoundTemp);
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
                    ExtractZipFile(zipFilePath, null, path);
                    //System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, path);
                    Result = path;
                }
            }
            catch (Exception ex) { Console.WriteLine("Update UnZip : Exception: " + ex.Message); }

            return Result;
        }

        public void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExtractZipFile :: Exception :: " + ex.Message); 
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
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
