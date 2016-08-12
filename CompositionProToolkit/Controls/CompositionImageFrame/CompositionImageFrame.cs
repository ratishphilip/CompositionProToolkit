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
// CompositionProToolkit v0.4.1
// 

using System;
using System.IO;
using System.Numerics;
using System.Threading;
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

        #region Fields

        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private ICompositionSurfaceImage _surfaceImage;
        private SpriteVisual _frameVisual;
        private Uri _currentUri;
        private readonly Timer _imageLoadTimer;
        private CompositionSurfaceImageOptions _imageOptions;

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
            _imageOptions = CompositionSurfaceImageOptions.UniformCenter;
            // Initialize the imageLoadTimer
            _imageLoadTimer = new Timer(OnImageLoadTimerTick, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
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
            // If the frameVisual has not been created yet, create it
            if (_frameVisual == null)
            {
                _frameVisual = _compositor.CreateSpriteVisual();
                _frameVisual.Brush = _compositor.CreateColorBrush(FrameBackground);
                ElementCompositionPreview.SetElementChildVisual(this, _frameVisual);
            }

            // Taking into account the BorderThickness and Padding
            var borders = BorderThickness;
            var padding = Padding;
            var borderSize = borders.CollapseThickness();
            var paddingSize = padding.CollapseThickness();

            // Calculate the Offset for the frameVisual
            var left = (borders.Left + padding.Left).Single();
            var top = (borders.Top + padding.Top).Single();
            // Calculate the Dimensions of the frameVisual
            var width = Math.Max(0, finalSize.Width - borderSize.Width - paddingSize.Width).Single();
            var height = Math.Max(0, finalSize.Height - borderSize.Height - paddingSize.Height).Single();

            // Set the frameVisual's Size and Offset
            _frameVisual.Size = new Vector2(width, height);
            _frameVisual.Offset = new Vector3(left, top, 0);

            // Update the imageOptions
            _imageOptions.Stretch = Stretch;
            _imageOptions.HorizontalAlignment = AlignX;
            _imageOptions.VerticalAlignment = AlignY;
            _imageOptions.Interpolation = Interpolation;
            _imageOptions.SurfaceBackgroundColor = FrameBackground;

            // If Source is valid then try loading/refreshing the surfaceImage
            if (Source != null)
            {
                _currentUri = Source;
                // Loading/Refreshing the surfaceImage happens on a separate thread
                _imageLoadTimer.Change(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(-1));
            }
            else
            {
                // Clear the previous brush of the frameVisual
                _frameVisual.Brush = _compositor.CreateColorBrush(FrameBackground);
                // If surfaceImage has any previous value, dispose it
                _surfaceImage?.Dispose();
                _surfaceImage = null;
            }

            return base.ArrangeOverride(finalSize);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the tick event of the ImageLoadTimer
        /// </summary>
        /// <param name="info">object</param>
        public async void OnImageLoadTimerTick(object info)
        {
            // Get the Uri to load the image
            var uri = _currentUri;
            if (uri == null)
                return;

            // Load the image
            await LoadImage(uri, _frameVisual.Size.ToSize());
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
        }

        /// <summary>
        /// Loads an image asynchronously from the given uri for the given size
        /// </summary>
        /// <param name="uri">Uri of the image to load</param>
        /// <param name="size">Render size of the image</param>
        /// <returns>Task</returns>
        private Task LoadImage(Uri uri, Size size)
        {
            return Task.Run(async () =>
            {
                try
                {
                    bool raiseEvent;
                    if (_surfaceImage == null)
                    {
                        // Since a new Uri is being loaded, ImageOpened event
                        // must be raised on successful load 
                        raiseEvent = true;
                        // Create the surfaceImage
                        _surfaceImage = await _generator.CreateSurfaceImageAsync(uri, size, _imageOptions);
                        // Apply the surfaceBrush to the visual
                        _frameVisual.Brush = _compositor.CreateSurfaceBrush(_surfaceImage.Surface);
                    }
                    else
                    {
                        // If a different Uri is being loaded, then ImageOpened event
                        // must be raised on successful load
                        raiseEvent = !uri.IsEqualTo(_surfaceImage.Uri);
                        // Redraw the surfaceImage (frameVisual will be automatically refreshed)
                        await _surfaceImage.RedrawAsync(uri, size, _imageOptions);
                    }

                    if (raiseEvent)
                    {
                        // Notify to subscribers that the image has been successfully loaded
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            ImageOpened?.Invoke(this, new CompositionImageEventArgs(_surfaceImage.Uri, string.Empty));
                        });
                    }

                }
                catch (IOException ex)
                {
                    // Notify to subscribers that loading of the image failed
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ImageFailed?.Invoke(this, new CompositionImageEventArgs(uri, ex.ToString()));
                    });
                }
            });
        }

        #endregion
    }
}
