namespace AFIO.Network
{
    public class COBS
    {
        // NOTE Slow, always allocate memory. Use int Encode(byte[] input, int length, byte[] output) to prevent memory allocation
        public static byte[] Encode(byte[] input)
        {
            int readIndex = 0, writeIndex = 1, length = input.Length;
            byte distance = 1;

            while (readIndex < length)
            {
                if (input[readIndex] == 0)
                {
                    writeIndex++;
                    distance = 1;
                    readIndex++;
                }
                else
                {
                    writeIndex++;
                    readIndex++;
                    distance++;

                    if (distance == 0xFF)
                    {
                        writeIndex++;
                        distance = 1;
                    }
                }
            }

            var output = new byte[writeIndex];

            Encode(input, length, output);

            return output;
        }

        // NOTE Slow, always allocate memory. Use int Decode(byte[] input, int length, byte[] output) to prevent memory allocation
        public static byte[] Decode(byte[] input)
        {
            int readIndex = 0, writeIndex = 0, length = input.Length;
            byte distance, i;

            while (readIndex < length)
            {
                distance = input[readIndex];

                if (readIndex + distance > length
                    && distance != 1)
                    return new byte[0];

                readIndex++;

                for (i = 1; i < distance; i++)
                {
                    writeIndex++;
                    readIndex++;
                }

                if (distance != 0xFF
                    && readIndex != length)
                    writeIndex++;
            }

            var output = new byte[writeIndex];

            Decode(input, length, output);

            return output;
        }

        public static int Encode(byte[] input, int length, byte[] output)
        {
            int readIndex = 0, writeIndex = 1, codeIndex = 0;
            byte distance = 1;

            while (readIndex < length)
            {
                if (input[readIndex] == 0)
                {
                    output[codeIndex] = distance;
                    codeIndex = writeIndex++;
                    distance = 1;
                    readIndex++;
                }
                else
                {
                    output[writeIndex++] = input[readIndex++];
                    distance++;

                    if (distance == 0xFF)
                    {
                        output[codeIndex] = distance;
                        codeIndex = writeIndex++;
                        distance = 1;
                    }
                }
            }

            if (codeIndex != 255
                && output.Length > 0)
                output[codeIndex] = distance;

            return writeIndex;
        }

        public static int Decode(byte[] input, int length, byte[] output)
        {
            int readIndex = 0, writeIndex = 0;
            byte distance, i;

            while (readIndex < length)
            {
                distance = input[readIndex];

                if (readIndex + distance > length
                    && distance != 1)
                    return 0;

                readIndex++;

                for (i = 1; i < distance; i++)
                    output[writeIndex++] = input[readIndex++];

                if (distance != 0xFF
                    && readIndex != length)
                    output[writeIndex++] = 0;
            }

            return writeIndex;
        }
    }
}
