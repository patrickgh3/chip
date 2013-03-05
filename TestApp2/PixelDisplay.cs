using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;

using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;


namespace TestApp2
{
    /// <summary>
    /// Displays and modifies the pixels, and stores relevant data.
    /// </summary>
    class PixelDisplay
    {
        Pixel[][] Pixels;
        Stack<Color[][]> UndoStack;
        Stack<Color[][]> RedoStack;
        public Color[][] PreviousColors { get; private set; }

        public int XCenter { get; private set; }
        public int YCenter { get; private set; }
        public int XOffset { get; private set; }
        public int YOffset { get; private set; }
        public int PixelSize { get; private set; }
        public bool GridEnabled { get; private set; }
        public bool BorderEnabled { get; private set; }
        public bool Select1Enabled { get; private set; }
        public bool Select2Enabled { get; private set; }
        public bool PointerDown { get; private set; }
        public DrawTool Tool { get; private set; }

        public readonly Canvas PixelCanvas;
        TextBlock StatusText;
        GridOverlay Overlay;
        ColorPicker Picker;

        struct Point { public int x; public int y; }
        struct PrecisePoint { public double x; public double y; }
        Point PreviousPoint;
        Point Point1;
        Point Point2;
        bool PressedInsideGrid;

        public enum DrawTool { Pencil, Dropper, FillBucket, Line, Rectangle, Oval, Grab };

        
        
        


        /// <summary>
        /// Constructor for PixelDisplay. Requires a Canvas reference.
        /// </summary>
        public PixelDisplay(Canvas c, TextBlock t, ColorPicker p)
        {
            PixelCanvas = c;
            StatusText = t;
            Picker = p;
            PixelSize = 20;
            XCenter = 550;
            YCenter = 450;
            PreviousPoint = new Point() { x = -1, y = -1 };
            Point1 = new Point() { x = -1, y = -1 };
            Point2 = new Point() { x = -1, y = -1 };

            Tool = DrawTool.Pencil;
            Overlay = new GridOverlay(this);
            NewImage(16,16);
        }

        private void UpdateDisplay()
        {
            int gridWidth = PixelSize * Pixels.Length;
            int gridHeight = PixelSize * Pixels[0].Length;
            XOffset = XCenter - gridWidth / 2;
            YOffset = YCenter - gridHeight / 2;

            PixelCanvas.Children.Clear();
            int minX = 0;
            int minY = 0;
            int maxX = 1000; // TODO: replace these values (and the ones in GridOverlay.UpdateDisplay) with ones from the actual window size.
            int maxY = 900;
            for (int x = 0; x < Pixels.Length; x++)
            {
                for (int y = 0; y < Pixels[0].Length; y++)
                {
                    int rectX = XOffset + x * PixelSize;
                    int rectY = YOffset + y * PixelSize;
                    if (rectX + PixelSize < minX || rectX > maxX || rectY + PixelSize < minY || rectY > maxY) continue; 
                    Rectangle rect = Pixels[x][y].Rect;
                    rect.Width = PixelSize;
                    rect.Height = PixelSize;
                    Canvas.SetLeft(rect, rectX);
                    Canvas.SetTop(rect, rectY);
                    PixelCanvas.Children.Add(rect);
                }
            }
            Overlay.UpdateDisplay();
        }

        /// <summary>
        /// Clear all data for the previous image session, and start from scratch.
        /// </summary>
        public void NewImage(int width, int height)
        {
            Pixels = new Pixel[width][];
            for (int x = 0; x < width; x++)
            {
                Pixels[x] = new Pixel[height];
                for (int y = 0; y < height; y++)
                {
                    Pixels[x][y] = new Pixel(Colors.White);
                }
            }
            PreviousColors = GetCurrentColors();
            UndoStack = new Stack<Color[][]>();
            RedoStack = new Stack<Color[][]>();
            Overlay.ResetLines();
            UpdateDisplay();
        }

        /// <summary>
        /// Clear all data for the previous image session, and load the new pixel data.
        /// </summary>
        public void LoadImage(Color[][] source)
        {
            int width = source.Length;
            int height = source[0].Length;
            Pixels = new Pixel[width][];
            for (int x = 0; x < width; x++)
            {
                Pixels[x] = new Pixel[height];
                for (int y = 0; y < height; y++)
                {
                    Pixels[x][y] = new Pixel(source[x][y]);
                }
            }
            PreviousColors = GetCurrentColors();
            UndoStack = new Stack<Color[][]>();
            RedoStack = new Stack<Color[][]>();
            Overlay.ResetLines();
            UpdateDisplay();
        }

