using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CompositionProToolkit.Expressions;
using CompositionProToolkit.Win2d;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CanvasGeometryParserPage : Page
    {
        private List<Color> _colors;

        private string _data = string.Empty;
        private StringBuilder _logger;

        private float _strokeThickness;
        private Color _strokeColor;
        private Color _fillColor;

        public CanvasGeometryParserPage()
        {
            this.InitializeComponent();
            _logger = new StringBuilder();
            _colors = new List<Color>()
            {
                Colors.Transparent,
                Colors.Black,
                Colors.White,
                Colors.Red,
                Colors.Blue,
                Colors.Green,
                Colors.Yellow
            };

            var colorList = new List<String>()
            {
                "Transparent",
                "Black",
                "White",
                "Red",
                "Blue",
                "Green",
                "Yellow"
            };

            StrokeList.ItemsSource = colorList;
            FillList.ItemsSource = colorList;

            StrokeThickness.Value = 1;
            StrokeList.SelectedIndex = 1;
            FillList.SelectedIndex = 0;
        }

        private void OnParseData(Object sender, RoutedEventArgs e)
        {
            _data = InputData.Text;
            RenderCanvas.Invalidate();
        }

        private void OnCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (String.IsNullOrWhiteSpace(_data))
            {
                CommandsList.Text = string.Empty;
                return;
            }

            _logger?.Clear();
            CommandsList.Text = string.Empty;

            var geometry = CanvasGeometryParser.Parse(sender, _data, _logger);
            CommandsList.Text = _logger.ToString();

            args.DrawingSession.FillGeometry(geometry, _fillColor);
            args.DrawingSession.DrawGeometry(geometry, _strokeColor, _strokeThickness);
        }

        private void RenderScroll_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderCanvas.Width = Math.Max(RenderScroll.ActualWidth, RenderCanvas.MinWidth);
            RenderCanvas.Height = Math.Max(RenderScroll.ActualHeight, RenderCanvas.MinHeight);
        }

        private void OnStrokeThicknessChanged(Object sender, RangeBaseValueChangedEventArgs e)
        {
            _strokeThickness = StrokeThickness.Value.Single();
            RenderCanvas.Invalidate();
        }

        private void OnStrokeColorChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (StrokeList.SelectedIndex > -1)
            {
                _strokeColor = _colors[StrokeList.SelectedIndex];
            }
            RenderCanvas.Invalidate();
        }

        private void OnFillColorChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (FillList.SelectedIndex > -1)
            {
                _fillColor = _colors[FillList.SelectedIndex];
            }
            RenderCanvas.Invalidate();
        }

        private void OnShowSample(Object sender, RoutedEventArgs e)
        {
            InputData.Text =
                "F0 M 656.500,400.500 C 656.500,350.637 598.572,307.493 514.292,286.708 C 493.507,202.428 450.363,144.500 400.500,144.500 C 350.637,144.500 307.493,202.428 286.708,286.708 C 202.428,307.493 144.500,350.637 144.500,400.500 C 144.500,450.363 202.428,493.507 286.708,514.292 C 307.493,598.572 350.637,656.500 400.500,656.500 C 450.363,656.500 493.507,598.572 514.292,514.292 C 598.572,493.507 656.500,450.363 656.500,400.500 ZM 581.519,219.481 C 546.261,184.222 474.793,194.676 400.500,239.574 C 326.207,194.676 254.739,184.222 219.481,219.481 C 184.222,254.739 194.676,326.207 239.574,400.500 C 194.676,474.792 184.222,546.261 219.481,581.519 C 254.739,616.778 326.207,606.324 400.500,561.426 C 474.793,606.324 546.261,616.778 581.519,581.519 C 616.778,546.261 606.324,474.792 561.426,400.500 C 606.324,326.207 616.778,254.739 581.519,219.481 ZM 688.500,688.500 L 112.500,688.500 L 112.500,112.500 L 688.500,112.500 L 688.500,688.500 Z";

            StrokeThickness.Value = 2;
            StrokeList.SelectedIndex = 3;
            FillList.SelectedIndex = 2;

            OnParseData(this, null);
        }

        private void OnClearCanvas(Object sender, RoutedEventArgs e)
        {
            InputData.Text = string.Empty;
            OnParseData(this, null);
        }
    }
}
