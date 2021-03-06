﻿// Copyright (c) Ratish Philip 
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

using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;

namespace CompositionProToolkit
{
    /// <summary>
    /// Class for rendering custom shaped geometries onto ICompositionSurface
    /// so that they can be used as masks on Composition Visuals. These geometries
    /// have a Gaussian Blur applied to them.
    /// </summary>
    internal sealed class GaussianMaskSurface : IGaussianMaskSurface
    {
        #region Fields

        private ICompositionGeneratorInternal _generator;
        private CompositionDrawingSurface _surface;
        private readonly object _surfaceLock;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Composition Generator
        /// </summary>
        public ICompositionGenerator Generator => _generator;

        /// <summary>
        /// Gets the Surface of GaussianMaskSurface
        /// </summary>
        public ICompositionSurface Surface => _surface;

        /// <summary>
        /// Gets the Geometry of the GaussianMaskSurface
        /// </summary>
        public CanvasGeometry Geometry { get; private set; }

        /// <summary>
        /// Gets the Size of the GaussianMaskSurface
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// Gets the offset from the top left corner of the ICompositionSurface where
        /// the Geometry is rendered.
        /// </summary>
        public Vector2 Offset { get; private set; }

        /// <summary>
        /// Radius of Gaussian Blur to be applied on the GaussianMaskSurface
        /// </summary>
        public float BlurRadius { get; private set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">ICompositionMaskGeneratorInternal object</param>
        /// <param name="size">Size of the GaussianMaskSurface</param>
        /// <param name="geometry">Geometry of the GaussianMaskSurface</param>
        /// <param name="offset">The offset from the top left corner of the ICompositionSurface where
        /// the Geometry is rendered.</param>
        /// <param name="blurRadius">Radius of Gaussian Blur to be applied on the GaussianMaskSurface</param>
        public GaussianMaskSurface(ICompositionGeneratorInternal generator, Size size, CanvasGeometry geometry, Vector2 offset, float blurRadius)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator), "CompositionGenerator cannot be null!");
            _surfaceLock = new object();
            Geometry = geometry;
            Offset = offset;
            BlurRadius = Math.Abs(blurRadius);
            // Create Mask Surface
            _surface = _generator.CreateDrawingSurface(_surfaceLock, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Subscribe to DeviceReplaced event
            _generator.DeviceReplaced += OnDeviceReplaced;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Redraws the GaussianMaskSurface
        /// </summary>
        public void Redraw()
        {
            // Redraw the mask surface
            RedrawSurface();
        }

        /// <summary>
        /// Applies the given blur radius to the IGaussianMaskSurface.
        /// </summary>
        /// <param name="blurRadius">Radius of Gaussian Blur to be applied on the GaussianMaskSurface</param>
        public void Redraw(float blurRadius)
        {
            Redraw(Size, Geometry, Offset, blurRadius);
        }

        /// <summary>
        /// Redraws the GaussianMaskSurface with the new geometry
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        public void Redraw(CanvasGeometry geometry)
        {
            Redraw(Size, geometry, Offset, BlurRadius);
        }

        /// <summary>
        /// Redraws the GaussianMaskSurface with the new geometry at the specified offset.
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="offset">The offset from the top left corner of the ICompositionSurface where
        /// the Geometry is rendered.</param>
        public void Redraw(CanvasGeometry geometry, Vector2 offset)
        {
            Redraw(Size, geometry, offset, BlurRadius);
        }

        /// <summary>
        /// Redraws the IGaussianMaskSurface with the new geometry and fills it with White color after applying
        /// the Gaussian blur with given blur radius.
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the GaussianMaskSurface</param>
        /// <param name="offset">The offset from the top left corner of the ICompositionSurface where
        /// the Geometry is rendered.</param>
        /// <param name="blurRadius">Radius of Gaussian Blur to be applied on the IGaussianMaskSurface</param>
        public void Redraw(CanvasGeometry geometry, Vector2 offset, float blurRadius)
        {
            Redraw(Size, geometry, offset, blurRadius);
        }

        /// <summary>
        /// Resizes the GaussianMaskSurface with the given size and redraws the GaussianMaskSurface
        /// with the new geometry and fills it with White color. A Gaussian blur is applied to the
        /// new geometry.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        public void Redraw(Size size, CanvasGeometry geometry)
        {
            Redraw(size, geometry, Offset, BlurRadius);
        }

        /// <summary>
        /// Resizes the GaussianMaskSurface with the given size and redraws the GaussianMaskSurface
        /// with the new geometry at the specified offset and fills it with White color. A Gaussian
        /// blur is applied to the new geometry.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="offset">The offset from the top left corner of the ICompositionSurface where
        /// the Geometry is rendered.</param>
        public void Redraw(Size size, CanvasGeometry geometry, Vector2 offset)
        {
            Redraw(size, geometry, offset, BlurRadius);
        }

        /// <summary>
        /// Resizes the GaussianMaskSurface with the given size and redraws the GaussianMaskSurface
        /// with the new geometry and fills it with White color. A Gaussian blur is applied to the
        /// new geometry.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="offset">The offset from the top left corner of the ICompositionSurface where
        /// the Geometry is rendered.</param>
        /// <param name="blurRadius">Radius of Gaussian Blur to be applied on the GaussianMaskSurface</param>
        public void Redraw(Size size, CanvasGeometry geometry, Vector2 offset, float blurRadius)
        {
            // Resize the mask surface
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Set the offset
            Offset = offset;
            // Set the new geometry
            Geometry = geometry;
            // Set the new blur radius
            BlurRadius = blurRadius;
            // Redraw the mask surface
            RedrawSurface();
        }

        /// <summary>
        /// Resizes the GaussianMaskSurface to the new size.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        public void Resize(Size size)
        {
            // resize the mask surface
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // Redraw the mask surface
            RedrawSurface();
        }

        /// <summary>
        /// Disposes the resources used by the GaussianMaskSurface
        /// </summary>
        public void Dispose()
        {
            _surface?.Dispose();
            Geometry?.Dispose();
            if (_generator != null)
                _generator.DeviceReplaced -= OnDeviceReplaced;
            _surface = null;
            _generator = null;
            Geometry = null;
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
            // Recreate the GaussianMaskSurface
            _surface = _generator.CreateDrawingSurface(_surfaceLock, Size);
            // Redraw the mask surface
            RedrawSurface();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper class to redraw the GaussianMaskSurface
        /// </summary>
        private void RedrawSurface()
        {
            _generator.RedrawGaussianMaskSurface(_surfaceLock, _surface, Size, Geometry, Offset, BlurRadius);
        }

        #endregion
    }
}