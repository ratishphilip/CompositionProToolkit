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

using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Internal interface for the CompositionMaskGenerator
    /// </summary>
    internal interface ICompositionMaskGeneratorInternal : ICompositionMaskGenerator
    {
        /// <summary>
        /// Creates the Mask Surface for the given size
        /// </summary>
        /// <param name="size">Size ofthe Mask Surface</param>
        /// <returns>CompositionDrawingSurface</returns>
        CompositionDrawingSurface CreateMaskSurface(Size size);

        /// <summary>
        /// Redraws the mask surface with the given size and geometry
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">Size ofthe Mask Surface</param>
        /// <param name="geometry">Geometry of the Mask Surface</param>
        /// <param name="color">Fill color of the geometry.</param>
        /// <returns>Task</returns>
        Task RedrawMaskSurfaceAsync(CompositionDrawingSurface surface, Size size, CanvasGeometry geometry, Color color);

        /// <summary>
        /// Resizes the Mask Surface to the given size
        /// </summary>
        /// <param name="surface">CompositionDrawingSurface</param>
        /// <param name="size">New size of the Mask Surface</param>
        void ResizeMaskSurface(CompositionDrawingSurface surface, Size size);
    }
}
