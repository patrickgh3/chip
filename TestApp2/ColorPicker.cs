using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace TestApp2
{
    /// <summary>
    /// Stores the logic for the color picker in the page's upper right corner.
    /// </summary>
    class ColorPicker
    {

        Slider Slider1; // Red   or Hue
        Slider Slider2; // Green or Saturation
        Slider Slider3; // Blue  or Luminence
        //Slider Slider4; // Alpha
        Rectangle Rect; // Displays the current color selection

        enum ColorType { RGB, HSL };

        ColorType Type = ColorType.RGB;
        public static Color CurrentColor = Colors.Black;

        public ColorPicker(Slider[] sliders, Rectangle rect)
        {
            Slider1 = sliders[0];
            Slider2 = sliders[1];
            Slider3 = sliders[2];
            //Slider4 = sliders[3];
            Rect = rect;

            Slider1.ValueChanged += Slider1_ValueChanged;
            Slider2.ValueChanged += Slider2_ValueChanged;
            Slider3.ValueChanged += Slider3_ValueChanged;
            //Slider4.ValueChanged += Slider4_ValueChanged;

            // todo: add choices to combobox here
        }

        void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Slider1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Type == ColorType.RGB) CurrentColor.R = (byte)Slider1.Value;
            updateRect();
        }

        void Slider2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Type == ColorType.RGB) CurrentColor.G = (byte)Slider2.Value;
            updateRect();
        }

        void Slider3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Type == ColorType.RGB) CurrentColor.B = (byte)Slider3.Value;
            updateRect();
        }

        void Slider4_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //if (Type == ColorType.RGB) CurrentColor.A = (byte)Slider4.Value;
            updateRect();
        }

        void updateRect()
        {
            ((SolidColorBrush)Rect.Fill).Color = CurrentColor;
            
        }
    }
}
