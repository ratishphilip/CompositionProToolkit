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
using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed partial class ImageMaskSurfacePage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private float _blurRadius;
        private IImageSurface _sourceImageSurface;
        private IImageMaskSurface _imageMaskSurface;
        private const float GridWidth = 400f;
        private Dictionary<string, Uri> _images;
        private Vector2 _defaultSize;
        private SpriteVisual _originalImageVisual;
        private IImageSurface _imageSurface;
        private SpriteVisual _maskVisual;
        private SpriteVisual _sourceImageVisual;
        private SpriteVisual _outputVisual;

        public ImageMaskSurfacePage()
        {
            this.InitializeComponent();
            _images = new Dictionary<string, Uri>
            {
                {"Image1", new Uri("ms-appx:///Assets/Images/RainbowRose.png")},
                {"Image2", new Uri("ms-appx:///Assets/Images/Flower.png")},
            };

            ImageList.ItemsSource = _images.Keys.ToList();

            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            Redraw();
        }

        private void OnBlurRadiusChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _blurRadius = BlurSlider.Value.ToSingle();
            _imageMaskSurface?.Redraw(ImageSurfaceOptions.GetDefaultImageMaskOptionsForBlur(_blurRadius));
        }


        private void OnOffsetChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Redraw(false);
        }

        private void OnImageSelected(object sender, SelectionChangedEventArgs e)
        {
            Redraw();
        }

        async void Redraw(bool recreateSurface = true)
        {
            if (_compositor == null)
            {
                _compositor = Window.Current.Compositor;
                _generator = _compositor.CreateCompositionGenerator();

                _defaultSize = new Vector2(GridWidth, GridWidth);
                // Create the original output image visual
                _originalImageVisual = _compositor.CreateSpriteVisual();
                _originalImageVisual.Size = _defaultSize;
                _imageSurface = await _generator.CreateImageSurfaceAsync(new Uri("ms-appx:///Assets/Images/cat.png"), _defaultSize.ToSize(), ImageSurfaceOptions.Default);
                _originalImageVisual.Brush = _compositor.CreateSurfaceBrush(_imageSurface);
                ElementCompositionPreview.SetElementChildVisual(OriginalOutputGrid, _originalImageVisual);

                // Create the source image visual
                _sourceImageVisual = _compositor.CreateSpriteVisual();
                _sourceImageVisual.Size = _defaultSize;
                ElementCompositionPreview.SetElementChildVisual(ImageGrid, _sourceImageVisual);

                //Create the mask visual
                _maskVisual = _compositor.CreateSpriteVisual();
                _maskVisual.Size = _defaultSize;
                ElementCompositionPreview.SetElementChildVisual(MaskGrid, _maskVisual);

                // Create the output visual
                _outputVisual = _compositor.CreateSpriteVisual();
                _outputVisual.Size = _defaultSize;
                ElementCompositionPreview.SetElementChildVisual(OutputGrid, _outputVisual);
            }

            var selIndex = ImageList.SelectedIndex;
            if (selIndex == -1)
            {
                return;
            }

            var uri = _images.Values.ElementAt(selIndex);
            if (uri == null)
            {
                return;
            }

            var offset = OffsetSlider.Value.ToSingle();

            var padding = new Thickness(offset);
            if (recreateSurface)
            {
                _sourceImageSurface =
                    await _generator.CreateImageSurfaceAsync(uri, _defaultSize.ToSize(), ImageSurfaceOptions.Default);
                _imageMaskSurface = _generator.CreateImageMaskSurface(_sourceImageSurface, _defaultSize.ToSize(),
                    padding, ImageSurfaceOptions.GetDefaultImageMaskOptionsForBlur(_blurRadius));
            }
            else
            {
                _imageMaskSurface.Resize(_defaultSize.ToSize(), padding, ImageSurfaceOptions.GetDefaultImageMaskOptionsForBlur(_blurRadius));
            }

            _sourceImageVisual.Brush = _compositor.CreateSurfaceBrush(_sourceImageSurface);
            _maskVisual.Brush = _compositor.CreateSurfaceBrush(_imageMaskSurface);
            var maskedBrush = _compositor.CreateMaskBrush();
            maskedBrush.Source = _originalImageVisual.Brush;
            maskedBrush.Mask = _compositor.CreateSurfaceBrush(_imageMaskSurface);

            _outputVisual.Brush = maskedBrush;
        }
    }
}
