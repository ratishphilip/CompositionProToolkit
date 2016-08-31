<img src="https://cloud.githubusercontent.com/assets/7021835/16889814/1784ed78-4a9e-11e6-80d0-7c2084d6c960.png" alt="CompositionProToolkit"></img>

>  IMPORTANT: **CompositionProToolkit has undergone some breaking API changes in v0.4.4. Look [here](https://wpfspark.wordpress.com/2016/08/27/compositionprotoolkit-v0-4-4-released/) for more details.**


# Table of Contents

- [Installing from NuGet](#installing-from-nuget)
- [CompositionProToolkit Internals](#compositionprotoolkit-internals)
  - [Rendering Surfaces](#1-rendering-surfaces) 
  - [Creating custom shaped `Visual` using `CanvasGeometry`](#2-creating-custom-shaped-visual-using-canvasgeometry)
    - [Using IMaskSurface](#using-imasksurface)
    - [Using IGeometrySurface](#using-igeometrysurface)
 - [Creating Masked Backdrop Brush using `IMaskSurface`](#3-creating-masked-backdrop-brush-using-imasksurface)
  - [Loading Images on Visual using `IImageSurface`](#4-loading-images-on-visual-using-iimagesurface)
  - [Creating the Reflection of a `ContainerVisual`](#5-creating-the-reflection-of-a-containervisual)
- [CompositionProToolkit Controls](#compositionprotoolkit-controls)
  - [FluidProgressRing](#1-fluidprogressring)
  - [FluidWrapPanel](#2-fluidwrappanel)
  - [ImageFrame](#3-imageframe)
    - [ImageFrame Source](#imageframe-source)
    - [Image Caching](#image-caching)
    - [ImageFrame Transitions](#imageframe-transitions)
    - [Using ImageFrame with FilePicker](#using-imageframe-with-filepicker)   
  - [FluidBanner](#4-fluidbanner)
- [CompositionProToolkit Expressions]()
- [Updates Chronology](#updates-chronology)

**CompositionProToolkit** is a collection of helper classes for Windows.UI.Composition. It also contains controls which can be used in UWP applications. It has dependency on the **Win2D** and the  [**CompositionExpressionToolkit**](https://github.com/ratishphilip/CompositionExpressionToolkit) libraries.

# Installing from NuGet

To install **CompositionProToolkit**, run the following command in the **Package Manager Console**

```
Install-Package CompositionProToolkit
```

More details available [here](https://www.nuget.org/packages/CompositionProToolkit/).

# CompositionProToolkit Internals

## 1. Rendering Surfaces
**CompositionProToolkit** provides three types of rendering surface interfaces which can be used for creating mask, rendering custom shapes and images.

- _**IRenderSurface**_ - This interface acts as the base interface for interfaces which render to the **ICompositionSurface**. It mainly contains references to an **ICompositionGenerator** object and an **ICompositionSurface** object which are the core objects required for rendering any geometry or image onto a **ICompositionSurface**.
- _**IMaskSurface**_ - This interface is used for rendering custom shaped geometries onto **ICompositionSurface** so that they can be useds as masks on Composition Visuals.
- _**IGeometrySurface**_ - This interface is used for rendering custom shaped geometries onto **ICompositionSurface**.
- _**IImageSurface**_ - This interface is used for rendering images onto **ICompositionSurface**.

**IMaskSurface**, **IGeometrySurface** and **IImageSurface** derive from **IRenderSurface**. Here is the interface hierarchy

<img src="https://cloud.githubusercontent.com/assets/7021835/18028363/a51f2d6a-6c31-11e6-9bcf-fe96d1ca93b6.png">

## 2. Creating custom shaped `Visual` using `CanvasGeometry`
As of now, you can customize the shape of Visuals by applying a mask on the Visual. The mask is defined using a **CompositionMaskBrush**. In the **CompositionMaskBrush** the `Mask` is defined by a **CompositionSurfaceBrush**. Into the **CompositionSurfaceBrush** an image, which defines the mask, is loaded. In this image, the areas which are to masked in the Visual are transparent whereas the areas to be shown in the Visual are white.  

Using **CompositionProToolkit** you can now define a mask for the **Visual** using **Win2D**'s **CanvasGeometry**. First you need an object implementing the **ICompositionGenerator** interface. It can be obtained by the static class **CompositionGeneratorFactory**. There are two APIS to obtain the **CompositionGenerator** - by providing a **Compositor** or by providing a **CompositionGraphicDevice**.

```C#
public static ICompositionGenerator GetCompositionGenerator(Compositor compositor,
    bool useSharedCanvasDevice = true, bool useSoftwareRenderer = false
public static ICompositionGenerator GetCompositionGenerator(CompositionGraphicsDevice graphicsDevice)
```
The first API also has couple of **optional** parameters
- **useSharedCanvasDevice** - indicates whether the **CompositionGenerator** should use a shared **CanvasDevice** or creates a new one.
- **useSoftwareRenderer** - this parameter is provided as a argument when creating a new **CanvasDevice** (i.e. when **usedSharedCanvasDevice** is **false**).

### Using `IMaskSurface`
Using the **ICompositionGenerator** an object implementing the **IMaskSurface** can be created. This object represents the mask that needs to be applied to the Visual using a **CompositionMaskBrush**.

The following API is provided in **ICompositionGenerator** to create a **IMaskSurface**

```C#
ICompositionMask CreateMaskSurface(Size size, CanvasGeometry geometry);
```

In this API, the provided geometry is filled with **White** color.

#### Example

The following code

```C#
// Get the Generator
ICompositionGenerator generator = CompositionGeneratorFactory.GetCompositionGenerator(compositor);

//Create the visual
SpriteVisual visual = compositor.CreateSpriteVisual();
visual.Size = new Vector2(400, 400);
visual.Offset = new Vector3(200, 0, 0);

// Create the combined geometry
var ellipse1 = CanvasGeometry.CreateEllipse(generator.Device, 200, 200, 150, 75);
var ellipse2 = CanvasGeometry.CreateEllipse(generator.Device, 200, 200, 75, 150);
var combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

// Create the MaskSurface
IMaskSurface maskSurface = generator.CreateMask(visual.Size.ToSize(), combinedGeometry);

// Create SurfaceBrush from MaskSurface
var mask = compositor.CreateSurfaceBrush(maskSurface.Surface);
var source = compositor.CreateColorBrush(Colors.Blue);

// Create mask brush
var maskBrush = compositor.CreateMaskBrush();
maskBrush.Mask = mask;
maskBrush.Source = source;

visual.Brush = maskBrush;
```

creates the following output.  

<img src="https://cloud.githubusercontent.com/assets/7021835/15728977/0f9f397a-2815-11e6-9df2-65b9ad1f5e9f.PNG" />

**IMaskSurface** provides the following APIs which allow you to update its geometry and size (and thus the shape of the Visual).
```C#
void Redraw();
void Redraw(CanvasGeometry geometry);
void Redraw(Size size, CanvasGeometry geometry);
void Resize(Size size);
```

### Using `IGeometrySurface`
If you want to render CanvasGeometry with a fill color and a background color, you have to use the **ICompositionGenerator** to create an object implementing the **IGeometrySurface**.

The following API is provided in **ICompositionGenerator** to create a **IGeometrySurface**

```C#
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, 
    Color foregroundColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, 
    Color foregroundColor, Color backgroundColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, 
    ICanvasBrush foregroundBrush);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, 
    ICanvasBrush foregroundBrush, ICanvasBrush backgroundBrush);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, 
    ICanvasBrush foregroundBrush, Color backgroundColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, 
    Color foregroundColor, ICanvasBrush backgroundBrush);
```
The previous example can also be written as

```C#
// Get the Generator
ICompositionGenerator generator = CompositionGeneratorFactory.GetCompositionGenerator(compositor);

//Create the visual
SpriteVisual visual = compositor.CreateSpriteVisual();
visual.Size = new Vector2(400, 400);
visual.Offset = new Vector3(200, 0, 0);

// Create the combined geometry
var ellipse1 = CanvasGeometry.CreateEllipse(generator.Device, 200, 200, 150, 75);
var ellipse2 = CanvasGeometry.CreateEllipse(generator.Device, 200, 200, 75, 150);
var combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

// Create the CompositionMask
IGeometrySurface geometrySurface = 
                  generator.CreateGeometrySurface(visual.Size.ToSize(), combinedGeometry, Colors.Blue);

// Create SurfaceBrush from GeometrySurface
var surfaceBrush = compositor.CreateSurfaceBrush(geometrySurface.Surface);

visual.Brush = surfaceBrush;
```

**IGeometrySurface** provides several APIs which allow you to update its geometry, size, fill and background (and thus the shape of the Visual).

```C#
void Redraw();
void Redraw(CanvasGeometry geometry);
void Redraw(Color foregroundColor);
void Redraw(Color foregroundColor, Color backgroundColor);
void Redraw(ICanvasBrush foregroundBrush);
void Redraw(ICanvasBrush foregroundBrush, ICanvasBrush backgroundBrush);
void Redraw(Color foregroundColor, ICanvasBrush backgroundBrush);
void Redraw(ICanvasBrush foregroundBrush, Color backgroundColor);
void Redraw(Size size, CanvasGeometry geometry);
void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor);
void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor, Color backgroundColor);
void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush);
void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush, ICanvasBrush backgroundBrush);
void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush foregroundBrush, Color backgroundColor);
void Redraw(Size size, CanvasGeometry geometry, Color foregroundColor, ICanvasBrush backgroundBrush);
void Resize(Size size);
```

Here is an example of a **CanvasAnimatedControl** having two visuals - A blue rectangular visual in the background and a red visual in the foreground. The red visual's mask is redrawn periodically to give an impression of animation. (_see the **[SampleGallery](https://github.com/ratishphilip/CompositionProToolkit/tree/master/SampleGallery)** project for more details on how it is implemented_)

```C#
private async void AnimatedCanvasCtrl_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
{
    angle = (float)((angle + 1) % 360);
    var radians = (float)((angle * Math.PI) / 180);
    
    // Calculate the new geometry based on the angle
    var updatedGeometry = outerGeometry.CombineWith(combinedGeometry, 
                                Matrix3x2.CreateRotation(radians, new Vector2(200, 200)),
                                CanvasGeometryCombine.Exclude);
        
    // Update the geometry in the Composition Mask
    animatedGeometrySurface.Redraw(updatedGeometry);
}
```

<img src="https://cloud.githubusercontent.com/assets/7021835/15728986/1baeab9c-2815-11e6-8e93-846b70a2a3ea.gif" />

## 3. Creating Masked Backdrop Brush using `IMaskSurface`
**CompositionProToolkit** now provides the following extension method for **Compositor** to create a masked Backdrop brush.

```C#
public static CompositionEffectBrush CreateMaskedBackdropBrush(this Compositor compositor, IMaskSurface mask,
            Color blendColor, float blurAmount, CompositionBackdropBrush backdropBrush = null)
```  

Using this method, you can apply a BackdropBrush with a custom shape to a visual. You can provide a **Color** to blend with the BackdropBrush, the amount by which the BackdropBrush should be blurred and an optional **CompositionBackdropBrush**. If no **CompositionBackdropBrush** is provided by the user then this method creates one.

**Note** : _Create only one instance of **CompositionBackdropBrush** and reuse it within your application. It provides a better performance._

<img src="https://cloud.githubusercontent.com/assets/7021835/16091854/562d255c-32ea-11e6-8952-424a513741ea.gif" />

## 4. Loading Images on Visual using `IImageSurface`
**IImageSurface** is an interface which encapsulates a **CompositionDrawingSurface** onto which an image can be loaded by providing a **Uri**. You can then use the **CompositionDrawingSurface** to create a **CompositionSurfaceBrush** which can be applied to any **Visual**.

<img src="https://cloud.githubusercontent.com/assets/7021835/17491822/0f3c390a-5d5e-11e6-89de-772c786fb2e2.png" />

**ICompositionGenerator** provides the following API which allows you to create an object implementing the **IImageSurface**

```C#
Task<IImageSurface> CreateImageSurfaceAsync(Uri uri, Size size, 
    ImageSurfaceOptions options);
```

This API requires the **Uri** of the image to be loaded, the **size** of the CompositionDrawingSurface (_usually the same size as the **Visual** on which it is finally applied_) and the **ImageSurfaceOptions**.

### ImageSurfaceOptions

The **ImageSurfaceOptions** class encapsulates a set of properties which influence the rendering of the image on the **ImageSurface**. The following table shows the list of these properties.  


| Property | Type | Description | Possible Values |
|---|---|---|---|
| **`AutoResize`** | `Boolean` | Specifies whether the surface should resize itself automatically to match the loaded image size. When set to **True**, the Stretch, HorizontalAlignment and VerticalAlignment options are ignored. | **False** |
| **`HorizontalAlignment`** | `AlignmentX` | Describes how image is positioned horizontally in the **`ImageSurface`**. | **`Left`**, **`Center`**, **`Right`** |
| **`Interpolation`** | `CanvasImageInterpolation` | Specifies the interpolation used to render the image on the **`ImageSurface`**.  | **`NearestNeighbor`**, **`Linear`**, **`Cubic`**, **`MultiSampleLinear`**, **`Anisotropic`**, **`HighQualityCubic`** |
| **`Opacity`** | `float` | Specifies the opacity of the rendered image. | **`0 - 1f`** inclusive |
| **`Stretch`**| `Stretch` | Describes how image is resized to fill its allocated space. | **`None`**, **`Uniform`**, **`Fill`**, **`UniformToFill`** |
| **`SurfaceBackgroundColor`** | `Color` | Specifies the color which will be used to fill the **`ImageSurface`** before rendering the image. | All possible values that can be created. |
| **`VerticalAlignment`** | `AlignmentY` | Describes how image is positioned vertically in the **`ImageSurface`**. | **`Top`**, **`Center`**, **`Bottom`** |


Here is how the image is aligned on the Visual's surface based on the **HorizontalAlignment** and **VerticalAlignment** properties

<img src="https://cloud.githubusercontent.com/assets/7021835/17491824/0f3e71ca-5d5e-11e6-805d-368df799d68b.PNG" />

### Example

The following example shows how you can load an image onto a **Visual**


```C#
var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
var generator = CompositionGeneratorFactory.GetCompositionGenerator(compositor);

var visual = compositor.CreateSpriteVisual();
visual.Size = new Vector2(this.ActualWidth.Single(), this.ActualHeight.Single());
visual.Offset = Vector3.Zero;

var options = new ImageSurfaceOptions(Stretch.Uniform, AlignmentX.Center, AlignmentY.Center, 0.9f,
                CanvasImageInterpolation.Cubic)
    {
        SurfaceBackgroundColor = Colors.Maroon
    };

var imageSurface =
    await generator.CreateImageSurfaceAsync(new Uri("ms-appx:///Assets/Images/Image3.jpg"), 
        visual.Size.ToSize(), options);

visual.Brush = compositor.CreateSurfaceBrush(imageSurface.Surface);
```

Once you create a **CompositionSurfaceBrush** from the **IImageSurface** and apply it to a **Visual**, you can use the following **IImageSurface** APIs to resize, provide a new Uri or change the rendering options

```C#
void Redraw();

Task Redraw(ImageSurfaceOptions options);

Task RedrawAsync(Uri uri, ImageSurfaceOptions options);

Task RedrawAsync(Uri uri, Size size, ImageSurfaceOptions options);

void Resize(Size size);

Task Resize(Size size, ImageSurfaceOptions options);
```

Once you call any of the above methods, the Visual's brush is also updated.

## 5. Creating the Reflection of a `ContainerVisual`
**CompositionProToolkit** provides the following API which allows you to create the reflection of any **ContainerVisual**

```C#
void CreateReflection(ContainerVisual visual, float reflectionDistance = 0f,
    float reflectionLength = 0.7f, ReflectionLocation location = ReflectionLocation.Bottom);
```

The parameters required for this API are
- **visual** - A **ContainerVisual** whose reflection has to be created.
- **reflectionDistance** - The distance between the visual and its reflection.
- **reflectionLength** - The normalized length (**0 - 1f**, inclusive) of the visual that must be visible in the reflection. _Default value is **0.7f**_.
- **location** - Specifies the location of the reflection with respect to the visual - Bottom, Top, Left or Right. _Default value is **ReflectionLocation.Bottom**_.

<img src="https://cloud.githubusercontent.com/assets/7021835/17491821/0f3c2384-5d5e-11e6-9e8e-7b5a196d4ef5.png" />

This API will create a reflection even if an effect is applied to the Visual.

<img src="https://cloud.githubusercontent.com/assets/7021835/17491820/0f398c14-5d5e-11e6-9ab6-5cf8bf1d7683.png" />

If the visual has multiple other visuals in its visual tree, then the entire visual tree is reflected.

<img src="https://cloud.githubusercontent.com/assets/7021835/17491823/0f3e67fc-5d5e-11e6-9116-91fb3fb55b83.png" />

# CompositionProToolkit Controls

## 1. FluidProgressRing
**FluidProgressRing** is a concept design for a modern version of the **ProgressRing** control in UWP. The **ProgressRing** control has remained the same since Windows 8 and it is high time it got a refresh. The **FluidProgressRing** consists of a set of small circles (**nodes**) rotating about a common center. Each node rotates until it hits the adjacent node (then it slows down to a stop). The adjacent node then rotates and hits the next node and this process continues. The animations are done using the **Windows.UI.Composition APIs** and provide a smooth look and feel.

<img src="https://cloud.githubusercontent.com/assets/7021835/16522118/838f2eec-3f50-11e6-825c-20e07300339c.gif" />

To use it you can just add it to your XAML and it will start rotating. It has a default size of **70 x 70** and has **7** nodes. The good thing about the **FluidProgressRing** control is that it can be easily configured. Here are the dependency properties which you can modify to alter the **FluidProgressRing's** appearance

| Dependency Property | Type | Description | Default Value |  
|----|----|----|----|
| **MaxNodes** | `int` | The maximum number of nodes that can be accommodated within the FluidProgressRing | **7** |   
| **ActiveNodes** | `int` | The number of stationary nodes in the FluidProgressRing. The FluidProgressRing will have an additional node which will be in motion. ActiveNodes should be less than or equal to MaxNodes. | **6** |  
| **NodeDuration** | `TimeSpan` | The time it takes for a node to travel and hit the adjacent node. | **0.5s** |  
| **RingDuration** | `TimeSpan` | The duration for one complete rotation of the FluidProgressRing. | **5s** |  
| **NodeSizeFactor** | `double` | The ratio of the node diameter to the width of the FluidProgressRing | **0.15** |  
| **NodeColor** | `Color` | The color of the nodes | **Blue** |  

You can also change the **Width** and **Height** properties of **FluidProgressRing** control. The final size of the **FluidProgressRing** control will be the largest square that can fit in the defined **Width** and **Height**.

## 2. FluidWrapPanel

**FluidWrapPanel** is a wrap panel which allows you to rearrange the children simply by dragging and placing them in the desired location. The remaining children will automatically reposition themselves to accommodate the dragged item in the new location. The children can be instances of any class which derives from **UIElement** (or its subclasses). Check out the [SampleGallery](https://github.com/ratishphilip/CompositionProToolkit/tree/master/SampleGallery) code to know how you can add your own custom controls to the FluidWrapPanel.

**FluidWrapPanel** internally uses **Composition** APIs to make the animation really smooth. The children of the **FluidWrapPanel** can all be of same size or varying size. _If they are varying size, ensure that their width and height are multiples of **ItemWidth** and **ItemHeight**, respectively._

Here is a demo of the **FluidWrapPanel** in action

<img src="https://cloud.githubusercontent.com/assets/7021835/16889802/077dc724-4a9e-11e6-8475-7693138f0b39.gif" alt="FluidWrapPanel demo"></img>

**FluidWrapPanel** has the following Dependency Properties

| Dependency Property | Type | Description | Default Value |
|---|---|---|---|
| **DragOpacity** | `Double` | Gets or sets the Opacity of the element when it is being dragged by the user. _**Range: 0.1 - 1.0 inclusive.**_ | **0.7** |
| **DragScale** | `Double` | Gets or sets the Scale Factor of the element when it is being dragged by the user. | **1.2** |
| **FluidItems** | `ObservableCollection<UIElement>` | Indicates the Observable Collection of the rearranged children | **null** |
| **IsComposing** | `Boolean` | Flag to indicate whether the children in the FluidWrapPanel can be rearranged or not. | **False** |
| **ItemHeight** | `Double` | Gets or sets the minimum height each child can have in the FluidWrapPanel. _The child's height should be a multiple of the **ItemHeight**_. | **0** |
| **ItemWidth** | `Double` | Gets or sets the minimum width each child can have in the FluidWrapPanel. _The child's width should be a multiple of the **ItemWidth**_. | **0** |
| **ItemsSource** | `IEnumerable` | Bindable property to which a collection of UIElement can be bound. | **null** |
| **OptimizeChildPlacement** | `Boolean` | Gets or sets the property that indicates whether the placement of the children is optimized. If set to true, the child is placed at the first available position from the beginning of the FluidWrapPanel. If set to false, each child occupies the same (or greater) row and/or column than the previous child. | **True** |
| **Orientation** | `System.Windows.Controls.Orientation` | Gets or sets the different orientations the FluidWrapPanel can have. _Possible values are **Horizontal** and **Vertical**_. | **Horizontal** | 

## 3. ImageFrame
**ImageFrame** is a control which can be used for displaying images asynchronously. It encapsulates a **ImageSurface** object which is used for loading and rendering the images. It also supports Pointer interactions and raises events accordingly.

<img src="https://cloud.githubusercontent.com/assets/7021835/17950778/43912a38-6a12-11e6-8877-7c3bf92da86d.gif" />

In order to configure the rendering of the image, **ImageFrame** has the following properties

| Dependency Property | Type | Description | Default Value |  
|----|----|----|----|
| **`AlignX`** | `AlignmentX` | Specifies how the image is positioned horizontally in the **ImageFrame**. | **AlignmentX.Center** |
| **`AlignY`** | `AlignmentY` | Specifies how the image is positioned vertically in the **ImageFrame**. | **AlignmentY.Center** |
| **`CornerRadius`** | `CornerRadius` | Indicates the corner radius of the the ImageFrame. The image will be rendered with rounded corners. | **(0, 0, 0, 0)** |
| **`DisplayShadow`** | `Boolean` | Indicates whether the shadow for this image should be displayed. | **False** |
| **`FrameBackground`** | `Color` | Specifies the background color of the **ImageFrame** to fill the area where image is not rendered. | **Colors.Black** |
| **`Interpolation`** | `CanvasImageInterpolation` | Specifies the interpolation used for rendering the image. | **HighQualityCubic** |
| **`PlaceholderBackground`** | `Color` | Indicates the background color of the Placeholder. | **Colors.Black** |
| **`PlaceholderColor`** | `Color` | Indicates the color with which the rendered placeholder geometry should be filled | **RGB(192, 192, 192)** |
| **`RenderFast`** | `Boolean` | Indicates whether the animations need to be switched off if the ImageFrame is being used in scenarios where it is being rapidly updated with new Source. | **False** |
| **`RenderOptimized`** | `Boolean` | Indicates whether optimization must be used to render the image.Set this property to True if the ImageFrame is very small compared to the actual image size. This will optimize memory usage. | **False** |
| **`ShadowBlurRadius`** | `Double` | Specifies the Blur radius of the shadow. | **0.0** |
| **`ShadowColor`** | `Color` | Specifies the color of the shadow. | **Colors.Transparent** |
| **`ShadowOffsetX`** | `Double` | Specifies the horizontal offset of the shadow. | **0.0** |
| **`ShadowOffsetY`** | `Double` | Specifies the vertical offset of the shadow. | **0.0** |
| **`ShadowOpacity`** | `Double` | Specifies the opacity of the shadow. | **1** |
| **`ShowPlaceHolder`** | `Boolean` | Indicates whether the placeholder needs to be displayed during image load or when no image is loaded in the ImageFrame. | **True** |
| **`Source`** | `object` | Indicates `Uri` or `StorageFile` or `IRandomAccessStream` of the image to be loaded into the **ImageFrame**. | **null** |
| **`Stretch`** | `Stretch` | Specifies how the image is resized to fill its allocated space in the **ImageFrame**. | **Stretch.Uniform** |
| **`TransitionDuration`** | `Double` | Indicates the duration of the crossfade animation while transitioning from one image to another. | **700ms** |
| **`TransitionMode`** | `TransitionModeType` | Indicates the type of transition animation to employ for displaying an image after it has been loaded. | **TransitionModeType.FadeIn** |

**ImageFrame** raises the following events  
- **ImageOpened** - when the image has been successfully loaded from the Uri and rendered.
- **ImageFailed** - when there is an error loading the image from the Uri.

### ImageFrame Source
The **Source** property of **ImageFrame** is of type **object** so that it can accept the following types

- **Uri**
- **String** (_from which a **Uri** can be successfully created_)
- **StorageFile**
- **IRandomAccessStream**

### Image Caching
**ImageFrame** internally caches the objects provided to **Source** property. The cache is located in the temporary folder of the application using the **ImageFrame**. Within the application, you can clear the cache anytime by using the following code

```C#
await ImageCache.ClearCacheAsync();
```

### ImageFrame Transitions

<img src="https://cloud.githubusercontent.com/assets/7021835/18138092/f4c56fe2-6f5f-11e6-9b6c-ea8a0f27ad08.gif" alt="ImageFrame Transitions" />

**ImageFrame** provides several transition animations while displaying the newly loaded image. You can configure which animation to run by setting the  **TransitionMode** property of the **imageFrame**. **TransitionMode** can be set to any of the following values
- **`FadeIn`** - The newly loaded image fades into view.
- **`SlideLeft`** - The newly loaded image slides into view from the right side of the ImageFrame and moves left.
- **`SlideRight`** - The newly loaded image slides into view from the left side of the ImageFrame and moves right.
- **`SlideUp`** - The newly loaded image slides into view from the bottom of the ImageFrame and moves up.
- **`SlideDown`** - The newly loaded image slides into view from the top of the ImageFrame and moves down.
- **`ZoomIn`** - The newly loaded image zooms into view from the center of the ImageFrame.

### Using ImageFrame with FilePicker
If you have a **ImageFrame** control in  you application and your want to use the **FilePicker** to select an image file to be displayed on the **CompostionImageFrame**, then you must do the following

```C#
var picker = new Windows.Storage.Pickers.FileOpenPicker
{
    ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
};
picker.FileTypeFilter.Add(".jpg");
picker.FileTypeFilter.Add(".jpeg");
picker.FileTypeFilter.Add(".png");
var file = await picker.PickSingleFileAsync();
imageFrame.Source = file;
```

## 4. FluidBanner

**FluidBanner** control is banner control created using Windows Composition. It allows for displaying multiple images in a unique interactive format.  It internally uses **ImageSurface** to host the images. 

<img src="https://cloud.githubusercontent.com/assets/7021835/17262223/03be56de-558f-11e6-809d-7bc8ae2e2fdd.gif" />

It provides the following properties which can be used to customize the **FluidBanner**.

| Dependency Property | Type | Description | Default Value |  
|----|----|----|----|
| **`AlignX`** | `AlignmentX` | Indicates how the image is positioned horizontally in the **FluidBanner** items. | **Center** |
| **`AlignY`** | `AlignmentY` | Indicates how the image is positioned vertically in the **FluidBanner** items. | **Center** |
| **`DecodeHeight`** | `int` | The height, in pixels, that the images are decoded to. (<em>**Optional**</em>) | **0** |
| **`DecodeWidth`** | `int` | The width, in pixels, that the images are decoded to. (<em>**Optional**</em>) | **0** |
| **`ItemBackground`** | `Color` | The background color of each item in the **FluidBanner** | **Black** |
| **`ItemGap`** | `double` | The gap between adjacent items in the **FluidBanner**. | **30** |
| **`ItemsSource`** | `IEnumerable&lt;Uri&gt;` | The collection of Uris of images to be shown in the **FluidBanner** | **null** |
| **`Padding`** | `Thickness` | The padding inside the **FluidBanner** | **Thickness(0)** |
| **`Stretch`** | `Stretch` | Indicates how the image is resized to fill its allocated space within each **FluidBanner** item. | **Uniform** |

# Updates Chronology

## v0.4.5.0
(**Wednesday, August 31, 2016**) - Merged `CompositionExpressionToolkit` with `CompositionProToolkit`. Transitions added to `ImageFrame`.

## v0.4.4.1
(**Saturday, August 27, 2016**) - Improved UX in ImageFrame. Bug fixes. Breaking changes - several classes refactored and renamed.

## v0.4.3
(**Wednesday, August 24, 2016**) - New features in `ImageFrame` - Placeholder, ImageCache, Load progress, animations. Bug fixes.

## v0.4.2
(**Thursday, August 18, 2016**) - `FluidBanner` Control Added. CornerRadius and Shadow features added in `ImageFrame`. Major refactoring of code. Breaking changes.

## v0.4.1
(**Friday, August 12, 2016**) - `ImageFrame` Control added. `ImageSurface` and `CompositionMask` optimized. Bug Fixes.

## v0.4.0
(**Monday, August 8, 2016**) - Added `IImageSurface` interface which allows loading of images on a `Visual` and `CreateReflectionAsync` method which creates the reflection of a `ContainerVisual`.

## v0.3.0
(**Friday, July 15, 2016**) - Added `FluidWrapPanel` Control. **SampleGallery** added.

## v0.2.0
(**Friday, July 1, 2016**) - Added `FluidProgressRing` Control.

## v0.1.1
(**Wednesday, June 15, 2016**) - Added Compositor Extension method to create Masked Backdrop Brush.

## v0.1.0
(**Wednesday, June 1, 2016**) - Initial Version.
