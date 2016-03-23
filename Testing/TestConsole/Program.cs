using System;


namespace TestConsole
{
    class Program
    {


        static void Main(string[] args)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;

            Console.ReadLine();
        }

        static Int64 output = 0;

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TH_Global.Logger.Log((output++).ToString());
        }
    }
}
