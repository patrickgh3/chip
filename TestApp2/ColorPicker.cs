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
using Windows.UI.Xaml;
using Windows.Foundation;

namespace TestApp2
{
    /// <summary>
    /// Stores the logic for the color picker in the page's upper right corner. Uses HSV / HSB.
    /// /// </summary>
    /// Color picker tutorial: http://www.switchonthecode.com/tutorials/javascript-interactive-color-picker
    /// Absolute position of an element: http://stackoverflow.com/questions/12387449/how-to-get-the-absolute-position-of-an-element/12388558#12388558
    class ColorPicker
    {
        Canvas PickerCanvas;
        Ellipse Circle;
        bool PressedInsideCanvas;

        Rectangle HueRect;
        Rectangle OverlayRect;
        Color HueColor;

        Slider Slider1;
        Slider Slider2;
        Slider Slider3;
        
        PaletteRect[] PalRects;
        public static Color CurrentColor;
        int PalIndex;

        public ColorPicker(Slider s1, Slider s2, Slider s3, StackPanel p1, StackPanel p2, Rectangle hr, Rectangle or, Canvas c)
        {
            Slider1 = s1;
            Slider2 = s2;
            Slider3 = s3;
            Slider1.ValueChanged += Slider1_ValueChanged;
            Slider2.ValueChanged += Slider2_ValueChanged;
            Slider3.ValueChanged += Slider3_ValueChanged;
            HueRect = hr;
            OverlayRect = or;
            PickerCanvas = c;
            Circle = new Ellipse() {
                Width = 20,
                Height = 20,
                Stroke = new SolidColorBrush(Colors.Black)
            };
            PickerCanvas.Children.Add(Circle);
            
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
            HueColor = Colors.White;
            ColorSelected(0);
        }
        
        public void ColorSelected(int index)
        {
            PalRects[PalIndex].Unselect();
            PalIndex = index;
            PalRects[PalIndex].Select();
            CurrentColor = PalRects[PalIndex].Color;
            UpdatePositions();
        }

        public void SetCurrentColor(Color c)
        {
            CurrentColor = c;
            PalRects[PalIndex].Color = CurrentColor;
            UpdatePositions();
        }

        private void UpdateCurrentColor()
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

        private void UpdatePositions()
        {
            double hue, sat, val;
            ChipUtil.RgbToHsv(CurrentColor.R, CurrentColor.G, CurrentColor.B, out hue, out sat, out val);
            Slider1.Value = hue;
            Slider2.Value = sat * 100;
            Slider3.Value = val * 100;
            UpdateCirclePosition();
        }

        private void UpdateCirclePosition()
        {
            int x, y;
            ChipUtil.HsvToPos((double)Slider2.Value / 100, (double)Slider3.Value / 100, out x, out y);
            Canvas.SetLeft(Circle, x - Circle.Width / 2);
            Canvas.SetTop(Circle, y - Circle.Width / 2);
        }

        private void Slider1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateCurrentColor();
        }

        private void Slider2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateCurrentColor();
            UpdateCirclePosition();
        }

        private void Slider3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateCurrentColor();
            UpdateCirclePosition();
        }

        public void PointerChanged(int pointerx, int pointery, bool pressed)
        {
            if (pressed)
            {
                var ttv = PickerCanvas.TransformToVisual(Window.Current.Content);
                Point screenCoords = ttv.TransformPoint(new Point(0, 0));
                int x = pointerx - (int)screenCoords.X;
                int y = pointery - (int)screenCoords.Y;
                if (x > 0 && x < 256 && y > 0 && y < 256)
                {
                    MoveCircle(pointerx, pointery);
                    PressedInsideCanvas = true;
                }
                else PressedInsideCanvas = false;
            }
            else PressedInsideCanvas = false;
        }

        public void PointerMoved(int pointerx, int pointery)
        {
            if (PressedInsideCanvas) MoveCircle(pointerx, pointery);
        }

        private void MoveCircle(int pointerx, int pointery)
        {
            var ttv = PickerCanvas.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            int x = pointerx - (int)screenCoords.X;
            int y = pointery - (int)screenCoords.Y;
            x = Math.Max(x, 0);
            x = Math.Min(x, (int)HueRect.Width-1);
            y = Math.Max(y, 0);
            y = Math.Min(y, (int)HueRect.Height - 1);
            Canvas.SetLeft(Circle, x - Circle.Width / 2);
            Canvas.SetTop(Circle, y - Circle.Width / 2);

            double s, v;
            ChipUtil.PosToHsv(x, y, Slider1.Value, out s, out v);
            Slider2.Value = s * 100;
            Slider3.Value = v * 100;

            byte r, g, b;
            ChipUtil.HsvToRgb(Slider1.Value, Slider2.Value / 100, Slider3.Value / 100, out r, out g, out b);
            CurrentColor.R = r;
            CurrentColor.G = g;
            CurrentColor.B = b;
            PalRects[PalIndex].Color = CurrentColor;
        }
    }
}
