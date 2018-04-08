using System;
using System.IO.Ports;
using System.Linq;

namespace AfIOSharp
{
    public class Program
    {
        public static void Main()
        {
            PrintAvaliablePorts();

            var devicesManager = new IODevicesManager();
            devicesManager.Start();

            Console.ReadKey();

            devicesManager.Finish();
        }

        static void PrintAvaliablePorts()
        {
            Console.WriteLine("Avaliable ports:");
            SerialPort.GetPortNames().ToList().ForEach(Console.WriteLine);
            Console.WriteLine();
        }
    }
}
