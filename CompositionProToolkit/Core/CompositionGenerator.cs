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
// CompositionProToolkit v0.4.5
// 

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Media;
using CompositionProToolkit.Common;
using CompositionProToolkit.Expressions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Composition;

namespace CompositionProToolkit
{
    /// <summary>
    /// Class to create mask which can be used to create custom shaped 
    /// Composition Visuals.
    /// </summary>
    internal sealed class CompositionGenerator : ICompositionGeneratorInternal
    {
        #region Events

        /// <summary>
        /// Device Replaced event
        /// </summary>
        public event EventHandler<object> DeviceReplaced;

        #endregion

        #region Fields

        private Compositor _compositor;
        private CanvasDevice _canvasDevice;
        private CompositionGraphicsDevice _graphicsDevice;
        private readonly object _disposingLock;

        private readonly bool _isGraphicsDeviceCreator;
        private readonly bool _isCanvasDeviceCreator;

        #endregion

        #region Properties

        /// <summary>
        /// CanvasDevice
        /// </summary>
        public CanvasDevice Device => _canvasDevice;

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="useSharedCanvasDevice">Whether to use a shared CanvasDevice or to create a new one.</param>
        /// <param name="useSoftwareRenderer">Whether to use Software Renderer when creating a new CanvasDevice.</param>
        public CompositionGenerator(Compositor compositor, bool useSharedCanvasDevice = true, bool useSoftwareRenderer = false)
        {
            if (compositor == null)
                throw new ArgumentNullException(nameof(compositor), "Compositor cannot be null!");

            // Compositor
            _compositor = compositor;

            // Disposing Lock
            _disposingLock = new object();

            // Canvas Device
            _canvasDevice = useSharedCanvasDevice ?
                CanvasDevice.GetSharedDevice() : new CanvasDevice(useSoftwareRenderer);
            _isCanvasDeviceCreator = !useSharedCanvasDevice;

            _canvasDevice.DeviceLost += DeviceLost;

            // Composition Graphics Device
            _graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _canvasDevice);
            _isGraphicsDeviceCreator = true;

            _graphicsDevice.RenderingDeviceReplaced += RenderingDeviceReplaced;
            if (!DesignMode.DesignModeEnabled)
                DisplayInformation.DisplayContentsInvalidated += OnDisplayContentsInvalidated;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphicsDevice">Composition Graphics Device</param>
        public CompositionGenerator(CompositionGraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice), "GraphicsDevice cannot be null!");

            // Composition Graphics Device
            _graphicsDevice = graphicsDevice;
            _isGraphicsDeviceCreator = false;
            _graphicsDevice.RenderingDeviceReplaced += RenderingDeviceReplaced;
            if (!DesignMode.DesignModeEnabled)
                DisplayInformation.DisplayContentsInvalidated += OnDisplayContentsInvalidated;

            // Compositor
            _compositor = _graphicsDevice.Compositor;

