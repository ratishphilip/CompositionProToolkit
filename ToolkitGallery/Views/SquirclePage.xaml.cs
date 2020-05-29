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
using CompositionProToolkit.Win2d;
using Microsoft.Graphics.Canvas.Geometry;
using System.Numerics;
using Windows.UI;
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
    public sealed partial class SquirclePage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private IGeometrySurface _roundedRectangleSurface;
        private IGeometrySurface _squircleSurface;
        private IGeometrySurface _differenceSurface;
        private IGeometrySurface _xorSurface;
        private float _radius = 0f;
        private SpriteVisual _roundedRectangleVisual;
        private SpriteVisual _squircleVisual;
        private SpriteVisual _differenceVisual;
        private SpriteVisual _xorVisual;
        private const float _width = 400f;
        private const float _height = 400f;

        public SquirclePage()
        {
            this.InitializeComponent();

            Loaded += SquirclePage_Loaded;
        }

        private void SquirclePage_Loaded(object sender, RoutedEventArgs e)
        {
            _compositor = Window.Current.Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            // Create Squircle shaped visual using CompositionSurfaceBrush
            _roundedRectangleVisual = _compositor.CreateSpriteVisual();
            _roundedRectangleVisual.Size = new Vector2(_width, _height);
            _roundedRectangleVisual.Offset = new Vector3(((RoundedRectangleGrid.ActualWidth - _width) / 2).ToSingle(),
                                          ((RoundedRectangleGrid.ActualHeight - _height) / 2).ToSingle(), 0);

            // Create the geometry surface for squircle filled with red color
            var roundedRectGeometry = CanvasGeometry.CreateRoundedRectangle(_generator.Device, 0, 0, _width, _height, _radius, _radius);

            _roundedRectangleSurface = _generator.CreateGeometrySurface(_roundedRectangleVisual.Size.ToSize(), roundedRectGeometry, Color.FromArgb(255, 192, 0, 0));
            // Create SurfaceBrush from CompositionMask
            var rectSurfaceBrush = _compositor.CreateSurfaceBrush(_roundedRectangleSurface.Surface);
            _roundedRectangleVisual.Brush = rectSurfaceBrush;

            ElementCompositionPreview.SetElementChildVisual(RoundedRectangleGrid, _roundedRectangleVisual);

            // Create Squircle shaped visual using CompositionSurfaceBrush
            _squircleVisual = _compositor.CreateSpriteVisual();
            _squircleVisual.Size = new Vector2(_width, _height);
            _squircleVisual.Offset = new Vector3(((SquircleGrid.ActualWidth - _width) / 2).ToSingle(),
                                          ((SquircleGrid.ActualHeight - _height) / 2).ToSingle(), 0);

            // Create the geometry surface for squircle filled with red color
            var squircleGeometry = CanvasObject.CreateSquircle(_generator.Device, 0, 0, _width, _height, _radius, _radius);

            _squircleSurface = _generator.CreateGeometrySurface(_squircleVisual.Size.ToSize(), squircleGeometry, Color.FromArgb(255, 192, 0, 0));
            // Create SurfaceBrush from CompositionMask
            var surfaceBrush = _compositor.CreateSurfaceBrush(_squircleSurface);
            _squircleVisual.Brush = surfaceBrush;

            ElementCompositionPreview.SetElementChildVisual(SquircleGrid, _squircleVisual);

            // Create visual showing difference between the two geometries
            _differenceVisual = _compositor.CreateSpriteVisual();
            _differenceVisual.Size = new Vector2(_width, _height);
            

            // Create the geometry surface for squircle filled with red color
            var differenceGeometry =
                roundedRectGeometry.CombineWith(squircleGeometry, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);

            _differenceSurface = _generator.CreateGeometrySurface(_differenceVisual.Size.ToSize(), differenceGeometry, Color.FromArgb(255, 192, 192, 0));
            // Create SurfaceBrush from CompositionMask
            _differenceVisual.Brush = _compositor.CreateSurfaceBrush(_differenceSurface);

            _xorVisual = _compositor.CreateSpriteVisual();
            _xorVisual.Size = _differenceVisual.Size;
            _xorVisual.Offset = new Vector3(((DifferenceGrid.ActualWidth - _width) / 2).ToSingle(),
                ((DifferenceGrid.ActualHeight - _height) / 2).ToSingle(), 0);

            var xorGeometry =
                roundedRectGeometry.CombineWith(squircleGeometry, Matrix3x2.Identity, CanvasGeometryCombine.Xor);

            _xorSurface = _generator.CreateGeometrySurface(_xorVisual.Size.ToSize(), xorGeometry, Color.FromArgb(255, 192, 0, 0));
            // Create SurfaceBrush from CompositionMask
            _xorVisual.Brush = _compositor.CreateSurfaceBrush(_xorSurface);
            _xorVisual.Children.InsertAtTop(_differenceVisual);

            ElementCompositionPreview.SetElementChildVisual(DifferenceGrid, _xorVisual);
        }

        private void OnRadiusChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _radius = (float)RadiusSlider.Value;

            if (_compositor == null)
            {
                return;
            }

            var roundRectGeometry =
                CanvasGeometry.CreateRoundedRectangle(_generator.Device, 0, 0, _width, _height, _radius, _radius);
            var squircleGeometry =
                CanvasObject.CreateSquircle(_generator.Device, 0, 0, _width, _height, _radius, _radius);
            var differenceGeometry =
                roundRectGeometry.CombineWith(squircleGeometry, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);
            var xorGeometry =
                roundRectGeometry.CombineWith(squircleGeometry, Matrix3x2.Identity, CanvasGeometryCombine.Xor); 
            
            _roundedRectangleSurface.Redraw(roundRectGeometry);
            _squircleSurface.Redraw(squircleGeometry);
            _differenceSurface.Redraw(differenceGeometry);
            _xorSurface.Redraw(xorGeometry);
        }
    }
}
