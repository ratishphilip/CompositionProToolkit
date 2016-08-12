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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Internal interface for the CompositionMaskGenerator
    /// </summary>
    internal interface ICompositionGeneratorInternal : ICompositionGenerator
    {
        /// <summary>
        /// Creates a CompositionDrawingSurface for the given size
        /// </summary>
        /// <param name="size">Size of the Mask Surface</param>
        /// <returns>CompositionDrawingSurface</returns>
        CompositionDrawingSurface CreateDrawingSurface(Size size);

        /// <summary>
        /// Resizes the Mask Surface to the given size
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">New size of the Mask Surface</param>
        void ResizeDrawingSurface(CompositionDrawingSurface surface, Size size);

        /// <summary>
        /// Redraws the mask surface with the given size, geometry and brush
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">Size ofthe Mask Surface</param>
        /// <param name="geometry">Geometry of the Mask Surface</param>
        /// <param name="brush">Brush to fill the geometry.</param>
        /// <returns>Task</returns>
        Task RedrawMaskSurfaceAsync(CompositionDrawingSurface surface, Size size, CanvasGeometry geometry, ICanvasBrush brush);

        /// <summary>
        /// Resizes the SurfaceImage with the given size and redraws the SurfaceImage by loading 
        /// image from the new Uri.
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">Size ofthe SurfaceImage</param>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <param name="canvasBitmap">The CanvasBitmap on which the image is loaded.</param>
        /// <returns>CanvasBitmap</returns>
        Task<CanvasBitmap> ReloadSurfaceImageAsync(CompositionDrawingSurface surface, Size size, Uri uri, 
            CompositionSurfaceImageOptions options, CanvasBitmap canvasBitmap);
    }
}