            // Canvas Device
            _canvasDevice = CanvasComposition.GetCanvasDevice(_graphicsDevice);
            _isCanvasDeviceCreator = false;
            _canvasDevice.DeviceLost += DeviceLost;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Creates a MaskSurface having the given size and geometry with MaskMode as True.
        /// The geometry is filled with white color. The surface not covered by the geometry is
        /// transparent.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <returns>IMaskSurface</returns>
        public IMaskSurface CreateMaskSurface(Size size, CanvasGeometry geometry)
        {
            // Initialize the mask
            IMaskSurface mask = new MaskSurface(this, size, geometry);
            // Render the mask
            mask.Redraw();

            return mask;
        }

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, foreground color with
        /// MaskMode as False.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="foregroundColor">Fill color of the geometry.</param>
        /// <returns>IGeometrySurface</returns>
        public IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, Color foregroundColor)
        {
            // Initialize the mask
            IGeometrySurface mask = new GeometrySurface(this, size, geometry, foregroundColor, Colors.Transparent);
            // Render the mask
            mask.Redraw();

            return mask;
        }

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, foreground color and
        /// background color with MaskMode as False.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="foregroundColor">Fill color of the geometry</param>
        /// <param name="backgroundColor">Fill color of the Mask surface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        public IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, Color foregroundColor, Color backgroundColor)
        {
            // Initialize the mask
            IGeometrySurface mask = new GeometrySurface(this, size, geometry, foregroundColor, backgroundColor);
            // Render the mask
            mask.Redraw();

            return mask;
        }

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry and foreground brush with
        /// MaskMode as False.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="foregroundBrush">The brush with which the geometry has to be filled</param>
        /// <returns>IGeometrySurface</returns>
        public IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush)
        {
            // Create the background brush
            var backgroundBrush = new CanvasSolidColorBrush(Device, Colors.Transparent);
            // Initialize the mask
            IGeometrySurface mask = new GeometrySurface(this, size, geometry, foregroundBrush, backgroundBrush);
            // Render the mask
            mask.Redraw();

            return mask;
        }

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, foreground brush and
        /// background brush with MaskMode as False.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="foregroundBrush">The brush with which the geometry has to be filled</param>
        /// <param name="backgroundBrush">The brush to fill the Mask background surface which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        public IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush,
            ICanvasBrush backgroundBrush)
        {
            // Initialize the mask
            IGeometrySurface mask = new GeometrySurface(this, size, geometry, foregroundBrush, backgroundBrush);
            // Render the mask
            mask.Redraw();

            return mask;
        }

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, foreground brush and
        /// background color with MaskMode as False.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="foregroundBrush">The brush with which the geometry has to be filled</param>
        /// <param name="backgroundColor">Fill color of the Mask background surface which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        public IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush,
            Color backgroundColor)
        {
            // Create the background brush
            var backgroundBrush = new CanvasSolidColorBrush(Device, backgroundColor);
            // Initialize the mask
            IGeometrySurface mask = new GeometrySurface(this, size, geometry, foregroundBrush, backgroundBrush);

            // Render the mask
            mask.Redraw();

            return mask;
        }

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, foreground color and
        /// background brush with MaskMode as False.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="foregroundColor">Fill color of the geometry</param>
        /// <param name="backgroundBrush">The brush to fill the Mask background surface which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        public IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, Color foregroundColor,
            ICanvasBrush backgroundBrush)
        {
            // Create the foreground brush
            var foregroundBrush = new CanvasSolidColorBrush(Device, foregroundColor);
            // Initialize the mask
            IGeometrySurface mask = new GeometrySurface(this, size, geometry, foregroundBrush, backgroundBrush);

            // Render the mask
            mask.Redraw();

            return mask;
        }

        /// <summary>
        /// Creates a ImageSurface having the given size onto which an image (based on the Uri
        /// and the options) is loaded.
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>ICompositionSurfaceImage</returns>
        public async Task<IImageSurface> CreateImageSurfaceAsync(Uri uri, Size size, ImageSurfaceOptions options)
        {
            // Initialize the SurfaceImage
            var surfaceImage = new ImageSurface(this, uri, size, options);

            // Render the image onto the surface
            await surfaceImage.RedrawAsync();

            return (IImageSurface)surfaceImage;
        }

        /// <summary>
        /// Creates a reflection of the given Visual
        /// </summary>
        /// <param name="visual">Visual whose reflection has to be created</param>
        /// <param name="reflectionDistance">Distance of the reflection from the visual</param>
        /// <param name="reflectionLength">Normalized Length of the reflected visual that will be visible.</param>
        /// <param name="location"> <see cref="ReflectionLocation"/> - Location of the reflection with respect 
        /// to the Visual - Bottom, Top, Left or Right</param>
        public void CreateReflection(ContainerVisual visual, float reflectionDistance = 0f,
            float reflectionLength = 0.7f, ReflectionLocation location = ReflectionLocation.Bottom)
        {
            // Create the visual layer that will contained the visual's reflection
            var reflectionLayer = _compositor.CreateLayerVisual();
            reflectionLayer.Size = visual.Size;
            reflectionLayer.CenterPoint = new Vector3(visual.Size * 0.5f, 0);

            // Create the effect to create the opacity mask
            var effect = new CompositeEffect
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

            var effectFactory = _compositor.CreateEffectFactory(effect);
            var effectBrush = effectFactory.CreateBrush();

            // Create the gradient brush for the effect
            var gradientBrush = new CanvasLinearGradientBrush(_canvasDevice,
                                                                Colors.White, Colors.Transparent);

            // Based on the reflection location,
            // Set the Offset, RotationAxis and RotationAngleInDegrees of the reflectionLayer and
            // set the StartPoint and EndPoint of the gradientBrush
            switch (location)
            {
                case ReflectionLocation.Bottom:
                    reflectionLayer.RotationAxis = new Vector3(1, 0, 0);
                    reflectionLayer.RotationAngleInDegrees = 180;
                    reflectionLayer.Offset = new Vector3(0, visual.Size.Y + reflectionDistance, 0);
                    gradientBrush.StartPoint = new Vector2(visual.Size.X * 0.5f, 0);
                    gradientBrush.EndPoint = new Vector2(visual.Size.X * 0.5f, visual.Size.Y * reflectionLength);
                    break;
                case ReflectionLocation.Top:
                    reflectionLayer.RotationAxis = new Vector3(1, 0, 0);
                    reflectionLayer.RotationAngleInDegrees = -180;
                    reflectionLayer.Offset = new Vector3(0, -visual.Size.Y - reflectionDistance, 0);
                    gradientBrush.StartPoint = new Vector2(visual.Size.X * 0.5f, visual.Size.Y);
                    gradientBrush.EndPoint = new Vector2(visual.Size.X * 0.5f, visual.Size.Y * (1f - reflectionLength));
                    break;
                case ReflectionLocation.Left:
                    reflectionLayer.RotationAxis = new Vector3(0, 1, 0);
                    reflectionLayer.RotationAngleInDegrees = -180;
                    reflectionLayer.Offset = new Vector3(-visual.Size.X - reflectionDistance, 0, 0);
                    gradientBrush.StartPoint = new Vector2(visual.Size.X, visual.Size.Y * 0.5f);
                    gradientBrush.EndPoint = new Vector2(visual.Size.X * (1f - reflectionLength), visual.Size.Y * 0.5f);
                    break;
                case ReflectionLocation.Right:
                    reflectionLayer.RotationAxis = new Vector3(0, 1, 0);
                    reflectionLayer.RotationAngleInDegrees = 180;
                    reflectionLayer.Offset = new Vector3(visual.Size.X + reflectionDistance, 0, 0);
                    gradientBrush.StartPoint = new Vector2(0, visual.Size.Y * 0.5f);
                    gradientBrush.EndPoint = new Vector2(visual.Size.X * reflectionLength, visual.Size.Y * 0.5f);
                    break;
            }

            // Create a mask filled with gradientBrush
            var mask = CreateGeometrySurface(visual.Size.ToSize(), null, Colors.Transparent, gradientBrush);

            // Set the 'mask' parameter of the effectBrush
            effectBrush.SetSourceParameter("mask", _compositor.CreateSurfaceBrush(mask.Surface));

            // Set the effect for the reflection layer
            reflectionLayer.Effect = effectBrush;

            // Now we need to duplicate the visual tree of the visual
            ArrangeVisualReflection(visual, reflectionLayer, true);

            visual.Children.InsertAtTop(reflectionLayer);
        }

        #endregion

        #region Internal APIs

        /// <summary>
        /// Creates a CompositionDrawingSurface of given size
        /// </summary>
        /// <param name="surfaceLock">The object to lock to prevent multiple threads
        /// from accessing the surface at the same time.</param>
        /// <param name="size">Size of the CompositionDrawingSurface</param>
        /// <returns>CompositionDrawingSurface</returns>
        public CompositionDrawingSurface CreateDrawingSurface(object surfaceLock, Size size)
        {
            var surfaceSize = size;
            if (surfaceSize.IsEmpty)
            {
                //
                // We start out with a size of 0,0 for the surface, because we don't know
                // the size of the image at this time. We resize the surface later.
                //
                surfaceSize = new Size(0, 0);
            }

            //
            // Since multiple threads could be trying to get access to the device/surface 
            // at the same time, we need to do any device/surface work under a lock.
            //
            lock (surfaceLock)
            {
                return _graphicsDevice.CreateDrawingSurface(surfaceSize, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            }
        }

        /// <summary>
        /// Resizes the Mask Surface to the given size
        /// </summary>
        /// <param name="surfaceLock">The object to lock to prevent multiple threads
        /// from accessing the surface at the same time.</param>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">New size of the Mask Surface</param>
        public void ResizeDrawingSurface(object surfaceLock, CompositionDrawingSurface surface, Size size)
        {
            // Cannot resize to Size.Empty. Will throw exception!
            if (size.IsEmpty)
                return;

            // Ensuring that the size contains positive values
            size.Width = Math.Max(0, size.Width);
            size.Height = Math.Max(0, size.Height);

            //
            // Since multiple threads could be trying to get access to the device/surface 
            // at the same time, we need to do any device/surface work under a lock.
            //
            lock (surfaceLock)
            {
                CanvasComposition.Resize(surface, size);
            }
        }

        /// <summary>
        /// Redraws the MaskSurface with the given size and geometry
        /// </summary>
        /// <param name="surfaceLock">The object to lock to prevent multiple threads
        /// from accessing the surface at the same time.</param>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">Size ofthe MaskSurface</param>
        /// <param name="geometry">Geometry of the MaskSurface</param>
        public void RedrawMaskSurface(object surfaceLock, CompositionDrawingSurface surface, Size size,
            CanvasGeometry geometry)
        {
            // If the surface is not created, create it
            if (surface == null)
            {
                surface = CreateDrawingSurface(surfaceLock, size);
            }

            // No need to render if the width and/or height of the surface is zero
            if (surface.Size.Width.IsZero() || surface.Size.Height.IsZero())
                return;

            //
            // Since multiple threads could be trying to get access to the device/surface 
            // at the same time, we need to do any device/surface work under a lock.
            //
            lock (surfaceLock)
            {
                // Render the mask to the surface
                using (var session = CanvasComposition.CreateDrawingSession(surface))
                {
                    if (geometry != null)
                    {
                        // If the geometry is not null then fill the geometry area
                        // with the White color. The rest of the area on the surface will be transparent.
                        // When this mask is applied to a visual, only the area that is white will be visible.
                        session.Clear(Colors.Transparent);
                        session.FillGeometry(geometry, Colors.White);
                    }
                    else
                    {
                        // If the geometry is null, then the entire mask should be filled the 
                        // the given color. If the color is white, then the masked visual will be seen completely.
                        session.FillRectangle(0, 0, size.Width.Single(), size.Height.Single(), Colors.White);
                    }
                }
            }
        }

        /// <summary>
        /// Redraws the GeometrySurface with the given size, geometry, foreground brush and background brush
        /// </summary>
        /// <param name="surfaceLock">The object to lock to prevent multiple threads
        /// from accessing the surface at the same time.</param>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">Size ofthe GeometrySurface</param>
        /// <param name="geometry">Geometry of the GeometrySurface</param>
        /// <param name="foregroundBrush">The brush with which the geometry has to be filled</param>
        /// <param name="backgroundBrush">The brush with which the GeometrySurface background has to be filled</param>
        public void RedrawGeometrySurface(object surfaceLock, CompositionDrawingSurface surface, Size size,
            CanvasGeometry geometry, ICanvasBrush foregroundBrush, ICanvasBrush backgroundBrush)
        {
            // If the surface is not created, create it
            if (surface == null)
            {
                surface = CreateDrawingSurface(surfaceLock, size);
            }

            // No need to render if the width and/or height of the surface is zero
            if (surface.Size.Width.IsZero() || surface.Size.Height.IsZero())
                return;

            //
            // Since multiple threads could be trying to get access to the device/surface 
            // at the same time, we need to do any device/surface work under a lock.
            //
            lock (surfaceLock)
            {
                // Render the mask to the surface
                using (var session = CanvasComposition.CreateDrawingSession(surface))
                {
                    // First fill the background
                    var brush = backgroundBrush as CanvasSolidColorBrush;
                    if (brush != null)
                    {
                        // If the backgroundBrush is a SolideColorBrush then use the Clear()
                        // method to fill the surface with background color. It is faster.
                        // Clear the surface with the background color
                        session.Clear(brush.Color);
                    }
                    else
                    {
                        // Fill the surface with the background brush.
                        session.FillRectangle(0, 0, size.Width.Single(), size.Height.Single(), backgroundBrush);
                    }

                    // If the geometry is not null then fill the geometry area with the foreground brush.
                    if (geometry != null)
                    {
                        session.FillGeometry(geometry, foregroundBrush);
                    }
                }
            }
        }

        /// <summary>
        /// Resizes the ImageSurface to the given size and redraws the ImageSurface
        /// by rendering the canvasBitmap onto the surface.
        /// </summary>
        /// <param name="surfaceLock">The object to lock to prevent multiple threads
        /// from accessing the surface at the same time.</param>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <param name="canvasBitmap">The CanvasBitmap on which the image is loaded.</param>
        public void RedrawImageSurface(object surfaceLock, CompositionDrawingSurface surface,
            ImageSurfaceOptions options, CanvasBitmap canvasBitmap)
        {
            // Render the image to the surface
            RenderBitmap(surfaceLock, surface, canvasBitmap, options);
        }

        /// <summary>
        /// Resizes the ImageSurface with the given size and redraws the ImageSurface by loading 
        /// image from the new Uri.
        /// </summary>
        /// <param name="surfaceLock">The object to lock to prevent multiple threads
        /// from accessing the surface at the same time.</param>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <param name="canvasBitmap">The CanvasBitmap on which the image is loaded.</param>
        /// <returns>CanvasBitmap</returns>
        public async Task<CanvasBitmap> RedrawImageSurfaceAsync(object surfaceLock, CompositionDrawingSurface surface,
            Uri uri, ImageSurfaceOptions options, CanvasBitmap canvasBitmap)
        {
            if ((canvasBitmap == null) && (uri != null))
            {
                try
                {
                    canvasBitmap = await CanvasBitmap.LoadAsync(_canvasDevice, uri);
                }
                catch (IOException)
                {
                    // Do nothing here as RenderBitmap method will fill the surface
                    // with options.SurfaceBackgroundColor as the image failed to load
                    // from Uri
                }
            }

            // Render the image to the surface
            RenderBitmap(surfaceLock, surface, canvasBitmap, options);

            return canvasBitmap;
        }

        /// <summary>
        /// Disposes the resources used by the CompositionMaskGenerator
        /// </summary>
        public void Dispose()
        {
            lock (_disposingLock)
            {
                _compositor = null;
                DisplayInformation.DisplayContentsInvalidated -= OnDisplayContentsInvalidated;

                if (_canvasDevice != null)
                {
                    _canvasDevice.DeviceLost -= DeviceLost;
                    //
                    // Only dispose the canvas device if we own the device.
                    //
                    if (_isCanvasDeviceCreator)
                    {
                        _canvasDevice.Dispose();
                    }
                    _canvasDevice = null;
                }

                if (_graphicsDevice == null)
                    return;

                _graphicsDevice.RenderingDeviceReplaced -= RenderingDeviceReplaced;
                //
                // Only dispose the composition graphics device if we own the device.
                //
                if (_isGraphicsDeviceCreator)
                {
                    _graphicsDevice.Dispose();
                }

                _graphicsDevice = null;
            }
        }

        #endregion

        #region Static APIs

        internal static CanvasGeometry GenerateGeometry(CanvasDevice device, Size size, CompositionPathInfo info, Vector2 offset)
        {
            //
            //   |--LeftTop----------------------RightTop--|
            //   |                                         |
            // TopLeft                                TopRight
            //   |                                         |
            //   |                                         |
            //   |                                         |
            //   |                                         |
            //   |                                         |
            //   |                                         |
            // BottomLeft                          BottomRight
            //   |                                         |
            //   |--LeftBottom----------------RightBottom--|
            //
            //  compute the coordinates of the key points
            var leftTop = new Vector2(info.LeftTop.Single(), 0);
            var rightTop = new Vector2((size.Width - info.RightTop).Single(), 0);
            var topRight = new Vector2(size.Width.Single(), info.TopRight.Single());
            var bottomRight = new Vector2(size.Width.Single(), (size.Height - info.BottomRight).Single());
            var rightBottom = new Vector2((size.Width - info.RightBottom).Single(), size.Height.Single());
            var leftBottom = new Vector2(info.LeftBottom.Single(), size.Height.Single());
            var bottomLeft = new Vector2(0, (size.Height - info.BottomLeft).Single());
            var topLeft = new Vector2(0, info.TopLeft.Single());

            //  check keypoints for overlap and resolve by partitioning corners according to
            //  the percentage of each one.  

            //  top edge
            if (leftTop.X > rightTop.X)
            {
                var v = ((info.LeftTop) / (info.LeftTop + info.RightTop) * size.Width).Single();
                leftTop.X = v;
                rightTop.X = v;
            }

            //  right edge
            if (topRight.Y > bottomRight.Y)
            {
                var v = ((info.TopRight) / (info.TopRight + info.BottomRight) * size.Height).Single();
                topRight.Y = v;
                bottomRight.Y = v;
            }

            //  bottom edge
            if (leftBottom.X > rightBottom.X)
            {
                var v = ((info.LeftBottom) / (info.LeftBottom + info.RightBottom) * size.Width).Single();
                rightBottom.X = v;
                leftBottom.X = v;
            }

            // left edge
            if (topLeft.Y > bottomLeft.Y)
            {
                var v = ((info.TopLeft) / (info.TopLeft + info.BottomLeft) * size.Height).Single();
                bottomLeft.Y = v;
                topLeft.Y = v;
            }

            // Apply offset
            leftTop += offset;
            rightTop += offset;
            topRight += offset;
            bottomRight += offset;
            rightBottom += offset;
            leftBottom += offset;
            bottomLeft += offset;
            topLeft += offset;

            //  create the border geometry
            var pathBuilder = new CanvasPathBuilder(device);

            // Begin path
            pathBuilder.BeginFigure(leftTop);

            // Top line
            pathBuilder.AddLine(rightTop);

            // Upper-right corners
            var radiusX = size.Width - rightTop.X;
            var radiusY = (double)topRight.Y;
            if (!radiusX.IsZero() || !radiusY.IsZero())
            {
                pathBuilder.AddArc(topRight, radiusX.Single(), radiusY.Single(), (Math.PI / 2f).Single(),
                    CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            }

            // Right line
            pathBuilder.AddLine(bottomRight);

            // Lower-right corners
            radiusX = size.Width - rightBottom.X;
            radiusY = size.Height - bottomRight.Y;
            if (!radiusX.IsZero() || !radiusY.IsZero())
            {
                pathBuilder.AddArc(rightBottom, radiusX.Single(), radiusY.Single(), (Math.PI / 2f).Single(),
                    CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            }

            // Bottom line
            pathBuilder.AddLine(leftBottom);

            // Lower-left corners
            radiusX = leftBottom.X;
            radiusY = size.Height - bottomLeft.Y;
            if (!radiusX.IsZero() || !radiusY.IsZero())
            {
                pathBuilder.AddArc(bottomLeft, radiusX.Single(), radiusY.Single(), (Math.PI / 2f).Single(),
                    CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            }

            // Left line
            pathBuilder.AddLine(topLeft);

            // Upper-left corners
            radiusX = leftTop.X;
            radiusY = topLeft.Y;
            if (!radiusX.IsZero() || !radiusY.IsZero())
            {
                pathBuilder.AddArc(leftTop, radiusX.Single(), radiusY.Single(), (Math.PI / 2f).Single(),
                    CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            }

            // End path
            pathBuilder.EndFigure(CanvasFigureLoop.Closed);

            return CanvasGeometry.CreatePath(pathBuilder);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the DeviceLost event
        /// </summary>
        /// <param name="sender">CanvasDevice</param>
        /// <param name="args">event arguments</param>
        private void DeviceLost(CanvasDevice sender, object args)
        {
            // Unsubscribe from DeviceLost event of the previous Canvas Device
            sender.DeviceLost -= DeviceLost;

            _canvasDevice = _isCanvasDeviceCreator ?
                new CanvasDevice(sender.ForceSoftwareRenderer) : CanvasDevice.GetSharedDevice();

            // Subscribe to DeviceLost event of the new Canvas Device
            _canvasDevice.DeviceLost += DeviceLost;
            // Update the CompositionGraphicsDevice
            CanvasComposition.SetCanvasDevice(_graphicsDevice, _canvasDevice);
        }

        /// <summary>
        /// Handles the RenderingDeviceReplaced event
        /// </summary>
        /// <param name="sender">CompositionGraphicsDevice</param>
        /// <param name="args">RenderingDeviceReplacedEventArgs</param>
        private void RenderingDeviceReplaced(CompositionGraphicsDevice sender, RenderingDeviceReplacedEventArgs args)
        {
            if (DeviceReplaced != null)
            {
                RaiseDeviceReplacedEvent();
            }
        }

        /// <summary>
        /// Handles the DisplayContentsInvalidated event
        /// </summary>
        /// <param name="sender">DisplayInformation</param>
        /// <param name="args">Event arguments</param>
        private void OnDisplayContentsInvalidated(DisplayInformation sender, object args)
        {
            //
            // This will trigger the device lost event
            //
            _canvasDevice.RaiseDeviceLost();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Raises the DeviceReplacedEvent
        /// </summary>
        private void RaiseDeviceReplacedEvent()
        {
            var deviceEvent = DeviceReplaced;
            deviceEvent?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Creates a duplicate of the visual tree of the given visual and arranges them within the reflectedParent.
        /// </summary>
        /// <param name="visual">Visual whose visual tree has to be duplicated</param>
        /// <param name="reflectedParent">Visual in which will host the duplicated visual tree</param>
        /// <param name="isRoot">Flag to indicate whether the given visual is the root of the visual tree to be duplicated.</param>
        private void ArrangeVisualReflection(ContainerVisual visual, ContainerVisual reflectedParent, bool isRoot = false)
        {
            if (visual == null)
                return;

            ContainerVisual reflectedVisual;

            if (visual is LayerVisual)
            {
                reflectedVisual = _compositor.CreateLayerVisual();
                ((LayerVisual)reflectedVisual).Effect = ((LayerVisual)visual).Effect;
            }
            else if (visual is SpriteVisual)
            {
                reflectedVisual = _compositor.CreateSpriteVisual();
                ((SpriteVisual)reflectedVisual).Brush = ((SpriteVisual)visual).Brush;
                ((SpriteVisual)reflectedVisual).Shadow = ((SpriteVisual)visual).Shadow;
            }
            else
            {
                reflectedVisual = _compositor.CreateContainerVisual();
            }

            // Copy the Visual properties
            reflectedVisual.AnchorPoint = visual.AnchorPoint;
            reflectedVisual.BackfaceVisibility = visual.BackfaceVisibility;
            reflectedVisual.BorderMode = visual.BorderMode;
            reflectedVisual.CenterPoint = visual.CenterPoint;
            reflectedVisual.Clip = visual.Clip;
            reflectedVisual.CompositeMode = visual.CompositeMode;
            reflectedVisual.ImplicitAnimations = visual.ImplicitAnimations;
            reflectedVisual.IsVisible = visual.IsVisible;
            reflectedVisual.Offset = isRoot ? Vector3.One : visual.Offset;
            reflectedVisual.Opacity = visual.Opacity;
            reflectedVisual.Orientation = visual.Orientation;
            reflectedVisual.RotationAngle = visual.RotationAngle;
            reflectedVisual.RotationAngleInDegrees = visual.RotationAngleInDegrees;
            reflectedVisual.RotationAxis = visual.RotationAxis;
            reflectedVisual.Scale = visual.Scale;
            reflectedVisual.Size = visual.Size;
            reflectedVisual.TransformMatrix = visual.TransformMatrix;

            // Add the reflectedVisual to the reflectedParent's Children (at the Top)
            reflectedParent.Children.InsertAtTop(reflectedVisual);

            if (!visual.Children.Any())
                return;

            // Iterate each of the visual's Children and add them to 
            // the reflectedVisual's Children (at the Top so that the 
            // correct order is obtained)
            foreach (var child in visual.Children)
            {
                ArrangeVisualReflection((ContainerVisual)child, reflectedVisual);
            }
        }

        /// <summary>
        /// Renders the CanvasBitmap on the CompositionDrawingSurface based on the given options.
        /// </summary>
        /// <param name="surfaceLock">The object to lock to prevent multiple threads
        /// from accessing the surface at the same time.</param>
        /// <param name="surface">CompositionDrawingSurface on which the CanvasBitmap has to be rendered.</param>
        /// <param name="canvasBitmap">CanvasBitmap created by loading the image from the Uri</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        private static void RenderBitmap(object surfaceLock, CompositionDrawingSurface surface, CanvasBitmap canvasBitmap,
            ImageSurfaceOptions options)
        {
            var surfaceSize = surface.Size;

            // If the canvasBitmap is null, then just fill the surface with the SurfaceBackgroundColor
            if (canvasBitmap == null)
            {
                // No need to render if the width and/or height of the surface is zero
                if (surfaceSize.IsEmpty || surfaceSize.Width.IsZero() || surfaceSize.Height.IsZero())
                    return;

                //
                // Since multiple threads could be trying to get access to the device/surface 
                // at the same time, we need to do any device/surface work under a lock.
                //
                lock (surfaceLock)
                {
                    using (var session = CanvasComposition.CreateDrawingSession(surface))
                    {
                        // Clear the surface with the SurfaceBackgroundColor
                        session.Clear(options.SurfaceBackgroundColor);
                    }

                    // No need to proceed further
                    return;
                }
            }

            //
            // Since multiple threads could be trying to get access to the device/surface 
            // at the same time, we need to do any device/surface work under a lock.
            //
            lock (surfaceLock)
            {
                // Is AutoResize Enabled?
                if (options.AutoResize)
                {
                    // If AutoResize is allowed and the canvasBitmap size and surface size are 
                    // not matching then resize the surface to match the canvasBitmap size.
                    //
                    // NOTE: HorizontalAlignment, Vertical Alignment and Stretch will be
                    // handled by the CompositionSurfaceBrush created using this surface.
                    //
                    if (canvasBitmap.Size != surfaceSize)
                    {
                        // Resize the surface
                        CanvasComposition.Resize(surface, canvasBitmap.Size);
                        surfaceSize = canvasBitmap.Size;
                    }

                    // No need to render if the width and/or height of the surface is zero
                    if (surfaceSize.IsEmpty || surface.Size.Width.IsZero() || surface.Size.Height.IsZero())
                        return;

                    // Draw the image to the surface
                    using (var session = CanvasComposition.CreateDrawingSession(surface))
                    {
                        // Render the image
                        session.DrawImage(canvasBitmap,                                         // CanvasBitmap
                            new Rect(0, 0, surfaceSize.Width, surfaceSize.Height),              // Target Rectangle
                            new Rect(0, 0, canvasBitmap.Size.Width, canvasBitmap.Size.Height),  // Source Rectangle
                            options.Opacity,                                                    // Opacity
                            options.Interpolation);                                             // Interpolation
                    }
                }
                else
                {
                    // No need to render if the width and/or height of the surface is zero
                    if (surfaceSize.IsEmpty || surface.Size.Width.IsZero() || surface.Size.Height.IsZero())
                        return;

                    var bitmapSize = canvasBitmap.Size;
                    var sourceWidth = bitmapSize.Width;
                    var sourceHeight = bitmapSize.Height;
                    var ratio = sourceWidth / sourceHeight;
                    var targetWidth = 0d;
                    var targetHeight = 0d;
                    var left = 0d;
                    var top = 0d;

                    // Stretch Mode
                    switch (options.Stretch)
                    {
                        case Stretch.None:
                            targetWidth = sourceWidth;
                            targetHeight = sourceHeight;
                            break;
                        case Stretch.Fill:
                            targetWidth = surfaceSize.Width;
                            targetHeight = surfaceSize.Height;
                            break;
                        case Stretch.Uniform:
                            // If width is greater than height
                            if (ratio > 1.0)
                            {
                                targetHeight = Math.Min(surfaceSize.Width / ratio, surfaceSize.Height);
                                targetWidth = targetHeight * ratio;
                            }
                            else
                            {
                                targetWidth = Math.Min(surfaceSize.Height * ratio, surfaceSize.Width);
                                targetHeight = targetWidth / ratio;
                            }
                            break;
                        case Stretch.UniformToFill:
                            // If width is greater than height
                            if (ratio > 1.0)
                            {
                                targetHeight = Math.Max(surfaceSize.Width / ratio, surfaceSize.Height);
                                targetWidth = targetHeight * ratio;
                            }
                            else
                            {
                                targetWidth = Math.Max(surfaceSize.Height * ratio, surfaceSize.Width);
                                targetHeight = targetWidth / ratio;
                            }
                            break;
                    }

                    // Horizontal Alignment
                    switch (options.HorizontalAlignment)
                    {
                        case AlignmentX.Left:
                            left = 0;
                            break;
                        case AlignmentX.Center:
                            left = (surfaceSize.Width - targetWidth) / 2.0;
                            break;
                        case AlignmentX.Right:
                            left = surfaceSize.Width - targetWidth;
                            break;
                    }

                    // Vertical Alignment
                    switch (options.VerticalAlignment)
                    {
                        case AlignmentY.Top:
                            top = 0;
                            break;
                        case AlignmentY.Center:
                            top = (surfaceSize.Height - targetHeight) / 2.0;
                            break;
                        case AlignmentY.Bottom:
                            top = surfaceSize.Height - targetHeight;
                            break;
                    }

                    // Draw the image to the surface
                    using (var session = CanvasComposition.CreateDrawingSession(surface))
                    {
                        // Clear the surface with the SurfaceBackgroundColor
                        session.Clear(options.SurfaceBackgroundColor);
                        // Render the image
                        session.DrawImage(canvasBitmap,                         // CanvasBitmap
                            new Rect(left, top, targetWidth, targetHeight),     // Target Rectangle
                            new Rect(0, 0, sourceWidth, sourceHeight),          // Source Rectangle
                            options.Opacity,                                    // Opacity
                            options.Interpolation);                             // Interpolation
                    }
                }
            }
        }

        #endregion
    }
}
