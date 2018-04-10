using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace AFIO.Network
{
    public class LORA : ISprintable
    {
        public enum EMode
        {
            Data,
            Commands
        }

        struct Version
        {
            public byte Series;
            public byte VersionNumber1;
            public byte VersionNumber2;
        }

        //readonly byte[] _kCommandReadOperatingParameters = {0xC1, 0xC1, 0xC1};
        readonly byte[] _kCommandReadVersion = {0xC3, 0xC3, 0xC3};

        readonly SerialPort _port;
        EMode _mode;

        byte[] _buffer;
        readonly Queue<byte> _listBuffer = new Queue<byte>();

        Action dataReceivedCallback; 

        public event Action<byte[]> OnDataReceived;

        public LORA(string portName, int baudRate)
        {
            var s = 1024 * 10;
            var n = 6457;
            var a = new byte[n];
            //var b = new byte[s];
            //var c = new byte[s];

            var r = new Random();
            for (int i = 0; i < n; i++)
                a[i] = (byte)r.Next(byte.MaxValue + 1);

            var b = COBS.Encode(a);
            var c = COBS.Decode(b);

            var eq = true;
            for (int i = 0; i < n; i++)
                if (a[i] != c[i])
                    eq = false;

            _port = new SerialPort(portName, baudRate);
            _port.DataReceived += PortOnDataReceived;

            _port.ReadBufferSize = 4096;
        }

        public void Start()
        {
            if (!_port.IsOpen)
            {
                _port.Open();

                _buffer = new byte[_port.ReadBufferSize];
                _listBuffer.Clear();

                //SendCommand(_kCommandReadVersion, () =>
                //{
                //    if (_listBuffer.Count < 4)
                //        return;
                //
                //    var version = ParseVersion(_listBuffer.Dequeue(), _listBuffer.Dequeue(), _listBuffer.Dequeue(), _listBuffer.Dequeue());
                //
                //    Console.WriteLine("Version E{0:X} {1} {2}", (int)version.Series, version.VersionNumber1, version.VersionNumber2);
                //});
            }
        }

        public void Finish()
        {
            if (_port.IsOpen)
                _port.Close();
        }

        public void SendPacket(byte[] data)
        {

        }

        public void SendRawData(byte[] data)
        {
            if (data.Length >= _port.WriteBufferSize)
                throw new ArgumentException("Big data size.");

             _port.Write(data, 0, data.Length);
        }

        void SendCommand(byte[] commandBytes, Action callBack)
        {
            if (_mode != EMode.Commands)
                SetMode(EMode.Commands);

            dataReceivedCallback = callBack;

            SendRawData(commandBytes);
        }

        void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var bufferLength = _port.BytesToRead;
            _port.Read(_buffer, 0, bufferLength);

            for (int i = 0; i < bufferLength; i++)
                _listBuffer.Enqueue(_buffer[i]);

            if (dataReceivedCallback != null)
                dataReceivedCallback();
        }

        void ProcessRecievedDataInDataMode()
        {
            
        }

        void SetMode(EMode newMode)
        {
            if (newMode == _mode)
                return;

            _mode = newMode;

            switch (_mode)
            {
                case EMode.Data:
                    dataReceivedCallback = ProcessRecievedDataInDataMode;
                    break;

                case EMode.Commands:
                    dataReceivedCallback = null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO switch pins
        }

        Version ParseVersion(byte v1, byte v2, byte v3, byte v4)
        {
            if (v1 != 0xC3)
                throw new ArgumentException("Bad responce");

            return new Version
            {
                Series = v2, VersionNumber1 = v3, VersionNumber2 = v4
            };
        }
    }
}
