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
// CompositionProToolkit v0.4.6
// 

using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Interface for rendering custom shaped geometries onto ICompositionSurface
    /// </summary>
    public interface IGeometrySurface : IRenderSurface
    {
        #region Properties

        /// <summary>
        /// Surface Geometry
        /// </summary>
        CanvasGeometry Geometry { get; }
        /// <summary>
        /// Brush with which the Geometry is filled.
        /// </summary>
        ICanvasBrush ForegroundBrush { get; }
        /// <summary>
        /// Brush with which the GeometrySurface background is filled.
        /// </summary>
        ICanvasBrush BackgroundBrush { get; }

        #endregion

        #region APIs

        /// <summary>
        /// Redraws the GeometrySurface with the new geometry
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        void Redraw(CanvasGeometry geometry);

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground color. 
        /// </summary>
        /// <param name="foregroundColor">Color with which the mask geometry is to be filled</param>
        void Redraw(Color foregroundColor);

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground color and the background with the background color. 
        /// </summary>
        /// <param name="foregroundColor">Color with which the mask geometry is to be filled</param>
        /// <param name="backgroundColor">Color with which the Mask surface background is to be filled</param>
        void Redraw(Color foregroundColor, Color backgroundColor);

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground brush. 
        /// </summary>
        /// <param name="foregroundBrush">Brush with which the mask geometry is to be filled</param>
        void Redraw(ICanvasBrush foregroundBrush);

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground brush and the background with the background brush.
        /// </summary>
        /// <param name="foregroundBrush">Brush with which the Mask geometry is to be filled</param>
        /// <param name="backgroundBrush">Brush with which the Mask surface background is to be filled</param>
        void Redraw(ICanvasBrush foregroundBrush, ICanvasBrush backgroundBrush);

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground color and the background with the background brush.
        /// </summary>
        /// <param name="foregroundColor">Color with which the mask geometry is to be filled</param>
        /// <param name="backgroundBrush">Brush with which the Mask surface background is to be filled</param>
        void Redraw(Color foregroundColor, ICanvasBrush backgroundBrush);

        /// <summary>
        /// Redraws the GeometrySurface by filling the existing geometry with
        /// the foreground brush and the background with the background brush.
        /// </summary>
        /// <param name="foregroundBrush">Brush with which the Mask geometry is to be filled</param>
        /// <param name="backgroundColor">Color with which the Mask surface background is to be filled</param>
        void Redraw(ICanvasBrush foregroundBrush, Color backgroundColor);

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        void Redraw(Size size, CanvasGeometry geometry);

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground color.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="foregroundColor">Fill color for the geometry</param>
        void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor);

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground color and
        /// fills the background with the background color. 
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="foregroundColor">Fill color for the geometry</param>
        /// <param name="backgroundColor">Fill color for the Mask surface background</param>
        void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor, Color backgroundColor);

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground brush.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="foregroundBrush">Brush to fill the geometry</param>
        void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush);

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground brush and fills
        /// the background with the background brush.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="foregroundBrush">Brush to fill the geometry</param>
        /// <param name="backgroundBrush">Brush to fill the Mask surface background</param>
        void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush,
            ICanvasBrush backgroundBrush);

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground brush and the background
        /// with the background color.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="foregroundBrush">Brush to fill the geometry</param>
        /// <param name="backgroundColor">Fill color for the Mask surface background</param>
        void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush,
            Color backgroundColor);

        /// <summary>
        /// Resizes the GeometrySurface with the given size and redraws the GeometrySurface
        /// with the new geometry and fills it with the foreground color and the background 
        /// with the background brush.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <param name="foregroundColor">Fill color for the geometry</param>
        /// <param name="backgroundBrush">Brush to fill the Mask surface background</param>
        void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor, ICanvasBrush backgroundBrush);

        #endregion
    }
}
