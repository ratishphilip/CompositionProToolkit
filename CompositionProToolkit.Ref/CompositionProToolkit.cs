namespace CompositionProToolkit
{
    public static partial class CompositionMaskFactory
    {
        public static CompositionProToolkit.ICompositionMaskGenerator GetCompositionMaskGenerator(Windows.UI.Composition.Compositor compositor, Windows.UI.Composition.CompositionGraphicsDevice graphicsDevice=null, object sharedLock=null) { return default(CompositionProToolkit.ICompositionMaskGenerator); }
    }
    public static partial class CompositorExtensions
    {
        public static Windows.UI.Composition.CompositionEffectBrush CreateMaskedBackdropBrush(this Windows.UI.Composition.Compositor compositor, CompositionProToolkit.ICompositionMask mask, Windows.UI.Color blendColor, float blurAmount, Windows.UI.Composition.CompositionBackdropBrush backdropBrush=null) { return default(Windows.UI.Composition.CompositionEffectBrush); }
    }
    public partial interface ICompositionMask : System.IDisposable
    {
        CompositionProToolkit.ICompositionMaskGenerator Generator { get; }
        Microsoft.Graphics.Canvas.Geometry.CanvasGeometry Geometry { get; }
        Windows.Foundation.Size Size { get; }
        Windows.UI.Composition.ICompositionSurface Surface { get; }
        System.Threading.Tasks.Task RedrawAsync();
        System.Threading.Tasks.Task RedrawAsync(Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        System.Threading.Tasks.Task RedrawAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        System.Threading.Tasks.Task ResizeAsync(Windows.Foundation.Size size);
    }
    public partial interface ICompositionMaskGenerator : System.IDisposable
    {
        Microsoft.Graphics.Canvas.CanvasDevice Device { get; }
        event System.EventHandler<object> DeviceReplaced;
        System.Threading.Tasks.Task<CompositionProToolkit.ICompositionMask> CreateMaskAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
    }
}
