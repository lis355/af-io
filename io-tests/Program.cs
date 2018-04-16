using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace io_tests
{
    public class Program
    {
        static readonly Dictionary<string, Func<Test>> _tests = new Dictionary<string, Func<Test>>
        {
            {"lora_speed", () => new NetworkSpeedTest()},
            {"messages", () => new NetworkMessagesTest()}
        };

        public static void Main(string[] args)
        {
            if (args.Contains("-p"))
                PrintAvaliablePorts();

            var tests = args.Where(x => x.StartsWith("-t")).ToList();
            if (tests.Count == 1)
            {
                var options = tests.First().Split('=');
                if (options.Length > 1)
                {
                    var testName = options[1];
                    if (_tests.ContainsKey(testName))
                        _tests[testName]().Run();
                }
                else
                {
                    Console.WriteLine("Avaliable tests:");

                    foreach (var test in _tests)
                        Console.WriteLine(test.Key);
                }
            }

            //Console.ReadKey();
        }

        static void PrintAvaliablePorts()
        {
            Console.WriteLine("Avaliable ports:");

            foreach (var portName in SerialPort.GetPortNames())
            {
                var serialPort = new SerialPort(portName);

                Console.WriteLine("Port: {0,-15} Baud rate: {1,-8} Open: {2,-5} Encoding: {3,-10} Buffer sizes: R {4,-8} W {5,-8}",
                    portName, serialPort.BaudRate, serialPort.IsOpen, serialPort.Encoding.EncodingName, serialPort.ReadBufferSize, serialPort.WriteBufferSize);
            }

            Console.WriteLine();
        }
    }
}
