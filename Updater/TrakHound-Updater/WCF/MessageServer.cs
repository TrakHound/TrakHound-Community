using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.ServiceModel;
using TH_Global;
using TH_Global.Functions;

namespace TrakHound_Updater
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MessageServer : WCF_Functions.IMessage
    {

        private WCF_Functions.IMessageCallback callback;


        public object SendData(WCF_Functions.MessageData data)
        {
            if (data != null && data.Id != null)
            {
                switch (data.Id.ToLower())
                {
                    case "check": Service1.GetUpdates(); break;

                    case "apply": Service1.ApplyUpdates(); break;

                    case "clear": Update.ClearAll(); break;
                }
            }

            return "Data Sent Successfully!";
        }

        public void SendCallback(WCF_Functions.MessageData data)
        {
            if (callback == null)
            {
                callback = OperationContext.Current.GetCallbackChannel<WCF_Functions.IMessageCallback>();
            }

            try
            {
                callback.OnCallback(data);
            }
            catch (Exception ex) { Logger.Log("Exception :: " + ex.Message, Logger.LogLineType.Error); }
        }

    }
}
