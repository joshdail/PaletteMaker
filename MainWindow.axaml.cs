using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using PaletteMaker.Utils; // <-- Make sure this matches your namespace structure

namespace PaletteMaker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PickImageButton.Click += async (_, __) => await PickImageAndProcessAsync();
        }

        private async Task PickImageAndProcessAsync()
        {
            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select an image",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Images")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" }
                    }
                }
            };

            var result = await this.StorageProvider.OpenFilePickerAsync(options);

            if (result == null || !result.Any())
                return;

            var file = result.First();
            await using var stream = await file.OpenReadAsync();
            await LoadImageAndGeneratePalette(stream);
        }

        private async Task LoadImageAndGeneratePalette(Stream imageStream)
        {
            PalettePanel.Children.Clear();

            var writeableBitmap = await Task.Run(() => WriteableBitmap.DecodeToWidth(imageStream, 200));

            int width = writeableBitmap.PixelSize.Width;
            int height = writeableBitmap.PixelSize.Height;

            // Refactored image utils
            var pixels = ImageUtils.ExtractPixels(writeableBitmap);
            var rgbPixels = ImageUtils.ConvertBgraToRgb(pixels);

            var labPixels = rgbPixels.Select(p => ColorUtils.RgbToLab(p.R, p.G, p.B)).ToList();

            var kmeans = new KMeansClusterer(4);
            var clusteredLabs = kmeans.Fit(labPixels);

            var clusterColors = clusteredLabs
                .Select(c => ColorUtils.LabToRgb(c.L, c.A, c.B))
                .ToList();

            foreach (var (r, g, b) in clusterColors)
            {
                var rect = new Border
                {
                    Width = 60,
                    Height = 60,
                    Background = new SolidColorBrush(Color.FromRgb(r, g, b)),
                    Margin = new Thickness(5)
                };
                PalettePanel.Children.Add(rect);

                var hexText = new TextBlock
                {
                    Text = $"#{r:X2}{g:X2}{b:X2}",
                    Foreground = Brushes.Black,
                    Margin = new Thickness(5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };
                PalettePanel.Children.Add(hexText);
            }
        }
    } // partial class MainWindow
} // namespace PaletteMaker
