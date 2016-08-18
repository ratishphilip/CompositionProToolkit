using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
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
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MaskedBackdropPage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private ICompositionMask _animatedCompositionMask;
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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _generator = CompositionGeneratorFactory.GetCompositionGenerator(_compositor);
            _backdropBrush = _compositor.CreateBackdropBrush();

            _width = 300f;
            _height = 300f;
            // Create the combined geometry
            var ellipse1 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.45f * _width, 0.225f * _height);
            var ellipse2 = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.225f * _width, 0.45f * _height);
            _combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

            // Create the container to host the visuals
            var container1 = _compositor.CreateContainerVisual();
            container1.Size = new Vector2(CompositionGrid1.ActualWidth.Single(), CompositionGrid1.ActualHeight.Single());

            // Background Visual
            _bgVisual1 = _compositor.CreateSpriteVisual();
            _bgVisual1.Size = new Vector2(_width * 0.45f, _height * 0.45f);
            _bgVisual1.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).Single(),
                                         ((CompositionGrid1.ActualHeight - _height) / 2).Single(), 0);

            _bgVisual1.Brush = _compositor.CreateColorBrush(Colors.DarkOrange);

            container1.Children.InsertAtBottom(_bgVisual1);

            // Create custom shaped visual using CompositionMaskBrush
            _visual1 = _compositor.CreateSpriteVisual();
            _visual1.Size = new Vector2(_width, _height);
            _visual1.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).Single(),
                                         ((CompositionGrid1.ActualHeight - _height) / 2).Single(), 0);
            // Create the CompositionMask
            var ellipseGeometry = CanvasGeometry.CreateEllipse(_generator.Device, _width / 2, _height / 2, 0.4f * _width, 0.4f * _height);
            var compositionMask = _generator.CreateMask(_visual1.Size.ToSize(), ellipseGeometry);
            // Create Masked BackdropBrush from CompositionMask
            _visual1.Brush = _compositor.CreateMaskedBackdropBrush(compositionMask, Colors.AntiqueWhite, 30f, _backdropBrush);

            container1.Children.InsertAtTop(_visual1);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid1, container1);

            // Initialize the visuals for the Animated Canvas
            // Create the container to host the visuals
            var container2 = _compositor.CreateContainerVisual();
            container2.Size = new Vector2(CompositionGrid2.ActualWidth.Single(), CompositionGrid2.ActualHeight.Single());

            // Background Visual
            _bgVisual2 = _compositor.CreateSpriteVisual();
            _bgVisual2.Size = new Vector2(_width * Factor, _height * Factor);
            _bgVisual2.Offset = new Vector3(((CompositionGrid2.ActualWidth - (_width * Factor)) / 2).Single(),
                                         ((CompositionGrid2.ActualHeight - (_height * Factor)) / 2).Single(), 0);

            //var radians = ((45f * Math.PI) / 180).Single();
            //var bgGeometry = _combinedGeometry.Transform(Matrix3x2.CreateRotation(radians, new Vector2(_width / 2, _height / 2)));
            //var bgMask = await _generator.CreateMaskAsync(_bgVisual2.Size.ToSize(), bgGeometry, Colors.DarkRed);
            //_bgVisual2.Brush = _compositor.CreateSurfaceBrush(bgMask.Surface);
            _bgVisual2.Brush = _compositor.CreateColorBrush(Colors.LawnGreen);

            container2.Children.InsertAtBottom(_bgVisual2);

            // Animated Visual
            _animatedVisual = _compositor.CreateSpriteVisual();
            _animatedVisual.Size = new Vector2(_width, _height);
            _animatedVisual.Offset = new Vector3(((CompositionGrid2.ActualWidth - _width) / 2).Single(),
                                                ((CompositionGrid2.ActualHeight - _height) / 2).Single(), 0);
            // Create the Excluded geometry
            _outerGeometry = CanvasGeometry.CreateRectangle(_generator.Device, 0, 0, _width, _height);
            var excludedGeometry = _outerGeometry.CombineWith(_combinedGeometry, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);
            // Create the CompositionMask
            _animatedCompositionMask = _generator.CreateMask(_animatedVisual.Size.ToSize(), excludedGeometry);
            var animBrush = _compositor.CreateMaskedBackdropBrush(_animatedCompositionMask, Colors.AntiqueWhite, 10f, _backdropBrush);
            _animatedVisual.Brush = animBrush;

            container2.Children.InsertAtTop(_animatedVisual);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid2, container2);
        }

        private async void AnimatedCanvasCtrl_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
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

            if (_bgVisual1 != null)
            {
                _bgVisual1.Offset = new Vector3(((CompositionGrid1.ActualWidth - _width) / 2).Single(),
                                         ((CompositionGrid1.ActualHeight - _height) / 2).Single(), 0);
            }

            if (_bgVisual2 != null)
            {
                _bgVisual2.Offset = new Vector3(((CompositionGrid2.ActualWidth - (_width * Factor)) / 2).Single(),
                                               ((CompositionGrid2.ActualHeight - (_height * Factor)) / 2).Single(), 0);
            }

            if (_animatedVisual != null)
            {
                _animatedVisual.Offset = new Vector3(((CompositionGrid2.ActualWidth - _width) / 2).Single(),
                                                     ((CompositionGrid2.ActualHeight - _height) / 2).Single(), 0);
            }
        }
    }
}
