using System.IO;
using AFIO.Geoposition;
using AFIO.Network;

namespace AFIO
{
    public class IODevicesManager : ISprintable
    {
        const string _kSettingsFilePath = "io-settings.xml";

        readonly Settings _settings;

        public GPS GPS { get; private set; }
        public LORA Network { get; private set; }

        public IODevicesManager()
        {
            if (!File.Exists(_kSettingsFilePath))
                throw new FileNotFoundException(_kSettingsFilePath);

            var serializer = new XmlSerializer<Settings>();
            _settings = serializer.Deserialize(_kSettingsFilePath);
        }

        public void Start()
        {
            //GPS = new GPS(_settings.GPSSerialPortName);
            //GPS.Start();

            Network = new LORA(_settings.LoraSerialPortName, _settings.LoraBaudRate);
            Network.Start();
        }

        public void Finish()
        {
            Network.Finish();
            //GPS.Finish();
        }
    }
}
