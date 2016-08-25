namespace CompositionProToolkit
{
    public delegate void CacheProgressHandler(int progress);
    public static partial class CompositionExtensions
    {
        public static Windows.UI.Composition.CompositionEffectBrush CreateMaskedBackdropBrush(this Windows.UI.Composition.Compositor compositor, CompositionProToolkit.ICompositionMask mask, Windows.UI.Color blendColor, float blurAmount, Windows.UI.Composition.CompositionBackdropBrush backdropBrush=null) { return default(Windows.UI.Composition.CompositionEffectBrush); }
        public static void UpdateSurfaceBrushOptions(this Windows.UI.Composition.CompositionSurfaceBrush surfaceBrush, Windows.UI.Xaml.Media.Stretch stretch, Windows.UI.Xaml.Media.AlignmentX alignX, Windows.UI.Xaml.Media.AlignmentY alignY, Windows.UI.Composition.ScalarKeyFrameAnimation alignXAnimation=null, Windows.UI.Composition.ScalarKeyFrameAnimation alignYAnimation=null) { }
    }
    public static partial class CompositionGeneratorFactory
    {
        public static CompositionProToolkit.ICompositionGenerator GetCompositionGenerator(Windows.UI.Composition.CompositionGraphicsDevice graphicsDevice) { return default(CompositionProToolkit.ICompositionGenerator); }
        public static CompositionProToolkit.ICompositionGenerator GetCompositionGenerator(Windows.UI.Composition.Compositor compositor, bool useSharedCanvasDevice=true, bool useSoftwareRenderer=false) { return default(CompositionProToolkit.ICompositionGenerator); }
    }
    public partial class CompositionSurfaceImageOptions
    {
        public CompositionSurfaceImageOptions() { }
        public bool AutoResize { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public static CompositionProToolkit.CompositionSurfaceImageOptions Default { get { return default(CompositionProToolkit.CompositionSurfaceImageOptions); } }
        public static CompositionProToolkit.CompositionSurfaceImageOptions DefaultOptimized { get { return default(CompositionProToolkit.CompositionSurfaceImageOptions); } }
        public Windows.UI.Xaml.Media.AlignmentX HorizontalAlignment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Windows.UI.Xaml.Media.AlignmentX); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.Graphics.Canvas.CanvasImageInterpolation Interpolation { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Microsoft.Graphics.Canvas.CanvasImageInterpolation); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public float Opacity { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(float); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Windows.UI.Xaml.Media.Stretch Stretch { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Windows.UI.Xaml.Media.Stretch); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Windows.UI.Color SurfaceBackgroundColor { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Windows.UI.Color); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Windows.UI.Xaml.Media.AlignmentY VerticalAlignment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(Windows.UI.Xaml.Media.AlignmentY); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial interface ICompositionGenerator : System.IDisposable
    {
        Microsoft.Graphics.Canvas.CanvasDevice Device { get; }
        event System.EventHandler<object> DeviceReplaced;
        CompositionProToolkit.ICompositionMask CreateMask(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        CompositionProToolkit.ICompositionMask CreateMask(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush brush);
        CompositionProToolkit.ICompositionMask CreateMask(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color color);
        void CreateReflection(Windows.UI.Composition.ContainerVisual visual, float reflectionDistance=0f, float reflectionLength=0.7f, CompositionProToolkit.ReflectionLocation location=(CompositionProToolkit.ReflectionLocation)(0));
        System.Threading.Tasks.Task<CompositionProToolkit.ICompositionSurfaceImage> CreateSurfaceImageAsync(System.Uri uri, Windows.Foundation.Size size, CompositionProToolkit.CompositionSurfaceImageOptions options);
    }
    public partial interface ICompositionMask : System.IDisposable
    {
        CompositionProToolkit.ICompositionGenerator Generator { get; }
        Microsoft.Graphics.Canvas.Geometry.CanvasGeometry Geometry { get; }
        Windows.Foundation.Size Size { get; }
        Windows.UI.Composition.ICompositionSurface Surface { get; }
        void Redraw();
        void Redraw(Microsoft.Graphics.Canvas.Brushes.ICanvasBrush brush);
        void Redraw(Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush brush);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color color);
        void Redraw(Windows.UI.Color color);
        void Resize(Windows.Foundation.Size size);
    }
    public partial interface ICompositionSurfaceImage : System.IDisposable
    {
        CompositionProToolkit.ICompositionGenerator Generator { get; }
        CompositionProToolkit.CompositionSurfaceImageOptions Options { get; }
        Windows.Foundation.Size Size { get; }
        Windows.UI.Composition.ICompositionSurface Surface { get; }
        System.Uri Uri { get; }
        void Redraw(CompositionProToolkit.CompositionSurfaceImageOptions options);
        System.Threading.Tasks.Task RedrawAsync();
        System.Threading.Tasks.Task RedrawAsync(System.Uri uri, CompositionProToolkit.CompositionSurfaceImageOptions options);
        System.Threading.Tasks.Task RedrawAsync(System.Uri uri, Windows.Foundation.Size size, CompositionProToolkit.CompositionSurfaceImageOptions options);
        void Resize(Windows.Foundation.Size size);
        void Resize(Windows.Foundation.Size size, CompositionProToolkit.CompositionSurfaceImageOptions options);
    }
    public static partial class ImageCache
    {
        public static System.Threading.Tasks.Task<System.Uri> GetCachedUriAsync(System.Uri uri, CompositionProToolkit.CacheProgressHandler progressHandler=null) { return default(System.Threading.Tasks.Task<System.Uri>); }
        public static System.Threading.Tasks.Task<System.Uri> GetCachedUriAsync(Windows.Storage.StorageFile file, CompositionProToolkit.CacheProgressHandler progressHandler=null) { return default(System.Threading.Tasks.Task<System.Uri>); }
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
        public static double ConvertToValidCornerValue(double corner) { return default(double); }
        public static Windows.Foundation.Rect Deflate(this Windows.Foundation.Rect rect, Windows.UI.Xaml.Thickness thick) { return default(Windows.Foundation.Rect); }
        public static Windows.Foundation.Rect Inflate(this Windows.Foundation.Rect rect, Windows.UI.Xaml.Thickness thick) { return default(Windows.Foundation.Rect); }
        public static bool IsCloseTo(this double value1, double value2) { return default(bool); }
        public static bool IsCloseTo(this float value1, float value2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Point point1, Windows.Foundation.Point point2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Rect rect1, Windows.Foundation.Rect rect2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Size size1, Windows.Foundation.Size size2) { return default(bool); }
        public static bool IsEqualTo(this System.Uri uri, System.Uri otherUri) { return default(bool); }
        public static bool IsEqualTo(this Windows.UI.Xaml.Media.Brush brush, Windows.UI.Xaml.Media.Brush otherBrush) { return default(bool); }
        public static bool IsGreaterThan(this double value1, double value2) { return default(bool); }
        public static bool IsGreaterThan(this float value1, float value2) { return default(bool); }
        public static bool IsLessThan(double value1, double value2) { return default(bool); }
        public static bool IsLessThan(float value1, float value2) { return default(bool); }
        public static bool IsNaN(double value) { return default(bool); }
        public static bool IsOne(this double value) { return default(bool); }
        public static bool IsOne(this float value) { return default(bool); }
        public static bool IsOpaqueSolidColorBrush(this Windows.UI.Xaml.Media.Brush brush) { return default(bool); }
        public static bool IsUniform(this Windows.UI.Xaml.CornerRadius corner) { return default(bool); }
        public static bool IsUniform(this Windows.UI.Xaml.Thickness thick) { return default(bool); }
        public static bool IsValid(this Windows.UI.Xaml.CornerRadius corner, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity) { return default(bool); }
        public static bool IsValid(this Windows.UI.Xaml.Thickness thick, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity) { return default(bool); }
        public static bool IsZero(this double value) { return default(bool); }
        public static bool IsZero(this float value) { return default(bool); }
        public static bool IsZero(this Windows.UI.Xaml.CornerRadius corner) { return default(bool); }
        public static bool IsZero(this Windows.UI.Xaml.Thickness thick) { return default(bool); }
        public static double RoundLayoutValue(double value, double dpiScale) { return default(double); }
    }
}
namespace CompositionProToolkit.CompositionProToolkit_XamlTypeInfo
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks", "14.0.0.0")]
    public sealed partial class XamlMetaDataProvider : Windows.UI.Xaml.Markup.IXamlMetadataProvider
    {
        public XamlMetaDataProvider() { }
        public Windows.UI.Xaml.Markup.IXamlType GetXamlType(string fullName) { return default(Windows.UI.Xaml.Markup.IXamlType); }
        public Windows.UI.Xaml.Markup.IXamlType GetXamlType(System.Type type) { return default(Windows.UI.Xaml.Markup.IXamlType); }
        public Windows.UI.Xaml.Markup.XmlnsDefinition[] GetXmlnsDefinitions() { return default(Windows.UI.Xaml.Markup.XmlnsDefinition[]); }
    }
}
namespace CompositionProToolkit.Controls
{
    public partial class CompositionImageEventArgs : Windows.UI.Xaml.RoutedEventArgs
    {
        public CompositionImageEventArgs(System.Uri source, string message) { }
        public string Message { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } }
        public System.Uri Source { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Uri); } }
    }
    public sealed partial class CompositionImageFrame : Windows.UI.Xaml.Controls.Control
    {
        public static readonly Windows.UI.Xaml.DependencyProperty AlignXProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty AlignYProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty CornerRadiusProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty DisplayShadowProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty FrameBackgroundProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty InterpolationProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty PlaceholderBackgroundProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty PlaceholderColorProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty RenderOptimizedProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ShadowBlurRadiusProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ShadowColorProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ShadowOffsetXProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ShadowOffsetYProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ShadowOpacityProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ShowPlaceholderProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty SourceProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty StretchProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty TransitionDurationProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty UseImageCacheProperty;
        public CompositionImageFrame() { }
        public Windows.UI.Xaml.Media.AlignmentX AlignX { get { return default(Windows.UI.Xaml.Media.AlignmentX); } set { } }
        public Windows.UI.Xaml.Media.AlignmentY AlignY { get { return default(Windows.UI.Xaml.Media.AlignmentY); } set { } }
        public Windows.UI.Xaml.CornerRadius CornerRadius { get { return default(Windows.UI.Xaml.CornerRadius); } set { } }
        public bool DisplayShadow { get { return default(bool); } set { } }
        public Windows.UI.Color FrameBackground { get { return default(Windows.UI.Color); } set { } }
        public Microsoft.Graphics.Canvas.CanvasImageInterpolation Interpolation { get { return default(Microsoft.Graphics.Canvas.CanvasImageInterpolation); } set { } }
        public Windows.UI.Color PlaceholderBackground { get { return default(Windows.UI.Color); } set { } }
        public Windows.UI.Color PlaceholderColor { get { return default(Windows.UI.Color); } set { } }
        public bool RenderOptimized { get { return default(bool); } set { } }
        public double ShadowBlurRadius { get { return default(double); } set { } }
        public Windows.UI.Color ShadowColor { get { return default(Windows.UI.Color); } set { } }
        public double ShadowOffsetX { get { return default(double); } set { } }
        public double ShadowOffsetY { get { return default(double); } set { } }
        public double ShadowOpacity { get { return default(double); } set { } }
        public bool ShowPlaceholder { get { return default(bool); } set { } }
        public System.Uri Source { get { return default(System.Uri); } set { } }
        public Windows.UI.Xaml.Media.Stretch Stretch { get { return default(Windows.UI.Xaml.Media.Stretch); } set { } }
        public System.TimeSpan TransitionDuration { get { return default(System.TimeSpan); } set { } }
        public bool UseImageCache { get { return default(bool); } set { } }
        public event Windows.UI.Xaml.RoutedEventHandler ImageFailed { add { } remove { } }
        public event Windows.UI.Xaml.RoutedEventHandler ImageOpened { add { } remove { } }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { return default(Windows.Foundation.Size); }
        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize) { return default(Windows.Foundation.Size); }
    }
    public sealed partial class FluidBanner : Windows.UI.Xaml.Controls.Panel
    {
        public static readonly Windows.UI.Xaml.DependencyProperty AlignXProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty AlignYProperty;
        public const int DefaultDecodeHeight = 0;
        public const int DefaultDecodeWidth = 0;
        public static readonly Windows.UI.Color DefaultItemBackground;
        public const double DefaultItemGap = 30;
        public static System.TimeSpan DefaultOpacityAnimationDuration;
        public static System.TimeSpan DefaultScaleAnimationDuration;
        public const float HoverScaleFactor = 1.1f;
        public static System.TimeSpan InsetAnimationDelayDuration;
        public static System.TimeSpan InsetAnimationDuration;
        public static System.TimeSpan InsetClipAnimationDuration;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemBackgroundProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemGapProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemsSourceProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty PaddingProperty;
        public const float ScaleDownFactor = 0.7f;
        public static readonly Windows.UI.Xaml.DependencyProperty StretchProperty;
        public const float TargetOpacity = 0f;
        public FluidBanner() { }
        public Windows.UI.Xaml.Media.AlignmentX AlignX { get { return default(Windows.UI.Xaml.Media.AlignmentX); } set { } }
        public Windows.UI.Xaml.Media.AlignmentY AlignY { get { return default(Windows.UI.Xaml.Media.AlignmentY); } set { } }
        public Windows.UI.Color ItemBackground { get { return default(Windows.UI.Color); } set { } }
        public double ItemGap { get { return default(double); } set { } }
        public System.Collections.Generic.IEnumerable<System.Uri> ItemsSource { get { return default(System.Collections.Generic.IEnumerable<System.Uri>); } set { } }
        public Windows.UI.Xaml.Thickness Padding { get { return default(Windows.UI.Xaml.Thickness); } set { } }
        public Windows.UI.Xaml.Media.Stretch Stretch { get { return default(Windows.UI.Xaml.Media.Stretch); } set { } }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { return default(Windows.Foundation.Size); }
        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize) { return default(Windows.Foundation.Size); }
    }
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
