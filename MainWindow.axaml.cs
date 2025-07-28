using Avalonia;
using Avalonia.Layout;
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
using PaletteMaker.Utils;

namespace PaletteMaker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PickImageButton.Click += async (_, __) => await PickImageAndProcessAsync();
            ClusterSlider.PropertyChanged += (_, args) =>
            {
                if (args.Property == Slider.ValueProperty)
                    ClusterCountLabel.Text = ((int)ClusterSlider.Value).ToString();
            };
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

            var kmeans = new KMeansClusterer(Convert.ToInt32(ClusterSlider.Value));
            var clusteredLabs = kmeans.Fit(labPixels);

            var clusterColors = clusteredLabs
                .Select(c => ColorUtils.LabToRgb(c.L, c.A, c.B))
                .ToList();

            foreach (var (r, g, b) in clusterColors)
            {
                var hsl = ColorUtils.RgbToHsl(r, g, b);
                string hex = $"#{r:X2}{g:X2}{b:X2}";
                string rgb = $"RGB: ({r}, {g}, {b})";
                string hslStr = $"HSL: ({(int)hsl.H}, {(int)(hsl.S * 100)}%, {(int)(hsl.L * 100)}%)";

                // Color Swatch (square)
                var rect = new Border
                {
                    Width = 80,
                    Height = 80,
                    Background = new SolidColorBrush(Color.FromRgb(r, g, b)),
                    Margin = new Thickness(5)
                };

                // Labels (in vertical stack)
                var labels = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                labels.Children.Add(new TextBlock { Text = hex, Classes = { "ColorSwatchLabel" } });
                labels.Children.Add(new TextBlock { Text = rgb, Classes = { "ColorSwatchLabel" } });
                labels.Children.Add(new TextBlock { Text = hslStr, Classes = { "ColorSwatchLabel" } });

                // Horizontal row: swatch + label block
                var itemRow = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = double.NaN // let it auto-size
                };

                itemRow.Children.Add(rect);
                itemRow.Children.Add(labels);

                PalettePanel.Children.Add(itemRow);
            } // foreach
        } // LoadImageAndGeneratePalette
    } // partial class MainWindow
} // namespace PaletteMaker
