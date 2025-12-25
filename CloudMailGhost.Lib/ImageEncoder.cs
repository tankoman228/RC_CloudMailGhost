using System.Security.Cryptography;

namespace CloudMailGhost.Lib
{
    public static class ImageEncoder
    {
        private static Random random = new Random();

        public const int Rarefaction = 10; // Не трогать, текущий алгоритм к нему крайне чувствителен!
        public const int JijkaMinValue = 64;
        public const int JijkaEntropy = 128;
        public const int RootsToExit = 30;
        public const int NoiseMin = 2;
        public const int NoiseMax = 4;
        public const int MinColorDifference = 2;
        public const int MaxColorDifference = 4;

        private static object pixelsReadyLock = new();

        /// <summary>
        /// Соль - добавление к ключу для каждой картинки индивидуально.
        /// Одинакова и для оригинала, и для результата!
        /// </summary>
        private static string GetSalt(ImageRepresenter original)
        {
            return $"{original.Pixels.Length}\a" +
                $"{original.Width}\t" +
                $"{original.Height}\n";
        }

        /// <summary>
        /// Версия самая первая, с высокой плотностью записи и со слабой защитой от терморектального криптоанализа
        /// </summary>
        /// <param name="original"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ImageRepresenter EncodeDataV1(ImageRepresenter original, string key, byte[] data, Action<float> updateProgress)
        {
            if (data.Length > original.CapacityBytes) throw new ArgumentException("недостаточно места");
            if (original.Pixels.Length % 16 != 0) throw new ArgumentException("Число пикселей не кратно 16");

            // Теперь надо эти данные привести к размеру original.CapacityBytes и чтобы data.Length была кратна 16
            data = [..data, ..(new byte[original.CapacityBytes - data.Length])];


            byte[] IV = RandomNumberGenerator.GetBytes(16); // AES-256

            // Шифрование данных
            data = Сryptographer.Encode(data, key + GetSalt(original), IV);

            data = [.. IV, ..data];

            // Из ключа шифрования получаем нужные числа
            var noise = NoiseGenerator.GenerateNoise(
                $"{key}:{GetSalt(original)}", // в качестве соли
                original.Pixels.Length,       // генерируем шум для картинки
                NoiseMin, NoiseMax,           // лимиты значений в шуме
                out var JijkaRaw);
            int Jijka = JijkaMinValue + JijkaRaw % JijkaEntropy;

            // Некодируемые пиксели. Номер первого некодируемого, номер второго вычисляется на основе
            var uncodedNoise = NoiseGenerator.GenerateNoise(
                $"{key}uwu{GetSalt(original)}",         // в качестве соли
                original.Pixels.Length / Rarefaction,   // генерируем шум для пикселей, не несущих информацию
                0, (byte)(Rarefaction - 1),             // лимиты значений в шуме
                out var noneed);

            // Это у нас будет новая картинка (она размотана в линию)
            ImageRepresenter result = new ImageRepresenter { 
                Pixels = (ImageRepresenter.Pixel[])original.Pixels.Clone(), 
                Height = original.Height, 
                Width = original.Width 
            };

            int pixelsReady = 0;

            void ParralelChunk(int pixelStart, int length)
            {
                int max = pixelStart + length;  
                for (int i = pixelStart; i < max; i++)
                {
                    byte encoded = data[i / Rarefaction]; // пикселей на 1 байт

                    void SolveEqualityForFragment(bool targetValue)
                    {
                        // Поиск ближайшего корня
                        var root = original.Pixels[i];

                        bool resDecode = DecodeEqualityV1(ref root, noise[i], Jijka);
                        if (resDecode == targetValue) return; // Корень уже какой надо                        

                        byte R = root.R;
                        byte G = root.G;
                        byte B = root.B;

                        int currentSum = (R + G + B + Jijka) % noise[i];
                        byte currentMod = (byte)(currentSum % noise[i]);

                        void SolveTowardsColor(ref byte R)
                        {
                            // Меняем один из каналов, решая уравнение (R + G + B + Jijka) % noise == 0
                            if (!targetValue)
                            {
                                if (R > 0) R--;
                                else R++;

                                resDecode = DecodeEqualityV1(ref root, noise[i], Jijka);
                                if (resDecode != targetValue) throw new Exception("пиздец 0" + i);

                                result.Pixels[i] = root;
                                return;
                            }

                            if (R > currentMod)
                            {
                                R -= currentMod;
                                resDecode = DecodeEqualityV1(ref root, noise[i], Jijka);

                                if (resDecode != targetValue) throw new Exception("пиздец A" + i);
                            }
                            else
                            {
                                R += (byte)(noise[i] - currentMod);
                                resDecode = DecodeEqualityV1(ref root, noise[i], Jijka);

                                if (resDecode != targetValue) throw new Exception($"{root.R}");
                            }
                        }

                        switch (random.Next() % 6)
                        {
                            case 0:
                            case 1:
                            case 2:
                                SolveTowardsColor(ref root.B); // Наивысший шанс, ибо синий видят хуже
                                break;

                            case 3:                              
                            case 4:
                                SolveTowardsColor(ref root.R);
                                break;

                            default:
                                SolveTowardsColor(ref root.G);
                                break;
                        }

                        result.Pixels[i] = root;
                    }

                    int uncodedId1 = uncodedNoise[i / Rarefaction];
                    int uncodedId2 = uncodedNoise[i / Rarefaction] == (Rarefaction - 1) ? 0 : uncodedId1 + 1;
                    int uncodedOffset = 0;

                    // Зона пустого шума, тут байт не кодируется. Имитатор шума будет в будущих версиях, сейчас шум равномерный
                    if (i % 10 == uncodedId1)
                    {
                        // Решил не забивать ничем, чтобы естественный шум просвечивал и оригинальные биты просачивались для масок... типа того, короче
                        //SolveEqualityForFragment(RandomNumberGenerator.GetBytes(1)[0] % 2 == 0);
                        continue;
                    }
                    else if (i % 10 > uncodedId1) uncodedOffset++;

                    // Зона пустого шума 2 на основе 1. TODO: отвязать от плотности записи 10
                    if (i % 10 == uncodedId2) 
                    {
                        //SolveEqualityForFragment(RandomNumberGenerator.GetBytes(1)[0] % 2 == 0);
                        continue;
                    }
                    else if (i % 10 > uncodedId2) uncodedOffset++;

                    bool bit = BitHelper.GetBitFromByte(encoded, (i % 10) - uncodedOffset);
                    SolveEqualityForFragment(bit);

                    // Теперь новый цвет пикселей содержит наши данные
                    lock (pixelsReadyLock)
                    {
                        pixelsReady += Rarefaction;
                    }
                }
            }

            int pixelsC = original.Pixels.Length;
            int ChunkSize = 0;

            while (pixelsC < Rarefaction)
            {
                ChunkSize += pixelsC;
                pixelsC -= Rarefaction * 8;
            }

            bool parallelIsGoing = true;
            Task.Run(async () => {
                while (parallelIsGoing)
                {
                    updateProgress(pixelsReady / (float)original.Pixels.Length * 100f);
                    await Task.Delay(25);
                }
            });
            Parallel.For(0, 8, i =>
            {
                if (i < 7)  ParralelChunk(i * ChunkSize, ChunkSize);
                else        ParralelChunk(ChunkSize * 7, pixelsC + ChunkSize);
            });

            parallelIsGoing = false;

            return result;
        }

