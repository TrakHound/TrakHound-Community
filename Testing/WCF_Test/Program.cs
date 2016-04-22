using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace WCF_Test
{

    [ServiceContract]
    public interface IStringReverser
    {
        [OperationContract]
        string ReverseString(string value);
    }

    public class StringReverser : IStringReverser
    {
        public string ReverseString(string value)
        {
            char[] retVal = value.ToCharArray();
            int idx = 0;
            for (int i = value.Length - 1; i >= 0; i--)
                retVal[idx++] = value[i];

            return new string(retVal);
        }
    }




    class Program
    {

        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(StringReverser), new Uri[]
                {
                    new Uri("http://localhost:8000"),
                    new Uri("net.pipe://localhost")
                }))
            {
                host.AddServiceEndpoint(typeof(IStringReverser),
                    new BasicHttpBinding(),
                    "Reverse");

                host.AddServiceEndpoint(typeof(IStringReverser),
                  new NetNamedPipeBinding(),
                  "PipeReverse");

                host.Open();

                Console.WriteLine("Service is available. " +
                  "Press <ENTER> to exit.");
                Console.ReadLine();

                host.Close();

            }




        }



    }
}
