// Copyright (c) Ratish Philip 
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
// CompositionProToolkit v0.9.5
// 

using Windows.Foundation;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Interface for rendering custom shaped geometries onto ICompositionSurface
    /// so that they can be used as masks on Composition Visuals.
    /// </summary>
    public interface IMaskSurface : IRenderSurface
    {
        #region Properties

        /// <summary>
        /// Gets the Geometry of the MaskSurface
        /// </summary>
        CanvasGeometry Geometry { get; }

        #endregion

        #region APIs

        /// <summary>
        /// Redraws the MaskSurface with the new geometry
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the MaskSurface</param>
        void Redraw(CanvasGeometry geometry);

        /// <summary>
        /// Resizes the MaskSurface with the given size and redraws the MaskSurface
        /// with the new geometry and fills it with White color
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the MaskSurface</param>
        void Redraw(Size size, CanvasGeometry geometry);

        #endregion
    }
}
