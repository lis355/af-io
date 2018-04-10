using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

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
        readonly List<byte> _listBuffer = new List<byte>();
        Action _dataReceivedCallback; 

        public event Action<byte[]> OnPacketReceived;

        public LORA(string portName, int baudRate)
        {
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

                SetMode(EMode.Data);

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
            if (_mode != EMode.Data)
                SetMode(EMode.Data);

            // TODO big length buffering

            // Packet structure
            // Data in cobs ..... n bytes
            // Hash of NON ENCODED data 1 bytes
            // Zero-byte

            var encodedSize = COBS.GetEncodedArraySize(data);
            var encoded = new byte[encodedSize + 2];
            COBS.Encode(data, data.Length, encoded);
            encoded[encodedSize] = (byte)HashCheck.Hash(data);
            encoded[encodedSize + 1] = 0;

            // TODO split length
            SendRawData(encoded);
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

            _dataReceivedCallback = callBack;

            SendRawData(commandBytes);
        }

        void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var bufferLength = _port.BytesToRead;
            _port.Read(_buffer, 0, bufferLength);

            for (int i = 0; i < bufferLength; i++)
                _listBuffer.Add(_buffer[i]);

            if (_dataReceivedCallback != null)
                _dataReceivedCallback();
        }

        void ProcessRecievedDataInDataMode()
        {
            for (int i = 0; i < _listBuffer.Count; i++)
            {
                if (_listBuffer[i] == 0)
                {
                    if (i > 2
                        && OnPacketReceived != null)
                    {
                        var dataLength = i - 1;
                        var data = new byte[dataLength];
                        for (int j = 0; j < dataLength; j++)
                            data[j] = _listBuffer[j];

                        var decodedDataLength = COBS.GetDecodedArraySize(data);
                        var decodedData = new byte[decodedDataLength];
                        COBS.Decode(data, dataLength, decodedData);

                        var hash = _listBuffer[dataLength];
                        var computedHash = HashCheck.Hash(decodedData);

                        if (hash == computedHash)
                            OnPacketReceived(decodedData);
                    }

                    _listBuffer.RemoveRange(0, i + 1);
                    i = -1;
                }
            }
        }

        void SetMode(EMode newMode)
        {
            _mode = newMode;

            switch (_mode)
            {
                case EMode.Data:
                    _dataReceivedCallback = ProcessRecievedDataInDataMode;
                    break;

                case EMode.Commands:
                    _dataReceivedCallback = null;
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