        public void PointerChanged(int pointerx, int pointery, bool b)
        {
            PointerDown = b;
            bool pointerInsideGrid = pointerx > XOffset && pointerx < XOffset + Pixels.Length * PixelSize &&
                                     pointery > YOffset && pointery < YOffset + Pixels[0].Length * PixelSize;
            PressedInsideGrid = pointerInsideGrid;
            if (pointerInsideGrid)
            {
                int x = (int)((pointerx - XOffset) / PixelSize);
                int y = (int)((pointery - YOffset) / PixelSize);
                if (PointerDown)
                {
                    if (Tool == DrawTool.Pencil)
                    {
                        PencilTool(x, y, ColorPicker.CurrentColor);
                        Point1.x = x;
                        Point1.y = y;
                    }
                    else if (Tool == DrawTool.Dropper)
                    {
                        Picker.SetCurrentColor(Pixels[x][y].Color);
                    }
                    else if (Tool == DrawTool.FillBucket)
                    {
                        if (Pixels[x][y].Color != ColorPicker.CurrentColor) FillTool(x, y, ColorPicker.CurrentColor, Pixels[x][y].Color);
                        AddUndoAction();
                    }
                    else if (Tool == DrawTool.Line)
                    {
                        Point1.x = x;
                        Point1.y = y;
                        Point2.x = x;
                        Point2.y = y;
                        PencilTool(x, y, ColorPicker.CurrentColor);
                    }
                    else if (Tool == DrawTool.Rectangle)
                    {
                        Point1.x = x;
                        Point1.y = y;
                        Point2.x = x;
                        Point2.y = y;
                        PencilTool(x, y, ColorPicker.CurrentColor);
                    }
                    else if (Tool == DrawTool.Oval)
                    {
                        Point1.x = x;
                        Point1.y = y;
                        Point2.x = x;
                        Point2.y = y;
                        PencilTool(x, y, ColorPicker.CurrentColor);
                    }
                }
            }
            if (!PointerDown)
            {
                if (Tool == DrawTool.Pencil) AddUndoAction();
                else if (Tool == DrawTool.FillBucket) { }
                else if (Tool == DrawTool.Line) AddUndoAction();
                else if (Tool == DrawTool.Rectangle) AddUndoAction();
                else if (Tool == DrawTool.Oval) AddUndoAction();
            }
        }

        public void PointerMoved(int pointerx, int pointery)
        {
            if (pointerx > XOffset && pointerx < XOffset + Pixels.Length * PixelSize &&
                pointery > YOffset && pointery < YOffset + Pixels[0].Length * PixelSize)
            {
                int x = (int)((pointerx - XOffset) / PixelSize);
                int y = (int)((pointery - YOffset) / PixelSize);

                if (Tool == DrawTool.Pencil && PointerDown)
                {
                    LineTool(Point1.x, Point1.y, x, y, ColorPicker.CurrentColor);
                    Point1.x = x;
                    Point1.y = y;
                }
                else if (Tool == DrawTool.FillBucket) { }
                else if (Tool == DrawTool.Line && PointerDown)
                {
                    Point2.x = x;
                    Point2.y = y;
                    UpdatePixelColors(PreviousColors);
                    LineTool(Point1.x, Point1.y, Point2.x, Point2.y, ColorPicker.CurrentColor);
                }
                else if (Tool == DrawTool.Rectangle && PointerDown)
                {
                    Point2.x = x;
                    Point2.y = y;
                    UpdatePixelColors(PreviousColors);
                    RectangleTool(Point1.x, Point1.y, Point2.x, Point2.y, ColorPicker.CurrentColor);
                }
                else if (Tool == DrawTool.Oval && PointerDown)
                {
                    Point2.x = x;
                    Point2.y = y;
                    UpdatePixelColors(PreviousColors);
                    OvalTool(Point1.x, Point1.y, Point2.x, Point2.y, ColorPicker.CurrentColor);
                }
            }
            if (Tool == DrawTool.Grab && PointerDown && PressedInsideGrid)
            {
                XCenter += pointerx - PreviousPoint.x;
                YCenter += pointery - PreviousPoint.y;
                UpdateDisplay();
            }
            PreviousPoint.x = pointerx;
            PreviousPoint.y = pointery;
        }

