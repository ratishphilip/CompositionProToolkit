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
