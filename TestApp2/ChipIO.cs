using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

namespace TestApp2
{
    /// <summary>
    /// Manages 
    /// </summary>
    static class ChipIO
    {
        public static PixelDisplay PixDisplay { set; private get; }

        public static async void TestLoad()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                var bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                var pixelProvider = await decoder.GetPixelDataAsync(
                    Windows.Graphics.Imaging.BitmapPixelFormat.Rgba8,
                    Windows.Graphics.Imaging.BitmapAlphaMode.Straight,
                    new Windows.Graphics.Imaging.BitmapTransform(),
                    Windows.Graphics.Imaging.ExifOrientationMode.RespectExifOrientation,
                    Windows.Graphics.Imaging.ColorManagementMode.ColorManageToSRgb
                    );
                var pixels = pixelProvider.DetachPixelData();
                uint width = decoder.OrientedPixelWidth;
                uint height = decoder.OrientedPixelHeight;

                Color[][] LoadedColors = new Color[width][];
                for (int x = 0; x < width; x++)
                    LoadedColors[x] = new Color[height];
                for (var i = 0; i < height; i++)
                {
                    for (var j = 0; j < width; j++)
                    {
                        byte r = pixels[(i * width + j) * 4 + 0];
                        byte g = pixels[(i * width + j) * 4 + 1];
                        byte b = pixels[(i * width + j) * 4 + 2];
                        byte a = pixels[(i * width + j) * 4 + 3];
                        LoadedColors[j][i] = ColorHelper.FromArgb(a, r, g, b);
                    }
                }
                PixDisplay.LoadImage(LoadedColors);
            }
            else
            {
                Debug.WriteLine("ChipIO: The file is null.");
            }
        }

        public static async void TestSave()
        {

        }
    }
}
