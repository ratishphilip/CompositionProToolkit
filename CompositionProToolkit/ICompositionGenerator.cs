// Copyright (c) 2017 Ratish Philip 
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
// CompositionProToolkit v0.7.0
// 

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using CompositionProToolkit.Win2d;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Interface for the CompositionMaskGenerator
    /// </summary>
    public interface ICompositionGenerator : IDisposable
    {
        /// <summary>
        /// Device Replaced Event
        /// </summary>
        event EventHandler<object> DeviceReplaced;

        /// <summary>
        /// Gets the Compositor
        /// </summary>
        Compositor Compositor { get; }

        /// <summary>
        /// Gets the CanvasDevice
        /// </summary>
        CanvasDevice Device { get; }

        /// <summary>
        /// Creates a MaskSurface having the given size and geometry. The geometry is filled 
        /// with white color. The surface not covered by the geometry is transparent.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <returns>IMaskSurface</returns>
        IMaskSurface CreateMaskSurface(Size size, CanvasGeometry geometry);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, stroke 
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="stroke">ICanvasStroke defining the outline for the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasStroke stroke);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, fill color
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="fillColor">Fill color of the geometry.</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, Color fillColor);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, stroke and fill color
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="stroke">ICanvasStroke defining the outline for the geometry</param>
        /// <param name="fillColor">Fill color of the geometry.</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasStroke stroke, 
            Color fillColor);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, fill color and
        /// background color.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="fillColor">Fill color of the geometry</param>
        /// <param name="backgroundColor">Fill color of the GeometrySurface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, Color fillColor, 
            Color backgroundColor);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, stroke, fill color and
        /// background color.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="stroke">ICanvasStroke defining the outline for the geometry</param>
        /// <param name="fillColor">Fill color of the geometry</param>
        /// <param name="backgroundColor">Fill color of the GeometrySurface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasStroke stroke,
            Color fillColor, Color backgroundColor);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry and fill brush.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="fillBrush">The brush with which the geometry has to be filled</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasBrush fillBrush);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, stroke and fill brush.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="stroke">ICanvasStroke defining the outline for the geometry</param>
        /// <param name="fillBrush">The brush with which the geometry has to be filled</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasStroke stroke,
            ICanvasBrush fillBrush);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, fill brush and
        /// background brush.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="fillBrush">The brush with which the geometry has to be filled</param>
        /// <param name="backgroundBrush">The brush to fill the GeometrySurface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasBrush fillBrush, 
            ICanvasBrush backgroundBrush);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, stroke, fill brush and
        /// background brush.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="stroke">ICanvasStroke defining the outline for the geometry</param>
        /// <param name="fillBrush">The brush with which the geometry has to be filled</param>
        /// <param name="backgroundBrush">The brush to fill the GeometrySurface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasStroke stroke, 
            ICanvasBrush fillBrush, ICanvasBrush backgroundBrush);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, fill brush and
        /// background color.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="fillBrush">The brush with which the geometry has to be filled</param>
        /// <param name="backgroundColor">Fill color of the GeometrySurface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasBrush fillBrush,
            Color backgroundColor);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, stroke, fill brush and
        /// background color.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="stroke">ICanvasStroke defining the outline for the geometry</param>
        /// <param name="fillBrush">The brush with which the geometry has to be filled</param>
        /// <param name="backgroundColor">Fill color of the GeometrySurface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasStroke stroke, 
            ICanvasBrush fillBrush, Color backgroundColor);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, fill color and
        /// background brush.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="fillColor">Fill color of the geometry</param>
        /// <param name="backgroundBrush">The brush to fill the GeometrySurface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, Color fillColor,
            ICanvasBrush backgroundBrush);

        /// <summary>
        /// Creates a GeometrySurface having the given size, geometry, stroke, fill color and
        /// background brush.
        /// </summary>
        /// <param name="size">Size of the GeometrySurface</param>
        /// <param name="geometry">Geometry to be rendered on the GeometrySurface</param>
        /// <param name="stroke">ICanvasStroke defining the outline for the geometry</param>
        /// <param name="fillColor">Fill color of the geometry</param>
        /// <param name="backgroundBrush">The brush to fill the GeometrySurface background which is 
        /// not covered by the geometry</param>
        /// <returns>IGeometrySurface</returns>
        IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, ICanvasStroke stroke, 
            Color fillColor, ICanvasBrush backgroundBrush);

        /// <summary>
        /// Creates a ImageSurface having the given size onto which an image (based on the Uri
        /// and the options) is loaded.
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>ICompositionSurfaceImage</returns>
        Task<IImageSurface> CreateImageSurfaceAsync(Uri uri, Size size, ImageSurfaceOptions options);

        /// <summary>
        /// Creates a reflection of the given Visual
        /// </summary>
        /// <param name="visual">Visual whose reflection has to be created</param>
        /// <param name="reflectionDistance">Distance of the reflection from the visual</param>
        /// <param name="reflectionLength">Normalized Length of the reflected visual that will be visible.</param>
        /// <param name="location"> <see cref="ReflectionLocation"/> - Location of the reflection with respect 
        /// to the Visual - Bottom, Top, Left or Right</param>
        void CreateReflection(ContainerVisual visual, float reflectionDistance = 0f, float reflectionLength = 0.7f,
            ReflectionLocation location = ReflectionLocation.Bottom);
    }
}
