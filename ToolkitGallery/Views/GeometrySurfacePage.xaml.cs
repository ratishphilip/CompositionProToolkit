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
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
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
    public sealed partial class GeometrySurfacePage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private IGeometrySurface _geometrySurface;
        private IGeometrySurface _animatedGeometrySurface;
        private CanvasGeometry _animatedGeometry;
        private CanvasGeometry _geometry;
        private float _angle;
        private float _width;
        private float _height;

        private SpriteVisual _visual1;
        private SpriteVisual _visual2;

        public GeometrySurfacePage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitComposition();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_visual1 != null)
            {
                _visual1.Offset = new Vector3(((RenderGrid.ActualWidth - _width) / 2).ToSingle(),
                    ((RenderGrid.ActualHeight - _height) / 2).ToSingle(), 0);
            }
            
            if (_visual2 != null)
            {
                _visual2.Offset = new Vector3(((AnimatedGrid.ActualWidth - _width) / 2).ToSingle(),
                    ((AnimatedGrid.ActualHeight - _height) / 2).ToSingle(), 0);
            }
        }

        private void InitComposition()
        {
            _compositor = Window.Current.Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            _angle = 0f;
            _width = 300f;
            _height = 300f;
            // Create the combined geometry
            var ellipse1 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.45f * _width, 0.225f * _height);
            var ellipse2 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.225f * _width, 0.45f * _height);
            _geometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

            _visual1 = _compositor.CreateSpriteVisual();
            _visual1.Size = new Vector2(_width, _height);
            _visual1.Offset = new Vector3(((RenderGrid.ActualWidth - _width) / 2).ToSingle(),
                                          ((RenderGrid.ActualHeight - _height) / 2).ToSingle(), 0);

            _geometrySurface = _generator.CreateGeometrySurface(_visual1.Size.ToSize(), _geometry, Color.FromArgb(255, 192, 0, 0));
            _visual1.Brush = _compositor.CreateSurfaceBrush(_geometrySurface);

            ElementCompositionPreview.SetElementChildVisual(RenderGrid, _visual1);

            _visual2 = _compositor.CreateSpriteVisual();
            _visual2.Size = new Vector2(_width, _height);
            _visual2.Offset = new Vector3(((AnimatedGrid.ActualWidth - _width) / 2).ToSingle(),
                ((AnimatedGrid.ActualHeight - _height) / 2).ToSingle(), 0);

            var ellipse3 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.45f * _width, 0.225f * _height);
            var ellipse4 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.225f * _width, 0.45f * _height);
            _animatedGeometry = ellipse3.CombineWith(ellipse4, Matrix3x2.Identity, CanvasGeometryCombine.Xor);

            _animatedGeometrySurface = _generator.CreateGeometrySurface(_visual1.Size.ToSize(), _animatedGeometry, Color.FromArgb(255, 192, 0, 0));
            _visual2.Brush = _compositor.CreateSurfaceBrush(_animatedGeometrySurface);

            ElementCompositionPreview.SetElementChildVisual(AnimatedGrid, _visual2);
        }

        private void OnRotationChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_geometrySurface == null)
                return;

            try
            {
                var angle = e.NewValue.ToSingle() * Scalar.DegreesToRadians;
                _geometrySurface.Redraw(_geometry.Transform(Matrix3x2.CreateRotation(angle, new Vector2(_width / 2f, _height / 2f))));
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        private void AnimatedCanvasCtrl_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            if (_animatedGeometrySurface == null)
                return;

            try
            {
                _angle = (float)((_angle + 1) % 360);
                var angle = _angle * Scalar.DegreesToRadians;
                _animatedGeometrySurface.Redraw(_animatedGeometry.Transform(Matrix3x2.CreateRotation(angle, new Vector2(_width / 2f, _height / 2f))));

                //_geometrySurface.Redraw(_geometry.Transform(Matrix3x2.CreateRotation(angle / 2f, new Vector2(_width / 2f, _height / 2f))));
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
    }
}
