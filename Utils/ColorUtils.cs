using System;

namespace PaletteMaker.Utils
{
    public static class ColorUtils
    {
        // RGB (0-255) to Linear RGB (0-1)
        private static double SrgbToLinear(double c)
        {
            c /= 255.0;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        // Linear RGB to XYZ
        private static void RgbToXyz(double r, double g, double b, out double x, out double y, out double z)
        {
            r = SrgbToLinear(r);
            g = SrgbToLinear(g);
            b = SrgbToLinear(b);

            x = r * 0.4124 + g * 0.3576 + b * 0.1805;
            y = r * 0.2126 + g * 0.7152 + b * 0.0722;
            z = r * 0.0193 + g * 0.1192 + b * 0.9505;
        }

        // XYZ to LAB (D65 reference white)
        public static (double L, double A, double B) RgbToLab(byte r, byte g, byte b)
        {
            RgbToXyz(r, g, b, out var x, out var y, out var z);

            // Normalize by D65 reference white
            x /= 0.95047;
            y /= 1.00000;
            z /= 1.08883;

            x = Pivot(x);
            y = Pivot(y);
            z = Pivot(z);

            double L = 116 * y - 16;
            double A = 500 * (x - y);
            double B = 200 * (y - z);

            return (L, A, B);
        }

        private static double Pivot(double n)
        {
            return n > 0.008856 ? Math.Pow(n, 1.0 / 3) : (7.787 * n) + (16.0 / 116);
        }

        public static double ColorDistance((double L, double A, double B) c1, (double L, double A, double B) c2)
        {
            double dl = c1.L - c2.L;
            double da = c1.A - c2.A;
            double db = c1.B - c2.B;
            return Math.Sqrt(dl * dl + da * da + db * db);
        }

        public static (byte R, byte G, byte B) LabToRgb(double l, double a, double b)
        {
            // Convert LAB to XYZ
            double y = (l + 16.0) / 116.0;
            double x = a / 500.0 + y;
            double z = y - b / 200.0;

            double x3 = Math.Pow(x, 3);
            double y3 = Math.Pow(y, 3);
            double z3 = Math.Pow(z, 3);

            x = 0.95047 * (x3 > 0.008856 ? x3 : (x - 16.0 / 116.0) / 7.787);
            y = 1.00000 * (y3 > 0.008856 ? y3 : (y - 16.0 / 116.0) / 7.787);
            z = 1.08883 * (z3 > 0.008856 ? z3 : (z - 16.0 / 116.0) / 7.787);

            // Convert XYZ to linear RGB
            double r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            double g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            double bC = x * 0.0557 + y * -0.2040 + z * 1.0570;

            // Apply gamma correction
            r = r > 0.0031308 ? 1.055 * Math.Pow(r, 1.0 / 2.4) - 0.055 : 12.92 * r;
            g = g > 0.0031308 ? 1.055 * Math.Pow(g, 1.0 / 2.4) - 0.055 : 12.92 * g;
            bC = bC > 0.0031308 ? 1.055 * Math.Pow(bC, 1.0 / 2.4) - 0.055 : 12.92 * bC;

            // Clamp to [0, 255]
            byte r8 = (byte)Math.Clamp(r * 255, 0, 255);
            byte g8 = (byte)Math.Clamp(g * 255, 0, 255);
            byte b8 = (byte)Math.Clamp(bC * 255, 0, 255);

            return (r8, g8, b8);
        }

    } // class ColorUtils
} // namespace PaletteMaker