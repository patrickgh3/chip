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

        public static async void Load()
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
        }

        public static async void Save()
        {
            Color[][] toSave = PixDisplay.PreviousColors;
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.FileTypeChoices.Add("PNG image", new string[] { ".png" });
            picker.DefaultFileExtension = ".png";
            var file = await picker.PickSaveFileAsync();
            
            if (file != null)
            {
                System.Guid encoderId;
                switch (file.FileType) {
                    case ".png":
                    default:
                        encoderId = Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId;
                        break;
                }
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                stream.Size = 0;
                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(
                    encoderId,
                    stream
                    );


                uint width = (uint)toSave.Length;
                uint height = (uint)toSave[0].Length;
                byte[] pixels = new byte[width*height*4];
                int k = 0;
                for (int i=0; i<height; i++)
                {
                    for (int j=0; j<width; j++)
                    {
                        pixels[k++] = toSave[j][i].R;
                        pixels[k++] = toSave[j][i].G;
                        pixels[k++] = toSave[j][i].B;
                        pixels[k++] = toSave[j][i].A;

                    }
                }
                encoder.SetPixelData(
                    Windows.Graphics.Imaging.BitmapPixelFormat.Rgba8,
                    Windows.Graphics.Imaging.BitmapAlphaMode.Straight,
                    width,
                    height,
                    96,
                    96,
                    pixels
                    );
                try {
                    await encoder.FlushAsync();
                } catch (Exception err) {
                    Debug.WriteLine("Error encoding the file for save.");
                }
            }
        }
    }
}
