using CloudMailGhost.Lib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Desktop.Singletones
{
    internal class ImageLoader
    {
        public static ImageRepresenter LoadImageFromFile(string filePath)
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

        public static void SaveImageToFile(ImageRepresenter image, string filePath)
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
