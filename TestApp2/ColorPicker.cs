using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace TestApp2
{
    /// <summary>
    /// Stores the logic for the color picker in the page's upper right corner. Uses HSV / HSB.
    /// /// </summary>
    /// Got help from: http://www.switchonthecode.com/tutorials/javascript-interactive-color-picker
    class ColorPicker
    {
        Rectangle HueRect;
        Rectangle OverlayRect;
        Color HueColor;

        Slider Slider1;
        Slider Slider2;
        Slider Slider3;
        
        PaletteRect[] PalRects;
        public static Color CurrentColor;
        int PalIndex;

        public ColorPicker(Slider s1, Slider s2, Slider s3, StackPanel p1, StackPanel p2, Rectangle hr, Rectangle or)
        {
            // TODO: possibly replace Rectangle[] with some sort of container/StackPanel and
            // dynamically add however many Rectangles to that guy.
            Slider1 = s1;
            Slider2 = s2;
            Slider3 = s3;
            Slider1.ValueChanged += Slider1_ValueChanged;
            Slider2.ValueChanged += Slider2_ValueChanged;
            Slider3.ValueChanged += Slider3_ValueChanged;

            HueRect = hr;
            OverlayRect = or;
            
            PalRects = new PaletteRect[8];
            for (int i = 0; i < PalRects.Length; i++)
            {
                Rectangle r = new Rectangle()
                {
                    Width = 60,
                    Height = 60,
                    Fill = new SolidColorBrush() { Color = Colors.Red },
                    Stroke = new SolidColorBrush(),
                    StrokeThickness = 5,
                    Margin = new Windows.UI.Xaml.Thickness() { Left = 10, Right = 10, Top = 10, Bottom = 10}
                };
                if (i < PalRects.Length / 2) p1.Children.Add(r);
                else p2.Children.Add(r);

                PalRects[i] = new PaletteRect(r,i,this);
            }
            PalRects[PalIndex].Select();
            CurrentColor = PalRects[PalIndex].Color;
            HueColor = Colors.White;
        }

        public void ColorSelected(int index)
        {
            PalRects[PalIndex].Unselect();
            PalIndex = index;
            PalRects[PalIndex].Select();
            CurrentColor = PalRects[PalIndex].Color;
            UpdateSliders();
        }

        public void SetCurrentColor(Color c)
        {
            CurrentColor = c;
            PalRects[PalIndex].Color = CurrentColor;
            UpdateSliders();
        }
        
        void Slider1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateColor();
        }

        void Slider2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateColor();
        }

        void Slider3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateColor();
        }

        void UpdateColor()
        {
            byte r, g, b;
            ChipUtil.HsvToRgb(Slider1.Value, Slider2.Value / 100, Slider3.Value / 100, out r, out g, out b);
            CurrentColor.R = r;
            CurrentColor.G = g;
            CurrentColor.B = b;
            PalRects[PalIndex].Color = CurrentColor;

            ChipUtil.HsvToRgb(Slider1.Value, 1, 1, out r, out g, out b);
            HueColor.R = r;
            HueColor.G = g;
            HueColor.B = b;
            ((SolidColorBrush)HueRect.Fill).Color = HueColor;
        }

        void UpdateSliders()
        {
            double hue, sat, val;
            ChipUtil.RgbToHsv(CurrentColor.R, CurrentColor.G, CurrentColor.B, out hue, out sat, out val);
            Slider1.Value = hue;
            Slider2.Value = sat * 100;
            Slider3.Value = val * 100;
        }
    }
}
