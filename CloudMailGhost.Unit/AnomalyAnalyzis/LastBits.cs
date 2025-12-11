using CloudMailGhost.Lib;
using static CloudMailGhost.Lib.ImageRepresenter;

namespace CloudMailGhost.Unit.AnomalyAnalyzis
{
    [TestClass]
    public class LastBits
    {
        [TestMethod]
        public void LastBitsDifference()
        {
            var imageFiles = Directory.GetFiles("Dataset");

            foreach (var imageFile in imageFiles)
            {
                var image = ImageLoader.LoadImageFromFile(imageFile);
                ImageRepresenter img2;
                try
                {
                    img2 = ImageEncoder.EncodeDataV1(image, "pavapepe gemabody", new byte[image.CapacityBytes], x => { });
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    continue;
                }

                Console.WriteLine($"\n\nTesting {imageFile} with capacity {image.CapacityBytes}\n");

                Console.WriteLine($"Original: ");
                var statsBefore = DifferToImage(image);
                

                Console.WriteLine($"Changed: ");
                var statsAfter = DifferToImage(img2);

                Console.WriteLine($"Summary: ");
                foreach (var key in statsBefore.Keys)
                {
                    Console.WriteLine($"{key}:\t\t\t{((statsAfter[key] - statsBefore[key]) / statsBefore[key] * 100f).ToString("F2")}% change");
                }
            }
        }

        private Dictionary<string, float> DifferToImage(ImageRepresenter image)
        {
            Dictionary<string, float> res = [];

            for (int bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                int sumDelta = 0;

                for (int x = 1; x < image.Width - 1; x++)
                {
                    for (int y = 1; y < image.Height - 1; y++)
                    {
                        var pixel = image.Pixels[y * image.Width + x];

                        var r = image.Pixels[(y) * image.Width + (x + 1)];
                        var l = image.Pixels[(y) * image.Width + (x - 1)];
                        var u = image.Pixels[(y - 1) * image.Width + (x)];
                        var d = image.Pixels[(y + 1) * image.Width + (x)];

                        sumDelta += lastBitsDiffSum(ref pixel, ref r, bitIndex);
                        sumDelta += lastBitsDiffSum(ref pixel, ref l, bitIndex);
                        sumDelta += lastBitsDiffSum(ref pixel, ref u, bitIndex);
                        sumDelta += lastBitsDiffSum(ref pixel, ref d, bitIndex);
                    }
                }

                res["sum delta bit " + bitIndex] = sumDelta;
            }

            return res;
        }

        private int lastBitsDiffSum(ref Pixel a, ref Pixel b, int bitIndex)
        {
            int sum = 0;
            var bitsCompared = new bool[16];

            var a1 = BitHelper.GetBitFromByte(a.R, bitIndex);
            var a2 = BitHelper.GetBitFromByte(a.G, bitIndex);
            var a3 = BitHelper.GetBitFromByte(a.B, bitIndex);
                                                   
            var b1 = BitHelper.GetBitFromByte(b.R, bitIndex);
            var b2 = BitHelper.GetBitFromByte(b.G, bitIndex);
            var b3 = BitHelper.GetBitFromByte(b.B, bitIndex);

            if (a1 != b1) sum++;
            if (a2 != b2) sum++;
            if (a3 != b3) sum++;

            return sum;
        }
    }
}
