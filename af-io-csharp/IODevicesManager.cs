using System.IO;

namespace AfIOSharp
{
    public class IODevicesManager : ISprintable
    {
        const string _kSettingsFilePath = "io-settings.xml";

        readonly Settings _settings;
        GPS _gps;

        public IODevicesManager()
        {
            if (!File.Exists(_kSettingsFilePath))
                throw new FileNotFoundException(_kSettingsFilePath);

            var serializer = new XmlSerializer<Settings>();
            _settings = serializer.Deserialize(_kSettingsFilePath);
        }

        public void Start()
        {
            _gps = new GPS(_settings.GPSSerialPort);
            _gps.Start();
        }

        public void Finish()
        {
            _gps.Finish();
        }
    }
}
