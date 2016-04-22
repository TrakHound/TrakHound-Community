using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

using TH_Global.Functions;
using TH_Global.Updates;

namespace WCF_Client
{
    //public class MessageCallback : WCF_Functions.IMessageCallback
    //{

    //    public void OnCallback(WCF_Functions.MessageData data)
    //    {
    //        switch (data.Id)
    //        {
    //            case "download_progress_percentage": DownloadProgressPercentage(data); break;

    //            case "download_completed": DownloadCompleted(data); break;

    //            case "update_ready": UpdateReady(data); break;

    //            case "up_to_date": UpToDate(data); break;
    //        }
    //    }

    //    data.Data01 = Application Name
    //     data.Data02 = Application Title
    //     data.Data03 = Application SubTitle
    //     data.Data04 = Download Progress(int)
    //    private void DownloadProgressPercentage(WCF_Functions.MessageData data)
    //    {
    //        string message = "Download Progress";
    //        string name = data.Data01.ToString();
    //        string title = data.Data02.ToString();
    //        string subTitle = data.Data03.ToString();
    //        int percentage = (int)data.Data04;

    //        Console.WriteLine(message + " : " + name + " : " + title + " : " + subTitle + " : " + percentage.ToString());
    //    }

    //    data.Data01 = Application Name
    //     data.Data02 = Application Title
    //     data.Data03 = Application SubTitle
    //    private void DownloadCompleted(WCF_Functions.MessageData data)
    //    {
    //        string message = "Download Completed";
    //        string name = data.Data01.ToString();
    //        string title = data.Data02.ToString();
    //        string subTitle = data.Data03.ToString();

    //        Console.WriteLine(message + " : " + name + " : " + title + " : " + subTitle);
    //    }

    //    data.Data01 = Application Name
    //     data.Data02 = Application Title
    //     data.Data03 = Application SubTitle
    //    private void UpdateReady(WCF_Functions.MessageData data)
    //    {
    //        string message = "Update Ready";
    //        string name = data.Data01.ToString();
    //        string title = data.Data02.ToString();
    //        string subTitle = data.Data03.ToString();

    //        Console.WriteLine(message + " : " + name + " : " + title + " : " + subTitle);
    //    }

    //    data.Data01 = Application Name
    //     data.Data02 = Application Title
    //     data.Data03 = Application SubTitle
    //    private void UpToDate(WCF_Functions.MessageData data)
    //    {
    //        string message = "Up to Date";
    //        string name = data.Data01.ToString();
    //        string title = data.Data02.ToString();
    //        string subTitle = data.Data03.ToString();

    //        Console.WriteLine(message + " : " + name + " : " + title + " : " + subTitle);
    //    }

    //}



    class Program : WCF_Functions.IMessageCallback
    {
        static void Main(string[] args)
        {
            var callback = new Program();

            var proxy = WCF_Functions.Client.GetWithCallback(UpdateConfiguration.UPDATER_PIPE_NAME, callback);

            while (true) // Loop indefinitely
            {
                Console.WriteLine(">"); // Prompt
                string line = Console.ReadLine(); // Get string from user

                switch (line.ToLower())
                {
                    case "check":

                        proxy.SendData(new WCF_Functions.MessageData("check"));
                        break;

                    case "apply": 

                        proxy.SendData(new WCF_Functions.MessageData("apply"));
                        break;

                    case "clear": 

                        proxy.SendData(new WCF_Functions.MessageData("clear"));
                        break;
                }
            }
        }

        public void OnCallback(WCF_Functions.MessageData data)
        {
            switch (data.Id)
            {
                case "download_progress_percentage": DownloadProgressPercentage(data); break;

                case "download_completed": DownloadCompleted(data); break;

                case "update_ready": UpdateReady(data); break;

                case "up_to_date": UpToDate(data); break;
            }
        }

        // data.Data01 = Application Name
        // data.Data02 = Application Title
        // data.Data03 = Application SubTitle
        // data.Data04 = Download Progress (int)
        private void DownloadProgressPercentage(WCF_Functions.MessageData data)
        {
            string message = "Download Progress";
            string name = data.Data01.ToString();
            string title = data.Data02.ToString();
            string subTitle = data.Data03.ToString();
            int percentage = (int)data.Data04;

            Console.WriteLine(message + " : " + name + " : " + title + " : " + subTitle + " : " + percentage.ToString());
        }

        // data.Data01 = Application Name
        // data.Data02 = Application Title
        // data.Data03 = Application SubTitle
        private void DownloadCompleted(WCF_Functions.MessageData data)
        {
            string message = "Download Completed";
            string name = data.Data01.ToString();
            string title = data.Data02.ToString();
            string subTitle = data.Data03.ToString();

            Console.WriteLine(message + " : " + name + " : " + title + " : " + subTitle);
        }

        // data.Data01 = Application Name
        // data.Data02 = Application Title
        // data.Data03 = Application SubTitle
        private void UpdateReady(WCF_Functions.MessageData data)
        {
            string message = "Update Ready";
            string name = data.Data01.ToString();
            string title = data.Data02.ToString();
            string subTitle = data.Data03.ToString();

            Console.WriteLine(message + " : " + name + " : " + title + " : " + subTitle);
        }

        // data.Data01 = Application Name
        // data.Data02 = Application Title
        // data.Data03 = Application SubTitle
        private void UpToDate(WCF_Functions.MessageData data)
        {
            string message = "Up to Date";
            string name = data.Data01.ToString();
            string title = data.Data02.ToString();
            string subTitle = data.Data03.ToString();

            Console.WriteLine(message + " : " + name + " : " + title + " : " + subTitle);
        }

    }
}
