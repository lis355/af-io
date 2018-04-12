
namespace NMEA
{
    public sealed class NMEAProprietarySentence : NMEASentence
    {
        public string SentenceIdString { get; set; }
        public ManufacturerCodes Manufacturer { get; set; }
    }
}
