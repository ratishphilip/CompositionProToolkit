<img src="https://cloud.githubusercontent.com/assets/7021835/16889814/1784ed78-4a9e-11e6-80d0-7c2084d6c960.png" alt="CompositionProToolkit"></img>

# What's new in v0.8?
- `ProgressRing3d` control added.

# Table of Contents

- [Installing from NuGet](#installing-from-nuget)
- [CompositionProToolkit Internals](#compositionprotoolkit-internals)
  - [Rendering Surfaces](#1-rendering-surfaces) 
  - [Creating custom shaped `Visual` using `CanvasGeometry`](#2-creating-custom-shaped-visual-using-canvasgeometry)
    - [Using IMaskSurface](#using-imasksurface)
    - [Using IGeometrySurface](#using-igeometrysurface)
  - [Creating Masked Backdrop Brush using `IMaskSurface`](#3-creating-masked-backdrop-brush-using-imasksurface)
  - [Creating a Frosted Glass Effect Brush using `IMaskSurface`](#4-creating-a-frosted-glass-effect-brush-using-imasksurface)
  - [Loading Images on Visual using `IImageSurface`](#5-loading-images-on-visual-using-iimagesurface)
  - [Creating the Reflection of a `ContainerVisual`](#6-creating-the-reflection-of-a-containervisual)
- [CompositionProToolkit Controls](#compositionprotoolkit-controls)
  - [FluidProgressRing](#1-fluidprogressring)
  - [FluidWrapPanel](#2-fluidwrappanel)
  - [ImageFrame](#3-imageframe)
    - [ImageFrame Source](#imageframe-source)
    - [Image Caching](#image-caching)
    - [ImageFrame Transitions](#imageframe-transitions)
    - [Using ImageFrame with FilePicker](#using-imageframe-with-filepicker)
    - [Guidelines for Disposing `ImageFrame` effectively](#guidelines-for-disposing-imageframe-effectively)
  - [FluidBanner](#4-fluidbanner)
  - [FluidToggleSwitch](#5-fluidtoggleswitch)
  - [ProfileControl](#6-profilecontrol)
  - [ProgressRing3d](#7-progressring3d)
- [CompositionProToolkit Expressions](#compositionprotoolkit-expressions)
- [Win2d Helpers](#win2d-helpers)
    - [ICanvasStroke and CanvasStroke](#icanvasstroke-and-canvasstroke)
    - [CanvasDrawingSession extension methods](#canvasdrawingsession-extension-methods)
    - [Win2d MiniLanguage specification](#win2d-mini-language-specification)
    - [Parsing Win2d Mini Language with CanvasObject](#parsing-win2d-mini-language-with-canvasobject)
    - [CanvasPathBuilder extension methods](#canvaspathbuilder-extension-methods)
    - [Creating multilayer Vector shapes with CanvasElement](#creating-multilayer-vector-shapes-with-canvaselement)
- [Utility methods and Constants](#utility-methods-and-constants)
- [Updates Chronology](#updates-chronology)

**CompositionProToolkit** is a collection of helper classes for the **Windows.UI.Composition** namespace and the **Win2d** project. It also contains controls which can be used in UWP applications.

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

Using **CompositionProToolkit** you can now define a mask for the **Visual** using **Win2D**'s **CanvasGeometry**. First you need an object implementing the **ICompositionGenerator** interface. It can be obtained by the **CreateCompositionGenerator()** extension method of the **Compositor**. There are two APIS to obtain the **CompositionGenerator** - by providing a **Compositor** or by providing a **CompositionGraphicDevice**.

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
ICompositionGenerator generator = compositor.CreateCompositionGenerator();

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
If you want to render CanvasGeometry with a stroke, fill color and a background color, you have to use the **ICompositionGenerator** to create an object implementing the **IGeometrySurface**.

The following APIs are provided in **ICompositionGenerator** to create a **IGeometrySurface**

```C#
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, 
    ICanvasStroke stroke);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry, 
    Color fillColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasStroke stroke, Color fillColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    Color fillColor, Color backgroundColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasStroke stroke, Color fillColor, Color backgroundColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasBrush fillBrush);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasStroke stroke, ICanvasBrush fillBrush);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasBrush fillBrush, ICanvasBrush backgroundBrush);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasStroke stroke, ICanvasBrush fillBrush, ICanvasBrush backgroundBrush);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasBrush fillBrush, Color backgroundColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasStroke stroke, ICanvasBrush fillBrush, Color backgroundColor);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    Color fillColor, ICanvasBrush backgroundBrush);
IGeometrySurface CreateGeometrySurface(Size size, CanvasGeometry geometry,
    ICanvasStroke stroke, Color fillColor, ICanvasBrush backgroundBrush);    
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
ICompositionGenerator generator = compositor.CreateCompositionGenerator();

//Create the visual
SpriteVisual visual = compositor.CreateSpriteVisual();
visual.Size = new Vector2(400, 400);
visual.Offset = new Vector3(200, 0, 0);

// Create the combined geometry
var ellipse1 = CanvasGeometry.CreateEllipse(generator.Device, 200, 200, 150, 75);
var ellipse2 = CanvasGeometry.CreateEllipse(generator.Device, 200, 200, 75, 150);
var combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

// Create the stroke
var stroke = new CanvasStroke(2f, Colors.Black);

// Create the CompositionMask
IGeometrySurface geometrySurface = 
    generator.CreateGeometrySurface(visual.Size.ToSize(), combinedGeometry, stroke, Colors.Blue);

// Create SurfaceBrush from GeometrySurface
var surfaceBrush = compositor.CreateSurfaceBrush(geometrySurface.Surface);

visual.Brush = surfaceBrush;
```

**IGeometrySurface** provides several APIs which allow you to update its geometry, size, stroke, fill and background (and thus the shape of the Visual).

```C#
void Redraw(CanvasGeometry geometry);
void Redraw(ICanvasStroke stroke);
void Redraw(Color fillColor);
void Redraw(ICanvasStroke stroke, Color fillColor);
void Redraw(Color fillColor, Color backgroundColor);
void Redraw(ICanvasStroke stroke, Color fillColor, Color backgroundColor);
void Redraw(ICanvasBrush fillBrush);
void Redraw(ICanvasStroke stroke, ICanvasBrush fillBrush);
void Redraw(ICanvasBrush fillBrush, ICanvasBrush backgroundBrush);
void Redraw(ICanvasStroke stroke, ICanvasBrush fillBrush, 
    ICanvasBrush backgroundBrush);
void Redraw(Color fillColor, ICanvasBrush backgroundBrush);
void Redraw(ICanvasStroke stroke, Color fillColor, 
    ICanvasBrush backgroundBrush);
void Redraw(ICanvasBrush fillBrush, Color backgroundColor);
void Redraw(ICanvasStroke stroke, ICanvasBrush fillBrush, 
    Color backgroundColor);
void Redraw(Size size, CanvasGeometry geometry);
void Redraw(Size size, CanvasGeometry geometry, ICanvasStroke stroke);
void Redraw(Size size, CanvasGeometry geometry, Color fillColor);
void Redraw(Size size, CanvasGeometry geometry, ICanvasStroke stroke, 
    Color fillColor);
void Redraw(Size size, CanvasGeometry geometry, Color fillColor, 
    Color backgroundColor);
void Redraw(Size size, CanvasGeometry geometry, ICanvasStroke stroke,
    Color fillColor, Color backgroundColor);
void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush fillBrush);
void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush fillBrush,
    ICanvasBrush backgroundBrush);
void Redraw(Size size, CanvasGeometry geometry, ICanvasStroke stroke,
    ICanvasBrush fillBrush, ICanvasBrush backgroundBrush);
void Redraw(Size size, CanvasGeometry geometry, ICanvasBrush fillBrush,
    Color backgroundColor);
void Redraw(Size size, CanvasGeometry geometry, ICanvasStroke stroke,
    ICanvasBrush fillBrush, Color backgroundColor);
void Redraw(Size size, CanvasGeometry geometry, Color fillColor,
    ICanvasBrush backgroundBrush);
void Redraw(Size size, CanvasGeometry geometry, ICanvasStroke stroke,
    Color fillColor, ICanvasBrush backgroundBrush);
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

## 4. Creating a Frosted Glass Effect Brush using `IMaskSurface`
**CompositionProToolkit** now provides the following extension method for **Compositor** to create a Frosted Glass effect brush.

```C#
public static CompositionEffectBrush CreateFrostedGlassBrush(this Compositor compositor, IMaskSurface mask,
    Color blendColor, float blurAmount, CompositionBackdropBrush backdropBrush = null,
    float multiplyAmount = 0, float colorAmount = 0.5f, float backdropAmount = 0.5f)
```  


Using this method, you can create a Frosted Glass effect brush with a custom shape to a visual. You can provide a **Color** to blend with the BackdropBrush, the amount by which the BackdropBrush should be blurred and an optional **CompositionBackdropBrush**. If no **CompositionBackdropBrush** is provided by the user then this method creates one.

<img src="https://cloud.githubusercontent.com/assets/7021835/18399591/8953d48c-7686-11e6-848a-f36e0574f1e0.png" />

The main difference between this method and the **CreateMaskedBackdropBrush** is that when you apply the FrostedGlassBrush to a visual with a DropShadow, it will look better, whereas with the MaskedBackdropBrush, the shadow will darken the visual.

## 5. Loading Images on Visual using `IImageSurface`
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
var generator = compositor.CreateCompositionGenerator();

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

## 6. Creating the Reflection of a `ContainerVisual`
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
| **FluidAnimationDuration** | `TimeSpan` | Indicates the duration of the rearrangement animation. | **570 ms** |
| **FluidItems** | `ObservableCollection<UIElement>` | Indicates the Observable Collection of the rearranged children | **null** |
| **InitialAnimationDuration** | `TimeSpan` | Indicates the duration of the first layout animation. | **300 ms** |
| **IsComposing** | `Boolean` | Flag to indicate whether the children in the FluidWrapPanel can be rearranged or not. | **False** |
| **ItemHeight** | `Double` | Gets or sets the minimum height each child can have in the FluidWrapPanel. _The child's height should be a multiple of the **ItemHeight**_. | **0** |
| **ItemWidth** | `Double` | Gets or sets the minimum width each child can have in the FluidWrapPanel. _The child's width should be a multiple of the **ItemWidth**_. | **0** |
| **ItemsSource** | `IEnumerable` | Bindable property to which a collection of UIElement can be bound. | **null** |
| **OpacityAnimationDuration** | `TimeSpan` | Indicates the duration of the animation to change the opacity of a child item when it is selected or deselected. | **300 ms** |
| **OptimizeChildPlacement** | `Boolean` | Gets or sets the property that indicates whether the placement of the children is optimized. If set to true, the child is placed at the first available position from the beginning of the FluidWrapPanel. If set to false, each child occupies the same (or greater) row and/or column than the previous child. | **True** |
| **Orientation** | `System.Windows.Controls.Orientation` | Gets or sets the different orientations the FluidWrapPanel can have. _Possible values are **Horizontal** and **Vertical**_. | **Horizontal** | 
| **ScaleAnimationDuration** | `TimeSpan` | Indicates the uration of the animation to change the scale of a child item when it is selected or deselected. | **400 ms** |

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
| **`OptimizeShadow`** | `Boolean` | Indicates whether the **ImageFrame** should use a shared shadow object to display the shadow. | **False** |
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

### Guidelines for Disposing `ImageFrame` effectively
Since **ImageFrame** internally uses several Composition objects which must be disposed properly to optimize the memory usage within your app, the responsibility, of disposing the **ImageFrame**, lies upon you. **ImageFrame** implements the **IDisposable** interface and this provides the **Dispose()** method for easy disposal of the instance. If the **ImageFrame** instances are not disposed, they will not be garbage collected causing your app memory requirement to bloat.
Consider a scenario where you are displaying several **ImageFrame** instances in a **GridView** within a page. When the app navigates _away_ from the page, then you must dispose the **ImageFrame** instances like this

```C#
protected override void OnNavigatedFrom(NavigationEventArgs e)
{
    foreach (var imgFrame in ItemGridView.GetDescendantsOfType<ImageFrame>())
    {
        imgFrame.Dispose();
    }

    VisualTreeHelper.DisconnectChildrenRecursive(ItemGridView);
    ItemGridView.ItemsSource = null;

    base.OnNavigatedFrom(e);
}
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

## 5. FluidToggleSwitch

**FluidToggleSwitch** is a toggleswitch control which uses Composition Visuals to render its components and provides a richer look and feel to the ToggleSwitch control. There are three primary components within the ToggleSwitch
- **Background** - The outermost circular area.
- **Track** - The pill shaped area.
- **Thumb** - The innermost circular area.

> The reason **FluidToggleSwitch** is encapsulated with a circular background component is because the hit area for a touch input is normally circular.

<img src="https://user-images.githubusercontent.com/7021835/28183439-50d4e274-67c4-11e7-90ff-376b2d7116cf.gif" />

The following properties allow you to customize the **FluidToggleSwitch**

| Dependency Property | Type | Description | Default Value |
|---|---|---|---|
| **ActiveColor** | `Color` | Gets or sets the Color of the FluidToggleSwitch in Checked state. | **#4cd964** |
| **InactiveColor** | `Color` |  Gets or sets the Color of the FluidToggleSwitch in Unchecked state. | **#dfdfdf** |
| **DisabledColor** | `Color` |  Gets or sets the Color of the FluidToggleSwitch in Disabled state. | **#eaeaea** |

The above properties define the color of the Background component. The color of the Track component is derived automatically from the above properties. The color of the Thumb is white.

## 6. ProfileControl

**ProfileControl** allows you to display an image (normally a user profile image) in an intuitive way. This control is mainly implemented using Composition Visuals and animations which provide a rich user experience. Depending on the width and height of the **ProfileControl**, its shape can be either circular or elliptical. There are two main components within the **ProfileControl**

 - **Background Visual** - The outermost circular or elliptical area. This area is filled with the CompositionBackdropBrush which blends the control with whatever is rendered beneath the control.
 - **Image Visual** - The innermost circular or elliptical area. This area renders the image provided.

<img src="https://user-images.githubusercontent.com/7021835/28183445-55a7f4b2-67c4-11e7-9d84-5bacbc84efae.gif" />

 The following properties allow you to customize the **ProfileControl**

| Dependency Property | Type | Description | Default Value |
|---|---|---|---|
| **BlurRadius** | `Double` | Gets or sets the amount by which the brush of the Background Visual must be blurred. | **20.0** |
| **BorderGap** | `Double` | Gets or sets the uniform gap between the Background visual and the Image visual. | **10.0** |
| **FluidReveal** | `Boolean` | Indicates whether the reveal animation should automatically be played when the Source property of the ProfileControl changes. If set to False, the image specified by the Source property is displayed directly without any animation. | **True** |
| **RevealDuration** | `TimeSpan` | Gets or sets the duration of the reveal animation. | **1 sec** |
| **Source** | `Uri` | Gets or sets the Uri of the image to be displayed in the ProfileControl. | **null** |
| **Stretch** | `Stretch` | Indicates how the image content is resized to fill its allocated space in the Image Visual. | **Stretch.Uniform** |
| **Tint** | `Color` | Gets or sets the color overlay on the background of the ProfileControl. | **Colors.White** |

## 7. ProgressRing3d

**ProgressRing3d** is the three-dimensional version of the default `ProgressRing` control. It is created using Windows Composition APIs. It consists of five shapes (nodes) revolving around a 3D point.

<img src="https://user-images.githubusercontent.com/7021835/35103423-a43c0c2c-fc1a-11e7-8d72-6c66b159b837.gif" />

It has the following properties

| Dependency Property | Type | Description | Default Value |
|---|---|---|---|
| **NodeColor** | `Color` | Gets or sets Color of each of the nodes. | **Blue** |
| **NodeShape** | `ProgressRing3d.NodeShapeType` | Gets or sets the shape of the node (circle or square). | **ProgressRing3d.NodeShapeType.Circle** |
| **SyncAccentColor** | `Boolean` | Gets or sets the property which indicates whether the NodeColor should be synced with the SystemAccent color. If _SyncAccentColor is set to true, the NodeColor property will be ignored_. | **True** |

# CompositionProToolkit Expressions
<img src="https://cloud.githubusercontent.com/assets/7021835/18138158/41f5d4aa-6f60-11e6-8373-9e1085130bff.png" />

*ExpressionAnimations* allow a developer to define a mathematical equation that can be used to calculate the value of a targeted animating property each frame. The mathematical equation can be defined using references to properties of Composition objects, mathematical functions and operators and Input. 
*CompositionProToolkit.Expressions* namespace provides a set of helper classes and extension methods which facilitate the developer to define the mathematical equation in the form of a lambda expression. They provide type-safety, IntelliSense support and allows catching of errors during compile time.

More details [here](https://github.com/ratishphilip/CompositionProToolkit/tree/master/CompositionProToolkit/Expressions).

# Win2d Helpers

## ICanvasStroke and CanvasStroke
In Win2d, the stroke, that is used to render an outline to a CanvasGeometry, is comprised of three components
- Stroke Width – defines the width of the stroke.
- Stroke Brush – defines the **ICanvasBrush** that will be used to render the stroke.
- Stroke Style – defines the **CanvasStrokeStyle** for the stroke.

**ICanvasStroke** interface, defined in the CompositionProToolkit.Win2d namespace, encapsulates these three components and the **CanvasStroke** class implements this interface. It provides several constructors to define the stroke.

```C#
public interface ICanvasStroke
{
  ICanvasBrush Brush { get; }
  float Width { get; }
  CanvasStrokeStyle Style { get; }
  Matrix3x2 Transform { get; set; }
}

public sealed class CanvasStroke : ICanvasStroke
{
  public float Width { get; }
  public CanvasStrokeStyle Style { get; }
  public ICanvasBrush Brush { get; }
  public Matrix3x2 Transform { get; set; }
  public CanvasStroke(ICanvasBrush brush, float strokeWidth = 1f);
  public CanvasStroke(ICanvasBrush brush, float strokeWidth, CanvasStrokeStyle strokeStyle);
  public CanvasStroke(ICanvasResourceCreator device, Color strokeColor, float strokeWidth = 1f);
  public CanvasStroke(ICanvasResourceCreator device, Color strokeColor, float strokeWidth, 
                      CanvasStrokeStyle strokeStyle);
}
```

The **Transform** property in CanvasStroke gets or sets the Transform property of the stroke brush.

## CanvasDrawingSession extension methods

The following extension methods have been created for CanvasDrawingSession to incorporate ICanvasStroke in its DrawXXX() methods

```C#
public static void DrawCircle(this CanvasDrawingSession session,
    Vector2 centerPoint, float radius, ICanvasStroke stroke);
public static void DrawCircle(this CanvasDrawingSession session,
    float x, float y, float radius, ICanvasStroke stroke);
public static void DrawEllipse(this CanvasDrawingSession session, 
    Vector2 centerPoint, float radiusX, float radiusY, ICanvasStroke stroke);
public static void DrawEllipse(this CanvasDrawingSession session,
    float x, float y, float radiusX, float radiusY, ICanvasStroke stroke);
public static void DrawGeometry(this CanvasDrawingSession session,
    CanvasGeometry geometry, ICanvasStroke stroke);
public static void DrawGeometry(this CanvasDrawingSession session,
    CanvasGeometry geometry, Vector2 offset, ICanvasStroke stroke);
public static void DrawGeometry(this CanvasDrawingSession session,
    CanvasGeometry geometry, float x, float y, ICanvasStroke stroke);
public static void DrawLine(this CanvasDrawingSession session, Vector2 point0, 
    Vector2 point1, ICanvasStroke stroke);
public static void DrawLine(this CanvasDrawingSession session, float x0, 
    float y0, float x1, float y1, ICanvasStroke stroke);
public static void DrawRectangle(this CanvasDrawingSession session, Rect rect, 
    ICanvasStroke stroke) ;
public static void DrawRectangle(this CanvasDrawingSession session, float x, 
    float y, float w, float h, ICanvasStroke stroke);
public static void DrawRoundedRectangle(this CanvasDrawingSession session, 
    Rect rect, float radiusX, float radiusY, ICanvasStroke stroke);
public static void DrawRoundedRectangle(this CanvasDrawingSession session, float x,
    float y, float w, float h, float radiusX, float radiusY, ICanvasStroke stroke);
```
## Win2d Mini Language specification
**Microsoft.Graphics.Canvas.Geometry.CanvasGeometry** class facilitates the drawing and manipulation of complex geometrical shapes. These shapes can be outlined with a stroke and filled with a brush (which can be a solid color, a bitmap pattern or a gradient). 
While the **CanvasGeometry** class provides various static methods to create predefined shapes like Circle, Ellipse, Rectangle, RoundedRectangle, the **CanvasPathBuilder** class provides several methods to create freeform CanvasGeometry objects.
Creation of a complex freeform geometric shape may involve invoking of several CanvasPathBuilder commands. For example, the following code shows how to create a triangle geometry using CanvasPathBuilder

```C#
CanvasPathBuilder pathBuilder = new CanvasPathBuilder(device);

pathBuilder.BeginFigure(1, 1);
pathBuilder.AddLine(300, 300);
pathBuilder.AddLine(1, 300);
pathBuilder.EndFigure(CanvasFigureLoop.Closed);

CanvasGeometry triangleGeometry = CanvasGeometry.CreatePath(pathBuilder);
```
**Win2d Mini Language** is a powerful and sophisticated language which facilitates specifying complex geometries, color, brushes, strokes and stroke styles in a more compact manner. 

Using Win2d Mini-Language, the geometry in above example can be created in the following way

```C#
string pathData = “M 1 1 300 300 1 300 Z”;
CanvasGeometry triangleGeometry = CanvasObject.CreateGeometry(device, pathData);
```

Win2d Mini Language is based on the [SVG (Scalable Vector Graphics) Path language specification](http://www.w3.org/TR/SVG11/paths.html). 

The following specification document describes the Win2d Markup Language in detail.

[Win2d Mini Language Specification.pdf](https://github.com/ratishphilip/CompositionProToolkit/blob/master/Win2d%20Mini%20Language%20Specification/Win2d%20Mini%20Language%20Specification.pdf)

## Parsing Win2d Mini Language with CanvasObject
The **CompositionProToolkit.CanvasObject** static class provides APIs that parse the Win2d Mini Language and instantiate the appropriate objects.

### Color
#### From Hexadecimal Color String or High Dynamic Range Color String
There are two APIs that convert the hexadecimal color string in #RRGGBB or #AARRGGBB format or the High Dynamic Range Color string in the R G B A format to the corresponding Color object. _The **#** character is optional in Hexadecimal color string. **R**, **G**, **B** & **A** should have value in the range between 0 and 1, inclusive._

```C#
public static Color CreateColor(string colorString);
public static bool TryCreateColor(string  colorString , out Color color);
```

The first API will raise an **ArgumentException** if the argument is not in the correct format while the second API will attempt to convert the color string without raising an exception.

#### From High Dynamic Range Color string
The following API Converts a Vector4 High Dynamic Range Color to Color object. Negative components of the Vector4 will be sanitized by taking the absolute value of the component. The HDR Color components should have value in the range [0, 1]. If their value is more than 1, they will be clamped at 1.  Vector4's X, Y, Z, W components match to Color's R, G, B, A components respectively. 

```C#
public static Color CreateColor(Vector4 hdrColor);
```

### CanvasGeometry
The following API converts a CanvasGeometry path string to CanvasGeometry object
```C#
public static CanvasGeometry CreateGeometry(ICanvasResourceCreator resourceCreator,
                                            string pathData, 
                                            StringBuilder logger = null);
```

_The **logger** parameter in this method is an option argument of type **StringBuilder** which can be used to obtain the **CanvasPathBuilder** commands in text format. It is mainly intended for information/debugging purpose only_.

### ICanvasBrush
The following API converts a brush data string to ICanvasBrush object
```C#
public static ICanvasBrush CreateBrush(ICanvasResourceCreator resourceCreator, 
                                       string brushData);
```

### CanvasStrokeStyle
The following API converts a style data string to CanvasStrokeStyle object
```C#
public static CanvasStrokeStyle CreateStrokeStyle(string styleData);
```

### ICanvasStroke
The following API converts a stroke data string to ICanvasStroke object
```C#
public static ICanvasStroke CreateStroke(ICanvasResourceCreator resourceCreator,
                                         string strokeData);
```

## CanvasPathBuilder extension methods

**CanvasPathBuilder** allows you to create a freeform path using lines, arcs, Quadratic Beziers and Cubic Beziers. You can then convert this path to a **CanvasGeometry**. Each path is composed of one or more figures. Each figure definition is encapsulated by the **BeginFigure()** and **EndFigure()** methods of **CanvasPathBuilder**. 

If you want to add a circle(or ellipse) or a polygon figure to your path, you need to break the figure into curves or line segments and add them to the path one by one.

The following extension methods have been added to the CanvasPathBuilder to add a circle, ellipse or a polygon figure directly to your path

```C#
public static void AddCircleFigure (CanvasPathBuilder pathBuilder, Vector2 center, float radius);
public static void AddCircleFigure (CanvasPathBuilder pathBuilder, float x, float y, float radius);
public static void AddEllipseFigure(CanvasPathBuilder pathBuilder, Vector2 center, float radiusX,
    float radiusY);
public static void AddEllipseFigure(CanvasPathBuilder pathBuilder, float x, float y, float radiusX,
    float radiusY);
public static void AddPolygonFigure(CanvasPathBuilder pathBuilder, int numSides, Vector2 center,
    float radius);
public static void AddPolygonFigure(CanvasPathBuilder pathBuilder, int numSides, float x, float y,
    float radius);
public static void AddRectangleFigure(this CanvasPathBuilder pathBuilder, float x, float y,
    float width, float height)
public static void AddRoundedRectangleFigure(this CanvasPathBuilder pathBuilder, float x, float y,
    float width, float height, float radiusX, float radiusY);
```

In the **AddPolygonFigure**, the **radius** parameter denotes the radius of the circle circumscribing the polygon vertices (i.e. the distance between the center of the polygon and its vertices). 

**Note**: _These methods add the required curves or line segments to your path internally. Since these methods add a figure to your path, you can invoke them only after closing the current figure in the path. They must not be called in between **BeginFigure()** and **EndFigure()** calls, otherwise an **ArgumentException** will be raised. These extension methods call the **BeginFigure()** and **EndFigure()** **CanvasPathBuilder** methods internally._

Check out the [Sample Gallery project](https://github.com/ratishphilip/CompositionProToolkit/tree/master/SampleGallery) where you can interact with the **CanvasObject** class by providing the SVG/XAML path data and converting it to **CanvasGeometry**. You can alter the **StrokeThickness**, **StrokeColor** and **FillColor** of the rendered geometry.

<img src="https://cloud.githubusercontent.com/assets/7021835/22484131/e58b0046-e7b4-11e6-8b31-335c57a738f0.png" />

You can view the **CanvasPathBuilder** commands called to create the parsed geometry.

<img src="https://cloud.githubusercontent.com/assets/7021835/22484130/e58a13c0-e7b4-11e6-8764-1670dc866372.png" />

## Creating multilayer Vector shapes with CanvasElement
The CanvasElement class allows the creation of multilayer vector shapes. Each layer is represented by the CanvasRenderLayer class. The CanvasRenderLayer implements the ICanvasRenderLayer interface which encapsulates three properties required for rendering a layer
- **CanvasGeometry** - The geometry to be rendered on the layer.
- **ICanvasBrush** - The brush to fill the geometry.
- **ICanvasStroke** - The stroke to outline the geometry.

The CanvasRenderLayer provides several constructors which accept the CanvasGeometry, ICanvasBrush and ICanvasStroke objects or their respective data definition strings.

```C#
public CanvasRenderLayer(CanvasGeometry geometry, ICanvasBrush brush,
    ICanvasStroke stroke);
public CanvasRenderLayer(ICanvasResourceCreator creator, string geometryData,
    string brushData, string strokeData);
public CanvasRenderLayer(ICanvasResourceCreator creator, string geometryData,
    ICanvasBrush brush, Color strokeColor, float strokeWidth = 1f);
public CanvasRenderLayer(ICanvasResourceCreator creator, string geometryData,
    ICanvasBrush brush, Color strokeColor, float strokeWidth, 
    CanvasStrokeStyle strokeStyle);
public CanvasRenderLayer(ICanvasResourceCreator creator, string geometryData,
    ICanvasBrush brush, ICanvasBrush strokeBrush, float strokeWidth = 1);
public CanvasRenderLayer(ICanvasResourceCreator creator, string geometryData, 
    ICanvasBrush brush, ICanvasBrush strokeBrush, float strokeWidth, 
    CanvasStrokeStyle strokeStyle);
```

The CanvasElement class implements the ICanvasElement interface which provides the following properties and APIs

```C#
interface ICanvasElement
{
    List<ICanvasRenderLayer> Layers { get; set; }
    bool ScaleStroke { get; set; }
    
    void Render(CanvasDrawingSession session, float width, float height, Vector2 offset,
        Vector4 padding, float rotation);

    SpriteVisual CreateVisual(ICompositionGenerator generator, float width, float height,
        Vector2 offset, Vector4 padding, float rotation);
}
```

The **Render** API renders the CanvasElement layers on a CanvasControl or a CanvasAnimated control for the given dimensions, offset, padding and rotation.

The **CreateVisual** API creates a SpriteVisual which contains several SpriteVisuals, each representing a layer of the CanvasElement.

The constructor of **CanvasElement** requires the base dimensions of the element, the layers of the CanvasElement and an option whether to scale the stroke width when the CanvasElement is scaled (default is true).

```C#
public CanvasElement(float baseWidth, float baseHeight, IEnumerable<ICanvasRenderLayer> layers,
            bool scaleStroke = true);
```

Using the base dimension, the CanvasLayer is able to scale its layers to any valid dimensions.

_The layers are rendered based on their order in the layers list, i.e. the first layer in the list is rendered first and the subsequent layers are drawn on top of the previous layer. Thus the first layer in the list appears at the bottom and the last layer in the list is rendered top most._

**CanvasElement** can be primarily used to create vector based icons for UWP applications.

The following example code

```C#
CanvasElement _element;

private void OnCanvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
{
    var geom1 = CanvasObject.CreateGeometry(sender, "O 116 116 128 128");
    var fill1 = CanvasObject.CreateBrush(sender, "SC #00adef");
    var stroke1 = CanvasObject.CreateStroke(sender, "ST 8 SC #2a388f");
    var layer1 = new CanvasRenderLayer(geom1, fill1, stroke1);

    var geom2 = CanvasObject.CreateGeometry(sender, "U 56 56 64 64 8 8");
    var fill2 = CanvasObject.CreateBrush(sender, "SC #ed1c24");
    var stroke2 = CanvasObject.CreateStroke(sender, "ST 2 SC #404041");
    var layer2 = new CanvasRenderLayer(geom2, fill2, stroke2);

    var geom3 = CanvasObject.CreateGeometry(sender, "U 136 56 64 64 8 8");
    var fill3 = CanvasObject.CreateBrush(sender, "SC #38b449");
    var layer3 = new CanvasRenderLayer(geom3, fill3, stroke2);

    var geom4 = CanvasObject.CreateGeometry(sender, "U 56 136 64 64 8 8");
    var fill4 = CanvasObject.CreateBrush(sender, "SC #fff100");
    var layer4 = new CanvasRenderLayer(geom4, fill4, stroke2);

    var geom5 = CanvasObject.CreateGeometry(sender, "R 96 96 64 64");
    var fill5 = CanvasObject.CreateBrush(sender, "SC #f7931d");
    var layer5 = new CanvasRenderLayer(geom5, fill5, stroke2);

    var layers = new List<CanvasRenderLayer> { layer1, layer2, layer3, layer4, layer5 };
    _element = new CanvasElement(256f, 256f, layers);
}

private void OnCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
{
    _element?.Render(args.DrawingSession, 512f, 512f, Vector2.Zero, new Vector4(10), 0f);
}
```

will create the following 

<img src="https://cloud.githubusercontent.com/assets/7021835/22483836/c040e680-e7b3-11e6-9cf4-ca8682610876.png" />

# Utility methods and Constants

## ToSingle extension method

The **Single()** extension method for **System.Double** is now marked as **obsolete**. _Your code will still work, but you will receive a warning during build_.

The **Single()** extension method is now replaced with **ToSingle()** extension method. It does the same job - converts **System.Double** to **System.Single**.

# Updates Chronology

## v0.8.0
(**Thursday, January 18, 2018**) - Added `ProgressRing3d` control.

## v0.7.0
(**Thursday, July 13, 2017**) - Added `FluidToggleSwitch` control and `ProfileControl`.

## v0.6.0  
(**Tuesday, June 13, 2017**) - Refactored `CompositionProToolkit.Expressions` namespace, added new extension methods.

## v0.5.1 
(**Tuesday, January 31, 2017**) -Added `ICanvasStroke`, `CanvasElement`, `CanvasRenderLayer`, Win2d Mini Language support, `CanvasPathBuilder` extension methods and `CanvasDrawingSession` extension methods.

## v0.5.0 
(**Saturday, December 24, 2016**) -Added `CanvasPathBuilder` extension methods and `CanvasGeometryParser` which parses XAML or SVG path data to CanvasGeometry.

## v0.4.6.1
(**Friday, September 9, 2016**) - `ImageFrame` now implements `IDisposable`. `OptimizeShadow` feature added to `ImageFrame`. `CreateFrostedGlassBrush` extension method added to compositor.

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
