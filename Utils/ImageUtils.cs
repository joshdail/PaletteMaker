using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PaletteMaker.Utils
{
    public static class ImageUtils
    {
        public static byte[] ExtractPixels(WriteableBitmap wb)
        {
            int size = wb.PixelSize.Width * wb.PixelSize.Height * 4;
            byte[] pixels = new byte[size];

            using (var fb = wb.Lock())
            {
                Marshal.Copy(fb.Address, pixels, 0, size);
            }

            return pixels;
        }

        public static List<(byte R, byte G, byte B)> ConvertBgraToRgb(byte[] pixels)
        {
            var rgbPixels = new List<(byte R, byte G, byte B)>(pixels.Length / 4);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                rgbPixels.Add((r, g, b));
            }

            return rgbPixels;
        }
    }
}
