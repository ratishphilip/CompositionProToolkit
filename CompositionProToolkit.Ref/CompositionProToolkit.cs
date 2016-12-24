namespace CompositionProToolkit
{
    public delegate void CacheProgressHandler(int progress);
    public static partial class CompositionExtensions
    {
        public static Windows.UI.Composition.CompositionEffectBrush CreateFrostedGlassBrush(this Windows.UI.Composition.Compositor compositor, CompositionProToolkit.IMaskSurface mask, Windows.UI.Color blendColor, float blurAmount, Windows.UI.Composition.CompositionBackdropBrush backdropBrush=null, float multiplyAmount=0f, float colorAmount=0.5f, float backdropAmount=0.5f) { throw null; }
        public static Windows.UI.Composition.CompositionEffectBrush CreateMaskedBackdropBrush(this Windows.UI.Composition.Compositor compositor, CompositionProToolkit.IMaskSurface mask, Windows.UI.Color blendColor, float blurAmount, Windows.UI.Composition.CompositionBackdropBrush backdropBrush=null) { throw null; }
        public static System.Collections.Generic.IEnumerable<Windows.UI.Xaml.DependencyObject> GetAncestors(this Windows.UI.Xaml.DependencyObject dependencyObject) { throw null; }
        public static System.Collections.Generic.IEnumerable<T> GetAncestorsOfType<T>(this Windows.UI.Xaml.DependencyObject dependencyObject) where T : Windows.UI.Xaml.DependencyObject { throw null; }
        public static System.Collections.Generic.IEnumerable<Windows.UI.Xaml.DependencyObject> GetDescendants(this Windows.UI.Xaml.DependencyObject dependencyObject) { throw null; }
        public static System.Collections.Generic.IEnumerable<T> GetDescendantsOfType<T>(this Windows.UI.Xaml.DependencyObject dependencyObject) where T : Windows.UI.Xaml.DependencyObject { throw null; }
        public static T GetFirstAncestorOfType<T>(this Windows.UI.Xaml.DependencyObject dependencyObject) where T : Windows.UI.Xaml.DependencyObject { throw null; }
        public static T GetFirstDescendantOfType<T>(this Windows.UI.Xaml.DependencyObject dependencyObject) where T : Windows.UI.Xaml.DependencyObject { throw null; }
        public static bool IsInVisualTree(this Windows.UI.Xaml.DependencyObject dependencyObject) { throw null; }
        public static void UpdateSurfaceBrushOptions(this Windows.UI.Composition.CompositionSurfaceBrush surfaceBrush, Windows.UI.Xaml.Media.Stretch stretch, Windows.UI.Xaml.Media.AlignmentX alignX, Windows.UI.Xaml.Media.AlignmentY alignY, Windows.UI.Composition.ScalarKeyFrameAnimation alignXAnimation=null, Windows.UI.Composition.ScalarKeyFrameAnimation alignYAnimation=null) { }
    }
    public static partial class CompositionGeneratorFactory
    {
        public static CompositionProToolkit.ICompositionGenerator GetCompositionGenerator(Windows.UI.Composition.CompositionGraphicsDevice graphicsDevice) { throw null; }
        public static CompositionProToolkit.ICompositionGenerator GetCompositionGenerator(Windows.UI.Composition.Compositor compositor, bool useSharedCanvasDevice=true, bool useSoftwareRenderer=false) { throw null; }
    }
    public partial interface ICompositionGenerator : System.IDisposable
    {
        Microsoft.Graphics.Canvas.CanvasDevice Device { get; }
        event System.EventHandler<object> DeviceReplaced;
        CompositionProToolkit.IGeometrySurface CreateGeometrySurface(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush);
        CompositionProToolkit.IGeometrySurface CreateGeometrySurface(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush backgroundBrush);
        CompositionProToolkit.IGeometrySurface CreateGeometrySurface(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush, Windows.UI.Color backgroundColor);
        CompositionProToolkit.IGeometrySurface CreateGeometrySurface(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color foregroundColor);
        CompositionProToolkit.IGeometrySurface CreateGeometrySurface(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color foregroundColor, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush backgroundBrush);
        CompositionProToolkit.IGeometrySurface CreateGeometrySurface(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color foregroundColor, Windows.UI.Color backgroundColor);
        System.Threading.Tasks.Task<CompositionProToolkit.IImageSurface> CreateImageSurfaceAsync(System.Uri uri, Windows.Foundation.Size size, CompositionProToolkit.ImageSurfaceOptions options);
        CompositionProToolkit.IMaskSurface CreateMaskSurface(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        void CreateReflection(Windows.UI.Composition.ContainerVisual visual, float reflectionDistance=0f, float reflectionLength=0.7f, CompositionProToolkit.ReflectionLocation location=(CompositionProToolkit.ReflectionLocation)(0));
    }
    public partial interface IGeometrySurface : CompositionProToolkit.IRenderSurface, System.IDisposable
    {
        Microsoft.Graphics.Canvas.Brushes.ICanvasBrush BackgroundBrush { get; }
        Microsoft.Graphics.Canvas.Brushes.ICanvasBrush ForegroundBrush { get; }
        Microsoft.Graphics.Canvas.Geometry.CanvasGeometry Geometry { get; }
        void Redraw(Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush);
        void Redraw(Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush backgroundBrush);
        void Redraw(Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush, Windows.UI.Color backgroundColor);
        void Redraw(Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush backgroundBrush);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush foregroundBrush, Windows.UI.Color backgroundColor);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color foregroundColor);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color foregroundColor, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush backgroundBrush);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry, Windows.UI.Color foregroundColor, Windows.UI.Color backgroundColor);
        void Redraw(Windows.UI.Color foregroundColor);
        void Redraw(Windows.UI.Color foregroundColor, Microsoft.Graphics.Canvas.Brushes.ICanvasBrush backgroundBrush);
        void Redraw(Windows.UI.Color foregroundColor, Windows.UI.Color backgroundColor);
    }
    public partial interface IImageSurface : CompositionProToolkit.IRenderSurface, System.IDisposable
    {
        CompositionProToolkit.ImageSurfaceOptions Options { get; }
        System.Uri Uri { get; }
        void Redraw(CompositionProToolkit.ImageSurfaceOptions options);
        System.Threading.Tasks.Task RedrawAsync(System.Uri uri, CompositionProToolkit.ImageSurfaceOptions options);
        System.Threading.Tasks.Task RedrawAsync(System.Uri uri, Windows.Foundation.Size size, CompositionProToolkit.ImageSurfaceOptions options);
        void Resize(Windows.Foundation.Size size, CompositionProToolkit.ImageSurfaceOptions options);
    }
    public static partial class ImageCache
    {
        public static System.Threading.Tasks.Task<System.Uri> GetCachedUriAsync(object objectToCache, CompositionProToolkit.CacheProgressHandler progressHandler=null) { throw null; }
    }
    public partial class ImageSurfaceOptions
    {
        public ImageSurfaceOptions() { }
        public bool AutoResize { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public static CompositionProToolkit.ImageSurfaceOptions Default { get { throw null; } }
        public static CompositionProToolkit.ImageSurfaceOptions DefaultOptimized { get { throw null; } }
        public Windows.UI.Xaml.Media.AlignmentX HorizontalAlignment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.Graphics.Canvas.CanvasImageInterpolation Interpolation { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public float Opacity { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Windows.UI.Xaml.Media.Stretch Stretch { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Windows.UI.Color SurfaceBackgroundColor { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Windows.UI.Xaml.Media.AlignmentY VerticalAlignment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial interface IMaskSurface : CompositionProToolkit.IRenderSurface, System.IDisposable
    {
        Microsoft.Graphics.Canvas.Geometry.CanvasGeometry Geometry { get; }
        void Redraw(Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
        void Redraw(Windows.Foundation.Size size, Microsoft.Graphics.Canvas.Geometry.CanvasGeometry geometry);
    }
    public partial interface IRenderSurface : System.IDisposable
    {
        CompositionProToolkit.ICompositionGenerator Generator { get; }
        Windows.Foundation.Size Size { get; }
        Windows.UI.Composition.ICompositionSurface Surface { get; }
        void Redraw();
        void Resize(Windows.Foundation.Size size);
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
    public static partial class Float
    {
        public const float DegreeToRadians = 0.0174532924f;
        public const float Pi = 3.14159274f;
        public const float PiByFour = 0.7853982f;
        public const float PiBySix = 0.5235988f;
        public const float PiByThree = 1.04719758f;
        public const float PiByTwo = 1.57079637f;
        public const float RadiansToDegree = 57.2957764f;
        public const float TwoPi = 6.28318548f;
    }
    public static partial class Utils
    {
        public static Windows.Foundation.Size CollapseThickness(this Windows.UI.Xaml.Thickness thick) { throw null; }
        public static double ConvertToValidCornerValue(double corner) { throw null; }
        public static Windows.Foundation.Rect Deflate(this Windows.Foundation.Rect rect, Windows.UI.Xaml.Thickness thick) { throw null; }
        public static Windows.Foundation.Rect Inflate(this Windows.Foundation.Rect rect, Windows.UI.Xaml.Thickness thick) { throw null; }
        public static bool IsCloseTo(this double value1, double value2) { throw null; }
        public static bool IsCloseTo(this float value1, float value2) { throw null; }
        public static bool IsCloseTo(this Windows.Foundation.Point point1, Windows.Foundation.Point point2) { throw null; }
        public static bool IsCloseTo(this Windows.Foundation.Rect rect1, Windows.Foundation.Rect rect2) { throw null; }
        public static bool IsCloseTo(this Windows.Foundation.Size size1, Windows.Foundation.Size size2) { throw null; }
        public static bool IsEqualTo(this System.Uri uri, System.Uri otherUri) { throw null; }
        public static bool IsEqualTo(this Windows.UI.Xaml.Media.Brush brush, Windows.UI.Xaml.Media.Brush otherBrush) { throw null; }
        public static bool IsGreaterThan(this double value1, double value2) { throw null; }
        public static bool IsGreaterThan(this float value1, float value2) { throw null; }
        public static bool IsLessThan(double value1, double value2) { throw null; }
        public static bool IsLessThan(float value1, float value2) { throw null; }
        public static bool IsNaN(double value) { throw null; }
        public static bool IsOne(this double value) { throw null; }
        public static bool IsOne(this float value) { throw null; }
        public static bool IsOpaqueSolidColorBrush(this Windows.UI.Xaml.Media.Brush brush) { throw null; }
        public static bool IsUniform(this Windows.UI.Xaml.CornerRadius corner) { throw null; }
        public static bool IsUniform(this Windows.UI.Xaml.Thickness thick) { throw null; }
        public static bool IsValid(this Windows.UI.Xaml.CornerRadius corner, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity) { throw null; }
        public static bool IsValid(this Windows.UI.Xaml.Thickness thick, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity) { throw null; }
        public static bool IsZero(this double value) { throw null; }
        public static bool IsZero(this float value) { throw null; }
        public static bool IsZero(this Windows.UI.Xaml.CornerRadius corner) { throw null; }
        public static bool IsZero(this Windows.UI.Xaml.Thickness thick) { throw null; }
        public static System.Numerics.Vector2 Reflect(System.Numerics.Vector2 a, System.Numerics.Vector2 b) { throw null; }
        public static double RoundLayoutValue(double value, double dpiScale) { throw null; }
    }
}
namespace CompositionProToolkit.CompositionProToolkit_XamlTypeInfo
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks", "14.0.0.0")]
    public sealed partial class XamlMetaDataProvider : Windows.UI.Xaml.Markup.IXamlMetadataProvider
    {
        public XamlMetaDataProvider() { }
        public Windows.UI.Xaml.Markup.IXamlType GetXamlType(string fullName) { throw null; }
        public Windows.UI.Xaml.Markup.IXamlType GetXamlType(System.Type type) { throw null; }
        public Windows.UI.Xaml.Markup.XmlnsDefinition[] GetXmlnsDefinitions() { throw null; }
    }
}
namespace CompositionProToolkit.Controls
{
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
        public Windows.UI.Xaml.Media.AlignmentX AlignX { get { throw null; } set { } }
        public Windows.UI.Xaml.Media.AlignmentY AlignY { get { throw null; } set { } }
        public Windows.UI.Color ItemBackground { get { throw null; } set { } }
        public double ItemGap { get { throw null; } set { } }
        public System.Collections.Generic.IEnumerable<System.Uri> ItemsSource { get { throw null; } set { } }
        public Windows.UI.Xaml.Thickness Padding { get { throw null; } set { } }
        public Windows.UI.Xaml.Media.Stretch Stretch { get { throw null; } set { } }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { throw null; }
        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize) { throw null; }
    }
    public partial class FluidPointerDragBehavior : Microsoft.Xaml.Interactivity.Behavior<Windows.UI.Xaml.UIElement>
    {
        public static readonly Windows.UI.Xaml.DependencyProperty DragButtonProperty;
        public FluidPointerDragBehavior() { }
        public CompositionProToolkit.Controls.FluidPointerDragBehavior.DragButtonType DragButton { get { throw null; } set { } }
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
        public int ActiveNodes { get { throw null; } set { } }
        public int MaxNodes { get { throw null; } set { } }
        public Windows.UI.Color NodeColor { get { throw null; } set { } }
        public System.TimeSpan NodeDuration { get { throw null; } set { } }
        public double NodeSizeFactor { get { throw null; } set { } }
        public System.TimeSpan RingDuration { get { throw null; } set { } }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { throw null; }
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
        public double DragOpacity { get { throw null; } set { } }
        public double DragScale { get { throw null; } set { } }
        public System.Collections.ObjectModel.ObservableCollection<Windows.UI.Xaml.UIElement> FluidItems { get { throw null; } }
        public bool IsComposing { get { throw null; } set { } }
        public double ItemHeight { get { throw null; } set { } }
        public System.Collections.IEnumerable ItemsSource { get { throw null; } set { } }
        public double ItemWidth { get { throw null; } set { } }
        public bool OptimizeChildPlacement { get { throw null; } set { } }
        public Windows.UI.Xaml.Controls.Orientation Orientation { get { throw null; } set { } }
        public System.Threading.Tasks.Task AddChildAsync(Windows.UI.Xaml.FrameworkElement child) { throw null; }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { throw null; }
        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize) { throw null; }
    }
    public sealed partial class ImageFrame : Windows.UI.Xaml.Controls.Control, System.IDisposable
    {
        public static readonly Windows.UI.Xaml.DependencyProperty AlignXProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty AlignYProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty CornerRadiusProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty DisplayShadowProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty FrameBackgroundProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty InterpolationProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty OptimizeShadowProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty PlaceholderBackgroundProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty PlaceholderColorProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty RenderFastProperty;
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
        public static readonly Windows.UI.Xaml.DependencyProperty TransitionModeProperty;
        public ImageFrame() { }
        public Windows.UI.Xaml.Media.AlignmentX AlignX { get { throw null; } set { } }
        public Windows.UI.Xaml.Media.AlignmentY AlignY { get { throw null; } set { } }
        public Windows.UI.Xaml.CornerRadius CornerRadius { get { throw null; } set { } }
        public bool DisplayShadow { get { throw null; } set { } }
        public Windows.UI.Color FrameBackground { get { throw null; } set { } }
        public Microsoft.Graphics.Canvas.CanvasImageInterpolation Interpolation { get { throw null; } set { } }
        public bool OptimizeShadow { get { throw null; } set { } }
        public Windows.UI.Color PlaceholderBackground { get { throw null; } set { } }
        public Windows.UI.Color PlaceholderColor { get { throw null; } set { } }
        public bool RenderFast { get { throw null; } set { } }
        public bool RenderOptimized { get { throw null; } set { } }
        public double ShadowBlurRadius { get { throw null; } set { } }
        public Windows.UI.Color ShadowColor { get { throw null; } set { } }
        public double ShadowOffsetX { get { throw null; } set { } }
        public double ShadowOffsetY { get { throw null; } set { } }
        public double ShadowOpacity { get { throw null; } set { } }
        public bool ShowPlaceholder { get { throw null; } set { } }
        public object Source { get { throw null; } set { } }
        public Windows.UI.Xaml.Media.Stretch Stretch { get { throw null; } set { } }
        public System.TimeSpan TransitionDuration { get { throw null; } set { } }
        public CompositionProToolkit.Controls.TransitionModeType TransitionMode { get { throw null; } set { } }
        public event Windows.UI.Xaml.RoutedEventHandler ImageFailed { add { } remove { } }
        public event Windows.UI.Xaml.RoutedEventHandler ImageOpened { add { } remove { } }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { throw null; }
        public void Dispose() { }
        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize) { throw null; }
    }
    public partial class ImageFrameEventArgs : Windows.UI.Xaml.RoutedEventArgs
    {
        public ImageFrameEventArgs(object source, string message) { }
        public string Message { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public object Source { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public enum TransitionModeType
    {
        FadeIn = 0,
        SlideDown = 4,
        SlideLeft = 1,
        SlideRight = 2,
        SlideUp = 3,
        ZoomIn = 5,
    }
}
namespace CompositionProToolkit.Expressions
{
    public static partial class CompositionAnimationExtensions
    {
        public static Windows.UI.Composition.KeyFrameAnimation InsertExpressionKeyFrame<T>(this Windows.UI.Composition.KeyFrameAnimation animation, float normalizedProgressKey, System.Linq.Expressions.Expression<CompositionProToolkit.Expressions.CompositionLambda<T>> expression, Windows.UI.Composition.CompositionEasingFunction easingFunction=null) { throw null; }
        public static System.Collections.Generic.Dictionary<string, object> SetExpression<T>(this Windows.UI.Composition.ExpressionAnimation animation, System.Linq.Expressions.Expression<CompositionProToolkit.Expressions.CompositionLambda<T>> expression) { throw null; }
        public static bool SetParameter<T>(this T animation, string key, object input) where T : Windows.UI.Composition.CompositionAnimation { throw null; }
        public static T SetParameters<T>(this T animation, System.Collections.Generic.Dictionary<string, object> parameters) where T : Windows.UI.Composition.CompositionAnimation { throw null; }
    }
    public partial class CompositionExpressionContext<T>
    {
        internal CompositionExpressionContext() { }
        public T FinalValue { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public T StartingValue { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Numerics.Vector2 Abs(System.Numerics.Vector2 value) { throw null; }
        public System.Numerics.Vector3 Abs(System.Numerics.Vector3 value) { throw null; }
        public System.Numerics.Vector4 Abs(System.Numerics.Vector4 value) { throw null; }
        public float Abs(float value) { throw null; }
        public float Acos(float value) { throw null; }
        public float Asin(float value) { throw null; }
        public float Atan(float value) { throw null; }
        public float Ceiling(float value) { throw null; }
        public float Clamp(float value, float min, float max) { throw null; }
        public Windows.UI.Color ColorLerp(Windows.UI.Color colorTo, Windows.UI.Color colorFrom, float progression) { throw null; }
        public Windows.UI.Color ColorLerpHSL(Windows.UI.Color colorTo, Windows.UI.Color colorFrom, float progression) { throw null; }
        public Windows.UI.Color ColorLerpRGB(Windows.UI.Color colorTo, Windows.UI.Color colorFrom, float progression) { throw null; }
        public System.Numerics.Quaternion Concatenate(System.Numerics.Quaternion value, System.Numerics.Quaternion value2) { throw null; }
        public float Cos(float value) { throw null; }
        public System.Numerics.Vector2 Distance(System.Numerics.Vector2 value1, System.Numerics.Vector2 value2) { throw null; }
        public System.Numerics.Vector3 Distance(System.Numerics.Vector3 value1, System.Numerics.Vector3 value2) { throw null; }
        public System.Numerics.Vector4 Distance(System.Numerics.Vector4 value1, System.Numerics.Vector4 value2) { throw null; }
        public float DistanceSquared(System.Numerics.Vector2 value1, System.Numerics.Vector2 value2) { throw null; }
        public float DistanceSquared(System.Numerics.Vector3 value1, System.Numerics.Vector3 value2) { throw null; }
        public float DistanceSquared(System.Numerics.Vector4 value1, System.Numerics.Vector4 value2) { throw null; }
        public float Floor(float value) { throw null; }
        public System.Numerics.Vector2 Inverse(System.Numerics.Vector2 value) { throw null; }
        public System.Numerics.Vector3 Inverse(System.Numerics.Vector3 value) { throw null; }
        public System.Numerics.Vector4 Inverse(System.Numerics.Vector4 value) { throw null; }
        public System.Numerics.Vector2 Length(System.Numerics.Vector2 value) { throw null; }
        public System.Numerics.Vector3 Length(System.Numerics.Vector3 value) { throw null; }
        public System.Numerics.Vector4 Length(System.Numerics.Vector4 value) { throw null; }
        public System.Numerics.Vector2 LengthSquared(System.Numerics.Vector2 value) { throw null; }
        public System.Numerics.Vector3 LengthSquared(System.Numerics.Vector3 value) { throw null; }
        public System.Numerics.Vector4 LengthSquared(System.Numerics.Vector4 value) { throw null; }
        public System.Numerics.Matrix3x2 Lerp(System.Numerics.Matrix3x2 value1, System.Numerics.Matrix3x2 value2, float progress) { throw null; }
        public System.Numerics.Matrix4x4 Lerp(System.Numerics.Matrix4x4 value1, System.Numerics.Matrix4x4 value2, float progress) { throw null; }
        public System.Numerics.Vector2 Lerp(System.Numerics.Vector2 value1, System.Numerics.Vector2 value2, float progress) { throw null; }
        public System.Numerics.Vector3 Lerp(System.Numerics.Vector3 value1, System.Numerics.Vector3 value2, float progress) { throw null; }
        public System.Numerics.Vector4 Lerp(System.Numerics.Vector4 value1, System.Numerics.Vector4 value2, float progress) { throw null; }
        public float Ln(float value) { throw null; }
        public float Log10(float value) { throw null; }
        public System.Numerics.Matrix3x2 Matrix3x2(float M11, float M12, float M21, float M22, float M31, float M32) { throw null; }
        public System.Numerics.Matrix3x2 Matrix3x2CreateFromScale(System.Numerics.Vector2 scale) { throw null; }
        public System.Numerics.Matrix3x2 Matrix3x2CreateFromTranslation(System.Numerics.Vector2 translation) { throw null; }
        public System.Numerics.Matrix4x4 Matrix4x4(float M11, float M12, float M13, float M14, float M21, float M22, float M23, float M24, float M31, float M32, float M33, float M34, float M41, float M42, float M43, float M44) { throw null; }
        public System.Numerics.Matrix4x4 Matrix4x4CreateFromAxisAngle(System.Numerics.Vector3 axis, float angle) { throw null; }
        public System.Numerics.Matrix4x4 Matrix4x4CreateFromScale(System.Numerics.Vector3 scale) { throw null; }
        public System.Numerics.Matrix4x4 Matrix4x4CreateFromTranslation(System.Numerics.Vector3 translation) { throw null; }
        public float Max(float value1, float value2) { throw null; }
        public float Min(float value1, float value2) { throw null; }
        public float Mod(float dividend, float divisor) { throw null; }
        public float Normalize() { throw null; }
        public System.Numerics.Vector2 Normalize(System.Numerics.Vector2 value) { throw null; }
        public System.Numerics.Vector3 Normalize(System.Numerics.Vector3 value) { throw null; }
        public System.Numerics.Vector4 Normalize(System.Numerics.Vector4 value) { throw null; }
        public float Pow(float value, int power) { throw null; }
        public System.Numerics.Quaternion QuaternionCreateFromAxisAngle(System.Numerics.Vector3 axis, float angle) { throw null; }
        public float Round(float value) { throw null; }
        public System.Numerics.Matrix3x2 Scale(System.Numerics.Matrix3x2 value, float factor) { throw null; }
        public System.Numerics.Matrix4x4 Scale(System.Numerics.Matrix4x4 value, float factor) { throw null; }
        public System.Numerics.Vector2 Scale(System.Numerics.Vector2 value, float factor) { throw null; }
        public System.Numerics.Vector3 Scale(System.Numerics.Vector3 value, float factor) { throw null; }
        public System.Numerics.Vector4 Scale(System.Numerics.Vector4 value, float factor) { throw null; }
        public float Sin(float value) { throw null; }
        public System.Numerics.Quaternion Slerp(System.Numerics.Quaternion value1, System.Numerics.Quaternion value2, float progress) { throw null; }
        public float Sqrt(float value) { throw null; }
        public float Square(float value) { throw null; }
        public float Tan(float value) { throw null; }
        public float ToDegrees(float radians) { throw null; }
        public float ToRadians(float degrees) { throw null; }
        public System.Numerics.Vector2 Transform(System.Numerics.Vector2 value, System.Numerics.Matrix3x2 matrix) { throw null; }
        public System.Numerics.Vector4 Transform(System.Numerics.Vector4 value, System.Numerics.Matrix4x4 matrix) { throw null; }
        public System.Numerics.Vector2 Vector2(float x, float y) { throw null; }
        public System.Numerics.Vector3 Vector3(float x, float y, float z) { throw null; }
        public System.Numerics.Vector4 Vector4(float x, float y, float z, float w) { throw null; }
    }
    public abstract partial class CompositionExpressionEngine
    {
        protected CompositionExpressionEngine() { }
        public static object GetObject(System.Linq.Expressions.Expression expression) { throw null; }
    }
    public partial class CompositionExpressionResult
    {
        public CompositionExpressionResult() { }
        public string Expression { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Collections.Generic.Dictionary<string, object> Parameters { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public delegate T CompositionLambda<T>(CompositionProToolkit.Expressions.CompositionExpressionContext<T> ctx);
    public static partial class CompositionObjectExtensions
    {
        public static string ScaleXY(this Windows.UI.Composition.CompositionObject compositionObject) { throw null; }
        public static void StartAnimation(this Windows.UI.Composition.CompositionObject compositionObject, System.Linq.Expressions.Expression<System.Func<object>> expression, Windows.UI.Composition.CompositionAnimation animation) { }
        public static void StopAnimation(this Windows.UI.Composition.CompositionObject compositionObject, System.Linq.Expressions.Expression<System.Func<object>> expression) { }
    }
    public static partial class CompositionPropertySetExtensions
    {
        public static T Get<T>(this Windows.UI.Composition.CompositionPropertySet propertySet, string key) { throw null; }
        public static void Insert<T>(this Windows.UI.Composition.CompositionPropertySet propertySet, string key, object input) { }
        public static Windows.UI.Composition.CompositionPropertySet ToPropertySet(object input, Windows.UI.Composition.Compositor compositor) { throw null; }
    }
    public static partial class CompositorExtensions
    {
        public static Windows.UI.Composition.CompositionEffectFactory CreateEffectFactory(this Windows.UI.Composition.Compositor compositor, Windows.Graphics.Effects.IGraphicsEffect graphicsEffect, params System.Linq.Expressions.Expression<System.Func<object>>[] animatablePropertyExpressions) { throw null; }
        public static Windows.UI.Composition.ExpressionAnimation CreateExpressionAnimation<T>(this Windows.UI.Composition.Compositor compositor, System.Linq.Expressions.Expression<CompositionProToolkit.Expressions.CompositionLambda<T>> expression) { throw null; }
        public static System.Linq.Expressions.Expression<CompositionProToolkit.Expressions.CompositionLambda<T>> CreateFinalValueExpression<T>(this Windows.UI.Composition.Compositor compositor) { throw null; }
        public static CompositionProToolkit.Expressions.KeyFrameAnimation<T> CreateKeyFrameAnimation<T>(this Windows.UI.Composition.Compositor compositor) { throw null; }
        public static void CreateScopedBatch(this Windows.UI.Composition.Compositor compositor, Windows.UI.Composition.CompositionBatchTypes batchType, System.Action action, System.Action postAction=null) { }
        public static void CreateScopedBatch(this Windows.UI.Composition.Compositor compositor, Windows.UI.Composition.CompositionBatchTypes batchType, System.Action<Windows.UI.Composition.CompositionScopedBatch> action, System.Action<Windows.UI.Composition.CompositionScopedBatch> postAction=null) { }
        public static System.Linq.Expressions.Expression<CompositionProToolkit.Expressions.CompositionLambda<T>> CreateStartingValueExpression<T>(this Windows.UI.Composition.Compositor compositor) { throw null; }
    }
    public static partial class DoubleExtensions
    {
        public static float Single(this double value) { throw null; }
    }
    public sealed partial class KeyFrame<T>
    {
        public KeyFrame(float normalizedProgressKey, T value, Windows.UI.Composition.CompositionEasingFunction easing=null) { }
        public Windows.UI.Composition.CompositionEasingFunction Easing { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public float Key { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public T Value { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public sealed partial class KeyFrameAnimation<T>
    {
        public KeyFrameAnimation(Windows.UI.Composition.KeyFrameAnimation animation) { }
        public Windows.UI.Composition.KeyFrameAnimation Animation { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.TimeSpan DelayTime { get { throw null; } set { } }
        public Windows.UI.Composition.AnimationDirection Direction { get { throw null; } set { } }
        public System.TimeSpan Duration { get { throw null; } set { } }
        public Windows.UI.Composition.AnimationIterationBehavior IterationBehavior { get { throw null; } set { } }
        public int IterationCount { get { throw null; } set { } }
        public int KeyFrameCount { get { throw null; } }
        public Windows.UI.Composition.AnimationStopBehavior StopBehavior { get { throw null; } set { } }
        public string Target { get { throw null; } set { } }
        public CompositionProToolkit.Expressions.KeyFrameAnimation<T> DelayBy(System.TimeSpan delayTime) { throw null; }
        public CompositionProToolkit.Expressions.KeyFrameAnimation<T> ForTarget(System.Linq.Expressions.Expression<System.Func<object>> targetExpression) { throw null; }
        public CompositionProToolkit.Expressions.KeyFrameAnimation<T> HavingDuration(System.TimeSpan duration) { throw null; }
        public void InsertExpressionKeyFrame(float normalizedProgressKey, System.Linq.Expressions.Expression<CompositionProToolkit.Expressions.CompositionLambda<T>> expression, Windows.UI.Composition.CompositionEasingFunction easingFunction=null) { }
        public void InsertKeyFrame(CompositionProToolkit.Expressions.KeyFrame<T> keyFrame) { }
        public void InsertKeyFrame(float normalizedProgressKey, T value, Windows.UI.Composition.CompositionEasingFunction easingFunction=null) { }
        public void InsertKeyFrames(params CompositionProToolkit.Expressions.KeyFrame<T>[] keyFrames) { }
        public CompositionProToolkit.Expressions.KeyFrameAnimation<T> InTheDirection(Windows.UI.Composition.AnimationDirection direction) { throw null; }
        public CompositionProToolkit.Expressions.KeyFrameAnimation<T> OnStop(Windows.UI.Composition.AnimationStopBehavior stopBehavior) { throw null; }
        public CompositionProToolkit.Expressions.KeyFrameAnimation<T> Repeats(int count) { throw null; }
        public CompositionProToolkit.Expressions.KeyFrameAnimation<T> RepeatsForever() { throw null; }
    }
    public static partial class TypeExtensions
    {
        public static System.Type BaseType(this System.Type type) { throw null; }
        public static System.Type[] GetGenericArguments(this System.Type type) { throw null; }
        public static System.Reflection.MethodInfo GetMethod(this System.Type type, string methodName) { throw null; }
        public static System.Reflection.PropertyInfo GetProperty(this System.Type type, string propertyName) { throw null; }
        public static object GetPropertyValue(this object instance, string propertyValue) { throw null; }
        public static System.Reflection.TypeInfo GetTypeInfo(this System.Type type) { throw null; }
        public static bool IsClass(this System.Type type) { throw null; }
        public static bool IsEnum(this System.Type type) { throw null; }
        public static bool IsGenericType(this System.Type type) { throw null; }
        public static bool IsPrimitive(this System.Type type) { throw null; }
        public static bool IsPublic(this System.Type type) { throw null; }
        public static bool IsSubclassOf(this System.Type type, System.Type parentType) { throw null; }
    }
}
namespace CompositionProToolkit.Win2d
{
    public static partial class CanvasGeometryParser
    {
        public static Microsoft.Graphics.Canvas.Geometry.CanvasGeometry Parse(Microsoft.Graphics.Canvas.ICanvasResourceCreator resourceCreator, string pathData, System.Text.StringBuilder logger=null) { throw null; }
    }
    public static partial class CanvasPathBuilderExtensions
    {
        public static void AddCircleFigure(this Microsoft.Graphics.Canvas.Geometry.CanvasPathBuilder pathBuilder, System.Numerics.Vector2 center, float radius) { }
        public static void AddCircleFigure(this Microsoft.Graphics.Canvas.Geometry.CanvasPathBuilder pathBuilder, float x, float y, float radius) { }
        public static void AddEllipseFigure(this Microsoft.Graphics.Canvas.Geometry.CanvasPathBuilder pathBuilder, System.Numerics.Vector2 center, float radiusX, float radiusY) { }
        public static void AddEllipseFigure(this Microsoft.Graphics.Canvas.Geometry.CanvasPathBuilder pathBuilder, float x, float y, float radiusX, float radiusY) { }
        public static void AddPolygonFigure(this Microsoft.Graphics.Canvas.Geometry.CanvasPathBuilder pathBuilder, int numSides, System.Numerics.Vector2 center, float radius) { }
        public static void AddPolygonFigure(this Microsoft.Graphics.Canvas.Geometry.CanvasPathBuilder pathBuilder, int numSides, float x, float y, float radius) { }
    }
}
