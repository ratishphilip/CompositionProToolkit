namespace CompositionProToolkit
{
    public static partial class CompositionGeneratorFactory
    {
        public static CompositionProToolkit.ICompositionGenerator GetCompositionGenerator(Windows.UI.Composition.Compositor compositor, Windows.UI.Composition.CompositionGraphicsDevice graphicsDevice=null, object sharedLock=null) { return default(CompositionProToolkit.ICompositionGenerator); }
    }
    public partial class CompositionSurfaceImageOptions
    {
        public CompositionSurfaceImageOptions(Windows.UI.Xaml.Media.Stretch stretch, Windows.UI.Xaml.Media.AlignmentX hAlign, Windows.UI.Xaml.Media.AlignmentY vAlign, float opacity=1f, Microsoft.Graphics.Canvas.CanvasImageInterpolation interpolation=(Microsoft.Graphics.Canvas.CanvasImageInterpolation)(5)) { }
        public static CompositionProToolkit.CompositionSurfaceImageOptions Default { get { return default(CompositionProToolkit.CompositionSurfaceImageOptions); } }
        public Windows.UI.Xaml.Media.AlignmentX HorizontalAlignment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Windows.UI.Xaml.Media.AlignmentX); } }
        public Microsoft.Graphics.Canvas.CanvasImageInterpolation Interpolation { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Microsoft.Graphics.Canvas.CanvasImageInterpolation); } }
        public float Opacity { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(float); } }
        public Windows.UI.Xaml.Media.Stretch Stretch { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Windows.UI.Xaml.Media.Stretch); } }
        public Windows.UI.Color SurfaceBackgroundColor { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Windows.UI.Color); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public static CompositionProToolkit.CompositionSurfaceImageOptions UniformCenter { get { return default(CompositionProToolkit.CompositionSurfaceImageOptions); } }
        public Windows.UI.Xaml.Media.AlignmentY VerticalAlignment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Windows.UI.Xaml.Media.AlignmentY); } }
    }
    public static partial class CompositorExtensions
    {
        public static Windows.UI.Composition.CompositionEffectBrush CreateMaskedBackdropBrush(this Windows.UI.Composition.Compositor compositor, CompositionProToolkit.ICompositionMask mask, Windows.UI.Color blendColor, float blurAmount, Windows.UI.Composition.CompositionBackdropBrush backdropBrush=null) { return default(Windows.UI.Composition.CompositionEffectBrush); }
    }
    public partial interface ICompositionGenerator : System.IDisposable
    {
        Microsoft.Graphics.Canvas.CanvasDevice Device { get; }
        event System.EventHandler<object> DeviceReplaced;
        System.Threading.Tasks.Task<CompositionProToolkit.ICompositionMask> CreateMaskAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        System.Threading.Tasks.Task<CompositionProToolkit.ICompositionMask> CreateMaskAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush brush);
        System.Threading.Tasks.Task<CompositionProToolkit.ICompositionMask> CreateMaskAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color color);
        System.Threading.Tasks.Task CreateReflectionAsync(Windows.UI.Composition.ContainerVisual visual, float reflectionDistance=0f, float reflectionLength=0.7f, CompositionProToolkit.ReflectionLocation location=(CompositionProToolkit.ReflectionLocation)(0));
        System.Threading.Tasks.Task<CompositionProToolkit.ICompositionSurfaceImage> CreateSurfaceImageAsync(System.Uri uri, Windows.Foundation.Size size, CompositionProToolkit.CompositionSurfaceImageOptions options);
    }
    public partial interface ICompositionMask : System.IDisposable
    {
        CompositionProToolkit.ICompositionGenerator Generator { get; }
        Microsoft.Graphics.Canvas.Geometry.CanvasGeometry Geometry { get; }
        Windows.Foundation.Size Size { get; }
        Windows.UI.Composition.ICompositionSurface Surface { get; }
        System.Threading.Tasks.Task RedrawAsync();
        System.Threading.Tasks.Task RedrawAsync(Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        System.Threading.Tasks.Task RedrawAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        System.Threading.Tasks.Task RedrawAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush brush);
        System.Threading.Tasks.Task RedrawAsync(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color color);
        System.Threading.Tasks.Task ResizeAsync(Windows.Foundation.Size size);
    }
    public partial interface ICompositionSurfaceImage : System.IDisposable
    {
        CompositionProToolkit.ICompositionGenerator Generator { get; }
        CompositionProToolkit.CompositionSurfaceImageOptions Options { get; }
        Windows.Foundation.Size Size { get; }
        Windows.UI.Composition.ICompositionSurface Surface { get; }
        System.Uri Uri { get; }
        System.Threading.Tasks.Task RedrawAsync();
        System.Threading.Tasks.Task RedrawAsync(CompositionProToolkit.CompositionSurfaceImageOptions options);
        System.Threading.Tasks.Task RedrawAsync(System.Uri uri, CompositionProToolkit.CompositionSurfaceImageOptions options);
        System.Threading.Tasks.Task RedrawAsync(System.Uri uri, Windows.Foundation.Size size, CompositionProToolkit.CompositionSurfaceImageOptions options);
        System.Threading.Tasks.Task ResizeAsync(Windows.Foundation.Size size);
        System.Threading.Tasks.Task ResizeAsync(Windows.Foundation.Size size, CompositionProToolkit.CompositionSurfaceImageOptions options);
    }
    public enum ReflectionLocation
    {
        Bottom = 0,
        Left = 2,
        Right = 3,
        Top = 1,
    }
}
namespace CompositionProToolkit.Common
{
    public static partial class Utils
    {
        public static Windows.Foundation.Size CollapseThickness(this Windows.UI.Xaml.Thickness thick) { return default(Windows.Foundation.Size); }
        public static Windows.Foundation.Rect Deflate(this Windows.Foundation.Rect rect, Windows.UI.Xaml.Thickness thick) { return default(Windows.Foundation.Rect); }
        public static Windows.Foundation.Rect Inflate(this Windows.Foundation.Rect rect, Windows.UI.Xaml.Thickness thick) { return default(Windows.Foundation.Rect); }
        public static bool IsCloseTo(this double value1, double value2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Point point1, Windows.Foundation.Point point2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Rect rect1, Windows.Foundation.Rect rect2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Size size1, Windows.Foundation.Size size2) { return default(bool); }
        public static bool IsEqualTo(this Windows.UI.Xaml.Media.Brush brush, Windows.UI.Xaml.Media.Brush otherBrush) { return default(bool); }
        public static bool IsGreaterThan(this double value1, double value2) { return default(bool); }
        public static bool IsLessThan(double value1, double value2) { return default(bool); }
        public static bool IsNaN(double value) { return default(bool); }
        public static bool IsOne(this double value) { return default(bool); }
        public static bool IsOpaqueSolidColorBrush(this Windows.UI.Xaml.Media.Brush brush) { return default(bool); }
        public static bool IsUniform(this Windows.UI.Xaml.CornerRadius corner) { return default(bool); }
        public static bool IsUniform(this Windows.UI.Xaml.Thickness thick) { return default(bool); }
        public static bool IsValid(this Windows.UI.Xaml.CornerRadius corner, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity) { return default(bool); }
        public static bool IsValid(this Windows.UI.Xaml.Thickness thick, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity) { return default(bool); }
        public static bool IsZero(this double value) { return default(bool); }
        public static bool IsZero(this Windows.UI.Xaml.CornerRadius corner) { return default(bool); }
        public static bool IsZero(this Windows.UI.Xaml.Thickness thick) { return default(bool); }
        public static double RoundLayoutValue(double value, double dpiScale) { return default(double); }
    }
}
namespace CompositionProToolkit.Controls
{
    public partial class FluidPointerDragBehavior : Microsoft.Xaml.Interactivity.Behavior<Windows.UI.Xaml.UIElement>
    {
        public static readonly Windows.UI.Xaml.DependencyProperty DragButtonProperty;
        public FluidPointerDragBehavior() { }
        public CompositionProToolkit.Controls.FluidPointerDragBehavior.DragButtonType DragButton { get { return default(CompositionProToolkit.Controls.FluidPointerDragBehavior.DragButtonType); } set { } }
        protected override void OnAttached() { }
        protected override void OnDetaching() { }
        public enum DragButtonType
        {
            MouseLeftButton = 0,
            MouseMiddleButton = 1,
            MouseRightButton = 2,
            Pen = 3,
            Touch = 4,
        }
    }
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
    public sealed partial class FluidWrapPanel : Windows.UI.Xaml.Controls.Panel
    {
        public static System.TimeSpan DefaultFluidAnimationDuration;
        public const double DefaultItemHeight = 10;
        public const double DefaultItemWidth = 10;
        public static System.TimeSpan DefaultOpacityAnimationDuration;
        public static System.TimeSpan DefaultScaleAnimationDuration;
        public const double DragOpacityDefault = 0.7;
        public static readonly Windows.UI.Xaml.DependencyProperty DragOpacityProperty;
        public const double DragScaleDefault = 1.2;
        public static readonly Windows.UI.Xaml.DependencyProperty DragScaleProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty FluidItemsProperty;
        public static System.TimeSpan InitializationAnimationDuration;
        public static readonly Windows.UI.Xaml.DependencyProperty IsComposingProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemHeightProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemsSourceProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemWidthProperty;
        public const double NormalOpacity = 1;
        public const double NormalScale = 1;
        public const double OpacityMin = 0.1;
        public static readonly Windows.UI.Xaml.DependencyProperty OptimizeChildPlacementProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty OrientationProperty;
        public const int ZIndexDrag = 10;
        public const int ZIndexIntermediate = 1;
        public const int ZIndexNormal = 0;
        public FluidWrapPanel() { }
        public double DragOpacity { get { return default(double); } set { } }
        public double DragScale { get { return default(double); } set { } }
        public System.Collections.ObjectModel.ObservableCollection<Windows.UI.Xaml.UIElement> FluidItems { get { return default(System.Collections.ObjectModel.ObservableCollection<Windows.UI.Xaml.UIElement>); } }
        public bool IsComposing { get { return default(bool); } set { } }
        public double ItemHeight { get { return default(double); } set { } }
        public System.Collections.IEnumerable ItemsSource { get { return default(System.Collections.IEnumerable); } set { } }
        public double ItemWidth { get { return default(double); } set { } }
        public bool OptimizeChildPlacement { get { return default(bool); } set { } }
        public Windows.UI.Xaml.Controls.Orientation Orientation { get { return default(Windows.UI.Xaml.Controls.Orientation); } set { } }
        public System.Threading.Tasks.Task AddChildAsync(Windows.UI.Xaml.FrameworkElement child) { return default(System.Threading.Tasks.Task); }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { return default(Windows.Foundation.Size); }
        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize) { return default(Windows.Foundation.Size); }
    }
}
