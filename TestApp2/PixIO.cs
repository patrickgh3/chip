using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Graphics.Imaging;

namespace TestApp2
{
    /// <summary>
    /// Saves and loads images from file.
    /// </summary>
    class PixIO
    {
        public static async void Save(Color[][] c)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "filename";
            StorageFile file = await savePicker.PickSaveFileAsync();


        }

        public static Color[][] Load()
        {
            Color[][] c = new Color[1][];


            return c;
        }
    }
}
