using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;

namespace TestApp2
{
    /// <summary>
    /// Stores the information for a pixel in the application - a Color, and a Rectangle.
    /// </summary>
    public class Pixel
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

        public Rectangle Rect
        {
            get;
            private set;
        }

        public Pixel(Color c)
        {
            Rect = new Rectangle()
            {
                Fill = new SolidColorBrush()
                {
                    Color = c
                },
                Width = 30,
                Height = 30
            };
            Color = c;
        }
        
    }
}
