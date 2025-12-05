using SixLabors.ImageSharp.ColorSpaces;

namespace CloudMailGhost.Lib
{
    public class ImageEncoder
    {
        private static Random random = new Random();


        public const int Rarefaction = 10;
        public const int FragmentValueLimit = 255 / Rarefaction + 4;
        public const int JijkaMinValue = FragmentValueLimit + 4;
        public const int ModFragmentKoef = (int)(FragmentValueLimit / 1.5);
        public const int MinColorDifference = 20;
        public const int MaxColorDifference = 60;
        public const int RootsToExit = 30;

        /// <summary>
        /// Версия самая первая, с высокой плотностью записи и со слабой защитой от терморектального криптоанализа
        /// </summary>
        /// <param name="original"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ImageRepresenter EncodeDataV1(ImageRepresenter original, string key, byte[] data)
        {
            if (data.Length != original.Pixels.Length / Rarefaction) throw new ArgumentException();

            // Из ключа шифрования получаем нужные числа
            var noise = NoiseGenerator.GenerateNoise(
                $"{key}:{original.Pixels.Length}", // в качестве соли: размер картинки
                original.Pixels.Length,              // генерируем шум для картинки
                1, 8, // лимиты значений в шуме
                out var JijkaRaw);
            int Jijka = JijkaMinValue + JijkaRaw % 7;

            // Это у нас будет новая картинка (она размотана в линию)
            ImageRepresenter result = new ImageRepresenter { Pixels = (ImageRepresenter.Pixel[])original.Pixels.Clone() };

            int i = 0; // номер пикселя
            while (i < original.Pixels.Length)
            {
                byte encoded = data[i / Rarefaction]; // пикселей на 1 байт

                // Разделяем, какие числа мы хотим закодировать
                int fragments = encoded / Rarefaction;
                int gragmentLast = encoded - fragments * (Rarefaction - 1); // целочисленное деление ест дроби

                // А далее решаем уравнение
                void SolveEqualityForFragment(int targetValue)
                {
                    // Поиск ближайшего корня
                    var root = original.Pixels[i];
                    var rgbStart = original.Pixels[i];

                    byte r = rgbStart.R;
                    byte g = rgbStart.G;
                    byte b = rgbStart.B;

                    bool skipG = true;
                    bool skipR = random.Next() % 2 == 0;
                    bool skipB = random.Next() % 2 == 0;

                    int h = 0;
                    int rootsFound = 0;

                    int minColorDifference = 0xFFFFFF;
                    int res = -1;

                    root = new(r, g, b);
                    res = DecodeEqualityV1(ref root, noise[i], Jijka);

                    while (
                        rootsFound < RootsToExit && 
                        minColorDifference > MinColorDifference 
                        || minColorDifference > MaxColorDifference)
                    {                       
                        var diff = Math.Abs(targetValue - res);
                        if (diff == 0)
                        {                        
                            var cd = ColorSumDiff(ref root, ref original.Pixels[i]);
                            if (cd < minColorDifference)
                            {
                                minColorDifference = cd;
                                result.Pixels[i] = root;
                            }

                            rootsFound++;
                            h = 99999;
                        }

                        h++; 
                        if (h > 25 || (!skipG && !skipR && !skipB))
                        {
                            r = (byte)random.Next();
                            g = (byte)random.Next();
                            b = (byte)random.Next();                           

                            root = new(r, g, b);
                            res = DecodeEqualityV1(ref root, noise[i], Jijka);

                            //Console.WriteLine($"shuffle bibkus");
                            h = 0;
                        }
                        else
                        {
                            //Console.WriteLine($"\n{diff}:\t{r} {g} {b}");
                        }

                        skipG = random.Next() % 2 == 0;
                        
                        if (b > 0)
                        {
                            var newRoot = new ImageRepresenter.Pixel(r, g, --b);
                            var newRes = DecodeEqualityV1(ref newRoot, noise[i], Jijka);
                            var new_diff = Math.Abs(newRes - targetValue);

                            if (new_diff >= diff) b++;
                            else
                            {
                                skipB = true;

                                root = newRoot;
                                res = newRes;

                                continue;
                            }

                        }

                        if (!skipB && b < 255)
                        {
                            var newRoot = new ImageRepresenter.Pixel(r, g, ++b);
                            var newRes = DecodeEqualityV1(ref newRoot, noise[i], Jijka);
                            var new_diff = Math.Abs(newRes - targetValue);

                            if (new_diff >= diff) b--;
                            else
                            {
                                skipB = true;

                                root = newRoot;
                                res = newRes;

                                continue;
                            }
                        }

                        skipR = random.Next() % 2 == 0;

                        if (r > 0)
                        {
                            var newRoot = new ImageRepresenter.Pixel(--r, g, b);
                            var newRes = DecodeEqualityV1(ref newRoot, noise[i], Jijka);
                            var new_diff = Math.Abs(newRes - targetValue);
                            
                            if (new_diff >= diff) r++;
                            else
                            {
                                skipR = true;

                                root = newRoot;
                                res = newRes;

                                continue;
                            }

                            if (new_diff == 0) continue;
                        }

                        if (!skipR && r < 255)
                        {
                            var newRoot = new ImageRepresenter.Pixel(++r, g, b);
                            var newRes = DecodeEqualityV1(ref newRoot, noise[i], Jijka);
                            var new_diff = Math.Abs(newRes - targetValue);

                            if (new_diff >= diff) r--;
                            else
                            {
                                skipR = true;

                                root = newRoot;
                                res = newRes;

                                continue;
                            }             
                        }


                        skipB = random.Next() % 2 == 0;

                        if (g > 0)
                        {
                            var newRoot = new ImageRepresenter.Pixel(r, --g, b);
                            var newRes = DecodeEqualityV1(ref newRoot, noise[i], Jijka);
                            var new_diff = Math.Abs(newRes - targetValue);

                            if (new_diff >= diff) g++;
                            else
                            {
                                skipG = true;

                                root = newRoot;
                                res = newRes;

                                continue;
                            }
                            
                        }

                        if (!skipG && g < 255)
                        {
                            var newRoot = new ImageRepresenter.Pixel(r, ++g, b);
                            var newRes = DecodeEqualityV1(ref newRoot, noise[i], Jijka);
                            var new_diff = Math.Abs(newRes - targetValue);

                            if (new_diff >= diff) g--;
                            else
                            {
                                skipG = true;

                                root = newRoot;
                                res = newRes;

                                continue;
                            }            
                        }
                    }
                }

                for (int k = 0; k < Rarefaction - 1; k++)
                {
                    SolveEqualityForFragment(fragments); i++;
                }

                SolveEqualityForFragment(gragmentLast); i++;

                // Теперь новый цвет пикселей содержит наши данные
            }
            return result;
        }

