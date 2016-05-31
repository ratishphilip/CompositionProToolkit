using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
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
        ICompositionMaskGenerator Generator { get; }

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
        /// <returns>Task</returns>
        Task RedrawAsync();

        /// <summary>
        /// Redraws the Mask surface with the new geometry
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <returns>Task</returns>
        Task RedrawAsync(CanvasGeometry geometry);

        /// <summary>
        /// Resizes the Mask with the given size and redraws the mask
        /// with the new geometry.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <returns></returns>
        Task RedrawAsync(Size size, CanvasGeometry geometry);

        /// <summary>
        /// Resizes the mask to the new size.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        Task ResizeAsync(Size size);
    }
}
