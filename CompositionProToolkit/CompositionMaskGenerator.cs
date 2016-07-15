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
// CompositionProToolkit v0.3
// 

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Composition;

namespace CompositionProToolkit
{
    /// <summary>
    /// Class to create mask which can be used to create custom shaped 
    /// Composition Visuals.
    /// </summary>
    internal sealed class CompositionMaskGenerator : ICompositionMaskGeneratorInternal
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
        public CompositionMaskGenerator(Compositor compositor, CompositionGraphicsDevice graphicsDevice = null,
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
            return Task.Run(() =>
            {
                //
                // Since the drawing is done asynchronously and multiple threads could
                // be trying to get access to the device/surface at the same time, we need
                // to do any device/surface work under a lock.
                //
                lock (_drawingLock)
                {
                    ICompositionMask mask = new CompositionMask(this, size, geometry, color);

                    // Render the mask to the surface
                    using (var session = CanvasComposition.CreateDrawingSession((CompositionDrawingSurface)mask.Surface))
                    {
                        // If the geometry is not null then fill the geometry area
                        // with the given color. The rest of the area on the surface will be transparent.
                        if (geometry != null)
                        {
                            session.Clear(Colors.Transparent);
                            session.FillGeometry(geometry, color);
                        }
                        else
                        {
                            // If the geometry is null, then the entire mask should be filled with
                            // the given color
                            session.Clear(color);
                        }
                    }

                    return mask;
                }
            });
        }

        /// <summary>
        /// Creates a CompositionDrawingSurface of given size
        /// </summary>
        /// <param name="size">Size of the CompositionDrawingSurface</param>
        /// <returns>CompositionDrawingSurface</returns>
        public CompositionDrawingSurface CreateMaskSurface(Size size)
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

            CompositionDrawingSurface surface;
            lock (_drawingLock)
            {
                surface = _graphicsDevice.CreateDrawingSurface(surfaceSize, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            }

            return surface;
        }

        /// <summary>
        /// Redraws the mask surface with the given size and geometry
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">Size ofthe Mask Surface</param>
        /// <param name="geometry">Geometry of the Mask Surface</param>
        /// <param name="color">Fill color of the geometry.</param>
        /// <returns>Task</returns>
        public Task RedrawMaskSurfaceAsync(CompositionDrawingSurface surface, Size size, CanvasGeometry geometry, Color color)
        {
            return Task.Run(() =>
            {
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
                            session.FillGeometry(geometry, color);
                        }
                        else
                        {
                            // If the geometry is null, then the entire mask should be filled the 
                            // the given color. If the color is white, then the masked visual will be seen completely.
                            session.Clear(color);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Resizes the Mask Surface to the given size
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">New size of the Mask Surface</param>
        public void ResizeMaskSurface(CompositionDrawingSurface surface, Size size)
        {
            if (size.IsEmpty)
                return;

            lock (_drawingLock)
            {
                CanvasComposition.Resize(surface, size);
            }
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

        #endregion
    }
}