        public static byte[] DecodeDataV1(ImageRepresenter message, string key)
        {
            // Из ключа шифрования восстанавилваем нужные числа
            var noise = NoiseGenerator.GenerateNoise(
                $"{key}:{message.Pixels.Length}", // в качестве соли: размер картинки
                message.Pixels.Length,            // восстанавилваем шум для картинки
                1, 8,                             // лимиты значений в шуме
                out var JijkaRaw);
            int Jijka = JijkaMinValue + JijkaRaw % 7;

            byte[] decoded = new byte[message.Pixels.Length / Rarefaction];

            // А далее используем уравнение
            for (int i = 0; i < message.Pixels.Length; i++)
            {
                decoded[i / Rarefaction] += (byte)DecodeEqualityV1(ref message.Pixels[i], noise[i], Jijka);
            }
            return decoded;
        }

        /// <summary>
        /// ключевая функция, уравнение шифрования
        /// </summary>
        private static int DecodeEqualityV1(ref ImageRepresenter.Pixel original, byte noise, int Jijka)
        {
            return (original.R % ModFragmentKoef + original.G % ModFragmentKoef + original.B % ModFragmentKoef) % (Jijka + noise);
        }


        public static int ColorSumDiff(ref ImageRepresenter.Pixel c1, ref ImageRepresenter.Pixel c2)
        {
            return Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) * 2 + Math.Abs(c1.B - c2.B) / 2;
        }
    }
}
