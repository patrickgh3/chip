﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.System;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace TestApp2
{
    /// <summary>
    /// The main page of the application. This stores several "global" application values.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        PixelDisplay PixDisplay;
        ColorPicker Picker;
        Boolean CtrlDown;



        public MainPage()
        {
            this.InitializeComponent();

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
            Window.Current.CoreWindow.PointerReleased += CoreWindow_PointerReleased;
            Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerMoved;
            Window.Current.CoreWindow.PointerWheelChanged += CoreWindow_PointerWheelChanged;
            

            PixDisplay = new PixelDisplay(pixelCanvas, debugText);
            Picker = new ColorPicker(colorSlider1, colorSlider2, colorSlider3, panel1, panel2, hueRect, overlayRect);

            // Set values now since PixDisplay isn't null.
            zoomSlider.Maximum = 10000;
            zoomSlider.Minimum = 100;
            zoomSlider.Value = 2000;
            zoomSlider.SmallChange = 100;
            zoomSlider.LargeChange = 100;
            zoomSlider.StepFrequency = 100;
            borderCheckBox.IsChecked = true;
        }

        // EVENT HANDLERS //

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        void CoreWindow_PointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            int x = (int)args.CurrentPoint.Position.X;
            int y = (int)args.CurrentPoint.Position.Y;
            PixDisplay.PointerMoved(x, y);
        }

        void CoreWindow_PointerPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            int x = (int)args.CurrentPoint.Position.X;
            int y = (int)args.CurrentPoint.Position.Y;
            PixDisplay.PointerChanged(x, y, true);
        }

        void CoreWindow_PointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            int x = (int)args.CurrentPoint.Position.X;
            int y = (int)args.CurrentPoint.Position.Y;
            PixDisplay.PointerChanged(x, y, false);
        }
        
        void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Control) CtrlDown = true;
            if (CtrlDown)
            {
                if (args.VirtualKey == VirtualKey.Z && !PixDisplay.PointerDown) PixDisplay.Undo();
                if (args.VirtualKey == VirtualKey.Y && !PixDisplay.PointerDown) PixDisplay.Redo();
            }

            switch (args.VirtualKey)
            {
                case VirtualKey.Number1: radio1.IsChecked = true; break;
                case VirtualKey.Number2: radio2.IsChecked = true; break;
                case VirtualKey.Number3: radio3.IsChecked = true; break;
            }
        }

        void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Control) CtrlDown = false;
        }

        void CoreWindow_PointerWheelChanged(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.MouseWheelDelta == -120) zoomSlider.Value -= 400;
            if (args.CurrentPoint.Properties.MouseWheelDelta == 120) zoomSlider.Value += 400;
        }
        
        private void ZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            PixDisplay.SetPixelSize((int)zoomSlider.Value / 100);
        }

        private void GridCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PixDisplay.SetGridEnabled(true);
        }

        private void GridCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PixDisplay.SetGridEnabled(false);
        }

        private void Radio1_Checked(object sender, RoutedEventArgs e)
        {
            if (PixDisplay != null) PixDisplay.SetDrawTool(PixelDisplay.DrawTool.Pencil);
        }

        private void Radio2_Checked(object sender, RoutedEventArgs e)
        {
            PixDisplay.SetDrawTool(PixelDisplay.DrawTool.FillBucket);
        }

        private void Radio3_Checked(object sender, RoutedEventArgs e)
        {
            PixDisplay.SetDrawTool(PixelDisplay.DrawTool.Line);
        }

        private void BorderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PixDisplay.SetBorderEnabled(true);
        }

        private void BorderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PixDisplay.SetBorderEnabled(false);
        }

        private void SaveButton_Clicked(object sender, RoutedEventArgs e)
        {
            //PixIO.Save(PixDisplay.GetCurrentColors());
        }
        
    }
}
