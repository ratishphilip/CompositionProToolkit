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
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CompositionProToolkit;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CanvasGeometry _outerGeometry;
        private CanvasGeometry _innerGeometry;
        private CanvasGeometry _combinedGeometry;

        private float _angle;
        private Compositor _compositor;
        private SpriteVisual _visual;
        private SpriteVisual _animatedVisual;
        private ICompositionMaskGenerator _generator;
        private ICompositionMask _compositionMask;
        private ICompositionMask _animatedCompositionMask;
        private CompositionBackdropBrush _backdropBrush;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _generator = CompositionMaskFactory.GetCompositionMaskGenerator(_compositor);
            _backdropBrush = _compositor.CreateBackdropBrush();

            //Create the visual
            _visual = _compositor.CreateSpriteVisual();
            _visual.Size = new Vector2(400, 400);
            _visual.Offset = new Vector3(200, 0, 0);
            // Create the combined geometry
            var ellipse1 = CanvasGeometry.CreateEllipse(_generator.Device, 200, 200, 150, 75);
            var ellipse2 = CanvasGeometry.CreateEllipse(_generator.Device, 200, 200, 75, 150);
            _combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);
            // Create the CompositionMask
            _compositionMask = await _generator.CreateMaskAsync(_visual.Size.ToSize(), _combinedGeometry);
            // Create SurfaceBrush from CompositionMask
            var mask = _compositor.CreateSurfaceBrush(_compositionMask.Surface);
            var source1 = _compositor.CreateColorBrush(Colors.Blue);
            // Create mask brush
            var maskBrush = _compositor.CreateMaskBrush();
            maskBrush.Mask = mask;
            maskBrush.Source = source1;

            _visual.Brush = maskBrush;

            ElementCompositionPreview.SetElementChildVisual(CanvasCtrl, _visual);

            var container = _compositor.CreateContainerVisual();
            container.Size = new Vector2(1000, 1000);
            // Background visual
            var bgVisual = _compositor.CreateSpriteVisual();
            bgVisual.Size = new Vector2(200, 200);
            bgVisual.Offset = new Vector3(300, 100, 0);
            bgVisual.Brush = source1;
            container.Children.InsertAtTop(bgVisual);

            // Animated Visual
            _animatedVisual = _compositor.CreateSpriteVisual();
            _animatedVisual.Size = new Vector2(400, 400);
            _animatedVisual.Offset = new Vector3(200, 0, 0);
            // Create the Excluded geometry
            _outerGeometry = CanvasGeometry.CreateRectangle(_generator.Device, 0, 0, 400, 400);
            var excludedGeometry = _outerGeometry.CombineWith(_combinedGeometry, Matrix3x2.Identity,
                CanvasGeometryCombine.Exclude);
            // Create the CompositionMask
            _animatedCompositionMask = await _generator.CreateMaskAsync(_animatedVisual.Size.ToSize(), excludedGeometry);
            //// Create SurfaceBrush from CompositionMask
            //var animatedMask = _compositor.CreateSurfaceBrush(_animatedCompositionMask.Surface);
            //var source2 = _compositor.CreateColorBrush(Colors.Red);
            //// Create mask brush
            //var animatedMaskBrush = _compositor.CreateMaskBrush();
            //animatedMaskBrush.Mask = animatedMask;
            //animatedMaskBrush.Source = source2;

            _animatedVisual.Brush = _compositor.CreateMaskedBackdropBrush(_animatedCompositionMask, Color.FromArgb(240, 232,232,232), 20f, _backdropBrush);
            container.Children.InsertAtTop(_animatedVisual);

            ElementCompositionPreview.SetElementChildVisual(AnimatedCanvasCtrl, container);
        }

        private async void AnimatedCanvasCtrl_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            if (_animatedCompositionMask == null)
                return;

            _angle = (float)((_angle + 1) % 360);
            var radians = (float)((_angle * Math.PI) / 180);
            // Calculate the new geometry based on the angle
            var updatedGeometry = _outerGeometry.CombineWith(_combinedGeometry, Matrix3x2.CreateRotation(radians, new Vector2(200, 200)),
                CanvasGeometryCombine.Exclude);
            // Update the geometry in the Composition Mask
            await _animatedCompositionMask.RedrawAsync(updatedGeometry);
        }
    }
}