        public static byte[] DecodeDataV1(ImageRepresenter message, string key)
        {
            // Из ключа шифрования восстанавилваем нужные числа
            var noise = NoiseGenerator.GenerateNoise(
                $"{key}:{GetSalt(message)}",      // в качестве соли: размер картинки
                message.Pixels.Length,            // восстанавилваем шум для картинки
                NoiseMin, NoiseMax,               // лимиты значений в шуме
                out var JijkaRaw);
            int Jijka = JijkaMinValue + JijkaRaw % JijkaEntropy;

            // Некодируемые пиксели. Номер первого некодируемого, номер второго вычисляется на основе
            var uncodedNoise = NoiseGenerator.GenerateNoise(
                $"{key}uwu{GetSalt(message)}",        
                message.Pixels.Length / Rarefaction,   // генерируем шум для пикселей, не несущих информацию
                0, (byte)(Rarefaction - 1),            // лимиты значений в шуме
                out var noneed);

            byte[] decoded = new byte[message.CapacityBytes + 16];

            // А далее используем уравнение
            for (int i = 0; i < message.Pixels.Length; i++)
            {
                int uncodedId1 = uncodedNoise[i / Rarefaction];
                int uncodedId2 = uncodedNoise[i / Rarefaction] == (Rarefaction - 1) ? 0 : uncodedId1 + 1;
                int uncodedOffset = 0;

                // Зона пустого шума, тут байт не кодируется. Имитатор шума будет в будущих версиях, сейчас шум равномерный
                if (i % 10 == uncodedId1) continue;               
                else if (i % 10 > uncodedId1) uncodedOffset++;

                // Зона пустого шума 2 на основе 1. TODO: отвязать от плотности записи 10
                if (i % 10 == uncodedId2) continue;
                else if (i % 10 > uncodedId2) uncodedOffset++;

                bool bit = DecodeEqualityV1(ref message.Pixels[i], noise[i], Jijka);

                BitHelper.SetBitInByte(ref decoded[i / Rarefaction], (i % 10 - uncodedOffset) , bit);
            }

            var data = decoded.Skip(16).ToArray();
            var IV   = decoded.Take(16).ToArray();

            // Шифрование данных
            decoded = Сryptographer.Decode(
                data,  // Данные начиная с 16 байта (первые 16 это IV)
                key + GetSalt(message),      // Ключ шифрования
                IV); // IV

            return decoded;
        }

        /// <summary>
        /// ключевая функция, уравнение шифрования. Бит
        /// </summary>
        private static bool DecodeEqualityV1(ref ImageRepresenter.Pixel original, byte noise, int Jijka)
        {
            byte R = original.R;
            byte G = original.G;
            byte B = original.B;

            return (R + G + B + Jijka) % noise == 0;
        }

        public static int ColorSumDiff(ref ImageRepresenter.Pixel c1, ref ImageRepresenter.Pixel c2)
        {
            return Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) * 2 + Math.Abs(c1.B - c2.B) / 2;
        }
    }
}
