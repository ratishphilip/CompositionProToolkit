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
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Colors = Windows.UI.Colors;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MaskedBackdropPage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private IMaskSurface _animatedCompositionMask;
        private CompositionBackdropBrush _backdropBrush;
        private CanvasGeometry _outerGeometry;
        private CanvasGeometry _combinedGeometry;
        private float _angle;
        private float _width;
        private float _height;

        private SpriteVisual _visual1;
        private SpriteVisual _bgVisual1;
        private SpriteVisual _bgVisual2;
        private SpriteVisual _animatedVisual;

        private const float Factor = 0.6f;

        public MaskedBackdropPage()
        {
            this.InitializeComponent();
            _angle = 0f;
            this.Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _generator = _compositor.CreateCompositionGenerator();
            _backdropBrush = _compositor.CreateBackdropBrush();

            _width = 300f;
            _height = 300f;
            // Create the combined geometry
            var ellipse1 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.45f * _width, 0.225f * _height);
            var ellipse2 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.225f * _width, 0.45f * _height);
            _combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

            // Create the container to host the visuals
            var container1 = _compositor.CreateContainerVisual();
            container1.Size = new Vector2(CompositionGrid1.ActualWidth.ToSingle(), CompositionGrid1.ActualHeight.ToSingle());

            var parentVisual = _compositor.CreateSpriteVisual();
            parentVisual.Size = new Vector2(_width, _height);
            parentVisual.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).ToSingle(),
                ((CompositionGrid1.ActualHeight - _height) / 2).ToSingle(), 0);

            // Background Visual
            _bgVisual1 = _compositor.CreateSpriteVisual();
            _bgVisual1.Size = new Vector2(_width * 0.5f, _height * 0.5f);
            _bgVisual1.Offset = new Vector3(((parentVisual.Size.X - _width * 0.5f) / 2f),
                                         ((parentVisual.Size.Y - _height * 0.5f) / 2f), 0f);

            //_bgVisual1.Brush = _compositor.CreateColorBrush(Colors.DarkOrange);

            // Create the CompositionMask
            var gaussianMask = _generator.CreateGaussianMaskSurface();
            // Create SurfaceBrush from CompositionMask
            var mask = _compositor.CreateSurfaceBrush(gaussianMask.Surface);
            //mask = ((CompositionBrush)_compositor.CreateGaussianMaskedBackdropBrush(gaussianMask, Colors.AntiqueWhite, 10, _backdropBrush));
            var source = _compositor.CreateColorBrush(Colors.DarkOrange);
            // Create mask brush
            var maskBrush = _compositor.CreateMaskBrush();
            maskBrush.Mask = mask;
            maskBrush.Source = source;
            _bgVisual1.Brush = maskBrush;

            parentVisual.Children.InsertAtBottom(_bgVisual1);

            // Create custom shaped visual using CompositionMaskBrush
            _visual1 = _compositor.CreateSpriteVisual();
            _visual1.Size = new Vector2(_width, _height);
            //_visual1.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).ToSingle(),
            //                             ((CompositionGrid1.ActualHeight - _height) / 2).ToSingle(), 0);
            // Create the CompositionMask
            var ellipseGeometry = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.4f * _width, 0.4f * _height);
            var rectGeometry = CanvasGeometry.CreateRectangle(_generator.Device, 50, 50, _width - 50, _height - 50);
            var compositionMask = _generator.CreateGaussianMaskSurface(_visual1.Size.ToSize(), ellipseGeometry, new Vector2(20),  20);
            // Create Masked BackdropBrush from CompositionMask
            _visual1.Brush = _compositor.CreateGaussianMaskedBackdropBrush(compositionMask, Colors.Transparent, 20f, _backdropBrush);



            parentVisual.Children.InsertAtTop(_visual1);

            var visual = _compositor.CreateSpriteVisual();
            visual.Size = new Vector2(400, 400);
            visual.Offset = new Vector3(((3 * CompositionGrid1.ActualWidth / 4f) - (_width / 2)).ToSingle(),
                                         ((3 * CompositionGrid1.ActualHeight / 4f) - (_height / 2)).ToSingle(), 0);
            var roundRectGeometry = CanvasObject.CreateSquircle(_generator.Device, 0, 0, _width, _height, _width / 8f, _height / 8f);
            var maskSurface = _generator.CreateMaskSurface(visual.Size.ToSize(), roundRectGeometry);

            var frostedBrush = _compositor.CreateFrostedGlassBrush(maskSurface, Colors.DarkGray, 30f, _backdropBrush);

            var shadow = _compositor.CreateDropShadow();
            shadow.Opacity = 0.5f;
            shadow.Color = Colors.Black;
            shadow.Offset = new Vector3(10, 10, 0);
            shadow.BlurRadius = 15;
            shadow.Mask = frostedBrush.GetSourceParameter("mask");

            visual.Brush = frostedBrush;
            visual.Shadow = shadow;

            container1.Children.InsertAtTop(visual);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid1, parentVisual);

            // Initialize the visuals for the Animated Canvas
            // Create the container to host the visuals
            var container2 = _compositor.CreateContainerVisual();
            container2.Size = new Vector2(CompositionGrid2.ActualWidth.ToSingle(), CompositionGrid2.ActualHeight.ToSingle());

            // Background Visual
            _bgVisual2 = _compositor.CreateSpriteVisual();
            _bgVisual2.Size = new Vector2(_width * Factor, _height * Factor);
            _bgVisual2.Offset = new Vector3(((CompositionGrid2.ActualWidth - (_width * Factor)) / 2).ToSingle(),
                                         ((CompositionGrid2.ActualHeight - (_height * Factor)) / 2).ToSingle(), 0);

            //var radians = ((45f * Math.PI) / 180).ToSingle();
            //var bgGeometry = _combinedGeometry.Transform(Matrix3x2.CreateRotation(radians, new Vector2(_width / 2, _height / 2)));
            //var bgMask = await _generator.CreateMaskAsync(_bgVisual2.Size.ToSize(), bgGeometry, Colors.DarkRed);
            //_bgVisual2.Brush = _compositor.CreateSurfaceBrush(bgMask.Surface);
            _bgVisual2.Brush = _compositor.CreateColorBrush(Colors.LawnGreen);

            container2.Children.InsertAtBottom(_bgVisual2);

            // Animated Visual
            _animatedVisual = _compositor.CreateSpriteVisual();
            _animatedVisual.Size = new Vector2(_width, _height);
            _animatedVisual.Offset = new Vector3(((CompositionGrid2.ActualWidth - _width) / 2).ToSingle(),
                                                ((CompositionGrid2.ActualHeight - _height) / 2).ToSingle(), 0);
            // Create the Excluded geometry
            _outerGeometry = CanvasObject.CreateSquircle(_generator.Device, 0, 0, _width, _height, _width / 8f, _height / 8f);
            var excludedGeometry = _outerGeometry.CombineWith(_combinedGeometry, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);
            // Create the CompositionMask
            _animatedCompositionMask = _generator.CreateMaskSurface(_animatedVisual.Size.ToSize(), excludedGeometry);
            var animBrush = _compositor.CreateMaskedBackdropBrush(_animatedCompositionMask, Colors.AntiqueWhite, 10f, _backdropBrush);
            _animatedVisual.Brush = animBrush;

            container2.Children.InsertAtTop(_animatedVisual);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid2, container2);
        }

        private void AnimatedCanvasCtrl_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            if (_animatedCompositionMask == null)
                return;

            _angle = (float)((_angle + 1) % 360);
            var radians = (float)((_angle * Math.PI) / 180);
            // Calculate the new geometry based on the angle
            var updatedGeometry = _outerGeometry.CombineWith(_combinedGeometry,
                                                             Matrix3x2.CreateRotation(radians, new Vector2(_width / 2, _height / 2)),
                                                             CanvasGeometryCombine.Exclude);
            // Update the geometry in the Composition Mask
            _animatedCompositionMask.Redraw(updatedGeometry);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_visual1 != null)
            {
                _visual1.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).ToSingle(),
                                         ((CompositionGrid1.ActualHeight - _height) / 2).ToSingle(), 0);
            }

            if (_bgVisual1 != null)
            {
                _bgVisual1.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).ToSingle(),
                                         ((CompositionGrid1.ActualHeight - _height) / 2).ToSingle(), 0);
            }

            if (_bgVisual2 != null)
            {
                _bgVisual2.Offset = new Vector3(((CompositionGrid2.ActualWidth - (_width * Factor)) / 2).ToSingle(),
                                               ((CompositionGrid2.ActualHeight - (_height * Factor)) / 2).ToSingle(), 0);
            }

            if (_animatedVisual != null)
            {
                _animatedVisual.Offset = new Vector3(((CompositionGrid2.ActualWidth - _width) / 2).ToSingle(),
                                                     ((CompositionGrid2.ActualHeight - _height) / 2).ToSingle(), 0);
            }
        }
    }
}
