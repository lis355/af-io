using System.IO;
using AFIO.Geoposition;
using AFIO.Network;

namespace AFIO
{
    public class IODevicesManager : ISprintable
    {
        const string _kSettingsFilePath = "io-settings.xml";

        public GPS GPS { get; }
        public LORA Network { get; }

        public IODevicesManager()
        {
            if (!File.Exists(_kSettingsFilePath))
                throw new FileNotFoundException(_kSettingsFilePath);

            var serializer = new XmlSerializer<Settings>();
            var settings = serializer.Deserialize(_kSettingsFilePath);

            GPS = new GPS(settings.GPSSerialPortName);
            Network = new LORA(settings.LoraSerialPortName, settings.LoraBaudRate);
        }

        public void Start()
        {
            //GPS.Start();
            Network.Start();
        }

        public void Finish()
        {
            Network.Finish();
            GPS.Finish();
        }
    }
}
