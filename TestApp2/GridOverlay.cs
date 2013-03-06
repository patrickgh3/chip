using System;
using System.Collections.Generic;

using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;

namespace TestApp2
{
    /// <summary>
    /// Displays and modifies the grid overlay.
    /// </summary>
    class GridOverlay
    {
        Line[] HLines;
        Line[] VLines;
        Line[] BorderLines;
        Rectangle SelectRect1;
        Rectangle SelectRect2;

        SolidColorBrush GridBrush;
        SolidColorBrush GridBrushBorder;
        SolidColorBrush GridBrushSelect;

        PixelDisplay Display;

        /// <summary>
        /// Creates a new GridOverlay.
        /// </summary>
        public GridOverlay(PixelDisplay d)
        {
            Display = d;
            GridBrush = new SolidColorBrush(Colors.LightGray);
            GridBrushBorder = new SolidColorBrush(Colors.LightGray);
            GridBrushSelect = new SolidColorBrush(Colors.DimGray);
            SelectRect1 = new Rectangle() { Stroke = GridBrushSelect };
            SelectRect2 = new Rectangle() { Stroke = GridBrushSelect };
            BorderLines = new Line[4];
            for (int i = 0; i < BorderLines.Length; i++) BorderLines[i] = new Line() { Stroke = GridBrushBorder, StrokeThickness = 4, StrokeEndLineCap = PenLineCap.Triangle };
        }

        public void ResetLines()
        {
            HLines = new Line[Display.GetPixelsHeight()-1];
            VLines = new Line[Display.GetPixelsWidth()-1];
            for (int i = 0; i < HLines.Length; i++) HLines[i] = new Line() { Stroke = GridBrush, StrokeThickness = 1 };
            for (int i = 0; i < VLines.Length; i++) VLines[i] = new Line() { Stroke = GridBrush, StrokeThickness = 1 };
        }


        /// <summary>
        /// Updates the position of the grid on the canvas.
        /// </summary>
        public void UpdateDisplay()
        {
            int gridWidth = Display.GetPixelsWidth() * Display.PixelSize;
            int gridHeight = Display.GetPixelsHeight() * Display.PixelSize;
            int minX = 0;
            int minY = 0;
            int maxX = 1200;
            int maxY = 900;
            for (int i = 0; i < HLines.Length; i++)
            {
                Line line = HLines[i];
                Display.PixelCanvas.Children.Remove(line);
                line.X1 = Display.XOffset;
                line.X2 = Display.XOffset + gridWidth;
                line.Y1 = line.Y2 = Display.YOffset + (i + 1) * Display.PixelSize;

                if (line.Y1 < minY || line.Y1 > maxY) continue;
                if (!Display.GridEnabled) continue;
                Display.PixelCanvas.Children.Add(line);
            }
            for (int i = 0; i < VLines.Length; i++)
            {
                Line line = VLines[i];
                Display.PixelCanvas.Children.Remove(line);
                line.Y1 = Display.YOffset;
                line.Y2 = Display.YOffset + gridHeight;
                line.X1 = line.X2 = Display.XOffset + (i + 1) * Display.PixelSize;

                if (line.X1 < minX || line.X1 > maxX) continue;
                if (!Display.GridEnabled) continue;
                Display.PixelCanvas.Children.Add(line);
            }
            for (int i = 0; i < BorderLines.Length; i++)
            {
                Line line = BorderLines[i];
                Display.PixelCanvas.Children.Remove(line);
                switch (i)
                {
                    // Vertical lines
                    case 0: line.Y1 = Display.YOffset; line.Y2 = Display.YOffset + gridHeight; line.X1 = line.X2 = Display.XOffset; break;
                    case 1: line.Y1 = Display.YOffset; line.Y2 = Display.YOffset + gridHeight; line.X1 = line.X2 = Display.XOffset + gridWidth; break;
                    // Horizontal lines
                    case 2: line.X1 = Display.XOffset; line.X2 = Display.XOffset + gridWidth; line.Y1 = line.Y2 = Display.YOffset; break;
                    case 3: line.X1 = Display.XOffset; line.X2 = Display.XOffset + gridWidth; line.Y1 = line.Y2 = Display.YOffset + gridHeight; break;
                }
                if (Display.BorderEnabled) Display.PixelCanvas.Children.Add(line);
            }

            Display.PixelCanvas.Children.Remove(SelectRect1);
            Display.PixelCanvas.Children.Remove(SelectRect2);
            if (Display.Select1Enabled)
            {
                
                SelectRect1.Width = SelectRect1.Height = Display.PixelSize;
                Canvas.SetLeft(SelectRect1, 0);
                Canvas.SetTop(SelectRect1, 0);
                Display.PixelCanvas.Children.Add(SelectRect1);
            }
            if (Display.Select2Enabled)
            {

            }
        }

        public void SetRect1Position(int x, int y)
        {

        }


    }
}
