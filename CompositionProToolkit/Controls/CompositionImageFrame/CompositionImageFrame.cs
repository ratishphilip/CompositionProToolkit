// Copyright (c) 2016 Ratish Philip 
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
// CompositionProToolkit v0.4.2
// 

using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using CompositionExpressionToolkit;
using CompositionProToolkit.Common;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace CompositionProToolkit.Controls
{
    /// <summary>
    /// Event Arguments for the ImageOpened and ImageFailed events
    /// </summary>
    public class CompositionImageEventArgs : RoutedEventArgs
    {
        #region Properties

        /// <summary>
        /// The Uri of the image
        /// </summary>
        public Uri Source { get; private set; }

        public string Message { get; private set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Uri of the image</param>
        /// <param name="message">Message</param>
        public CompositionImageEventArgs(Uri source, string message)
        {
            Source = source;
            Message = message;
        }

        #endregion
    }

    /// <summary>
    /// Control which can be used for displaying images
    /// </summary>
    public sealed class CompositionImageFrame : Control
    {
        #region Events and Delegates

        /// <summary>
        /// Fired when Image has been loaded successfully
        /// </summary>
        public event RoutedEventHandler ImageOpened;
        /// <summary>
        /// Fired when loading of the Image failed
        /// </summary>
        public event RoutedEventHandler ImageFailed;

        #endregion

        #region Constants

        public static TimeSpan MinimumTransitionDuration = TimeSpan.FromMilliseconds(1);
        public static TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(700);

        #endregion

        #region Fields

        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private ICompositionSurfaceImage _surfaceImage;
        private ICompositionSurfaceImage _nextSurfaceImage;
        private ICompositionMask _frameLayerMask;
        private ContainerVisual _rootContainer;
        private SpriteVisual _shadowVisual;
        private LayerVisual _frameLayer;
        private SpriteVisual _frameBackgroundVisual;
        private SpriteVisual _frameContentVisual;
        private SpriteVisual _nextVisualContent;
        private DropShadow _shadow;
        private CompositionEffectBrush _layerEffectBrush;
        private CompositionSurfaceImageOptions _imageOptions;
        private ScalarKeyFrameAnimation _fadeOutAnimation;
        private ScalarKeyFrameAnimation _fadeInAnimation;
        private ColorKeyFrameAnimation _colorAnimation;

        #endregion

        #region Dependency Properties

        #region AlignX

        /// <summary>
        /// AlignX Dependency Property
        /// </summary>
        public static readonly DependencyProperty AlignXProperty =
            DependencyProperty.Register("AlignX", typeof(AlignmentX), typeof(CompositionImageFrame),
                new PropertyMetadata(AlignmentX.Center, OnAlignXChanged));

        /// <summary>
        /// Gets or sets the AlignX property. This dependency property 
        /// indicates how the image is positioned horizontally in the CompositionImageFrame.
        /// </summary>
        public AlignmentX AlignX
        {
            get { return (AlignmentX)GetValue(AlignXProperty); }
            set { SetValue(AlignXProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AlignX property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnAlignXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnAlignXChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the AlignX property.
        /// </summary>
        private void OnAlignXChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region AlignY

        /// <summary>
        /// AlignY Dependency Property
        /// </summary>
        public static readonly DependencyProperty AlignYProperty =
            DependencyProperty.Register("AlignY", typeof(AlignmentY), typeof(CompositionImageFrame),
                new PropertyMetadata(AlignmentY.Center, OnAlignYChanged));

        /// <summary>
        /// Gets or sets the AlignY property. This dependency property 
        /// indicates how the image is positioned vertically in the CompositionImageFrame.
        /// </summary>
        public AlignmentY AlignY
        {
            get { return (AlignmentY)GetValue(AlignYProperty); }
            set { SetValue(AlignYProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AlignY property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnAlignYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnAlignYChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the AlignY property.
        /// </summary>
        private void OnAlignYChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// CornerRadius Dependency Property
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CompositionImageFrame),
                new PropertyMetadata(new CornerRadius(0.0), OnCornerRadiusChanged));

        /// <summary>
        /// Gets or sets the CornerRadius property. This dependency property 
        /// indicates the corner radius of the the ImageFrame. The image will
        /// be rendered with rounded corners.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CornerRadius property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnCornerRadiusChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the CornerRadius property.
        /// </summary>
        private void OnCornerRadiusChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region DisplayShadow

        /// <summary>
        /// DisplayShadow Dependency Property
        /// </summary>
        public static readonly DependencyProperty DisplayShadowProperty =
            DependencyProperty.Register("DisplayShadow", typeof(bool), typeof(CompositionImageFrame),
                new PropertyMetadata(false, OnDisplayShadowChanged));

        /// <summary>
        /// Gets or sets the DisplayShadow property. This dependency property 
        /// indicates whether the shadow for this image should be displayed.
        /// </summary>
        public bool DisplayShadow
        {
            get { return (bool)GetValue(DisplayShadowProperty); }
            set { SetValue(DisplayShadowProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DisplayShadow property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDisplayShadowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnDisplayShadowChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the DisplayShadow property.
        /// </summary>
        private void OnDisplayShadowChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region FrameBackground

        /// <summary>
        /// FrameBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty FrameBackgroundProperty =
            DependencyProperty.Register("FrameBackground", typeof(Color), typeof(CompositionImageFrame),
                new PropertyMetadata(Colors.Black, OnFrameBackgroundChanged));

        /// <summary>
        /// Gets or sets the FrameBackground property. This dependency property 
        /// indicates the background color of the CompositionImageFrame to fill the area 
        /// where image is not rendered.
        /// </summary>
        public Color FrameBackground
        {
            get { return (Color)GetValue(FrameBackgroundProperty); }
            set { SetValue(FrameBackgroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the FrameBackground property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnFrameBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnFrameBackgroundChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the FrameBackground property.
        /// </summary>
        private void OnFrameBackgroundChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region Interpolation

        /// <summary>
        /// Interpolation Dependency Property
        /// </summary>
        public static readonly DependencyProperty InterpolationProperty =
            DependencyProperty.Register("Interpolation", typeof(CanvasImageInterpolation), typeof(CompositionImageFrame),
                new PropertyMetadata(CanvasImageInterpolation.HighQualityCubic, OnInterpolationChanged));

        /// <summary>
        /// Gets or sets the Interpolation property. This dependency property 
        /// indicates the interpolation used for rendering the image.
        /// </summary>
        public CanvasImageInterpolation Interpolation
        {
            get { return (CanvasImageInterpolation)GetValue(InterpolationProperty); }
            set { SetValue(InterpolationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Interpolation property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnInterpolationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnInterpolationChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the Interpolation property.
        /// </summary>
        private void OnInterpolationChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region RenderOptimized

        /// <summary>
        /// RenderOptimized Dependency Property
        /// </summary>
        public static readonly DependencyProperty RenderOptimizedProperty =
            DependencyProperty.Register("RenderOptimized", typeof(bool), typeof(CompositionImageFrame),
                new PropertyMetadata(false, OnRenderOptimizedChanged));

        /// <summary>
        /// Gets or sets the RenderOptimized property. This dependency property 
        /// indicates whether optimization must be used to render the image.
        /// Set this property to True if the CompositionImageFrame is very small
        /// compared to the actual image size. This will optimize memory usage.
        /// </summary>
        public bool RenderOptimized
        {
            get { return (bool)GetValue(RenderOptimizedProperty); }
            set { SetValue(RenderOptimizedProperty, value); }
        }

        /// <summary>
        /// Handles changes to the RenderOptimized property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnRenderOptimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnRenderOptimizedChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the RenderOptimized property.
        /// </summary>
        private void OnRenderOptimizedChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region ShadowBlurRadius

        /// <summary>
        /// ShadowBlurRadius Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShadowBlurRadiusProperty =
            DependencyProperty.Register("ShadowBlurRadius", typeof(double), typeof(CompositionImageFrame),
                new PropertyMetadata(0.0, OnShadowBlurRadiusChanged));

        /// <summary>
        /// Gets or sets the ShadowBlurRadius property. This dependency property 
        /// indicates the blur radius of the CompositionImageFrame shadow.
        /// </summary>
        public double ShadowBlurRadius
        {
            get { return (double)GetValue(ShadowBlurRadiusProperty); }
            set { SetValue(ShadowBlurRadiusProperty, CoerceShadowBlurRadius(value)); }
        }

        /// <summary>
        /// Coerces the ShadowBlurRadius to a more acceptable value
        /// </summary>
        /// <param name="newShadowBlurRadius">ShadowBlurRadius value to be coerced.</param>
        /// <returns>Coerced ShadowBlurRadius value</returns>
        private object CoerceShadowBlurRadius(double newShadowBlurRadius)
        {
            return Math.Max(0.0, newShadowBlurRadius);
        }

        /// <summary>
        /// Handles changes to the ShadowBlurRadius property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnShadowBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnShadowBlurRadiusChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the ShadowBlurRadius property.
        /// </summary>
        private void OnShadowBlurRadiusChanged()
        {
            // Refresh Layout if shadow is displayed
            if (DisplayShadow)
            {
                InvalidateArrange();
            }
        }

        #endregion

        #region ShadowColor

        /// <summary>
        /// ShadowColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShadowColorProperty =
            DependencyProperty.Register("ShadowColor", typeof(Color), typeof(CompositionImageFrame),
                new PropertyMetadata(Colors.Transparent, OnShadowColorChanged));

        /// <summary>
        /// Gets or sets the ShadowColor property. This dependency property 
        /// indicates the color of the CompositionImageFrame shadow.
        /// </summary>
        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ShadowColor property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnShadowColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnShadowColorChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the ShadowColor property.
        /// </summary>
        private void OnShadowColorChanged()
        {
            // Refresh Layout if shadow is displayed
            if (DisplayShadow)
            {
                InvalidateArrange();
            }
        }

        #endregion

        #region ShadowOffsetX

        /// <summary>
        /// ShadowOffsetX Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShadowOffsetXProperty =
            DependencyProperty.Register("ShadowOffsetX", typeof(double), typeof(CompositionImageFrame),
                new PropertyMetadata(0.0, OnShadowOffsetXChanged));

        /// <summary>
        /// Gets or sets the ShadowOffsetX property. This dependency property 
        /// indicates the horizontal offset of the CompositionImageFrame shadow.
        /// </summary>
        public double ShadowOffsetX
        {
            get { return (double)GetValue(ShadowOffsetXProperty); }
            set { SetValue(ShadowOffsetXProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ShadowOffsetX property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnShadowOffsetXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnShadowOffsetXChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the ShadowOffsetX property.
        /// </summary>
        private void OnShadowOffsetXChanged()
        {
            // Refresh Layout if shadow is displayed
            if (DisplayShadow)
            {
                InvalidateArrange();
            }
        }

        #endregion

        #region ShadowOffsetY

        /// <summary>
        /// ShadowOffsetY Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShadowOffsetYProperty =
            DependencyProperty.Register("ShadowOffsetY", typeof(double), typeof(CompositionImageFrame),
                new PropertyMetadata(0.0, OnShadowOffsetYChanged));

        /// <summary>
        /// Gets or sets the ShadowOffsetY property. This dependency property 
        /// indicates the vertical offset of the CompositionImageFrame shadow.
        /// </summary>
        public double ShadowOffsetY
        {
            get { return (double)GetValue(ShadowOffsetYProperty); }
            set { SetValue(ShadowOffsetYProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ShadowOffsetY property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnShadowOffsetYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnShadowOffsetYChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the ShadowOffsetY property.
        /// </summary>
        private void OnShadowOffsetYChanged()
        {
            // Refresh Layout if shadow is displayed
            if (DisplayShadow)
            {
                InvalidateArrange();
            }
        }

        #endregion

        #region ShadowOpacity

        /// <summary>
        /// ShadowOpacity Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShadowOpacityProperty =
            DependencyProperty.Register("ShadowOpacity", typeof(double), typeof(CompositionImageFrame),
                new PropertyMetadata(1.0, OnShadowOpacityChanged));

        /// <summary>
        /// Gets or sets the ShadowOpacity property. This dependency property 
        /// indicates the opacity of the CompositionImageFrame shadow.
        /// </summary>
        public double ShadowOpacity
        {
            get { return (double)GetValue(ShadowOpacityProperty); }
            set { SetValue(ShadowOpacityProperty, CoerceShadowOpacity(value)); }
        }

        /// <summary>
        /// Coerces the ShadowOpacity to a more acceptable value.
        /// </summary>
        /// <param name="newShadowOpacity">ShadowOpacity value to be coerced.</param>
        /// <returns>Coerced ShadowOpacity value</returns>
        private object CoerceShadowOpacity(double newShadowOpacity)
        {
            // Opacity value should be between 0 and 1 inclusive
            var opacity = Math.Max(0, newShadowOpacity);
            return Math.Min(1.0, opacity);
        }

        /// <summary>
        /// Handles changes to the ShadowOpacity property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnShadowOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnShadowOpacityChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the ShadowOpacity property.
        /// </summary>
        private void OnShadowOpacityChanged()
        {
            // Refresh Layout if shadow is displayed
            if (DisplayShadow)
            {
                InvalidateArrange();
            }
        }

        #endregion
        
        #region Source

        /// <summary>
        /// Source Dependency Property
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(CompositionImageFrame),
                new PropertyMetadata(null, OnSourceChanged));

        /// <summary>
        /// Gets or sets the Source property. This dependency property 
        /// indicates the Uri of the image to be loaded into the CompositionImageFrame.
        /// </summary>
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Source property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var imageFrame = (CompositionImageFrame)d;
            imageFrame.OnSourceChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the Source property.
        /// </summary>
        private void OnSourceChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region Stretch

        /// <summary>
        /// Stretch Dependency Property
        /// </summary>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(CompositionImageFrame),
                new PropertyMetadata(Stretch.Uniform, OnStretchChanged));

        /// <summary>
        /// Gets or sets the Stretch property. This dependency property 
        /// indicates how the image is resized to fill its allocated space 
        /// in the CompositionImageFrame.
        /// </summary>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Stretch property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnStretchChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the Stretch property.
        /// </summary>
        private void OnStretchChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region TransitionDuration

        /// <summary>
        /// TransitionDuration Dependency Property
        /// </summary>
        public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register("TransitionDuration", typeof(TimeSpan), typeof(CompositionImageFrame),
                new PropertyMetadata(DefaultTransitionDuration, OnTransitionDurationChanged));

        /// <summary>
        /// Gets or sets the TransitionDuration property. This dependency property 
        /// indicates the duration of the crossfade animation for transitioning 
        /// from one image to another.
        /// </summary>
        public TimeSpan TransitionDuration
        {
            get { return (TimeSpan)GetValue(TransitionDurationProperty); }
            set { SetValue(TransitionDurationProperty, CoerceTransitionDuration(value)); }
        }

        /// <summary>
        /// Coerces the TransitionDuration to a more acceptable value
        /// </summary>
        /// <param name="duration">Transition Duration</param>
        /// <returns>Coerced Transition Duration</returns>
        private object CoerceTransitionDuration(TimeSpan duration)
        {
            return (duration.TotalMilliseconds.IsZero() || (duration.TotalMilliseconds < 0)) ? MinimumTransitionDuration : duration;
        }

        /// <summary>
        /// Handles changes to the TransitionDuration property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTransitionDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CompositionImageFrame)d;
            var newTransitionDuration = target.TransitionDuration;
            target.OnTransitionDurationChanged(newTransitionDuration);
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the TransitionDuration property.
        /// </summary>
		/// <param name="newTransitionDuration">New Value</param>
        private void OnTransitionDurationChanged(TimeSpan newTransitionDuration)
        {
            // Update the animations if they are already created
            if (_fadeOutAnimation != null)
            {
                _fadeOutAnimation.Duration = newTransitionDuration;
            }

            if (_fadeInAnimation != null)
            {
                _fadeInAnimation.Duration = newTransitionDuration;
            }

            if (_colorAnimation != null)
            {
                _colorAnimation.Duration = newTransitionDuration;
            }
        }

        #endregion

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        public CompositionImageFrame()
        {
            // Set the default Style Key
            DefaultStyleKey = typeof(CompositionImageFrame);

            // Setting the background to transparent so that it will
            // receive Pointer events
            Background = new SolidColorBrush(Colors.Transparent);
            // Initialize the ImageOptions
            _imageOptions = CompositionSurfaceImageOptions.Default;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Handles the Measure pass during Layout
        /// </summary>
        /// <param name="availableSize">Available size</param>
        /// <returns>Total Size required to accommodate the Children</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // Initialize Composition elements for the the very first time
            if (_compositor == null)
            {
                InitComposition();
            }

            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Handles the Arrange pass during Layout
        /// </summary>
        /// <param name="finalSize">Final Size of the control</param>
        /// <returns>Total size occupied by the Children</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Taking into account the BorderThickness and Padding
            var borders = BorderThickness;
            var padding = Padding;
            var corners = CornerRadius;
            var borderSize = borders.CollapseThickness();
            var paddingSize = padding.CollapseThickness();

            // Calculate the Offset for the frameVisual
            var left = (borders.Left + padding.Left).Single();
            var top = (borders.Top + padding.Top).Single();

            // Calculate the Dimensions of the frameVisual
            var width = Math.Max(0, finalSize.Width - borderSize.Width - paddingSize.Width).Single();
            var height = Math.Max(0, finalSize.Height - borderSize.Height - paddingSize.Height).Single();

            // Set the Size and Offset of visuals in the ImageFrame
            var frameSize = new Vector2(width, height);
            _rootContainer.Size = frameSize;
            _frameLayer.Size = frameSize;
            _frameBackgroundVisual.Size = frameSize;
            _frameContentVisual.Size = frameSize;
            _shadowVisual.Size = frameSize;
            _rootContainer.Offset = new Vector3(left, top, 0);

            // Update the frameLayerMask in case the CornerRadius or 
            // BorderThickness or Padding has changed
            var pathInfo = new CompositionPathInfo(corners, borders, padding, false);
            using (var geometry =
                CompositionGenerator.GenerateGeometry(_generator.Device, frameSize.ToSize(),
                    pathInfo, Vector2.Zero))
            {
                _frameLayerMask.Redraw(_frameLayer.Size.ToSize(), geometry);
            }

            // If the FrameBackground has changed since the last time it was
            // applied to the frameBackgroundVisual, then animate the brush's
            // color to the new color.
            var brush = _frameBackgroundVisual.Brush as CompositionColorBrush;
            if (brush != null)
            {
                if (!brush.Color.Equals(FrameBackground))
                {
                    _colorAnimation.InsertKeyFrame(1f, FrameBackground);
                    brush.StartAnimation("Color", _colorAnimation);
                }
            }

            // Update the imageOptions
            _imageOptions.Interpolation = Interpolation;
            _imageOptions.SurfaceBackgroundColor = Colors.Transparent;
            _imageOptions.AutoResize = !RenderOptimized;

            // If Source is valid then try loading/refreshing the surfaceImage
            if (Source != null)
            {
                // Load/Refresh the image
                LoadImage();
            }
            else
            {
                // If the frameContentVisual had any previous brush, fade it out
                if (_surfaceImage != null)
                {
                    _compositor.CreateScopedBatch(CompositionBatchTypes.Animation,
                        () =>
                        {
                            _frameContentVisual.StartAnimation("Opacity", _fadeOutAnimation);
                        },
                        () =>
                        {
                            // Make the frameVisualContent transparent
                            _frameContentVisual.Brush = _compositor.CreateColorBrush(Colors.Transparent);
                            _frameContentVisual.Opacity = 1;
                            // Dispose the surfaceImage
                            _surfaceImage.Dispose();
                            _surfaceImage = null;
                        });
                }
                else
                {
                    // Make the frameVisualContent transparent
                    _frameContentVisual.Brush = _compositor.CreateColorBrush(Colors.Transparent);
                }
            }

            // Handle shadow
            if (DisplayShadow)
            {
                if (_shadow == null)
                {
                    _shadow = _compositor.CreateDropShadow();
                }

                _shadow.BlurRadius = ShadowBlurRadius.Single();
                _shadow.Color = ShadowColor;
                _shadow.Offset = new Vector3(ShadowOffsetX.Single(), ShadowOffsetY.Single(), 0);
                _shadow.Opacity = ShadowOpacity.Single();
                _shadow.Mask = _layerEffectBrush.GetSourceParameter("mask");

                _shadowVisual.Shadow = _shadow;
            }
            else
            {
                _shadowVisual.Shadow = null;
            }

            return base.ArrangeOverride(finalSize);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Initializes the Composition elements
        /// </summary>
        private void InitComposition()
        {
            // Compositor
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            // CompositionGenerator
            _generator = CompositionGeneratorFactory.GetCompositionGenerator(_compositor);
            // Fade Out Animation
            _fadeOutAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _fadeOutAnimation.InsertKeyFrame(1f, 0);
            _fadeOutAnimation.Duration = TransitionDuration;
            // Fade In Animation
            _fadeInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _fadeInAnimation.InsertKeyFrame(1f, 1);
            _fadeInAnimation.Duration = TransitionDuration;
            // Color Animation
            _colorAnimation = _compositor.CreateColorKeyFrameAnimation();
            _colorAnimation.Duration = TransitionDuration;

            // Visuals
            _rootContainer = _compositor.CreateContainerVisual();
            _frameLayer = _compositor.CreateLayerVisual();
            _frameContentVisual = _compositor.CreateSpriteVisual();
            _frameBackgroundVisual = _compositor.CreateSpriteVisual();

            _frameLayer.Children.InsertAtBottom(_frameBackgroundVisual);
            _frameLayer.Children.InsertAtTop(_frameContentVisual);

            // Shadow visual
            _shadowVisual = _compositor.CreateSpriteVisual();

            _rootContainer.Children.InsertAtBottom(_shadowVisual);
            _rootContainer.Children.InsertAtTop(_frameLayer);

            _frameBackgroundVisual.Brush = _compositor.CreateColorBrush(FrameBackground);

            // Create the effect to create the opacity mask
            var layerEffect = new CompositeEffect
            {
                // CanvasComposite.DestinationIn - Intersection of source and mask. 
                // Equation: O = MA * S
                // where O - Output pixel, MA - Mask Alpha, S - Source pixel.
                Mode = CanvasComposite.DestinationIn,
                Sources =
                        {
                            new CompositionEffectSourceParameter("source"),
                            new CompositionEffectSourceParameter("mask")
                        }
            };

            var layerEffectFactory = _compositor.CreateEffectFactory(layerEffect);
            _layerEffectBrush = layerEffectFactory.CreateBrush();

            // The mask for the imageFrame
            _frameLayerMask = _generator.CreateMask(new Size(0, 0), null);
            _layerEffectBrush.SetSourceParameter("mask", _compositor.CreateSurfaceBrush(_frameLayerMask.Surface));
            // Apply the mask effect to the frameLayer
            _frameLayer.Effect = _layerEffectBrush;

            ElementCompositionPreview.SetElementChildVisual(this, _rootContainer);
        }

        /// <summary>
        /// Loads the image from the Uri specified in the Source property
        /// </summary>
        private async void LoadImage()
        {
            // Load the image
            await LoadImageAsync(Source, _frameLayer.Size.ToSize());
        }

        /// <summary>
        /// Loads an image asynchronously from the given uri for the given size
        /// </summary>
        /// <param name="uri">Uri of the image to load</param>
        /// <param name="size">Render size of the image</param>
        /// <returns>Task</returns>
        private async Task LoadImageAsync(Uri uri, Size size)
        {
            try
            {
                bool raiseEvent;
                // Does the CompositionImageFrame contain no previously
                // rendered image?
                if (_surfaceImage == null)
                {
                    // Since a new Uri is being loaded, ImageOpened event
                    // must be raised on successful load 
                    raiseEvent = true;
                    // Create the surfaceImage
                    _surfaceImage = await _generator.CreateSurfaceImageAsync(uri, size, _imageOptions);
                    // Set initial opacity to 0 so that the contentVisual can be faded in
                    _frameContentVisual.Opacity = 0;
                    // Apply the surfaceBrush to the visual
                    var surfaceBrush = _compositor.CreateSurfaceBrush(_surfaceImage.Surface);
                    // Update the surface brush based on the Stretch and Alignment options
                    surfaceBrush.UpdateSurfaceBrushOptions(Stretch, AlignX, AlignY);
                    _frameContentVisual.Brush = surfaceBrush;
                    // Fade in the frameVisualContent
                    _frameContentVisual.StartAnimation(() => _frameContentVisual.Opacity, _fadeInAnimation);
                }
                else
                {
                    // Check whether the Uri to load is same as the existing image's Uri
                    if (uri.IsEqualTo(_surfaceImage.Uri))
                    {
                        // Since the Uri has not changed, no need to raise the ImageOpened event
                        // Just resize the surfaceImage with the given imageOptions and
                        // update the frameContentVisual's brush
                        raiseEvent = false;
                        _surfaceImage.Resize(size, _imageOptions);
                        // Update the surface brush based on the Stretch and Alignment options
                        (_frameContentVisual.Brush as CompositionSurfaceBrush)?.UpdateSurfaceBrushOptions(Stretch, AlignX, AlignY);
                    }
                    else
                    {
                        // Since a different Uri is being loaded, then ImageOpened event
                        // must be raised on successful load
                        raiseEvent = true;

                        // Create a temporary visual which loads the new Uri
                        _nextSurfaceImage = await _generator.CreateSurfaceImageAsync(uri, size, _imageOptions);
                        _nextVisualContent = _compositor.CreateSpriteVisual();
                        _nextVisualContent.Size = _frameContentVisual.Size;
                        _nextVisualContent.Opacity = 0;
                        // Create the surface brush for the next image
                        var nextSurfaceBrush = _compositor.CreateSurfaceBrush(_nextSurfaceImage.Surface);
                        // Update the surface brush based on the Stretch and Alignment options
                        nextSurfaceBrush.UpdateSurfaceBrushOptions(Stretch, AlignX, AlignY);
                        _nextVisualContent.Brush = nextSurfaceBrush;
                        // Place it at the top of frameVisualContainer's Children
                        _frameLayer.Children.InsertAtTop(_nextVisualContent);

                        // Commence crossfade animation
                        _compositor.CreateScopedBatch(CompositionBatchTypes.Animation, () => // Action
                        {
                            _frameContentVisual.StartAnimation(() => _frameContentVisual.Opacity, _fadeOutAnimation);
                            _nextVisualContent.StartAnimation(() => _nextVisualContent.Opacity, _fadeInAnimation);
                        }, () => // PostAction
                        {
                            // Now that the crossfade animation has ended, apply the new brush 
                            // to the  frameVisualContent
                            _frameContentVisual.Brush = _nextVisualContent.Brush;
                            // Discard the old surface image
                            _surfaceImage.Dispose();
                            _surfaceImage = _nextSurfaceImage;
                            // Make the frameVisualContent visible again
                            _frameContentVisual.Opacity = 1;
                            // Remove and dispose the temporary visual
                            _frameLayer.Children.Remove(_nextVisualContent);
                            _nextVisualContent.Dispose();
                            _nextSurfaceImage = null;
                        });
                    }
                }

                if (raiseEvent)
                {
                    // Notify to subscribers that the image has been successfully loaded
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, 
                        () => { ImageOpened?.Invoke(this, new CompositionImageEventArgs(_surfaceImage.Uri, string.Empty)); });
                }
            }
            catch (IOException ex)
            {
                // Notify to subscribers that loading of the image failed
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, 
                    () => { ImageFailed?.Invoke(this, new CompositionImageEventArgs(uri, ex.ToString())); });
            }
        }

        #endregion
    }
}
