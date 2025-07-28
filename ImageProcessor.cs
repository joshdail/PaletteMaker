using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing; // For Resize

namespace PaletteMaker
{
    public static class ImageProcessor
    {
        public static List<(byte R, byte G, byte B)> ExtractPixels(Stream imageStream)
        {
            using var image = Image.Load<Rgba32>(imageStream);

            // Resize to reduce processing cost (optional)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(200, 0)
            }));

            var pixels = new List<(byte R, byte G, byte B)>();

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image[x, y];
                    pixels.Add((pixel.R, pixel.G, pixel.B));
                }
            }

            return pixels;
        }
    }
}
