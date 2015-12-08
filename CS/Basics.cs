using System;
using System.IO;

namespace ghazar_m
{
    internal class Basics//stupid noob stuff about reading-writing a txt file. fuckmylife
    {
        public static void MyReader(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine("> The file does not exist.");
                    return;
                }
                using (var sr = new StreamReader(filename))
                {
                    while (sr.Peek() >= 0)
                    {
                        Console.WriteLine(sr.ReadLine());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e);
            }
            Console.ReadLine();
        }

        public static void MyWriter(string filename)
        {
            bool exist = true;
            try
            {
                if (!File.Exists(filename))
                {
                    exist = false;
                    Console.WriteLine("> The file does not exist. Create it ? (Y/N)");
                    string tmp = Console.ReadLine();
                    if (tmp != "y" && tmp != "Y") return;
                }
                using (var sw = new StreamWriter(filename, true))
                {
                    Console.Clear();
                    if (!exist) Console.WriteLine("> Empty text file created successfully.");
                    string str = "quarante-deux"; // mort de lol <3
                    Console.WriteLine("> Enter something to write on a new line (empty to stop) :");
                    while (str != "")
                    {
                        str = Console.ReadLine();
                        sw.WriteLine(str);
                    }
                    Console.WriteLine("> Finished.");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e);
                Console.ReadLine();
            }
        }
    }
}