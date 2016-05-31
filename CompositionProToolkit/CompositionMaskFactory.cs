using Windows.UI.Composition;

namespace CompositionProToolkit
{
    /// <summary>
    /// Factory class to instantiate the CompositionMaskGenerator
    /// </summary>
    public static class CompositionMaskFactory
    {
        /// <summary>
        /// Instantiates a MaskGenerator object
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="graphicsDevice">CompositionGraphics device (optional)</param>
        /// <param name="sharedLock">shared lock (optional)</param>
        /// <returns>ICompositionMaskGenerator</returns>
        public static ICompositionMaskGenerator GetCompositionMaskGenerator(Compositor compositor, 
            CompositionGraphicsDevice graphicsDevice = null, object sharedLock = null)
        {
            return new CompositionMaskGenerator(compositor, graphicsDevice, sharedLock);
        }
    }
}
