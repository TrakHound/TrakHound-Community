// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ServiceModel;

using TH_Global;
using TH_Global.Functions;

namespace TrakHound_Updater
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MessageServer : WCF_Functions.IMessage
    {
        public MessageServer()
        {
            callback = OperationContext.Current.GetCallbackChannel<WCF_Functions.IMessageCallback>();
        }


        private static WCF_Functions.IMessageCallback callback;


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

        public static void SendCallback(WCF_Functions.MessageData data)
        {
            try
            {
                callback.OnCallback(data);
            }
            catch (Exception ex) { Logger.Log("Exception :: " + ex.Message, Logger.LogLineType.Error); }
        }

    }
}
