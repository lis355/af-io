using System;

namespace AfIOSharp
{
    class Program
    {
        const string _kSettingsFilePath = "io-settings.xml";

        static void Main()
        {
            var serializer = new XmlSerializer<Settings>();
            var settings = serializer.Deserialize(_kSettingsFilePath);

            var gps = new GPS(settings.GPSSerialPort);
            gps.Start();

            Console.ReadKey();

            gps.Finish();
        }
    }
}
