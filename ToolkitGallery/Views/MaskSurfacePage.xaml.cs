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
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MaskSurfacePage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private IMaskSurface _animatedMaskSurface;
        private CompositionBackdropBrush _backdropBrush;
        private CanvasGeometry _outerGeometry;
        private float _angle;
        private float _width;
        private float _height;
        private Vector2 _center;

        private SpriteVisual _visual1;
        private SpriteVisual _visual2;
        private CanvasGeometry _ellipse1;
        private CanvasGeometry _ellipse2;
        private ContainerVisual _container;

        public MaskSurfacePage()
        {
            InitializeComponent();
            _angle = 0f;
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _generator = _compositor.CreateCompositionGenerator();
            _backdropBrush = _compositor.CreateBackdropBrush();

            _width = 300f;
            _height = 300f;
            _center = new Vector2(150f);
            // Create the combined geometry
            _ellipse1 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.45f * _width, 0.225f * _height);
            _ellipse2 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.225f * _width, 0.45f * _height);
            var combinedGeometry = _ellipse1.CombineWith(_ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);
            _outerGeometry = CanvasObject.CreateSquircle(_generator.Device, 0, 0, _width, _height, _width, _height);

            var excludedGeometry = _outerGeometry.CombineWith(combinedGeometry, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);

            // Create custom shaped visual using CompositionMaskBrush
            _visual1 = _compositor.CreateSpriteVisual();
            _visual1.Size = new Vector2(_width, _height);
            _visual1.Offset = new Vector3(((RenderGrid1.ActualWidth - _width) / 2).ToSingle(),
                                         ((RenderGrid1.ActualHeight - _height) / 2).ToSingle(), 0);

            _animatedMaskSurface = _generator.CreateMaskSurface(_visual1.Size.ToSize(), excludedGeometry);

            // Create SurfaceBrush from CompositionMask
            var mask = _compositor.CreateSurfaceBrush(_animatedMaskSurface);
            var source = _compositor.CreateColorBrush(Color.FromArgb(255, 192, 0, 0));
            // Create mask brush
            var maskBrush = _compositor.CreateMaskBrush();
            maskBrush.Mask = mask;
            maskBrush.Source = source;
            _visual1.Brush = maskBrush;

            ElementCompositionPreview.SetElementChildVisual(RenderGrid1, _visual1);

            // Create custom shaped visual using CompositionMaskBrush
            _visual2 = _compositor.CreateSpriteVisual();
            _visual2.Size = new Vector2(_width, _height);

            _visual2.Brush =
                _compositor.CreateMaskedBackdropBrush(_animatedMaskSurface, Colors.AntiqueWhite, 10, _backdropBrush);

            _container = _compositor.CreateContainerVisual();
            _container.Size = new Vector2(_width, _height);
            _container.Offset = new Vector3(((RenderGrid2.ActualWidth - _width) / 2).ToSingle(),
                ((RenderGrid2.ActualHeight - _height) / 2).ToSingle(), 0);

            var bgVisual = _compositor.CreateSpriteVisual();
            bgVisual.Size = _container.Size * 0.6f;
            bgVisual.Brush = _compositor.CreateColorBrush(Colors.Maroon);
            bgVisual.Offset = new Vector3(_container.Size * 0.2f, 0);
            _container.Children.InsertAtTop(bgVisual);
            _container.Children.InsertAtTop(_visual2);

            ElementCompositionPreview.SetElementChildVisual(RenderGrid2, _container);
        }

        private void AnimatedCanvasCtrl_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            if (_animatedMaskSurface == null)
                return;

            _angle = (float)((_angle + 1) % 360);
            var angle = _angle * Scalar.DegreesToRadians;
            // Calculate the new geometry based on the angle
            var combinedGeometry = _ellipse1.CombineWith(_ellipse2, Matrix3x2.CreateRotation(angle, _center ), CanvasGeometryCombine.Union);
            var updatedGeometry = _outerGeometry.CombineWith(combinedGeometry,
                                                             Matrix3x2.CreateRotation(angle / 2, _center),
                                                             CanvasGeometryCombine.Xor);
            // Update the geometry in the Composition Mask
            _animatedMaskSurface.Redraw(updatedGeometry);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_visual1 != null)
            {
                _visual1.Offset = new Vector3(((RenderGrid1.ActualWidth - _width) / 2).ToSingle(),
                                         ((RenderGrid1.ActualHeight - _height) / 2).ToSingle(), 0);
            }

            if (_container != null)
            {
                _container.Offset = new Vector3(((RenderGrid2.ActualWidth - _width) / 2).ToSingle(),
                                         ((RenderGrid2.ActualHeight - _height) / 2).ToSingle(), 0);
            }
        }
    }
}
