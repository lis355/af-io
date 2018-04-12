using System;
using System.IO.Ports;
using System.Linq;

namespace io_tests
{
    public class Program
    {
        public static void Main()
        {
            PrintAvaliablePorts();

            //new NetworkSpeedTest();
            new NetworkMessagesTest();
        }
        
        static void PrintAvaliablePorts()
        {
            Console.WriteLine("Avaliable ports:");
            SerialPort.GetPortNames().ToList().ForEach(Console.WriteLine);
            Console.WriteLine();
        }
    }
}
