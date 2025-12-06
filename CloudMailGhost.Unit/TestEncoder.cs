using CloudMailGhost.Lib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;


namespace CloudMailGhost.Unit
{
    [TestClass]
    public class TestEncoder
    {
        // TODO: VS редкостная тварь изговнячила кодировку. Придётся через git откатывать

        [TestMethod]
        public void EncodeDecode_V1_ShouldWorkCorrectly()
        {
            string key = "�����";
            byte[] testData = NoiseGenerator.GenerateNoise(new Random(0).Next() + "", 
                ImageEncoder.Rarefaction * 10, 0, 255, out var s);

            int pixelCount = testData.Length * ImageEncoder.Rarefaction;
            var originalPixels = new ImageRepresenter.Pixel[pixelCount];

            Random random = new Random();
            for (int i = 0; i < pixelCount; i++)
            {
                originalPixels[i].R = (byte)(random.Next() % 255);
                originalPixels[i].G = (byte)(random.Next() % 255);
                originalPixels[i].B = (byte)(random.Next() % 255);
            }

            var originalImage = new ImageRepresenter { Pixels = originalPixels };

            // Act - �������� ������
            var encodedImage = ImageEncoder.EncodeDataV1(originalImage, key, testData, d => { });

            // Act - ���������� ������
            var decodedData = ImageEncoder.DecodeDataV1(encodedImage, key);

            // Assert - ���������, ��� ������ �������������� ���������
            CollectionAssert.AreEqual(testData, decodedData,
                "�������������� ������ ������ ��������� � ���������");


            // ������� � ������� ���������� ��� �������
            Console.WriteLine("=== ����-���� ImageEncoder ===");
            Console.WriteLine($"����: {key}");
            Console.WriteLine($"�������� ������ ({testData.Length} ����): {BitConverter.ToString(testData)}");
            Console.WriteLine($"������ �����������: {pixelCount} �������� ({pixelCount / ImageEncoder.Rarefaction} ���� �������)");
            Console.WriteLine();

            // �������� ��������� ������ (������ ���� ����������)
            int maxChange = 0;
            int totalChange = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                int change = ImageEncoder.ColorSumDiff(ref encodedImage.Pixels[i], ref originalImage.Pixels[i]);
                totalChange += change;
                if (change > maxChange) maxChange = change;
            }

            double avgChange = (double)totalChange / pixelCount;
            Console.WriteLine($"���������� ���������:");
            Console.WriteLine($"  ������������ ��������� �����: {maxChange}");
            Console.WriteLine($"  ������� ��������� �����: {avgChange:F2}");
            Console.WriteLine($"  ����� ���������: {totalChange}");

            Console.WriteLine();

            // �������������� ��������: ��������� ������������ � ������������ ������
            string wrongKey = "����������������";
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

            Assert.IsTrue(dataDiffers,
                "������������� � ������������ ������ ������ ������ ������ ������");

            Console.WriteLine($"������������� � ������������ ������ ��� ������ ������: {(dataDiffers ? "��������" : "������")}");
        }


