using System;
using System.Collections.Generic;
using System.IO;

//using TH_MTConnect.Components;
//using TH_MTConnect.Streams;
//using TH_Status;

using TH_GitHub;

using TH_Global.Functions;

using TH_Configuration;
using TH_Mobile;
using TH_MTConnect;


namespace TestConsole
{
    class Program
    {

        static void Main(string[] args)
        {

            //var config = new Configuration();
            //config.UniqueId = Configuration.GenerateUniqueID();

            //TH_Mobile.Database.CreateTable("testuser", config);

            //var updateData = new TH_Mobile.UpdateData();
            //updateData.Connected = true;
            //updateData.Status = "Production";
            //updateData.ProductionStatus = "Full Production";
            //updateData.ProductionStatusTimer = 5000;

            //TH_Mobile.Database.Update("testuser", config, updateData);

            //TH_Mobile.Database.Get("testuser");

            //var returnData = TH_MTConnect.Components.Requests.Get("http://agent.mtconnect.org", null);
            //if (returnData != null)
            //{
            //    var items = returnData.Devices[0].GetAllDataItems();
            //    if (items != null)
            //    {

            //    }
            //}

            TH_Database.DatabasePluginReader.ReadPlugins();

            while (true)
            {
                var configs = TH_Configuration.Configuration.ReadAll(@"C:\TrakHound\Devices\");
                foreach (var config in configs)
                {
                    if (config.ServerEnabled)
                    {
                        TH_Database.Global.Initialize(config.Databases_Server);

                        string msg = "";
                        bool ping = TH_Database.Global.Ping(config.Databases_Server.Databases[0], out msg);

                        Console.WriteLine(ping.ToString() + " :: " + msg);
                    }
                }

                System.Threading.Thread.Sleep(5000);
            }
            

            Console.ReadLine();







            //FileSystem_Functions.DeleteDirectory(@"C:\TestClientPlugins");




            //var infos = new List<StatusInfo>();

            //var probeData = TH_MTConnect.Components.Requests.Get("http://127.0.0.1:5000/OKUMA.Lathe/probe", null, 2000, 1);
            //if (probeData != null) infos = TH_Status.StatusInfo.GetList(probeData);

            //var currentData = TH_MTConnect.Streams.Requests.Get("http://127.0.0.1:5000/OKUMA.Lathe/current", null, 2000, 1);
            //if (currentData != null)
            //{
            //    TH_Status.StatusInfo.ProcessList(currentData, infos);
            //    foreach (var info in infos)
            //    {
            //        Console.WriteLine(info.Address + " :: " + info.Id + " = " + info.Value);
            //    }
            //}

            //var credentials = new Authentication.Crendentials();
            //credentials.Username = "patrickritchie";
            //credentials.Password = "ethan123";

            //var issue = new TH_GitHub.Issues.Issue();
            //issue.Title = "Test Issue 2";
            //issue.Content = "Issue Content";
            //issue.Comments = "Issue Comments";
            //issue.Username = "TrakHound";
            //issue.Type = Issues.IssueType.UserSubmitted;

            //TH_GitHub.Issues.Create(issue, credentials);



            //string time = TH_Global.Functions.TimeSpan_Functions.ToFormattedString(new TimeSpan(0, 0, 45));
            //Console.WriteLine(time);

            //time = TH_Global.Functions.TimeSpan_Functions.ToFormattedString(new TimeSpan(0, 1, 23));
            //Console.WriteLine(time);

            //time = TH_Global.Functions.TimeSpan_Functions.ToFormattedString(new TimeSpan(2,39, 1));
            //Console.WriteLine(time);

            //time = TH_Global.Functions.TimeSpan_Functions.ToFormattedString(new TimeSpan(0, 0, 0, 0, 329));
            //Console.WriteLine(time);


            //time = TH_Global.Functions.TimeSpan_Functions.ToFormattedString(new TimeSpan(3, 14, 7, 56, 246));
            //Console.WriteLine(time);

            //Console.ReadLine();

            
        }

    }
}