        public void ResetPosition()
        {
            XCenter = 550;
            YCenter = 450;
            UpdateDisplay();
        }
        
        public void Undo()
        {
            if (UndoStack.Count == 0)
            {
                //debugText.Text = "Nothing to Undo.";
                return;
            }
            //debugText.Text = "Undid Action.";
            RedoStack.Push(GetCurrentColors());
            Color[][] colors = UndoStack.Pop();
            UpdatePixelColors(colors);
            PreviousColors = GetCurrentColors();
        }
        
        public void Redo()
        {
            if (RedoStack.Count == 0)
            {
                //debugText.Text = "Nothing to Redo";
                return;
            }
            //debugText.Text = "Redid Action.";
            UndoStack.Push(GetCurrentColors());
            Color[][] colors = RedoStack.Pop();
            UpdatePixelColors(colors);
            PreviousColors = colors;
        }

        private void AddUndoAction()
        {
            if (ColorArraysEqual(GetCurrentColors(), PreviousColors)) return;
            // First, if we had one, add the previous state to the stack.
            UndoStack.Push(PreviousColors);
            PreviousColors = GetCurrentColors();
            RedoStack.Clear();
            // Then, save the current state so we can add it to the stack later, if we need to.

        }
        
        private void UpdatePixelColors(Color[][] colors)
        {
            for (int x = 0; x < Pixels.Length; x++)
            {
                for (int y = 0; y < Pixels[0].Length; y++)
                {
                    if (Pixels[x][y].Color != colors[x][y])
                        Pixels[x][y].Color = colors[x][y];
                }
            }
        }
        
        private Color[][] GetCurrentColors()
        {
            Color[][] colors = new Color[Pixels.Length][];
            for (int x = 0; x < Pixels.Length; x++)
            {
                colors[x] = new Color[Pixels[0].Length];
                for (int y = 0; y < Pixels[0].Length; y++)
                {
                    colors[x][y] = Pixels[x][y].Color;
                }
            }
            return colors;
        }

        private Boolean ColorArraysEqual(Color[][] c1, Color[][] c2)
        {
            if (c1.Length != c2.Length || c1[0].Length != c2[0].Length) throw new FormatException("Color Array Dimensions not equal. Sadface.");
            for (int x = 0; x < c1.Length; x++)
            {
                for (int y = 0; y < c1[0].Length; y++)
                {
                    if (c1[x][y].R != c2[x][y].R || c1[x][y].G != c2[x][y].G || c1[x][y].B != c2[x][y].B) return false;
                }
            }
            return true;

        }

        // TOOL DRAWING //

        private void PencilTool(int x, int y, Color c)
        {
            Pixels[x][y].Color = c;
        }

        private void FillTool(int x, int y, Color c, Color overridden)
        {
            Pixels[x][y].Color = c;
            if (x != Pixels.Length - 1 && Pixels[x + 1][y].Color == overridden) FillTool(x + 1, y, c, overridden);
            if (x != 0 && Pixels[x - 1][y].Color == overridden) FillTool(x - 1, y, c, overridden);
            if (y != Pixels[0].Length - 1 && Pixels[x][y + 1].Color == overridden) FillTool(x, y + 1, c, overridden);
            if (y != 0 && Pixels[x][y - 1].Color == overridden) FillTool(x, y - 1, c, overridden);
        }

