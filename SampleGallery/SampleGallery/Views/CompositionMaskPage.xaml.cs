using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CompositionExpressionToolkit;
using CompositionProToolkit;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompositionMaskPage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private ICompositionMask _animatedCompositionMask;
        private CanvasGeometry _outerGeometry;
        private CanvasGeometry _combinedGeometry;
        private float _angle;
        private float _width;
        private float _height;

        private SpriteVisual _visual1;
        private SpriteVisual _visual2;
        private SpriteVisual _bgVisual;
        private SpriteVisual _animatedVisual;

        public CompositionMaskPage()
        {
            this.InitializeComponent();
            _angle = 0f;
            this.Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _generator = CompositionGeneratorFactory.GetCompositionGenerator(_compositor);

            _width = 300f;
            _height = 300f;
            // Create the combined geometry
            var ellipse1 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.45f * _width, 0.225f * _height);
            var ellipse2 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.225f * _width, 0.45f * _height);
            _combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

            // Create custom shaped visual using CompositionMaskBrush
            _visual1 = _compositor.CreateSpriteVisual();
            _visual1.Size = new Vector2(_width, _height);
            _visual1.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).Single(), 
                                         ((CompositionGrid1.ActualHeight - _height) / 2).Single(), 0);

            var visualChild = _compositor.CreateSpriteVisual();
            visualChild.Size = new Vector2(_width * 0.75f, _height * 0.75f);
            visualChild.Offset = new Vector3(_width*0.125f, _height*0.125f, 0);

            _visual1.Children.InsertAtTop(visualChild);

            // Create the CompositionMask
            var compositionMask = _generator.CreateMask(_visual1.Size.ToSize(), _combinedGeometry);
            // Create SurfaceBrush from CompositionMask
            var mask = _compositor.CreateSurfaceBrush(compositionMask.Surface);
            var source = _compositor.CreateColorBrush(Color.FromArgb(255, 0, 173, 239));
            // Create mask brush
            var maskBrush = _compositor.CreateMaskBrush();
            maskBrush.Mask = mask;
            maskBrush.Source = source;
            _visual1.Brush = maskBrush;

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid1, _visual1);

            // Create custom shaped visual using CompositionMaskBrush
            _visual2 = _compositor.CreateSpriteVisual();
            _visual2.Size = new Vector2(_width, _height);
            _visual2.Offset = new Vector3(((CompositionGrid2.ActualWidth - _width) / 2).Single(), 
                                         ((CompositionGrid2.ActualHeight - _height) / 2).Single(), 0);

            // Create the CompositionMask filled with color
            var compositionMask2 =
                _generator.CreateMask(_visual2.Size.ToSize(), _combinedGeometry, Color.FromArgb(192, 192, 0, 0));
            // Create SurfaceBrush from CompositionMask
            var surfaceBrush = _compositor.CreateSurfaceBrush(compositionMask2.Surface);
            _visual2.Brush = surfaceBrush;

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid2, _visual2);

            // Initialize the visuals for the Animated Canvas
            // Create the container to host the visuals
            var container = _compositor.CreateContainerVisual();
            container.Size = new Vector2(CompositionGrid3.ActualWidth.Single(), CompositionGrid3.ActualHeight.Single());

            // Background Visual
            _bgVisual = _compositor.CreateSpriteVisual();
            _bgVisual.Size = new Vector2(_width, _height);
            _bgVisual.Offset = new Vector3(((CompositionGrid3.ActualWidth - _width) / 2).Single(),
                                         ((CompositionGrid3.ActualHeight - _height) / 2).Single(), 0);

            var radians = ((45f*Math.PI)/180).Single();
            var bgGeometry = _combinedGeometry.Transform(Matrix3x2.CreateRotation(radians, new Vector2(_width/2, _height/2)));
            var bgMask = _generator.CreateMask(_bgVisual.Size.ToSize(), bgGeometry, Color.FromArgb(255, 0, 173, 239));
            var bgBrush = _compositor.CreateSurfaceBrush(bgMask.Surface);
            _bgVisual.Brush = bgBrush;

            container.Children.InsertAtBottom(_bgVisual);

            // Animated Visual
            _animatedVisual = _compositor.CreateSpriteVisual();
            _animatedVisual.Size = new Vector2(_width, _height);
            _animatedVisual.Offset = new Vector3(((CompositionGrid3.ActualWidth - _width) / 2).Single(),
                                                ((CompositionGrid3.ActualHeight - _height) / 2).Single(), 0);
            // Create the Excluded geometry
            _outerGeometry = CanvasGeometry.CreateRectangle(_generator.Device, 0, 0, _width, _height);
            var excludedGeometry = _outerGeometry.CombineWith(_combinedGeometry, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);
            // Create the CompositionMask
            _animatedCompositionMask = _generator.CreateMask(_animatedVisual.Size.ToSize(), excludedGeometry, Color.FromArgb(192, 192, 0, 0));
            var animBrush = _compositor.CreateSurfaceBrush(_animatedCompositionMask.Surface);
            _animatedVisual.Brush = animBrush;

            container.Children.InsertAtTop(_animatedVisual);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid3, container);
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
                _visual1.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).Single(),
                                         ((CompositionGrid1.ActualHeight - _height) / 2).Single(), 0);
            }

            if (_visual2 != null)
            {
                _visual2.Offset = new Vector3(((CompositionGrid2.ActualWidth - _width) / 2).Single(),
                                         ((CompositionGrid2.ActualHeight - _height) / 2).Single(), 0);
            }

            if (_bgVisual != null)
            {
                _bgVisual.Offset = new Vector3(((CompositionGrid3.ActualWidth - _width) / 2).Single(),
                                         ((CompositionGrid3.ActualHeight - _height) / 2).Single(), 0);
            }

            if (_animatedVisual != null)
            {
                _animatedVisual.Offset = new Vector3(((CompositionGrid3.ActualWidth - _width) / 2).Single(),
                                                ((CompositionGrid3.ActualHeight - _height) / 2).Single(), 0);
            }
        }
    }
}
