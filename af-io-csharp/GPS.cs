using System;
using System.IO.Ports;

namespace AfIOSharp
{
    public class GPS
    {
        readonly SerialPort _port;

        public GPS(string portName)
        {
            _port = new SerialPort(portName, 9600);
            _port.DataReceived += PortOnDataReceived;
        }

        public void Start()
        {
            if (!_port.IsOpen)
                _port.Open();
        }

        public void Finish()
        {
            if (_port.IsOpen)
                _port.Close();
        }

        void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var msg = _port.ReadExisting().Trim();

            Console.WriteLine(msg);
        }
    }
}
