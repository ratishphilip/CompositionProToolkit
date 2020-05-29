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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using CompositionProToolkit;
using CompositionProToolkit.Expressions;
using CompositionProToolkit.Expressions.Templates;
using Microsoft.Graphics.Canvas.Geometry;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompositionGeometricClipPage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private SpriteVisual _dragVisual;
        private CompositionPropertySet _pointerTrackerSet;
        private SpriteVisual _rootVisual;
        private List<SpriteVisual> _visuals;
        private KeyFrameAnimation<Vector3> _pointerTrackerAnimation;

        private const float DefaultLerpAmount = 0.9f;
        private readonly TimeSpan ChildOffsetAnimationDuration = TimeSpan.FromSeconds(0.07f);
        private readonly TimeSpan PointerTrackerAnimationDuration = TimeSpan.FromSeconds(1f);

        public CompositionGeometricClipPage()
        {
            this.InitializeComponent();
            Loaded += OnPageLoaded;
            _visuals = new List<SpriteVisual>();
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = Window.Current.Compositor;
            _generator = _compositor.CreateCompositionGenerator();
            var gridSize = new Vector2((float)RootGrid.ActualWidth, (float)RootGrid.ActualHeight);
            var anim = _compositor.CreatePathKeyFrameAnimation();
            _rootVisual = _compositor.CreateSpriteVisual();
            _rootVisual.Size = gridSize;

            // Create the surface brush from the image 
            var imageSurface = await _generator.CreateImageSurfaceAsync(
                new Uri("ms-appx:///Assets/Images/Cat.png"),
                new Size(400, 400),
                ImageSurfaceOptions.Default);
            var imageBrush = _compositor.CreateSurfaceBrush(imageSurface);

            // Create the clipped visuals
            for (var i = 0; i < 145; i++)
            {
                var visual = _compositor.CreateSpriteVisual();
                visual.Offset = new Vector3(400, 400, 0);
                visual.Size = new Vector2(400, 400);
                visual.Brush = imageBrush;
                visual.AnchorPoint = new Vector2(0.5f);
                var radius = 290 - (i * 2);
                // Create the GeometricClip for this visual
                var clipGeometry = CanvasGeometry.CreateCircle(null, new Vector2(200, 200), radius);
                visual.Clip = _compositor.CreateGeometricClip(clipGeometry);

                _rootVisual.Children.InsertAtTop(visual);
                _visuals.Add(visual);
            }

            // Display the rootVisual
            ElementCompositionPreview.SetElementChildVisual(RootGrid, _rootVisual);

            // Reverse the visuals list so that the items in the list are now sorted
            // in z-order from top to bottom
            _visuals.Reverse();
            // The topmost visual would track the pointer position
            _dragVisual = _visuals.First();

            // Get the CompositionPropertySet which tracks the pointer position on the RootGrid
            _pointerTrackerSet = ElementCompositionPreview.GetPointerPositionPropertySet(RootGrid);
            // Animate the topmost visual so that it tracks and follows the pointer position
            _pointerTrackerAnimation = _compositor.GenerateVector3KeyFrameAnimation()
                .HavingDuration(PointerTrackerAnimationDuration)
                .RepeatsForever();

            _pointerTrackerAnimation.InsertExpressionKeyFrame(0f, c => new VisualTarget().Offset);
            _pointerTrackerAnimation.InsertExpressionKeyFrame(
                1f,
                c => c.Lerp(new VisualTarget().Offset, _pointerTrackerSet.Get<Vector3>("Position"), DefaultLerpAmount),
                _compositor.CreateEaseOutQuinticEasingFunction());

            // Animate the remaining visuals in such a way that each visual tracks and follows the
            // position of the visual above it.
            var prevChild = _dragVisual;
            foreach (var child in _visuals.Skip(1))
            {
                var offsetAnimation = _compositor.GenerateVector3KeyFrameAnimation()
                                                    .HavingDuration(ChildOffsetAnimationDuration)
                                                    .RepeatsForever();

                offsetAnimation.InsertExpressionKeyFrame(0f, c => new VisualTarget().Offset);
                offsetAnimation.InsertExpressionKeyFrame(
                    1f,
                    c => c.Lerp(new VisualTarget().Offset, prevChild.Offset, DefaultLerpAmount),
                    _compositor.CreateEaseOutQuinticEasingFunction());

                child.StartAnimation(() => child.Offset, offsetAnimation);

                prevChild = child;
            }
        }

        /// <summary>
        /// Starts the animation to follow the pointer position
        /// </summary>
        /// <param name="sender">Pointer</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _dragVisual.StartAnimation(() => _dragVisual.Offset, _pointerTrackerAnimation);
        }

        /// <summary>
        /// Starts the animation to follow the pointer position
        /// </summary>
        /// <param name="sender">Pointer</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _dragVisual.StopAnimation(() => _dragVisual.Offset);
        }
    }
}
