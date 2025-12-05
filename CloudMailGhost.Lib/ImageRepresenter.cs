using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Lib
{
    public class ImageRepresenter
    {
        public Pixel[] Pixels;

        public int Width { get; set; }
        public int Height { get; set; }


        public struct Pixel
        {
            public byte R;
            public byte G;
            public byte B;

            public Pixel() { }
            public Pixel(byte R, byte G, byte B)
            {
                this.R = R;
                this.G = G;
                this.B = B;
            }
        }

        // Конструктор для создания из массива пикселей
        public ImageRepresenter(Pixel[] pixels, int width, int height)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
        }

        // Конструктор для совместимости с существующим кодом
        public ImageRepresenter() { }
    }
}
