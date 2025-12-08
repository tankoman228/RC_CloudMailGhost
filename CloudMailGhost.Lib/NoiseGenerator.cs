using System.Security.Cryptography;
using System.Text;

namespace CloudMailGhost.Lib
{
    public class NoiseGenerator
    {
        public static byte[] GenerateNoise(string key, long length, byte min, byte max, out int sum)
        {
            sum = 0;
            byte[] bytes = GenerateSequence(key, length);
            byte l = (byte)(max - min);

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(min + bytes[i] % l);
                sum += bytes[i];
            }
            return bytes;
        }

        private static byte[] GenerateSequence(string masterKeyString, long bytesToGenerate)
        {
            byte[] keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(masterKeyString));

            using (var hmac = new HMACSHA256(keyBytes))
            {
                using (var memoryStream = new MemoryStream())
                {
                    byte[] currentSeed = new byte[hmac.HashSize / 8]; 

                    currentSeed = hmac.ComputeHash(Encoding.UTF8.GetBytes(bytesToGenerate + "OIIAIUIIAI" + masterKeyString.Length));

                    long generatedBytes = 0;
                    while (generatedBytes < bytesToGenerate)
                    {
                        byte[] nextBlock = hmac.ComputeHash(currentSeed);

                        int bytesToWrite = (int)Math.Min(nextBlock.Length, bytesToGenerate - generatedBytes);
                        memoryStream.Write(nextBlock, 0, bytesToWrite);
                        generatedBytes += bytesToWrite;

                        currentSeed = nextBlock;
                    }

                    return memoryStream.ToArray();
                }
            }
        }
    }
}
