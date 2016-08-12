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
using CompositionExpressionToolkit;
using CompositionProToolkit.Common;
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
        private readonly object _drawingLock;
        private readonly bool _isDeviceCreator;

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
        /// <param name="graphicsDevice">CompositionGraphicsDevice</param>
        /// <param name="sharedLock">shared lock</param>
        public CompositionGenerator(Compositor compositor, CompositionGraphicsDevice graphicsDevice = null,
                    object sharedLock = null)
        {
            if (compositor == null)
                throw new ArgumentNullException(nameof(compositor), "Compositor cannot be null!");

            _compositor = compositor;
            _drawingLock = sharedLock ?? new object();

            if (_canvasDevice == null)
            {
                _canvasDevice = CanvasDevice.GetSharedDevice();
                _canvasDevice.DeviceLost += DeviceLost;
            }

            if (graphicsDevice == null)
            {
                // Create the Composition Graphics Device
                _graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _canvasDevice);
                _isDeviceCreator = true;
            }
            else
            {
                _graphicsDevice = graphicsDevice;
                _isDeviceCreator = false;
            }

            // Subscribe to events
            _graphicsDevice.RenderingDeviceReplaced += RenderingDeviceReplaced;
            if (!DesignMode.DesignModeEnabled)
                DisplayInformation.DisplayContentsInvalidated += OnDisplayContentsInvalidated;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Creates a CompositionMask having the given size and geometry
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <returns></returns>
        public Task<ICompositionMask> CreateMaskAsync(Size size, CanvasGeometry geometry)
        {
            // If the geometry is not null then fill the geometry area
            // with white. The rest of the area on the surface will be transparent.
            // When this mask is applied to a visual, only the area that is white
            // will be visible.
            return CreateMaskAsync(size, geometry, Colors.White);
        }

        /// <summary>
        /// Creates the CompositionMask having the given size, geometry & color
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="color">Fill color of the geometry.</param>
        /// <returns>ICompositionMask</returns>
        public Task<ICompositionMask> CreateMaskAsync(Size size, CanvasGeometry geometry, Color color)
        {
            return Task.Run(async () =>
            {
                // Initialize the mask
                ICompositionMask mask = new CompositionMask(this, size, geometry, color);

                // Render the mask
                await mask.RedrawAsync();

                return mask;
            });
        }

        /// <summary>
        /// Creates the CompositionMask having the given size, geometry & brush
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="brush">Brush to fill the geometry.</param>
        /// <returns>ICompositionMask</returns>
        public Task<ICompositionMask> CreateMaskAsync(Size size, CanvasGeometry geometry, ICanvasBrush brush)
        {
            return Task.Run(async () =>
           {
               // Initialize the mask
               ICompositionMask mask = new CompositionMask(this, size, geometry, brush);

               // Render the mask
               await mask.RedrawAsync();

               return mask;
           });
        }

        /// <summary>
        /// Creates a CompositionSurfaceImage having the given size onto which an image (based on the Uri
        /// and the options) is loaded.
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>ICompositionSurfaceImage</returns>
        public Task<ICompositionSurfaceImage> CreateSurfaceImageAsync(Uri uri, Size size, CompositionSurfaceImageOptions options)
        {
            return Task.Run(async () =>
            {
                // Initialize the SurfaceImage
                ICompositionSurfaceImage surfaceImage = new CompositionSurfaceImage(this, uri, size, options);

                // Render the image onto the surface
                await surfaceImage.RedrawAsync();

                return surfaceImage;
            });
        }

        /// <summary>
        /// Creates a reflection of the given Visual
        /// </summary>
        /// <param name="visual">Visual whose reflection has to be created</param>
        /// <param name="reflectionDistance">Distance of the reflection from the visual</param>
        /// <param name="reflectionLength">Normalized Length of the reflected visual that will be visible.</param>
        /// <param name="location"> <see cref="ReflectionLocation"/> - Location of the reflection with respect 
        /// to the Visual - Bottom, Top, Left or Right</param>
        /// <returns>Task</returns>
        public Task CreateReflectionAsync(ContainerVisual visual, float reflectionDistance = 0f,
            float reflectionLength = 0.7f, ReflectionLocation location = ReflectionLocation.Bottom)
        {
            return Task.Run(async () =>
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
                //
                // Since the drawing is done asynchronously and multiple threads could
                // be trying to get access to the device/surface at the same time, we need
                // to do any device/surface work under a lock.
                //
                CanvasLinearGradientBrush gradientBrush;
                lock (_drawingLock)
                {
                    gradientBrush = new CanvasLinearGradientBrush(_canvasDevice, Colors.White, Colors.Transparent);
                }

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
                var mask = await CreateMaskAsync(visual.Size.ToSize(), null, gradientBrush);
                // Set the 'mask' parameter of the effectBrush
                effectBrush.SetSourceParameter("mask", _compositor.CreateSurfaceBrush(mask.Surface));

                // Set the effect for the reflection layer
                reflectionLayer.Effect = effectBrush;

                // Now we need to duplicate the visual tree of the visual
                ArrangeVisualReflection(visual, reflectionLayer, true);

                visual.Children.InsertAtTop(reflectionLayer);
            });
        }

        /// <summary>
        /// Creates a CompositionDrawingSurface of given size
        /// </summary>
        /// <param name="size">Size of the CompositionDrawingSurface</param>
        /// <returns>CompositionDrawingSurface</returns>
        public CompositionDrawingSurface CreateDrawingSurface(Size size)
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

            lock (_drawingLock)
            {
                return _graphicsDevice.CreateDrawingSurface(surfaceSize, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            }
        }

        /// <summary>
        /// Resizes the Mask Surface to the given size
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">New size of the Mask Surface</param>
        public void ResizeDrawingSurface(CompositionDrawingSurface surface, Size size)
        {
            // Cannot resize to Size.Empty. Will throw exception!
            if (size.IsEmpty)
                return;

            // Ensuring that the size contains positive values
            size.Width = Math.Max(0, size.Width);
            size.Height = Math.Max(0, size.Height);

            //
            // Since the drawing is done asynchronously and multiple threads could
            // be trying to get access to the device/surface at the same time, we need
            // to do any device/surface work under a lock.
            //
            lock (_drawingLock)
            {
                CanvasComposition.Resize(surface, size);
            }
        }

        /// <summary>
        /// Redraws the mask surface with the given size, geometry and brush
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">Size ofthe Mask Surface</param>
        /// <param name="geometry">Geometry of the Mask Surface</param>
        /// <param name="brush">Brush to fill the geometry.</param>
        /// <returns>Task</returns>
        public Task RedrawMaskSurfaceAsync(CompositionDrawingSurface surface, Size size, CanvasGeometry geometry, ICanvasBrush brush)
        {
            return Task.Run(() =>
            {
                // No need to render if the width and/or height of the surface is zero
                if (surface.Size.Width.IsZero() || surface.Size.Height.IsZero())
                    return;

                //
                // Since the drawing is done asynchronously and multiple threads could
                // be trying to get access to the device/surface at the same time, we need
                // to do any device/surface work under a lock.
                //
                lock (_drawingLock)
                {
                    // Render the mask to the surface
                    using (var session = CanvasComposition.CreateDrawingSession(surface))
                    {
                        // If the geometry is not null then fill the geometry area
                        // with the given color. The rest of the area on the surface will be transparent.
                        // If the color is white and this mask is applied to a visual, only the area that is white
                        // will be visible.
                        if (geometry != null)
                        {
                            session.Clear(Colors.Transparent);
                            session.FillGeometry(geometry, brush);
                        }
                        else
                        {
                            // If the geometry is null, then the entire mask should be filled the 
                            // the given color. If the color is white, then the masked visual will be seen completely.
                            session.FillRectangle(0, 0, size.Width.Single(), size.Height.Single(), brush);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Resizes the SurfaceImage with the given size and redraws the SurfaceImage by loading 
        /// image from the new Uri. It returns the CanvasBitmap so that it can be cached for efficiency.
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">Size ofthe SurfaceImage</param>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <param name="canvasBitmap">The CanvasBitmap on which the image is loaded.</param>
        /// <returns>CanvasBitmap</returns>
        public Task<CanvasBitmap> ReloadSurfaceImageAsync(CompositionDrawingSurface surface, Size size, Uri uri,
            CompositionSurfaceImageOptions options, CanvasBitmap canvasBitmap)
        {
            return Task.Run(async () =>
            {
                // Load the image from the Uri if it is not already loaded
                if (canvasBitmap == null)
                {
                    canvasBitmap = await CanvasBitmap.LoadAsync(_canvasDevice, uri);
                }

                // Render the image to the surface
                RenderBitmap(surface, canvasBitmap, size, options);

                return canvasBitmap;
            });
        }

        /// <summary>
        /// Disposes the resources used by the CompositionMaskGenerator
        /// </summary>
        public void Dispose()
        {
            lock (_drawingLock)
            {
                _compositor = null;
                DisplayInformation.DisplayContentsInvalidated -= OnDisplayContentsInvalidated;

                if (_canvasDevice != null)
                {
                    _canvasDevice.DeviceLost -= DeviceLost;
                    //
                    // Only dispose the canvas device if we own the device.
                    //
                    if (_isDeviceCreator)
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
                if (_isDeviceCreator)
                {
                    _graphicsDevice.Dispose();
                }

                _graphicsDevice = null;
            }
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
            sender.DeviceLost -= DeviceLost;

            _canvasDevice = CanvasDevice.GetSharedDevice();
            _canvasDevice.DeviceLost += DeviceLost;

            CanvasComposition.SetCanvasDevice(_graphicsDevice, _canvasDevice);
        }

        /// <summary>
        /// Handles the RenderingDeviceReplaced event
        /// </summary>
        /// <param name="sender">CompositionGraphicsDevice</param>
        /// <param name="args">RenderingDeviceReplacedEventArgs</param>
        private void RenderingDeviceReplaced(CompositionGraphicsDevice sender, RenderingDeviceReplacedEventArgs args)
        {
            Task.Run(() =>
            {
                if (DeviceReplaced != null)
                {
                    RaiseDeviceReplacedEvent();
                }
            });
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
            CanvasDevice.GetSharedDevice();
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
        /// <param name="surface">CompositionDrawingSurface on which the CanvasBitmap has to be rendered.</param>
        /// <param name="canvasBitmap">CanvasBitmap created by loading the image from the Uri</param>
        /// <param name="surfaceSize">Size of the CompositionDrawingSurface</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        private void RenderBitmap(CompositionDrawingSurface surface, CanvasBitmap canvasBitmap, Size surfaceSize, CompositionSurfaceImageOptions options)
        {
            // Ensuring that the surfaceSize contains positive values
            surfaceSize.Width = Math.Max(0, surfaceSize.Width);
            surfaceSize.Height = Math.Max(0, surfaceSize.Height);

            // No need to render if the width and/or height of the surface is zero
            if (surfaceSize.IsEmpty || surface.Size.Width.IsZero() || surface.Size.Height.IsZero())
                return;

            //
            // Because the drawing is done asynchronously and multiple threads could
            // be trying to get access to the device/surface at the same time, we need
            // to do any device/surface work under a lock.
            //
            lock (_drawingLock)
            {
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

        #endregion
    }
}
