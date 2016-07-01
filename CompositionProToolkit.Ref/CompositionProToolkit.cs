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
        System.Threading.Tasks.Task RedrawAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color color);
        System.Threading.Tasks.Task ResizeAsync(Windows.Foundation.Size size);
    }
    public partial interface ICompositionMaskGenerator : System.IDisposable
    {
        Microsoft.Graphics.Canvas.CanvasDevice Device { get; }
        event System.EventHandler<object> DeviceReplaced;
        System.Threading.Tasks.Task<CompositionProToolkit.ICompositionMask> CreateMaskAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        System.Threading.Tasks.Task<CompositionProToolkit.ICompositionMask> CreateMaskAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color color);
    }
}
namespace CompositionProToolkit.Controls
{
    public partial class FluidProgressRing : Windows.UI.Xaml.Controls.Control
    {
        public static readonly Windows.UI.Xaml.DependencyProperty ActiveNodesProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty MaxNodesProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty NodeColorProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty NodeDurationProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty NodeSizeFactorProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty RingDurationProperty;
        public FluidProgressRing() { }
        public int ActiveNodes { get { return default(int); } set { } }
        public int MaxNodes { get { return default(int); } set { } }
        public Windows.UI.Color NodeColor { get { return default(Windows.UI.Color); } set { } }
        public System.TimeSpan NodeDuration { get { return default(System.TimeSpan); } set { } }
        public double NodeSizeFactor { get { return default(double); } set { } }
        public System.TimeSpan RingDuration { get { return default(System.TimeSpan); } set { } }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { return default(Windows.Foundation.Size); }
    }
}
