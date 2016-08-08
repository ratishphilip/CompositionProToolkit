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
// CompositionProToolkit v0.4
// 

using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Class to define a Mask surface using a CanvasGeometry.
    /// This mask surface can be used to initialize a 
    /// CompositionMaskBrush to create custom shaped visuals.
    /// </summary>
    internal sealed class CompositionMask : ICompositionMask
    {
        #region Fields

        private ICompositionGeneratorInternal _generator;
        private CompositionDrawingSurface _surface;
        private CanvasGeometry _geometry;
        private ICanvasBrush _brush;

        #endregion

        #region Properties

        /// <summary>
        /// Mask Generator
        /// </summary>
        public ICompositionGenerator Generator => _generator;
        /// <summary>
        /// Mask Surface
        /// </summary>
        public ICompositionSurface Surface => _surface;
        /// <summary>
        /// Mask Geometry
        /// </summary>
        public CanvasGeometry Geometry => _geometry;
        /// <summary>
        /// Mask Size
        /// </summary>
        public Size Size { get; private set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">ICompositionMaskGeneratorInternal object</param>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="color">Fill color of the geometry</param>
        public CompositionMask(ICompositionGeneratorInternal generator, Size size, CanvasGeometry geometry, Color color)
        {
            _generator = generator;
            // Create Mask Surface
            _surface = _generator.CreateDrawingSurface(size);
            Size = _surface?.Size ?? Size.Empty;
            _geometry = geometry;
            _brush = new CanvasSolidColorBrush(_generator.Device, color);
            // Subscribe to DeviceReplaced event
            _generator.DeviceReplaced += OnDeviceReplaced;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">ICompositionMaskGeneratorInternal object</param>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="brush">Brush to fill the geometry.</param>
        public CompositionMask(ICompositionGeneratorInternal generator, Size size, CanvasGeometry geometry, ICanvasBrush brush)
        {
            _generator = generator;
            // Create Mask Surface
            _surface = _generator.CreateDrawingSurface(size);
            Size = _surface?.Size ?? Size.Empty;
            _geometry = geometry;
            _brush = brush;
            // Subscribe to DeviceReplaced event
            _generator.DeviceReplaced += OnDeviceReplaced;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Redraws the Mask surface
        /// </summary>
        /// <returns>Task</returns>
        public Task RedrawAsync()
        {
            // Redraw the mask surface
            return RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Redraws the Mask surface with the new geometry
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <returns>Task</returns>
        public Task RedrawAsync(CanvasGeometry geometry)
        {
            // Set the new geometry
            _geometry = geometry;
            // Redraw the mask surface
            return RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Resizes the Mask with the given size and redraws the mask
        /// with the new geometry.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <returns>Task</returns>
        public async Task RedrawAsync(Size size, CanvasGeometry geometry)
        {
            // Resize the mask surface
            await ResizeInternalAsync(size, false);
            // Set the new geometry
            _geometry = geometry;
            // Redraw the mask surface
            await RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Resizes the Mask with the given size and redraws the mask
        /// with the new geometry and fills it with the given color.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="color">Fill color for the geometry</param>
        /// <returns>Task</returns>
        public async Task RedrawAsync(Size size, CanvasGeometry geometry, Color color)
        {
            // ResizeAsync the mask surface
            await ResizeInternalAsync(size, false);
            // Set the new geometry
            _geometry = geometry;
            // Set the brush
            _brush = new CanvasSolidColorBrush(_generator.Device, color);
            // Redraw the mask surface
            await RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Resizes the Mask with the given size and redraws the mask
        /// with the new geometry and fills it with the given color.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="brush">Brush to fill the geometry.</param>
        /// <returns>Task</returns>
        public async Task RedrawAsync(Size size, CanvasGeometry geometry, ICanvasBrush brush)
        {
            // ResizeAsync the mask surface
            await ResizeInternalAsync(size, false);
            // Set the new geometry
            _geometry = geometry;
            // Set the brush
            _brush = brush;
            // Redraw the mask surface
            await RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Resizes the mask to the new size.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <returns>Task</returns>
        public Task ResizeAsync(Size size)
        {
            return ResizeInternalAsync(size, true);
        }

        /// <summary>
        /// Disposes the resources used by the mask
        /// </summary>
        public void Dispose()
        {
            _surface.Dispose();
            _geometry.Dispose();
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
        private async void OnDeviceReplaced(object sender, object e)
        {
            // Redraw the mask surface
            await RedrawSurfaceWorkerAsync();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Resizes the mask to the new size.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="redraw">Flag to indicate whether the mask surface should be redrawn</param>
        /// <returns>Task</returns>
        private async Task ResizeInternalAsync(Size size, bool redraw)
        {
            // resize the mask surface
            _generator.ResizeDrawingSurface(_surface, size);
            Size = size;
            if (redraw)
            {
                // Redraw the mask surface
                await RedrawSurfaceWorkerAsync();
            }
        }

        /// <summary>
        /// Helper class to redraw
        /// </summary>
        /// <returns>Task</returns>
        private async Task RedrawSurfaceWorkerAsync()
        {
            await _generator.RedrawMaskSurfaceAsync(_surface, Size, _geometry, _brush);
        }

        #endregion
    }
}
