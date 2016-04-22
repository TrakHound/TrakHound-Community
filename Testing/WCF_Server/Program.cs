using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.ServiceModel;

using TH_Global.Functions;

namespace WCF_Server
{
    //public interface ICallybacky
    //{
    //    [OperationContract]
    //    void OnCallback(string message);
    //}

    //[ServiceContract (CallbackContract = typeof(ICallybacky))]
    //public interface IStringReverser
    //{
    //    [OperationContract]
    //    string ReverseString(string value);

    //    [OperationContract]
    //    void Start();

    //    [OperationContract]
    //    void Stop();

    //    [OperationContract]
    //    long GetProgressValue();
    //}

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MessageServer : WCF_Functions.IMessage
    {
        ManualResetEvent stop;

        private long progressValue;

        WCF_Functions.IMessageCallback callback;

        public object SendData(WCF_Functions.MessageData data)
        {
            switch (data.Id)
            {
                case "start": Start(); break;

                case "stop": Stop(); break;
            }

            return "Data Sent Successfully!";
        } 



        public void Start()
        {
            Console.WriteLine("Server Started");
            callback = OperationContext.Current.GetCallbackChannel<WCF_Functions.IMessageCallback>();
            callback.OnCallback(new WCF_Functions.MessageData("start", "Message Server Started"));


            stop = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(Worker));
        }

        private void Worker(object o)
        {
            progressValue = 0;

            while (!stop.WaitOne(0, true))
            {
                progressValue++;

                Console.WriteLine(progressValue);
                callback.OnCallback(new WCF_Functions.MessageData("progress_value", progressValue.ToString()));

                Thread.Sleep(50);
            }
        }

        public void Stop()
        {
            Console.WriteLine("Stopped");
            callback.OnCallback(new WCF_Functions.MessageData("stop", "Message Server Stopped"));

            stop.Set();
        }

        public long GetProgressValue()
        {
            return progressValue;
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            var host = WCF_Functions.Server.Create<MessageServer>();

            Console.ReadLine();

            host.Close();
        }
    }
}
