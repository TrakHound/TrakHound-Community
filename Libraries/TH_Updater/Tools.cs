using System;
using System.Text;
using System.Net;
using System.IO;

namespace TH_Updater
{
    public static class Tools
    {

        public static void DownloadFile(string url, string localFile)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(url, localFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during Update DownloadFile() : " + ex.Message);
            }
        }

        static Random random = new Random();
        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string CreateLocalFileName()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory + "temp";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string file = RandomString(20);

            string path = directory + "\\" + file;

            if (File.Exists(path)) CreateLocalFileName();

            return path;
        }

    }
}