        [TestMethod]
        public void EncodeDecode_V1_WithRealImage()
        {
            // ����������
            string key = "�����";

            // �������� ����������� ����� ������ (��� ������� ������������)
            // � �������������� ����� ����� ������������ ��������������� ����
//#if DEBUG
            string imagePath = "C:\\Users\\2100\\Downloads\\png.png";
            if (string.IsNullOrEmpty(imagePath))
            {
                Assert.Inconclusive("����������� �� �������, ���� ��������.");
                return;
            }
//#else
            // ��� �������������� ������ - ���������� ���� �� ��������� ��� ����������
            /*string imagePath = "test_image.png";
            if (!File.Exists(imagePath))
            {
                Assert.Inconclusive($"�������� ����������� �� �������: {imagePath}");
                return;
            }*/
//#endif

            try
            {
                // ��������� �����������
                ImageRepresenter originalImage = LoadImageFromFile(imagePath);

                // ���������, ��� ���������� �������� ������
                int pixelCount = originalImage.Pixels.Length;
                if (pixelCount % ImageEncoder.Rarefaction != 0)
                {
                    // �������� �� ���������� ��������
                    int newPixelCount = pixelCount - (pixelCount % ImageEncoder.Rarefaction);
                    Array.Resize(ref originalImage.Pixels, newPixelCount);
                    pixelCount = newPixelCount;
                    Console.WriteLine($"����������� �������� �� {pixelCount} �������� (������ 4)");
                }

                // ���������� ��������� ������
                int dataLength = pixelCount / ImageEncoder.Rarefaction;
                byte[] testData = new byte[dataLength];
                Random random = new Random();
                random.NextBytes(testData);

                Console.WriteLine($"=== ���� � �������� ������������ ===");
                Console.WriteLine($"�����������: {Path.GetFileName(imagePath)}");
                Console.WriteLine($"������: {originalImage.Width}x{originalImage.Height}");
                Console.WriteLine($"��������: {pixelCount}");
                Console.WriteLine($"�����������: {dataLength} ����");
                Console.WriteLine($"�������� ������: {BitConverter.ToString(testData.Take(16).ToArray())}...");

                // Act - �������� ������
                var encodedImage = ImageEncoder.EncodeDataV1(originalImage, key, testData, d => { });

                // ��������������� ������� ��� ����������
                encodedImage.Width = originalImage.Width;
                encodedImage.Height = originalImage.Height;

                // ��������� �������������� �����������
                string directory = Path.GetDirectoryName(imagePath);
                string fileName = Path.GetFileNameWithoutExtension(imagePath);
                string extension = Path.GetExtension(imagePath);
                string encodedFilePath = Path.Combine(directory, $"{fileName}_encoded{extension}");

                SaveImageToFile(encodedImage, encodedFilePath);
                Console.WriteLine($"�������������� ����������� ���������: {encodedFilePath}");

                // Act - ���������� ������ �� ����������� �����
                var loadedEncodedImage = LoadImageFromFile(encodedFilePath);
                var decodedData = ImageEncoder.DecodeDataV1(loadedEncodedImage, key);

                // Assert - ���������, ��� ������ �������������� ���������
                CollectionAssert.AreEqual(testData, decodedData,
                    "�������������� ������ ������ ��������� � ���������");

                // �������� ����������
                int maxChange = 0;
                long totalChange = 0;
                for (int i = 0; i < pixelCount; i++)
                {
                    int change = ImageEncoder.ColorSumDiff(ref encodedImage.Pixels[i], ref originalImage.Pixels[i]);
                    totalChange += change;
                    if (change > maxChange) maxChange = change;
                }

                double avgChange = (double)totalChange / pixelCount;
                Console.WriteLine($"\n���������� ���������:");
                Console.WriteLine($"  ������������ ��������� �����: {maxChange}");
                Console.WriteLine($"  ������� ��������� �����: {avgChange:F2}");
                Console.WriteLine($"  ����� ���������: {totalChange}");


                // �������� � ������������ ������
                string wrongKey = "����������������";
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

                Assert.IsTrue(dataDiffers,
                    "������������� � ������������ ������ ������ ������ ������ ������");

                Console.WriteLine($"\n������������� � ������������ ������: {(dataDiffers ? "������ ����������� (��������)" : "������ ��������� (������)")}");
            }
            catch (Exception ex)
            {
                Assert.Fail($"������ ��� ������ � ������������: {ex.Message}");
            }
        }

        private ImageRepresenter LoadImageFromFile(string filePath)
        {
            using (var image = Image.Load<Rgba32>(filePath))
            {
                var pixels = new ImageRepresenter.Pixel[image.Width * image.Height];

                // ����������� ����������� � ����� (�� �������)
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = image[x, y];
                        // ����������� RGBA � 24-������ ���� (��� �����-������)
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
                // ��������������� ����������� �� �����
                for (int i = 0; i < image.Pixels.Length; i++)
                {
                    int x = i % image.Width;
                    int y = i / image.Width;

                    if (y < image.Height) // �������� �� ������ �������
                    {
                        outputImage[x, y] = new Rgba32(image.Pixels[i].R, image.Pixels[i].G, image.Pixels[i].B, 255); // ����� = 255 (������������)
                    }
                }

                outputImage.Save(filePath);
            }
        }
    }
}