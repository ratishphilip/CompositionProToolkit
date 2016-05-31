using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit
{
    /// <summary>
    /// Class to define a Mask surface using a CanvasGeometry.
    /// This mask surface can be used to initialize a 
    /// CompositionMaskBrush to create custom shaped visuals.
    /// </summary>
    internal sealed class CompositionMask : ICompositionMask
    {
        #region Fields

        private ICompositionMaskGeneratorInternal _generator;
        private CompositionDrawingSurface _surface;
        private CanvasGeometry _geometry;

        #endregion

        #region Properties

        /// <summary>
        /// Mask Generator
        /// </summary>
        public ICompositionMaskGenerator Generator => _generator;
        /// <summary>
        /// Mask Surface
        /// </summary>
        public ICompositionSurface Surface => _surface;
        /// <summary>
        /// Mask Geometry
        /// </summary>
        public CanvasGeometry Geometry => _geometry;
        /// <summary>
        /// Mask Size
        /// </summary>
        public Size Size { get; private set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">ICompositionMaskGeneratorInternal object</param>
        /// <param name="size">Size of the mask</param>
        /// <param name="geometry">Geometry of the mask</param>
        public CompositionMask(ICompositionMaskGeneratorInternal generator, Size size, CanvasGeometry geometry)
        {
            _generator = generator;
            // Create Mask Surface
            _surface = _generator.CreateMaskSurface(size);
            Size = _surface?.Size ?? Size.Empty;
            _geometry = geometry;
            // Subscribe to DeviceReplaced event
            _generator.DeviceReplaced += OnDeviceReplaced;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Redraws the Mask surface
        /// </summary>
        /// <returns>Task</returns>
        public Task RedrawAsync()
        {
            // Redraw the mask surface
            return RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Redraws the Mask surface with the new geometry
        /// </summary>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <returns>Task</returns>
        public Task RedrawAsync(CanvasGeometry geometry)
        {
            // Set the new geometry
            _geometry = geometry;
            // Redraw the mask surface
            return RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Resizes the Mask with the given size and redraws the mask
        /// with the new geometry.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        /// <param name="geometry">New CanvasGeometry to be applied to the mask</param>
        /// <returns></returns>
        public async Task RedrawAsync(Size size, CanvasGeometry geometry)
        {
            // ResizeAsync the mask surface
            await ResizeAsync(size);
            // Set the new geometry
            _geometry = geometry;
            // Redraw the mask surface
            await RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Resizes the mask to the new size.
        /// </summary>
        /// <param name="size">New size of the mask</param>
        public Task ResizeAsync(Size size)
        {
            _generator.ResizeMaskSurface(_surface, size);
            Size = size;
            // Redraw the mask surface
            return RedrawSurfaceWorkerAsync();
        }

        /// <summary>
        /// Disposes the resources used by the mask
        /// </summary>
        public void Dispose()
        {
            _surface.Dispose();
            _geometry.Dispose();
            _generator.DeviceReplaced -= OnDeviceReplaced;
            _surface = null;
            _generator = null;
            _geometry = null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the DeviceReplaced event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">object</param>
        private async void OnDeviceReplaced(object sender, object e)
        {
            // Redraw the mask surface
            await RedrawSurfaceWorkerAsync();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper class to redraw
        /// </summary>
        /// <returns>Task</returns>
        private async Task RedrawSurfaceWorkerAsync()
        {
            await _generator.RedrawMaskSurfaceAsync(_surface, Size, _geometry);
        }

        #endregion
    }
}
