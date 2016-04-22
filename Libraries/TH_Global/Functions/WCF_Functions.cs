using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;

namespace TH_Global.Functions
{
    public static class WCF_Functions
    {

        public const string DEFAULT_PIPE_NAME = "trakhound-wcf";

        public class MessageData
        {
            public MessageData() { }

            public MessageData(string id, object data)
            {
                Id = id;
                Data = data;
            }

            public string Id { get; set; }
            public object Data { get; set; }
        }

        public interface IMessageCallback
        {
            [OperationContract]
            void OnCallback(MessageData data);
        }

        [ServiceContract(CallbackContract = typeof(IMessageCallback))]
        public interface IMessage
        {
            [OperationContract]
            object SendData(MessageData data);
        }


        public static class Server
        {

            public static ServiceHost Create<T>()
            {
                return _Create<T>(typeof(IMessage));
            }

            public static ServiceHost Create<T>(Type interfaceType)
            {
                return _Create<T>(interfaceType);
            }

            public static ServiceHost Create<T>(Type interfaceType, string pipeName)
            {
                return _Create<T>(interfaceType, pipeName);
            }

            private static ServiceHost _Create<T>(Type interfaceType, string pipeName = DEFAULT_PIPE_NAME)
            {
                var host = new ServiceHost(typeof(T), new Uri[]
                   {
                    new Uri("net.pipe://localhost")
                   });

                host.AddServiceEndpoint(interfaceType, new NetNamedPipeBinding(), pipeName);
                host.Open();

                return host;
            }

        }

        public static class Client
        {

            public static IMessage Get()
            {
                return _Get<IMessage>();
            }

            public static T Get<T>()
            {
                return _Get<T>();
            }

            public static T Get<T>(string pipeName)
            {
                return _Get<T>(pipeName);
            }


            public static IMessage GetWithCallback(IMessageCallback callback)
            {
                return _Get<IMessage>(DEFAULT_PIPE_NAME, callback);
            }

            public static T GetWithCallback<T>(object callback)
            {
                return _Get<T>(DEFAULT_PIPE_NAME, callback);
            }

            public static T GetWithCallback<T>(string pipeName, object callback)
            {
                return _Get<T>(pipeName, callback);
            }

            private static T _Get<T>(string pipeName = DEFAULT_PIPE_NAME, object callback = null)
            {
                if (callback != null)
                {
                    DuplexChannelFactory<T> pipeFactory =
                        new DuplexChannelFactory<T>(
                        new InstanceContext(callback),
                        new NetNamedPipeBinding(),
                        new EndpointAddress(
                        "net.pipe://localhost/" + pipeName));

                    return pipeFactory.CreateChannel();
                }
                else
                {
                    ChannelFactory<T> pipeFactory =
                        new ChannelFactory<T>(
                        new NetNamedPipeBinding(),
                        new EndpointAddress(
                        "net.pipe://localhost/" + pipeName));

                    return pipeFactory.CreateChannel();
                }
            }

        }
        
    }
}