        private void LineTool(int x1, int y1, int x2, int y2, Color c)
        {
            Pixels[x1][y1].Color = c;
            Pixels[x2][y2].Color = c;
            PrecisePoint slope = new PrecisePoint() { x = -1, y = -1 };
            int xdistance = Math.Abs(x2 - x1);
            int ydistance = Math.Abs(y2 - y1);
            
            // Right or left quadrants
            if (xdistance >= ydistance && xdistance != 0)
            {
                
                slope.x = Math.Sign(x2 - x1);
                slope.y = (double)(y2 - y1) / (double)Math.Abs(x2 - x1);
                double y = y1 + 0.5 + slope.y;
                for (int x = x1 + (int)slope.x; (x2 - x1 > 0 && x < x2) || (x2 - x1 < 0 && x > x2); x += (int)slope.x)
                {
                    Pixels[x][(int)y].Color = c;
                    y += slope.y;
                }
            }
            // Top or bottom quadrants
            else
            {
                slope.y = Math.Sign(y2 - y1);
                slope.x = (double)(x2 - x1) / (double)Math.Abs(y2 - y1);
                double x = x1 + 0.5 + slope.x;
                for (int y = y1 + (int)slope.y; (y2 - y1 > 0 && y < y2) || (y2 - y1 < 0 && y > y2); y += (int)slope.y)
                {
                    Pixels[(int)x][y].Color = c;
                    x += slope.x;
                }
            }
        }

        private void RectangleTool(int x1, int y1, int x2, int y2, Color c)
        {
            int xo = Math.Sign(x2 - x1);
            for (int x = x1; (x <= x2 && x2 > x1) || (x >= x2 && x1 > x2); x += xo)
            {
                Pixels[x][y1].Color = c;
                Pixels[x][y2].Color = c;
            }
            int yo = Math.Sign(y2 - y1);
            for (int y = y1; (y <= y2 && y2 > y1) || (y >= y2 && y1 > y2); y += yo)
            {
                Pixels[x1][y].Color = c;
                Pixels[x2][y].Color = c;
            }
        }

        // http://gamedev.sk/bresenham-s-ellipse-algorithm
        // TODO: fix pixels too concentrated :(
        private void OvalTool(int x1, int y1, int x2, int y2, Color c)
        {

            
            //int a = Math.Abs((int)((x2 - x1) / 2));
            //int b = Math.Abs((int)((y2 - y1) / 2));
            //int x0 = Math.Max(x1, x2) - a/2;
            //int y0 = Math.Max(y1, y2) - b/2;
            
            int x0 = x1;
            int y0 = y1;
            int a = Math.Abs(x2 - x1);
            int b = Math.Abs(y2 - y1);


            if (a == 0 || b == 0)
                return;
            int a2 = 2 * a * a;
            int b2 = 2 * b * b;
            int error = a * a * b;
            int x = 0;
            int y = b;
            int stopy = 0;
            int stopx = a2 * b;
            while (stopy <= stopx)
            {
                SetPixelChecked(x0 + x, y0 + y, c);
                SetPixelChecked(x0 - x, y0 + y, c);
                SetPixelChecked(x0 - x, y0 - y, c);
                SetPixelChecked(x0 + x, y0 - y, c);
                Debug.WriteLine("x0: " + x0 + " x: " + x);
                x++;
                error -= b2 * (x - 1);
                stopy += b2;
                if (error <= 0)
                {
                    error += a2 * (y - 1);
                    y--;
                    stopx -= a2;
                }
            }

            error = b * b * a;
            x = a;
            y = 0;
            stopy = b2 * a;
            stopx = 0;
            while (stopy >= stopx)
            {
                SetPixelChecked(x0 + x, y0 + y, c);
                SetPixelChecked(x0 - x, y0 + y, c);
                SetPixelChecked(x0 - x, y0 - y, c);
                SetPixelChecked(x0 + x, y0 - y, c);
                y++;
                error -= a2 * (y - 1);
                stopx += a2;
                if (error < 0)
                {
                    error += b2 * (x - 1);
                    x--;
                    stopy -= b2;
                }
            }
        }

        void SetPixelChecked(int x, int y, Color c)
        {
            if (x >= 0 && x < Pixels.Length && y >= 0 && y < Pixels[0].Length)
                Pixels[x][y].Color = c;
        }

        // SETTERS FROM EVENTS //

        public void SetGridEnabled(bool b)
        {
            GridEnabled = b;
            Overlay.UpdateDisplay();
        }

        public void SetBorderEnabled(bool b)
        {
            BorderEnabled = b;
            Overlay.UpdateDisplay();
        }

        public void SetPixelSize(int size)
        {
            PixelSize = size;
            UpdateDisplay();
        }

        public void SetDrawTool(DrawTool t)
        {
            Tool = t;
        }

        public int GetPixelsWidth()
        {
            return Pixels.Length;
        }

        public int GetPixelsHeight()
        {
            return Pixels[0].Length;
        }

    }
}
