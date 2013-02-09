using System;
using System.Collections.Generic;
using System.Windows;

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
        Color[][] PreviousColors;

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

        struct Point { public int x; public int y; }
        struct PrecisePoint { public double x; public double y; }
        Point PreviousPoint;
        Point LinePoint1;
        Point LinePoint2;

        public enum DrawTool { Pencil, FillBucket, Line };

        
        
        


        /// <summary>
        /// Constructor for PixelDisplay. Requires a Canvas reference.
        /// </summary>
        public PixelDisplay(Canvas c, TextBlock t)
        {
            PixelCanvas = c;
            StatusText = t;
            PixelSize = 20;
            XCenter = 550;
            YCenter = 450;
            PreviousPoint = new Point() { x = -1, y = -1 };
            LinePoint1 = new Point() { x = -1, y = -1 };
            LinePoint2 = new Point() { x = -1, y = -1 };

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
            for (int x = 0; x < Pixels.Length; x++)
            {
                for (int y = 0; y < Pixels[0].Length; y++)
                {
                    Rectangle rect = Pixels[x][y].Rect;
                    rect.Width = PixelSize;
                    rect.Height = PixelSize;
                    Canvas.SetLeft(rect, XOffset + x * PixelSize);
                    Canvas.SetTop(rect, YOffset + y * PixelSize);
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
            Pixels = new Pixel[16][];
            for (int x = 0; x < 16; x++)
            {
                Pixels[x] = new Pixel[16];
                for (int y = 0; y < 16; y++)
                {
                    Pixels[x][y] = new Pixel(Colors.White);
                }
            }
            PreviousColors = GetCurrentColors();
            UndoStack = new Stack<Color[][]>();
            RedoStack = new Stack<Color[][]>();
            UpdateDisplay();
            // TODO: reset overlay here
        }

        /// <summary>
        /// Clear all data for the previous image session, and load the new pixel data.
        /// </summary>
        public void loadImage(Color[][] c)
        {

        }

        public void PointerChanged(int pointerx, int pointery, bool b)
        {
            PointerDown = b;
            bool pointerInsideGrid = pointerx > XOffset && pointerx < XOffset + Pixels.Length * PixelSize &&
                                     pointery > YOffset && pointery < YOffset + Pixels[0].Length * PixelSize;
            if (pointerInsideGrid)
            {
                int x = (int)((pointerx - XOffset) / PixelSize);
                int y = (int)((pointery - YOffset) / PixelSize);
                if (PointerDown)
                {
                    if (Tool == DrawTool.Pencil) PencilTool(x, y, ColorPicker.CurrentColor);
                    else if (Tool == DrawTool.FillBucket) {
                        FillTool(x, y, ColorPicker.CurrentColor, Pixels[x][y].Color);
                        AddUndoAction();
                    }
                    else if (Tool == DrawTool.Line)
                    {
                        LinePoint1.x = x;
                        LinePoint1.y = y;
                        LinePoint2.x = x;
                        LinePoint2.y = y;
                        PencilTool(x, y, ColorPicker.CurrentColor);
                    }
                }
            }
            if (!PointerDown)
            {
                if (Tool == DrawTool.Pencil) AddUndoAction();
                else if (Tool == DrawTool.FillBucket) { }
                else if (Tool == DrawTool.Line) AddUndoAction();
            }
        }

        public void PointerMoved(int pointerx, int pointery)
        {
            if (pointerx > XOffset && pointerx < XOffset + Pixels.Length * PixelSize &&
                pointery > YOffset && pointery < YOffset + Pixels[0].Length * PixelSize)
            {
                int x = (int)((pointerx - XOffset) / PixelSize);
                int y = (int)((pointery - YOffset) / PixelSize);
                if (PreviousPoint.x == -1) { PreviousPoint.x = x; PreviousPoint.y = y; }

                if (Tool == DrawTool.Pencil && PointerDown) PencilTool(x, y, ColorPicker.CurrentColor);
                else if (Tool == DrawTool.FillBucket) { }
                else if (Tool == DrawTool.Line && PointerDown)
                {
                    LinePoint2.x = x;
                    LinePoint2.y = y;
                    UpdatePixelColors(PreviousColors);
                    LineTool(LinePoint1.x, LinePoint1.y, LinePoint2.x, LinePoint2.y, ColorPicker.CurrentColor);
                }

                PreviousPoint.x = x;
                PreviousPoint.y = y;
            }
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
        
        public Color[][] GetCurrentColors()
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
                for (int y = 0; y < c1.Length; y++)
                {
                    if (c1[x][y].R != c2[x][y].R || c1[x][y].G != c2[x][y].G || c1[x][y].B != c2[x][y].B) return false;
                }
            }
            return true;

        }

        // TOOL DRAWING //

        private void PencilTool(int x, int y, Color c)
        {
            LineTool(PreviousPoint.x, PreviousPoint.y, x, y, c);
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
