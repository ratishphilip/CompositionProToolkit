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
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageSurfacePage : Page
    {
        private List<String> _images;
        private Dictionary<int, Stretch> _stretchModes;
        private Dictionary<int, AlignmentX> _alignXModes;
        private Dictionary<int, AlignmentY> _alignYModes;
        private List<Uri> _uris;

        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private IImageSurface _imageSurface;
        private ImageSurfaceOptions _imageOptions;
        private SpriteVisual _imageVisual;

        public ImageSurfacePage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;

            _images = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                _images.Add($"Image{i + 1}");
            }
            ImageCB.ItemsSource = _images;

            _stretchModes = new Dictionary<int, Stretch>()
            {
                [0] = Stretch.Uniform,
                [1] = Stretch.Fill,
                [2] = Stretch.UniformToFill,
                [3] = Stretch.None
            };
            StretchCB.ItemsSource = new List<String>()
            {
                "Uniform",
                "Fill",
                "UniformToFill",
                "None"
            };

            _alignXModes = new Dictionary<int, AlignmentX>()
            {
                [0] = AlignmentX.Left,
                [1] = AlignmentX.Center,
                [2] = AlignmentX.Right
            };
            AlignXCB.ItemsSource = new List<String>()
            {
                "Left",
                "Center",
                "Right"
            };

            _alignYModes = new Dictionary<int, AlignmentY>()
            {
                [0] = AlignmentY.Top,
                [1] = AlignmentY.Center,
                [2] = AlignmentY.Bottom
            };
            AlignYCB.ItemsSource = new List<String>()
            {
                "Top",
                "Center",
                "Bottom"
            };

            ImageCB.SelectedIndex = -1;
            StretchCB.SelectedIndex = -1;
            AlignXCB.SelectedIndex = -1;
            AlignYCB.SelectedIndex = -1;

            _uris = new List<Uri>()
            {
                new Uri("ms-appx:///Assets/Images/Image7.jpg"),
                new Uri("ms-appx:///Assets/Images/Image8.jpg"),
                new Uri("ms-appx:///Assets/Images/Image9.jpg"),
                new Uri("ms-appx:///Assets/Images/RainbowRose.png"),
                new Uri("ms-appx:///Assets/Images/seascape.jpg"),
                new Uri("ms-appx:///Assets/Images/Autumn.jpg")
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = Window.Current.Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            _imageVisual = _compositor.CreateSpriteVisual();
            _imageVisual.Size = new Vector2(RenderGrid.Width.ToSingle(), RenderGrid.Height.ToSingle());
            _imageOptions = ImageSurfaceOptions.DefaultOptimized;
            _imageOptions.SurfaceBackgroundColor = Colors.Black;
            _imageSurface = _generator.CreateImageSurface(null, _imageVisual.Size.ToSize(), _imageOptions);
            _imageVisual.Brush = _compositor.CreateSurfaceBrush(_imageSurface);
            ElementCompositionPreview.SetElementChildVisual(RenderGrid, _imageVisual);
            
            ImageCB.SelectedIndex = 0;
            StretchCB.SelectedIndex = 0;
            AlignXCB.SelectedIndex = 1;
            AlignYCB.SelectedIndex = 1;

        }

        private void OnImageSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_compositor == null)
                return;

            var index = ImageCB.SelectedIndex;

            _imageSurface.RedrawAsync(_uris.ElementAt(index), _imageVisual.Size.ToSize(), _imageOptions);
        }

        private void OnStretchChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_compositor == null)
                return;

            _imageOptions.Stretch = _stretchModes[StretchCB.SelectedIndex];
            _imageSurface.Redraw(_imageOptions);
        }

        private void OnAlignXChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_compositor == null)
                return;

            _imageOptions.HorizontalAlignment = _alignXModes[AlignXCB.SelectedIndex];
            _imageSurface.Redraw(_imageOptions);
        }

        private void OnAlignYChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_compositor == null)
                return;

            _imageOptions.VerticalAlignment = _alignYModes[AlignYCB.SelectedIndex];
            _imageSurface.Redraw(_imageOptions);
        }
    }
}
