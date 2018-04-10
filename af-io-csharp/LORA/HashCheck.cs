namespace AFIO.Network
{
    public class HashCheck
    {
        public static int Hash(byte[] data)
        {
            var hash = 0;
            for (int i = 0; i < data.Length; i++)
                hash = hash ^ data[i];

            return hash;
        }
    }
}
