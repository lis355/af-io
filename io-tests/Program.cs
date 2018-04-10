using System;
using System.IO.Ports;
using System.Linq;
using AFIO;

namespace io_tests
{
    public class Program
    {
        [Serializable]
        public abstract class Message
        {
        }

        public class TextMessage
        {
            public string Text;
        }

        public static void Main()
        {
            PrintAvaliablePorts();

            var devicesManager = new IODevicesManager();
            devicesManager.Start();

            devicesManager.Network.OnPacketReceived += NetworkOnPacketReceived;

            //SendPacket(Encoding.UTF8.GetBytes("PRIVET 1"));
            
            Console.ReadKey();

            devicesManager.Finish();
        }

        static void NetworkOnPacketReceived(byte[] obj)
        {
            //Console.WriteLine(Encoding.UTF8.GetString(data));
        }

        static void PrintAvaliablePorts()
        {
            Console.WriteLine("Avaliable ports:");
            SerialPort.GetPortNames().ToList().ForEach(Console.WriteLine);
            Console.WriteLine();
        }
    }
}
