using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using NMEA;

namespace AFIO.Geoposition
{
    // NMEA 0183 sentence parser/builder https://www.codeproject.com/Articles/279647/NMEA-sentence-parser-builder 
    // NMEA sentences http://www.gpsinformation.org/dale/nmea.htm#nmea

    public class GPS : ISprintable
    {
        const char _kTrimZeroChar = '\0';

        readonly SerialPort _port;
        StringBuilder _buffer;
        readonly Dictionary<SentenceIdentifiers, Action<object[]>> _cmdProcessor;
        
        public bool IsDataValid { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public GPS(string portName)
        {
            _port = new SerialPort(portName, 9600);
            _port.DataReceived += PortOnDataReceived;

            _cmdProcessor = new Dictionary<SentenceIdentifiers, Action<object[]>>
            {
                {SentenceIdentifiers.GLL, ProcessGLL}
            };
        }

        public void Start()
        {
            if (!_port.IsOpen)
            {
                _port.Open();

                _buffer = new StringBuilder();
            }
        }

        public void Finish()
        {
            if (_port.IsOpen)
                _port.Close();
        }

        void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            int bytesToRead = _port.BytesToRead;
            var buffer = new byte[bytesToRead];
            _port.Read(buffer, 0, bytesToRead);

            var dataString = Encoding.ASCII.GetString(buffer);

            _buffer.Append(dataString);

            var temp = _buffer.ToString();

            int lIndex = temp.LastIndexOf(NMEAParser.SentenceEndDelimiter, StringComparison.InvariantCulture);
            if (lIndex >= 0)
            {
                _buffer = _buffer.Remove(0, lIndex + 2);
                if (lIndex + 2 < temp.Length)
                    temp = temp.Remove(lIndex + 2);

                temp = temp.Trim(_kTrimZeroChar);

                var lines = temp.Split(new[] {NMEAParser.SentenceEndDelimiter}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x + NMEAParser.SentenceEndDelimiter);

                foreach (var line in lines)
                {
                    try
                    {
                        var result = NMEAParser.Parse(line);
                        ProcessNMEASentence(result);
                    }
                    catch
                    {
                        // TODO errors
                    }
                }
            }

            if (_buffer.Length >= _port.ReadBufferSize * 2)
                _buffer.Remove(0, _port.ReadBufferSize);
        }

        void ProcessNMEASentence(NMEASentence sentence)
        {
            var standartSentence = sentence as NMEAStandartSentence;
            if (standartSentence == null)
                return;

            var sentenceId = standartSentence.SentenceId;

            // DEBUG
            Console.WriteLine("GPS: {0} {1}", standartSentence.TalkerId, sentenceId);

            if (_cmdProcessor.ContainsKey(sentenceId))
                _cmdProcessor[sentenceId](standartSentence.Parameters);
        }

        void ProcessGLL(object[] parameters)
        {
            if (parameters[5].ToString() != "Valid")
                return;

            IsDataValid = true;

            // TODO timeFix use
            //var timeFix = (DateTime)parameters[4];

            Latitude = (double)parameters[0];
            var latC = (Cardinals)Enum.Parse(typeof(Cardinals), (string)parameters[1]);
            if (latC == Cardinals.South)
                Latitude = -Latitude;

            Longitude = (double)parameters[2];
            var lonC = (Cardinals)Enum.Parse(typeof(Cardinals), (string)parameters[3]);
            if (lonC == Cardinals.West)
                Longitude = -Longitude;

            // DEBUG
            Console.WriteLine("GPS: Latitude {0:F.4} Longitude {1:F.4}", Latitude, Longitude);
        }
    }
}
