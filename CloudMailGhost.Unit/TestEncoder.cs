using CloudMailGhost.Lib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;


namespace CloudMailGhost.Unit
{
    [TestClass]
    public class TestEncoder
    {
        string imagePath = "C:\\Users\\2100\\Downloads\\png.png";

        [TestMethod]
        public void EncodeDecode_V1_ShouldWorkCorrectly()
        {
            string key = "PPLGOND";
            byte[] testData = NoiseGenerator.GenerateNoise(new Random().Next() + "", 
                ImageEncoder.Rarefaction * 32 * (new Random(0).Next(17) + 1), 0, 255, out var s);

            int pixelCount = (testData.Length + 16) * ImageEncoder.Rarefaction;
            var originalPixels = new ImageRepresenter.Pixel[pixelCount];

            Random random = new Random();
            for (int i = 0; i < pixelCount; i++)
            {
                originalPixels[i].R = (byte)(random.Next() % 255);
                originalPixels[i].G = (byte)(random.Next() % 255);
                originalPixels[i].B = (byte)(random.Next() % 255);
            }

            var originalImage = new ImageRepresenter { Pixels = originalPixels };

            var encodedImage = ImageEncoder.EncodeDataV1(originalImage, key, testData, d => { });
            var decodedData  = ImageEncoder.DecodeDataV1(encodedImage, key);

            Console.WriteLine($"{testData[0]} == {testData[1]}");
            CollectionAssert.AreEqual(testData, decodedData);

            Console.WriteLine($"key: {key}");
            Console.WriteLine($"datalen {testData.Length}");
            Console.WriteLine($"pixels: {pixelCount} : {pixelCount / ImageEncoder.Rarefaction - 16 * ImageEncoder.Rarefaction} bytes");
            Console.WriteLine();

            int maxChange = 0;
            int totalChange = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                int change = ImageEncoder.ColorSumDiff(ref encodedImage.Pixels[i], ref originalImage.Pixels[i]);
                totalChange += change;
                if (change > maxChange) maxChange = change;
            }

            double avgChange = (double)totalChange / pixelCount;
            Console.WriteLine($"maxChange: {maxChange}");
            Console.WriteLine($"avgChange: {avgChange:F2}");
            Console.WriteLine($"totalChange: {totalChange}");

            Console.WriteLine();

            string wrongKey = "Babka";
            var wrongDecoded = ImageEncoder.DecodeDataV1(encodedImage, wrongKey);

            bool dataDiffers = false;
            for (int i = 0; i < testData.Length; i++)
            {
                if (testData[i] != wrongDecoded[i])
                {
                    dataDiffers = true;
                    break;
                }
            }

            Assert.IsTrue(dataDiffers);
        }

        [TestMethod]
        public void EncodeDecode_V1_WithRealImage()
        {
            string key = "Bibki";

            ImageRepresenter originalImage = LoadImageFromFile(imagePath);

            int pixelCount = originalImage.Pixels.Length;
            int dataCapacity = originalImage.CapacityBytes;

            byte[] testData = new byte[dataCapacity];
            Random random = new Random();
            random.NextBytes(testData);

            Console.WriteLine($"{Path.GetFileName(imagePath)}");
            Console.WriteLine($"{originalImage.Width} x {originalImage.Height} =  {pixelCount}");
            Console.WriteLine($"Data size: {dataCapacity}");

            var encodedImage = ImageEncoder.EncodeDataV1(originalImage, key, testData, d => { });

            encodedImage.Width = originalImage.Width;
            encodedImage.Height = originalImage.Height;

            string directory = Path.GetDirectoryName(imagePath);
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string extension = Path.GetExtension(imagePath);
            string encodedFilePath = Path.Combine(directory, $"{fileName}_encoded{extension}");

            SaveImageToFile(encodedImage, encodedFilePath);

            var loadedEncodedImage = LoadImageFromFile(encodedFilePath);
            var decodedData = ImageEncoder.DecodeDataV1(loadedEncodedImage, key);

            CollectionAssert.AreEqual(testData, decodedData);

            int maxChange = 0;
            long totalChange = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                int change = ImageEncoder.ColorSumDiff(ref encodedImage.Pixels[i], ref originalImage.Pixels[i]);
                totalChange += change;
                if (change > maxChange) maxChange = change;
            }

            double avgChange = (double)totalChange / pixelCount;
            Console.WriteLine($"maxChange: {maxChange}");
            Console.WriteLine($"avgChange: {avgChange:F2}");
            Console.WriteLine($"totalChange: {totalChange}");

            string wrongKey = "Bobki";
            var wrongDecoded = ImageEncoder.DecodeDataV1(encodedImage, wrongKey);

            bool dataDiffers = false;
            for (int i = 0; i < testData.Length; i++)
            {
                if (testData[i] != wrongDecoded[i])
                {
                    dataDiffers = true;
                    break;
                }
            }

            Assert.IsTrue(dataDiffers);
        }

        private ImageRepresenter LoadImageFromFile(string filePath)
        {
            using (var image = Image.Load<Rgba32>(filePath))
            {
                var pixels = new ImageRepresenter.Pixel[image.Width * image.Height];

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = image[x, y];
                        pixels[y * image.Width + x] = new ImageRepresenter.Pixel(pixel.R, pixel.G, pixel.B);
                    }
                }

                return new ImageRepresenter(pixels, image.Width, image.Height);
            }
        }

        private void SaveImageToFile(ImageRepresenter image, string filePath)
        {
            using (var outputImage = new Image<Rgba32>(image.Width, image.Height))
            {
                for (int i = 0; i < image.Pixels.Length; i++)
                {
                    int x = i % image.Width;
                    int y = i / image.Width;

                    if (y < image.Height) 
                    {
                        outputImage[x, y] = new Rgba32(image.Pixels[i].R, image.Pixels[i].G, image.Pixels[i].B, 255); 
                    }
                }

                outputImage.Save(filePath);
            }
        }
    }
}