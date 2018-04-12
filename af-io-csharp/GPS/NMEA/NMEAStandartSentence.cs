
namespace NMEA
{
    public sealed class NMEAStandartSentence : NMEASentence
    {
        public TalkerIdentifiers TalkerId { get; set; }
        public SentenceIdentifiers SentenceId { get; set; }        
    }
}
