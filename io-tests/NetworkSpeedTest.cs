using System;
using System.IO.Ports;
using System.Threading;

namespace io_tests
{
    public class NetworkSpeedTest
    {
        const string _kSenderPortName = "COM5";
        const string _kRecieverPortName = "COM7";

        public NetworkSpeedTest()
        {
            var startTime = DateTime.Now;

            var sender = new SerialPort(_kSenderPortName, 9600);
            sender.Open();

            var sendedLength = 0;

            var worker = new Thread(() =>
            {
                byte messageNumber = 1;

                while (true)
                {
                    Thread.Sleep(1000);

                    var data = new byte[200];
                    data[0] = messageNumber;
                    sender.Write(data, 0, data.Length);
                    sendedLength += data.Length;

                    var speed = sendedLength / (DateTime.Now - startTime).TotalSeconds;
                    Console.WriteLine("Send {0} bytes speed {1:F2}", sendedLength, speed);

                    messageNumber++;
                }
            });

            worker.Start();

            var reciever = new SerialPort(_kRecieverPortName, 9600);
            reciever.Open();

            var recievedLength = 0;
            
            reciever.DataReceived += (obj, args) =>
            {
                var data = new byte[reciever.BytesToRead];
                reciever.Read(data, 0, data.Length);
                recievedLength += data.Length;
                
                var speed = recievedLength / (DateTime.Now - startTime).TotalSeconds;
                Console.WriteLine("Recieve {0} bytes speed {1:F2} diff {2}", recievedLength, speed, sendedLength - recievedLength);
                //for (int i = 0; i < data.Length; i++)
                //{
                //    if (data[i] != 0)
                //        Console.WriteLine(data[i]);
                //}
            };
            
            Console.ReadKey();
        }
    }
}
