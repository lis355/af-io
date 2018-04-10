using System;

namespace AFIO
{
    [Serializable]
    public class Settings
    {
        public string GPSSerialPortName;
        public string LoraSerialPortName;
        public int LoraBaudRate;
    }
}
