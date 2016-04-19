using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using TH_Database;
using TH_Configuration;
using TH_ShiftTable;

namespace TableCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true) // Loop indefinitely
            {
                Console.WriteLine("Enter input:"); // Prompt
                string line = Console.ReadLine(); // Get string from user

                ReadCommand(line);

                //if (line == "exit") // Check string
                //{
                //    break;
                //}
                Console.Write("You typed "); // Report output
                Console.Write(line.Length);
                Console.WriteLine(" character(s)");
            }
        }

        private static void ReadCommand(string s)
        {
            var args = s.Split(' ');

            string configPath = args[0];
            string command = args[1];

            switch (command.ToLower())
            {
                case "shifts": Shifts.Create(configPath, args); break;
            }
        }


        

    }
}
