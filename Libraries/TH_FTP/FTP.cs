using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

using System.Drawing;

namespace TH_FTP
{
    public class FTP
    {

        public static bool Upload(string username, string password, string remotePath, string localPath)
        {
            bool result = false;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remotePath);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(username, password);


                byte[] fileData = File.ReadAllBytes(localPath);

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileData, 0, fileData.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                result = true;

                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Upload() : Exception : " + ex.Message);
            }

            return result;
        }



        public static Image DownloadImageTest(string username, string password, string remotePath)
        {
            Image result = null;

            var filePath = remotePath;
            WebRequest request = WebRequest.Create(filePath);
            request.Credentials = new NetworkCredential(username, password);
            using (var response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                using (System.IO.FileStream fs = new FileStream(@"C:\Temp\TestDownload", FileMode.Create))
                {
                    byte[] buffer = new byte[102400];
                    int read = 0;
                    do
                    {
                        read = stream.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, read);
                        fs.Flush();
                    } while (!(read == 0));
 
                    fs.Flush();
                    fs.Close();
                }
            }


            //using (var img = Image.FromStream(stream))
            //{
            //    result = img;
            //}


            //FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remotePath);
            //request.Method = WebRequestMethods.Ftp.DownloadFile;

            //request.Credentials = new NetworkCredential(username, password);

            //using (var response = (FtpWebResponse)request.GetResponse())
            //{
            //    using (var responseStream = response.GetResponseStream())
            //    {
            //        result = null;
            //    }
            //}

            
            //StreamReader reader = new StreamReader(responseStream);

            //result = reader.BaseStream;

            //reader.Close();
            //response.Close();
            
            return result;
        }


        //public static Stream Download(string username, string password, string remotePath)
        //{
        //    Stream result = null;

        //    try
        //    {
        //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remotePath);
        //    request.Method = WebRequestMethods.Ftp.DownloadFile;

        //    request.Credentials = new NetworkCredential(username, password);

        //    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

        //    Stream responseStream = response.GetResponseStream();
        //    //StreamReader reader = new StreamReader(responseStream);

        //    //result = reader.BaseStream;

        //    //reader.Close();
        //    response.Close();


        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log("Download() : Exception : " + ex.Message);
        //    }

        //    return result;
        //}


    }
}
