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
// CompositionProToolkit v0.4.4
// 

using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Class for rendering custom shaped geometries onto ICompositionSurface
    /// </summary>
    internal sealed class GeometrySurface : IGeometrySurface
    {
        #region Fields

        private ICompositionGeneratorInternal _generator;
        private CompositionDrawingSurface _surface;
        private CanvasGeometry _geometry;
        private ICanvasBrush _foregroundBrush;
        private ICanvasBrush _backgroundBrush;
        private readonly object _surfaceLock;

        #endregion

        #region Properties

        /// <summary>
        /// Surface Generator
        /// </summary>
        public ICompositionGenerator Generator => _generator;
        /// <summary>
        /// Surface of the GeometrySurface
        /// </summary>
        public ICompositionSurface Surface => _surface;
        /// <summary>
        /// Geometry of the GeometrySurface
        /// </summary>
        public CanvasGeometry Geometry => _geometry;
        /// <summary>
        /// Brush with which the Geometry is filled.
        /// </summary>
        public ICanvasBrush ForegroundBrush => _foregroundBrush;
        /// <summary>
        /// Brush with which the GeometrySurface background is filled.
        /// </summary>
        public ICanvasBrush BackgroundBrush => _backgroundBrush;
        /// <summary>
        /// Size of the GeometrySurface
        /// </summary>
        public Size Size { get; private set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">ICompositionMaskGeneratorInternal object</param>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry of the GeometrySurface</param>
        /// <param name="foregroundColor">Fill color of the geometry</param>
        /// <param name="backgroundColor">Brush to fill the GeometrySurface background surface which is 
        /// not covered by the geometry</param>
        public GeometrySurface(ICompositionGeneratorInternal generator, Size size, CanvasGeometry geometry,
            Color foregroundColor, Color backgroundColor)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator), "CompositionGenerator cannot be null!");

            _generator = generator;
            _surfaceLock = new object();
            _geometry = geometry;
            _foregroundBrush = new CanvasSolidColorBrush(_generator.Device, foregroundColor);
            _backgroundBrush = new CanvasSolidColorBrush(_generator.Device, backgroundColor);

            // Create GeometrySurface
            _surface = _generator.CreateDrawingSurface(_surfaceLock, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Subscribe to DeviceReplaced event
            _generator.DeviceReplaced += OnDeviceReplaced;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">ICompositionMaskGeneratorInternal object</param>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry of the GeometrySurface</param>
        /// <param name="foregroundBrush">Brush to fill the geometry</param>
        /// <param name="backgroundBrush">Brush to fill the GeometrySurface background surface which is 
        /// not covered by the geometry</param>
        public GeometrySurface(ICompositionGeneratorInternal generator, Size size, CanvasGeometry geometry,
            ICanvasBrush foregroundBrush, ICanvasBrush backgroundBrush)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator), "CompositionGenerator cannot be null!");

            _generator = generator;
            _surfaceLock = new object();
            _geometry = geometry;
            _foregroundBrush = foregroundBrush;
            _backgroundBrush = backgroundBrush;
            // Create GeometrySurface
            _surface = _generator.CreateDrawingSurface(_surfaceLock, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Subscribe to DeviceReplaced event
            _generator.DeviceReplaced += OnDeviceReplaced;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Redraws the GeometrySurface
        /// </summary>
        public void Redraw()
        {
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Redraws the GeometrySurface with the new geometry
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the GeometrySurface</param>
        public void Redraw(CanvasGeometry geometry)
        {
            // Set the new geometry
            _geometry = geometry;
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground color. 
        /// </summary>
        /// <param name="foregroundColor">Color with which the GeometrySurface geometry is to be filled</param>
        public void Redraw(Color foregroundColor)
        {
            // Set the foregroundBrush
            _foregroundBrush = new CanvasSolidColorBrush(_generator.Device, foregroundColor);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground color and the background with the background color. 
        /// </summary>
        /// <param name="foregroundColor">Color with which the GeometrySurface geometry is to be filled</param>
        /// <param name="backgroundColor">Color with which the GeometrySurface background is to be filled</param>
        public void Redraw(Color foregroundColor, Color backgroundColor)
        {
            // Set the foregroundBrush
            _foregroundBrush = new CanvasSolidColorBrush(_generator.Device, foregroundColor);
            // Set the backgroundBrush
            _backgroundBrush = new CanvasSolidColorBrush(_generator.Device, backgroundColor);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground brush. 
        /// </summary>
        /// <param name="foregroundBrush">Brush with which the GeometrySurface geometry is to be filled</param>
        public void Redraw(ICanvasBrush foregroundBrush)
        {
            // Set the foregroundBrush
            _foregroundBrush = foregroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground brush and the background with the background brush.
        /// </summary>
        /// <param name="foregroundBrush">Brush with which the GeometrySurface geometry is to be filled</param>
        /// <param name="backgroundBrush">Brush with which the GeometrySurface background is to be filled</param>
        public void Redraw(ICanvasBrush foregroundBrush, ICanvasBrush backgroundBrush)
        {
            // Set the foregroundBrush
            _foregroundBrush = foregroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Set the backgroundBrush
            _backgroundBrush = backgroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground color and the background with the background brush.
        /// </summary>
        /// <param name="foregroundColor">Color with which the GeometrySurface geometry is to be filled</param>
        /// <param name="backgroundBrush">Brush with which the GeometrySurface background is to be filled</param>
        public void Redraw(Color foregroundColor, ICanvasBrush backgroundBrush)
        {
            // Set the foregroundBrush
            _foregroundBrush = new CanvasSolidColorBrush(_generator.Device, foregroundColor);
            // Set the backgroundBrush
            _backgroundBrush = backgroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground brush and the background with the background brush.
        /// </summary>
        /// <param name="foregroundBrush">Brush with which the GeometrySurface geometry is to be filled</param>
        /// <param name="backgroundColor">Color with which the GeometrySurface background is to be filled</param>
        public void Redraw(ICanvasBrush foregroundBrush, Color backgroundColor)
        {
            // Set the foregroundBrush
            _foregroundBrush = foregroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Set the backgroundBrush
            _backgroundBrush = new CanvasSolidColorBrush(_generator.Device, backgroundColor);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface with the new geometry
        /// </summary>
        /// <param name="size">New size of the GeometrySurface</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the GeometrySurface</param>
        public void Redraw(Size size, CanvasGeometry geometry)
        {
            // Resize the GeometrySurface
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Set the new geometry
            _geometry = geometry;
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground color.
        /// </summary>
        /// <param name="size">New size of the GeometrySurface</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the GeometrySurface</param>
        /// <param name="foregroundColor">Fill color for the geometry</param>
        public void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor)
        {
            // Resize the GeometrySurface
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Set the new geometry
            _geometry = geometry;
            // Set the foregroundBrush
            _foregroundBrush = new CanvasSolidColorBrush(_generator.Device, foregroundColor);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground color and
        /// fills the background with the background color. 
        /// </summary>
        /// <param name="size">New size of the GeometrySurface</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the GeometrySurface</param>
        /// <param name="foregroundColor">Fill color for the geometry</param>
        /// <param name="backgroundColor">Fill color for the GeometrySurface background</param>
        public void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor, Color backgroundColor)
        {
            // Resize the GeometrySurface
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Set the new geometry
            _geometry = geometry;
            // Set the foregroundBrush
            _foregroundBrush = new CanvasSolidColorBrush(_generator.Device, foregroundColor);
            // Set the backgroundBrush
            _backgroundBrush = new CanvasSolidColorBrush(_generator.Device, backgroundColor);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground brush.
        /// </summary>
        /// <param name="size">New size of the GeometrySurface</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the GeometrySurface</param>
        /// <param name="foregroundBrush">Brush to fill the geometry</param>
        public void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush)
        {
            // Resize the GeometrySurface
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Set the new geometry
            _geometry = geometry;
            // Set the foregroundBrush
            _foregroundBrush = foregroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground brush and fills
        /// the background with the background brush.
        /// </summary>
        /// <param name="size">New size of the GeometrySurface</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the GeometrySurface</param>
        /// <param name="foregroundBrush">Brush to fill the geometry</param>
        /// <param name="backgroundBrush">Brush to fill the GeometrySurface background</param>
        public void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush,
            ICanvasBrush backgroundBrush)
        {
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Set the new geometry
            _geometry = geometry;
            // Set the foregroundBrush
            _foregroundBrush = foregroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Set the backgroundBrush
            _backgroundBrush = backgroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground brush and the background
        /// with the background color.
        /// </summary>
        /// <param name="size">New size of the GeometrySurface</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the GeometrySurface</param>
        /// <param name="foregroundBrush">Brush to fill the geometry</param>
        /// <param name="backgroundColor">Fill color for the GeometrySurface background</param>
        public void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush,
            Color backgroundColor)
        {
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Set the new geometry
            _geometry = geometry;
            // Set the foregroundBrush
            _foregroundBrush = foregroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Set the backgroundBrush
            _backgroundBrush = new CanvasSolidColorBrush(_generator.Device, backgroundColor);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground color and the background 
        /// with the background brush.
        /// </summary>
        /// <param name="size">New size of the GeometrySurface</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the GeometrySurface</param>
        /// <param name="foregroundColor">Fill color for the geometry</param>
        /// <param name="backgroundBrush">Brush to fill the GeometrySurface background</param>
        public void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor,
            ICanvasBrush backgroundBrush)
        {
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Set the new geometry
            _geometry = geometry;
            // Set the foregroundBrush
            _foregroundBrush = new CanvasSolidColorBrush(_generator.Device, foregroundColor);
            // Set the backgroundBrush
            _backgroundBrush = backgroundBrush ?? new CanvasSolidColorBrush(_generator.Device, Colors.Transparent);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Resizes the GeometrySurface to the new size.
        /// </summary>
        /// <param name="size">New size of the GeometrySurface</param>
        public void Resize(Size size)
        {
            // resize the GeometrySurface
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        /// <summary>
        /// Disposes the resources used by the GeometrySurface
        /// </summary>
        public void Dispose()
        {
            _surface?.Dispose();
            _geometry?.Dispose();
            _generator.DeviceReplaced -= OnDeviceReplaced;
            _surface = null;
            _generator = null;
            _geometry = null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the DeviceReplaced event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">object</param>
        private void OnDeviceReplaced(object sender, object e)
        {
            // Recreate the GeometrySurface
            _surface = _generator.CreateDrawingSurface(_surfaceLock, Size);
            // Redraw the GeometrySurface
            RedrawSurfaceInternal();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper class to redraw the surface
        /// </summary>
        private void RedrawSurfaceInternal()
        {
            _generator.RedrawGeometrySurface(_surfaceLock, _surface, Size, _geometry, _foregroundBrush, _backgroundBrush);
        }

        #endregion
    }
}
