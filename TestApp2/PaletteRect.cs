using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;

namespace TestApp2
{
    /// <summary>
    /// Stores the information for a color in the palette - a Color and a Rectangle.
    /// </summary>
    class PaletteRect
    {
        private Color _color;
        public Color Color {
            get { return _color; }
            set
            {
                _color = value;
                ((SolidColorBrush)Rect.Fill).Color = value;
            }
        }
        public Rectangle Rect { get; private set; }
        public int index { get; private set; }
        ColorPicker Picker;

        static SolidColorBrush SelectedStrokeBrush = new SolidColorBrush() { Color = Colors.Gray };

        public PaletteRect(Rectangle r, int i, ColorPicker p)
        {
            Rect = r;
            _color = ((SolidColorBrush)Rect.Fill).Color;
            index = i;
            Picker = p;
            Rect.PointerPressed += Rect_Selected;
        }

        void Rect_Selected(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Picker.ColorSelected(index);
        }

        public void Select()
        {
            Rect.Stroke = SelectedStrokeBrush;
        }

        public void Unselect()
        {
            Rect.Stroke = null;
        }
    }
}
