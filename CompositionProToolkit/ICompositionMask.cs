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
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Interface for creating custom shaped 
    /// masks for Composition Visuals.
    /// </summary>
    public interface ICompositionMask : IDisposable
    {
        /// <summary>
        /// Mask Generator
        /// </summary>
        ICompositionGenerator Generator { get; }

        /// <summary>
        /// Mask Surface
        /// </summary>
        ICompositionSurface Surface { get; }

        /// <summary>
        /// Mask Geometry
        /// </summary>
        CanvasGeometry Geometry { get; }

        /// <summary>
        /// Mask Size
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Redraws the Mask surface
        /// </summary>
        void Redraw();

        /// <summary>
        /// Redraws the Mask surface with the new geometry
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        void Redraw(CanvasGeometry geometry);

        /// <summary>
        /// Resizes the Mask with the given size and redraws the mask
        /// with the new geometry and fills it with white color.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        void Redraw(Size size, CanvasGeometry geometry);

        /// <summary>
        /// Resizes the Mask with the given size and redraws the mask
        /// with the new geometry and fills it with the given color.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="color">Fill color for the geometry</param>
        void Redraw(Size size, CanvasGeometry geometry, Color color);

        /// <summary>
        /// Resizes the Mask with the given size and redraws the mask
        /// with the new geometry and fills it with the given brush.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="brush">Brush to fill the geometry</param>
        void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush brush);

        /// <summary>
        /// Resizes the mask to the new size.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        void Resize(Size size);
    }
}
