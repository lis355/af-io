using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using AFIO;

namespace io_tests
{
    public class NetworkMessagesTest : Test
    {
        [Serializable]
        public abstract class Message
        {
        }
        
        [Serializable]
        public class TextMessage : Message
        {
            public string Text;
        }

        IODevicesManager _devicesManager;

        public override void Run()
        {
            _devicesManager = new IODevicesManager();
            _devicesManager.Network.OnPacketReceived += NetworkOnPacketReceived;

            _devicesManager.Start();
            
            var workerThread = new Thread(WorkerThreadFunction);
            workerThread.Start();

            Console.ReadKey();

            workerThread.Abort();

            _devicesManager.Finish();
        }

        void WorkerThreadFunction()
        {
            int messageNumber = 1;

            while (true)
            {
                Thread.Sleep(1000);

                var message = new TextMessage
                {
                    Text = $"PRVT {messageNumber} {DateTime.UtcNow.ToLongTimeString()}"
                };

                messageNumber++;

                var formatter = new BinaryFormatter();
                var stream = new MemoryStream();
                formatter.Serialize(stream, message);
                var data = stream.ToArray();
                
                _devicesManager.Network.SendPacket(data);

                Console.WriteLine("SendPacket TextMessage: {0}", message.Text);
            }
        }

        void NetworkOnPacketReceived(byte[] data)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream(data);
            var message = formatter.Deserialize(stream) as Message;

            var textMessage = message as TextMessage;
            if (textMessage != null)
                Console.WriteLine("PacketReceived TextMessage on {0}: {1}", DateTime.UtcNow.ToLongTimeString(), textMessage.Text);
        }
    }
}
