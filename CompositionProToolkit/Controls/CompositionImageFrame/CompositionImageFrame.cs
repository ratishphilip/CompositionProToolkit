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
// CompositionProToolkit v0.4.3
// 

using System;
using System.IO;
using System.Linq;
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
using Microsoft.Graphics.Canvas.Geometry;

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
        #region Enums

        private enum ImageEngineState
        {
            Idle = 0,
            Loading = 1,
            Scheduled = 2
        }

        #endregion

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

        private static TimeSpan MinimumTransitionDuration = TimeSpan.FromMilliseconds(1);
        private static TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(700);
        private static TimeSpan AlignmentTransitionDuration = TimeSpan.FromMilliseconds(500);
        private static readonly Size PlaceholderSize = new Size(48, 48);

        #endregion

        #region Fields

        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private ICompositionSurfaceImage _surfaceImage;
        private ICompositionSurfaceImage _nextSurfaceImage;
        private ICompositionMask _frameLayerMask;
        private ICompositionMask _placeholderMask;
        private CompositionSurfaceBrush _placeholderContentBrush;
        private CompositionSurfaceBrush _nextSurfaceBrush;
        private ContainerVisual _rootContainer;
        private SpriteVisual _shadowVisual;
        private LayerVisual _frameLayer;
        private SpriteVisual _frameBackgroundVisual;
        private SpriteVisual _placeholderVisual;
        private SpriteVisual _frameContentVisual;
        private SpriteVisual _nextVisualContent;
        private DropShadow _shadow;
        private CompositionEffectBrush _layerEffectBrush;
        private CompositionSurfaceImageOptions _imageOptions;
        private ScalarKeyFrameAnimation _fadeOutAnimation;
        private ScalarKeyFrameAnimation _fadeInAnimation;
        private ColorKeyFrameAnimation _colorAnimation;
        private ScalarKeyFrameAnimation _alignXAnimation;
        private ScalarKeyFrameAnimation _alignYAnimation;

        private ImageEngineState _engineState;
        private Uri _scheduledUri;

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

        #region PlaceholderColor

        /// <summary>
        /// PlaceholderColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty PlaceholderColorProperty =
            DependencyProperty.Register("PlaceholderColor", typeof(Color), typeof(CompositionImageFrame),
                new PropertyMetadata(Color.FromArgb(255, 192, 192, 192), OnPlaceholderColorChanged));

        /// <summary>
        /// Gets or sets the PlaceholderColor property. This dependency property 
        /// indicates the color with which the rendered placeholder geometry should be filled.
        /// </summary>
        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the PlaceholderColor property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnPlaceholderColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnPlaceholderColorChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the PlaceholderColor property.
        /// </summary>
        private void OnPlaceholderColorChanged()
        {
            // Refresh Layout
            InvalidateArrange();
        }

        #endregion

        #region PlaceholderBackground

        /// <summary>
        /// PlaceholderBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty PlaceholderBackgroundProperty =
            DependencyProperty.Register("PlaceholderBackground", typeof(Color), typeof(CompositionImageFrame),
                new PropertyMetadata(Colors.Black, OnPlaceholderBackgroundChanged));

        /// <summary>
        /// Gets or sets the PlaceholderBackground property. This dependency property 
        /// indicates the background color of the Placeholder.
        /// </summary>
        public Color PlaceholderBackground
        {
            get { return (Color)GetValue(PlaceholderBackgroundProperty); }
            set { SetValue(PlaceholderBackgroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the PlaceholderBackground property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnPlaceholderBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnPlaceholderBackgroundChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the PlaceholderBackground property.
        /// </summary>
        private void OnPlaceholderBackgroundChanged()
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

        #region ShowPlaceholder

        /// <summary>
        /// ShowPlaceholder Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShowPlaceholderProperty =
            DependencyProperty.Register("ShowPlaceholder", typeof(bool), typeof(CompositionImageFrame),
                new PropertyMetadata(false, OnShowPlaceholderChanged));

        /// <summary>
        /// Gets or sets the ShowPlaceholder property. This dependency property 
        /// indicates whether the placeholder needs to be displayed during image load or
        /// when no image is loaded in the CompositionImageFrame.
        /// </summary>
        public bool ShowPlaceholder
        {
            get { return (bool)GetValue(ShowPlaceholderProperty); }
            set { SetValue(ShowPlaceholderProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ShowPlaceholder property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnShowPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnShowPlaceholderChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the ShowPlaceholder property.
        /// </summary>
        private void OnShowPlaceholderChanged()
        {
            // Refresh Layout
            InvalidateArrange();
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

        #region UseImageCache

        /// <summary>
        /// UseImageCache Dependency Property
        /// </summary>
        public static readonly DependencyProperty UseImageCacheProperty =
            DependencyProperty.Register("UseImageCache", typeof(bool), typeof(CompositionImageFrame),
                new PropertyMetadata(true, OnUseImageCacheChanged));

        /// <summary>
        /// Gets or sets the UseImageCache property. This dependency property 
        /// indicates whether the images obtained by loading the Uris should be cached
        /// for faster reload.
        /// </summary>
        public bool UseImageCache
        {
            get { return (bool)GetValue(UseImageCacheProperty); }
            set { SetValue(UseImageCacheProperty, value); }
        }

        /// <summary>
        /// Handles changes to the UseImageCache property.
        /// </summary>
        /// <param name="d">CompositionImageFrame</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnUseImageCacheChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = (CompositionImageFrame)d;
            frame.OnUseImageCacheChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the UseImageCache property.
        /// </summary>
        private void OnUseImageCacheChanged()
        {
            // Refresh Layout
            InvalidateArrange();
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
            _placeholderVisual.Size = frameSize;
            _placeholderVisual.Children.ElementAt(0).Size = frameSize;
            _shadowVisual.Size = frameSize;
            _nextVisualContent.Size = frameSize;
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

            // If the PlaceholderBackground has changed since the last time it was
            // applied to the placeholderVisual, then update the brush's
            // color to the new color.
            brush = _placeholderVisual.Brush as CompositionColorBrush;
            if (brush != null)
            {
                if (!brush.Color.Equals(PlaceholderBackground))
                {
                    brush.Color = PlaceholderBackground;
                }
            }

            // Redraw the placeholder content with the latest placeholder color
            _placeholderMask.Redraw(PlaceholderColor);

            // Set the stretch property of placeholder content's brush according to its size
            if ((width > PlaceholderSize.Width) && (height > PlaceholderSize.Height))
            {
                _placeholderContentBrush.Stretch = CompositionStretch.None;
            }
            else
            {
                _placeholderContentBrush.Stretch = CompositionStretch.Uniform;
            }

            // Update the imageOptions
            _imageOptions.Interpolation = Interpolation;
            _imageOptions.SurfaceBackgroundColor = Colors.Transparent;
            _imageOptions.AutoResize = !RenderOptimized;

            // If Source is valid then try loading/refreshing the surfaceImage
            if (Source != null)
            {
                // Load/Refresh the image
                PreloadImage();
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
                            DisplayPlaceHolder();
                            _engineState = ImageEngineState.Idle;
                        });
                }
                else
                {
                    // Make the frameVisualContent transparent
                    _frameContentVisual.Brush = _compositor.CreateColorBrush(Colors.Transparent);
                    DisplayPlaceHolder();
                    _engineState = ImageEngineState.Idle;
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
            _frameBackgroundVisual = _compositor.CreateSpriteVisual();
            _frameContentVisual = _compositor.CreateSpriteVisual();
            _placeholderVisual = _compositor.CreateSpriteVisual();
            _nextVisualContent = _compositor.CreateSpriteVisual();

            _frameLayer.Children.InsertAtBottom(_frameBackgroundVisual);
            _frameLayer.Children.InsertAtTop(_frameContentVisual);
            _frameLayer.Children.InsertAtTop(_placeholderVisual);
            _frameLayer.Children.InsertAtTop(_nextVisualContent);

            // Placeholder content
            _placeholderMask = _generator.CreateMask(PlaceholderSize, GetPlaceHolderGeometry(), PlaceholderColor);
            _placeholderContentBrush = _compositor.CreateSurfaceBrush(_placeholderMask.Surface);
            _placeholderVisual.Brush = _compositor.CreateColorBrush(PlaceholderBackground);
            var placeholderContent = _compositor.CreateSpriteVisual();
            placeholderContent.Brush = _placeholderContentBrush;
            _placeholderVisual.Children.InsertAtTop(placeholderContent);

            // By default placeholder visual will not be visible
            _placeholderVisual.Opacity = 0;

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

            // Alignment animations
            _alignXAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _alignXAnimation.Duration = AlignmentTransitionDuration;

            _alignYAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _alignYAnimation.Duration = AlignmentTransitionDuration;

            ElementCompositionPreview.SetElementChildVisual(this, _rootContainer);
        }

        /// <summary>
        /// Creates the geometry for the placeholder
        /// </summary>
        /// <param name="progress">Progress percentage</param>
        /// <returns>CanvasGeometry</returns>
        private CanvasGeometry GetPlaceHolderGeometry(int progress = -1)
        {
            using (var pathBuilder = new CanvasPathBuilder(_generator.Device))
            using (var pathBuilder2 = new CanvasPathBuilder(_generator.Device))
            {
                pathBuilder.BeginFigure(33.690f, 18.000f);
                pathBuilder.AddLine(30.082f, 22.793f);
                pathBuilder.AddLine(39.174f, 39.616f);
                pathBuilder.AddLine(28.297f, 25.166f);
                pathBuilder.AddLine(17.250f, 10.490f);
                pathBuilder.AddLine(3.750f, 27.596f);
                pathBuilder.AddLine(3.750f, 3.750f);
                pathBuilder.AddLine(44.250f, 3.750f);
                pathBuilder.AddLine(44.250f, 30.523f);
                pathBuilder.AddLine(33.690f, 18.000f);
                pathBuilder.EndFigure(CanvasFigureLoop.Closed);

                pathBuilder2.BeginFigure(0.000f, 0.000f);
                pathBuilder2.AddLine(0.000f, 32.348f);
                pathBuilder2.AddLine(0.000f, 48.000f);
                pathBuilder2.AddLine(0.000f, 48.000f);
                pathBuilder2.AddLine(48.000f, 48.000f);
                pathBuilder2.AddLine(48.000f, 48.000f);
                pathBuilder2.AddLine(48.000f, 34.971f);
                pathBuilder2.AddLine(48.000f, 0.000f);
                pathBuilder2.AddLine(0.000f, 0.000f);
                pathBuilder2.EndFigure(CanvasFigureLoop.Closed);
                using (var geom1 = CanvasGeometry.CreatePath(pathBuilder))
                using (var geom2 = CanvasGeometry.CreatePath(pathBuilder2))
                {
                    var geom3 = geom2.CombineWith(geom1, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);
                    if ((progress < 0) || (progress >= 100))
                    {
                        // No need to display the progress
                        return geom3;
                    }
                    else
                    {
                        // Create the progress geometry
                        using (var geom4 = CanvasGeometry.CreateRectangle(_generator.Device, 2, 40, 44, 6))
                        {
                            var geom5 = geom3.CombineWith(geom4, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);
                            var width = progress * 0.40f;
                            using (var geom6 = CanvasGeometry.CreateRectangle(_generator.Device, 4, 42, width, 2))
                            {
                                return geom5.CombineWith(geom6, Matrix3x2.Identity, CanvasGeometryCombine.Union);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Placeholder geometry in the PlaceholderMask
        /// based on the progress value.
        /// </summary>
        /// <param name="progress">Progress value</param>
        private void ProgressHandler(int progress)
        {
            if (ShowPlaceholder)
            {
                _placeholderMask.Redraw(GetPlaceHolderGeometry(progress));
            }
        }

        /// <summary>
        /// Updates the state of the the ImageLoadEngine before loading an image
        /// </summary>
        private async void PreloadImage()
        {
            _scheduledUri = Source;
            switch (_engineState)
            {
                case ImageEngineState.Idle:
                    _engineState = ImageEngineState.Loading;
                    // Load the image
                    await LoadImageAsync(_scheduledUri, _frameLayer.Size.ToSize());
                    break;
                case ImageEngineState.Loading:
                    _engineState = ImageEngineState.Scheduled;
                    break;
                case ImageEngineState.Scheduled:
                    break;
            }
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
                // Does the CompositionImageFrame contain no previously rendered image?
                if (_surfaceImage == null)
                {
                    // Since a new Uri is being loaded, ImageOpened event
                    // must be raised on successful load 
                    raiseEvent = true;
                    // Show the placeholder, if required
                    DisplayPlaceHolder();
                    // Create the surfaceImage
                    bool loadNextScheduledUri;

                    do
                    {
                        var cachedUri = UseImageCache ? await ImageCache.GetCachedUriAsync(uri, ProgressHandler) : uri;

                        if (!UseImageCache)
                        {
                            // Report 25% progress, since we are not caching the image
                            // 25% progress will indicate that the image is being directly loaded
                            ProgressHandler(25);
                        }

                        // Load the new uri
                        _surfaceImage = await _generator.CreateSurfaceImageAsync(cachedUri, size, _imageOptions);

                        // Report 99% progress
                        ProgressHandler(99);

                        // Since the loading of the uri takes some time, it could be possible
                        // that a new Uri has been scheduled for load. In that case, discard
                        // the current Uri and load the scheduled Uri
                        if (_engineState == ImageEngineState.Scheduled)
                        {
                            loadNextScheduledUri = true;
                            uri = _scheduledUri;
                            _engineState = ImageEngineState.Loading;
                        }
                        else
                        {
                            loadNextScheduledUri = false;
                        }
                    } while (loadNextScheduledUri);

                    // Set initial opacity to 0 so that the contentVisual can be faded in
                    _frameContentVisual.Opacity = 0;
                    // Apply the surfaceBrush to the visual
                    var surfaceBrush = _compositor.CreateSurfaceBrush(_surfaceImage.Surface);
                    // Update the surface brush based on the Stretch and Alignment options
                    surfaceBrush.UpdateSurfaceBrushOptions(Stretch, AlignX, AlignY);
                    _frameContentVisual.Brush = surfaceBrush;
                    // Report 100% progress
                    ProgressHandler(100);
                    // Hide the placeholder
                    _placeholderVisual.Opacity = 0;
                    // Fade in the frameVisualContent
                    _frameContentVisual.StartAnimation(() => _frameContentVisual.Opacity, _fadeInAnimation);
                }
                else
                {
                    var hashedUri = UseImageCache ? ImageCache.GetHashedUri(uri) : uri;
                    // Check whether the Uri to load is same as the existing image's Uri
                    // NOTE: Checking against both uri and hashedUri because InvalidateArrange is
                    // called when ShowPlaceholder property changes which triggers a redraw/reload
                    // of the surface. We do not want to show the placeholder and reload the same
                    // image on the surface.
                    if (_surfaceImage.Uri.IsEqualTo(hashedUri) || _surfaceImage.Uri.IsEqualTo(uri))
                    {
                        // Since the Uri has not changed, no need to raise the ImageOpened event
                        // Just resize the surfaceImage with the given imageOptions and
                        // update the frameContentVisual's brush
                        raiseEvent = false;
                        _surfaceImage.Resize(size, _imageOptions);
                        // Update the surface brush based on the Stretch and Alignment options
                        (_frameContentVisual.Brush as CompositionSurfaceBrush)?.UpdateSurfaceBrushOptions(Stretch, AlignX, AlignY, _alignXAnimation, _alignYAnimation);
                    }
                    else
                    {
                        // Since a different Uri is being loaded, then ImageOpened event
                        // must be raised on successful load
                        raiseEvent = true;
                        // Show the placeholder, if required
                        DisplayPlaceHolder();

                        //// Create a temporary visual which loads the new Uri
                        _nextVisualContent.Opacity = 0;
                        bool loadNextScheduledUri;

                        do
                        {
                            var cachedUri = UseImageCache ? await ImageCache.GetCachedUriAsync(uri, ProgressHandler) : uri;

                            if (!UseImageCache)
                            {
                                // Report 25% progress, since we are not caching the image
                                // 25% progress will indicate that the image is being directly loaded
                                ProgressHandler(25);
                            }

                            // Load the new uri
                            _nextSurfaceImage = await _generator.CreateSurfaceImageAsync(cachedUri, size, _imageOptions);

                            // Report 99% progress
                            ProgressHandler(99);
                            
                            // Since the loading of the uri takes some time, it could be possible
                            // that a new Uri has been scheduled for load. In that case, discard
                            // the current Uri and load the scheduled Uri
                            if (_engineState == ImageEngineState.Scheduled)
                            {
                                loadNextScheduledUri = true;
                                uri = _scheduledUri;
                                _engineState = ImageEngineState.Loading;
                            }
                            else
                            {
                                loadNextScheduledUri = false;
                            }
                        } while (loadNextScheduledUri);

                        // Create the surface brush for the next image
                        _nextSurfaceBrush = _compositor.CreateSurfaceBrush(_nextSurfaceImage.Surface);
                        // Update the surface brush based on the Stretch and Alignment options
                        _nextSurfaceBrush.UpdateSurfaceBrushOptions(Stretch, AlignX, AlignY);
                        _nextVisualContent.Brush = _nextSurfaceBrush;

                        // Report 100% progress
                        ProgressHandler(100);

                        _compositor.CreateScopedBatch(CompositionBatchTypes.Animation,
                            () =>
                            {
                                _frameContentVisual.StartAnimation(() => _frameContentVisual.Opacity, _fadeOutAnimation);
                                _nextVisualContent.StartAnimation(() => _nextVisualContent.Opacity, _fadeInAnimation);
                            },
                            () =>
                            {
                                // apply the new brush to the frameVisualContent
                                _frameContentVisual.Brush = _nextSurfaceBrush;
                                // Update the surface image
                                _surfaceImage = _nextSurfaceImage;
                                // Make the frameVisualContent visible again
                                _frameContentVisual.Opacity = 1;
                                // Hide the placeholder
                                _placeholderVisual.Opacity = 0;
                                // Hide the nextVisualContent
                                _nextVisualContent.Opacity = 0;
                            });
                    }
                }

                if (raiseEvent)
                {
                    // Notify to subscribers that the image has been successfully loaded
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            ImageOpened?.Invoke(this, new CompositionImageEventArgs(_surfaceImage.Uri, string.Empty));
                        });
                }
            }
            catch (IOException ex)
            {
                // Notify to subscribers that loading of the image failed
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ImageFailed?.Invoke(this, new CompositionImageEventArgs(uri, ex.ToString()));
                    });
            }

            // Update the engineState after load is completed
            PostLoadImage();
        }

        /// <summary>
        /// Updates the state of the the ImageLoadEngine after loading an image
        /// </summary>
        private async void PostLoadImage()
        {
            switch (_engineState)
            {
                case ImageEngineState.Idle:
                    // Do Nothing
                    _scheduledUri = null;
                    break;
                case ImageEngineState.Loading:
                    // Loading is complete. No image pending load.
                    _engineState = ImageEngineState.Idle;
                    _scheduledUri = null;
                    break;
                case ImageEngineState.Scheduled:
                    // New image waiting in the pipeline to be rendered.
                    _engineState = ImageEngineState.Loading;
                    // Load the image
                    await LoadImageAsync(_scheduledUri, _frameLayer.Size.ToSize());
                    break;
            }
        }

        /// <summary>
        /// If ShowPlaceholder option is enabled then displays the placeholder
        /// </summary>
        private void DisplayPlaceHolder()
        {
            if (ShowPlaceholder)
            {
                _placeholderVisual.Opacity = 1;
            }
        }

        #endregion
    }
}
