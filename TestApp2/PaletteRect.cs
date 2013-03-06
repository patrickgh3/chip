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
                ((SolidColorBrush)OuterRect.Fill).Color = value;
                ((SolidColorBrush)InnerRect.Fill).Color = value;
            }
        }
        public Rectangle OuterRect { get; private set; }
        public int index { get; private set; }
        ColorPicker Picker;
        Rectangle InnerRect;

        static SolidColorBrush SelectedStrokeBrush = new SolidColorBrush() { Color = Colors.Gray };

        public PaletteRect(Rectangle rOuter, Rectangle rInner, int i, ColorPicker p)
        {
            OuterRect = rOuter;
            InnerRect = rInner;
            _color = ((SolidColorBrush)OuterRect.Fill).Color;
            index = i;
            Picker = p;
            InnerRect.PointerPressed += InnerRect_Selected;
        }

        void InnerRect_Selected(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Picker.ColorSelected(index);
        }

        public void Select()
        {
            ((SolidColorBrush)OuterRect.Fill).Color = _color;
            InnerRect.StrokeThickness = 0;
            OuterRect.StrokeThickness = 2;
        }

        public void Unselect()
        {
            ((SolidColorBrush)OuterRect.Fill).Color = Colors.Transparent;
            OuterRect.StrokeThickness = 0;
            InnerRect.StrokeThickness = 2;
        }
    }
}
