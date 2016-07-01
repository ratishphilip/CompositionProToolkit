using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Interface for the CompositionMaskGenerator
    /// </summary>
    public interface ICompositionMaskGenerator : IDisposable
    {
        /// <summary>
        /// Device Replace Event
        /// </summary>
        event EventHandler<object> DeviceReplaced;

        /// <summary>
        /// CanvasDevice
        /// </summary>
        CanvasDevice Device { get; }

        /// <summary>
        /// Creates the CompositionMask having the given size and geometry.
        /// The geometry is filled with white color.
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <returns>ICompositionMask</returns>
        Task<ICompositionMask> CreateMaskAsync(Size size, CanvasGeometry geometry);

        /// <summary>
        /// Creates the CompositionMask having the given size, geometry & color
        /// </summary>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        /// <param name="color">Fill color of the geometry.</param>
        /// <returns>ICompositionMask</returns>
        Task<ICompositionMask> CreateMaskAsync(Size size, CanvasGeometry geometry, Color color);
    }
}
