// Copyright (c) Ratish Philip 
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
// 
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software. 
// 
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE. 
//
// This file is part of the CompositionProToolkit project: 
// https://github.com/ratishphilip/CompositionProToolkit
//
// CompositionProToolkit v1.0.1
// 

using CompositionProToolkit;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GaussianMaskSurfacePage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private CanvasGeometry _combinedGeometry;
        private float _blurRadius;
        private IGaussianMaskSurface _gaussianSurface;
        private const float GridWidth = 400f;

        public GaussianMaskSurfacePage()
        {
            InitializeComponent();

            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = Window.Current.Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            var size = new Vector2(GridWidth, GridWidth);
            // Create the image visual
            var imageVisual = _compositor.CreateSpriteVisual();
            imageVisual.Size = size;
            var imageSurface = await _generator.CreateImageSurfaceAsync(new Uri("ms-appx:///Assets/Images/cat.png"),
                size.ToSize(), ImageSurfaceOptions.Default);
            imageVisual.Brush = _compositor.CreateSurfaceBrush(imageSurface);
            ElementCompositionPreview.SetElementChildVisual(ImageGrid, imageVisual);

            //Create the mask visual
            var maskVisual = _compositor.CreateSpriteVisual();
            maskVisual.Size = size;

            // Create the combined geometry
            var ellipse1 = CanvasGeometry.CreateEllipse(_generator.Device, GridWidth / 2, GridWidth / 2, GridWidth * 0.375f, GridWidth * 0.1875f);
            var ellipse2 = CanvasGeometry.CreateEllipse(_generator.Device, GridWidth / 2, GridWidth / 2, GridWidth * 0.1875f, GridWidth * 0.375f);
            _combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

            // Create SurfaceBrush from MaskSurface

            ElementCompositionPreview.SetElementChildVisual(MaskGrid, maskVisual);

            var outputVisual = _compositor.CreateSpriteVisual();
            outputVisual.Size = size;

            var maskedBrush = _compositor.CreateMaskBrush();
            maskedBrush.Source = imageVisual.Brush;
            _gaussianSurface = _generator.CreateGaussianMaskSurface(size.ToSize(), _combinedGeometry, Vector2.Zero, 0);
            maskedBrush.Mask = _compositor.CreateSurfaceBrush(_gaussianSurface);

            maskVisual.Brush = _compositor.CreateSurfaceBrush(_gaussianSurface); ;
            outputVisual.Brush = maskedBrush;

            ElementCompositionPreview.SetElementChildVisual(OutputGrid, outputVisual);
        }

        private void OnBlurRadiusChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _blurRadius = BlurSlider.Value.ToSingle();
            _gaussianSurface?.Redraw(_blurRadius);
        }


        private void OnOffsetChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Redraw();
        }

        private CanvasGeometry CalculateGeometry(Vector2 offset)
        {
            var sourceWidth = GridWidth;
            var sourceHeight = GridWidth;
            var width = sourceWidth - 2 * offset.X;
            var height = sourceHeight - 2 * offset.Y;

            var scaleX = width / sourceWidth;
            var scaleY = height / sourceHeight;

            var transform = Matrix3x2.CreateScale(scaleX, scaleY);

            var newGeometry = _combinedGeometry?.Transform(transform);
            return newGeometry;
        }

        private void OnToggled(object sender, RoutedEventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            var offset = OffsetSlider != null ? new Vector2(OffsetSlider.Value.ToSingle()) : Vector2.Zero;

            if (GeometrySwitch.IsOn)
            {
                var newGeometry = CalculateGeometry(offset);
                _gaussianSurface?.Redraw(newGeometry, offset);
            }
            else
            {
                _gaussianSurface?.Redraw(null, offset);
            }
        }

        private void OnMaskToggled(object sender, RoutedEventArgs e)
        {

        }
    }
}
