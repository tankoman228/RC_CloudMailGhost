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
            // 1. Преобразуем строку-ключ в массив байтов (используя SHA256, чтобы убедиться, что ключ имеет нужный размер и энтропию)
            byte[] keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(masterKeyString));

            // 2. Используем HMACSHA256 для создания криптографически стойкого потока
            using (var hmac = new HMACSHA256(keyBytes))
            {
                using (var memoryStream = new MemoryStream())
                {
                    byte[] currentSeed = new byte[hmac.HashSize / 8]; // Начальный вектор или сид для первого блока

                    // Чтобы гарантировать детерминированность, начнем с пустого массива или константы
                    // Например, можно использовать хэш от 0 как начальный сид
                    currentSeed = hmac.ComputeHash(Encoding.UTF8.GetBytes("InitialSeedConstant"));

                    long generatedBytes = 0;
                    while (generatedBytes < bytesToGenerate)
                    {
                        // Вычисляем хэш текущего сида/блока, используя мастер-ключ
                        byte[] nextBlock = hmac.ComputeHash(currentSeed);

                        int bytesToWrite = (int)Math.Min(nextBlock.Length, bytesToGenerate - generatedBytes);
                        memoryStream.Write(nextBlock, 0, bytesToWrite);
                        generatedBytes += bytesToWrite;

                        // Обновляем сид для следующей итерации, используя предыдущий результат
                        currentSeed = nextBlock;
                    }

                    return memoryStream.ToArray();
                }
            }
        }
    }
}
