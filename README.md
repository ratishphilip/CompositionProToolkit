<img src="https://cloud.githubusercontent.com/assets/7021835/16889814/1784ed78-4a9e-11e6-80d0-7c2084d6c960.png" alt="CompositionProToolkit"></img>

# Table of Contents

- [Installing from NuGet](#installing-from-nuget)
- [CompositionProToolkit Internals](#compositionprotoolkit-internals)
  - [Creating custom shaped `Visual` using `CanvasGeometry`](#1-creating-custom-shaped-visual-using-canvasgeometry)
  - [Creating Masked Backdrop Brush using `ICompositionMask`](#2-creating-masked-backdrop-brush-using-icompositionmask)
  - [Loading Images on Visual using `ICompositionSurfaceImage`](#3-loading-images-on-visual-using-icompositionsurfaceimage)
  - [Creating the Reflection of a `ContainerVisual`](#4-creating-the-reflection-of-a-containervisual)
- [CompositionProToolkit Controls](#compositionprotoolkit-controls)
  - [FluidProgressRing](#1-fluidprogressring)
  - [FluidWrapPanel](#2-fluidwrappanel)
- [Updates Chronology](updates-chronology)

**CompositionProToolkit** is a collection of helper classes for Windows.UI.Composition. It also contains controls which can be used in UWP applications. It has dependency on the **Win2D** and the  [**CompositionExpressionToolkit**](https://github.com/ratishphilip/CompositionExpressionToolkit) libraries.

# Installing from NuGet

To install **CompositionProToolkit**, run the following command in the **Package Manager Console**

```
Install-Package CompositionProToolkit
```

More details available [here](https://www.nuget.org/packages/CompositionProToolkit/).

# CompositionProToolkit Internals

## 1. Creating custom shaped `Visual` using `CanvasGeometry`
As of now, you can customize the shape of Visuals by applying a mask on the Visual. The mask is defined using a **CompositionMaskBrush**. In the **CompositionMaskBrush** the `Mask` is defined by a **CompositionSurfaceBrush**. Into the **CompositionSurfaceBrush** an image, which defines the mask, is loaded. In this image, the areas which are to masked in the Visual are transparent whereas the areas to be shown in the Visual are white.  

Using **CompositionProToolkit** you can now define a mask for the **Visual** using **Win2D**'s **CanvasGeometry**. It provides two interfaces **ICompositionMask** and **ICompositionGenerator** and a static class **CompositionGeneratorFactory** which provides an object implementing the **ICompositionGenerator**. Using the **ICompositionGenerator** an object implementing the **ICompositionMask** can be created. This object represents the mask that needs to be applied to the Visual using a **CompositionMaskBrush**.

The following APIs are provided in **ICompositionGenerator** to create a **ICompositionMask**

```C#
Task<ICompositionMask> CreateMaskAsync(Size size, CanvasGeometry geometry);
Task<ICompositionMask> CreateMaskAsync(Size size, CanvasGeometry geometry, Color color);
Task<ICompositionMask> CreateMaskAsync(Size size, CanvasGeometry geometry, ICanvasBrush brush);
```

### Example

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

// Create the CompositionMask
ICompositionMask compositionMask = await generator.CreateMaskAsync(visual.Size.ToSize(), combinedGeometry);

// Create SurfaceBrush from CompositionMask
var mask = compositor.CreateSurfaceBrush(compositionMask.Surface);
var source = compositor.CreateColorBrush(Colors.Blue);

// Create mask brush
var maskBrush = compositor.CreateMaskBrush();
maskBrush.Mask = mask;
maskBrush.Source = source;

visual.Brush = maskBrush;
```

creates the following output.  

<img src="https://cloud.githubusercontent.com/assets/7021835/15728977/0f9f397a-2815-11e6-9df2-65b9ad1f5e9f.PNG" />

You can also provide a fill color while creating a **ICompositionMask**. You can then use this **ICompositionMask** to create a **CompositionSurfaceBrush** which will achieve the same result. The previous example can also be written as

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
ICompositionMask compositionMask = 
                  await generator.CreateMaskAsync(visual.Size.ToSize(), combinedGeometry, Colors.Blue);

// Create SurfaceBrush from CompositionMask
var surfaceBrush = compositor.CreateSurfaceBrush(compositionMask.Surface);

visual.Brush = surfaceBrush;
```


**ICompositionMask** provides a **RedrawAsync** API which allows you to update the geometry of the mask (and thus the shape of the Visual).  

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
    await animatedCompositionMask.RedrawAsync(updatedGeometry);
}
```

<img src="https://cloud.githubusercontent.com/assets/7021835/15728986/1baeab9c-2815-11e6-8e93-846b70a2a3ea.gif" />

## 2. Creating Masked Backdrop Brush using `ICompositionMask`
**CompositionProToolkit** now provides the following extension method for **Compositor** to create a masked Backdrop brush.

```C#
public static CompositionEffectBrush CreateMaskedBackdropBrush(this Compositor compositor, ICompositionMask mask,
            Color blendColor, float blurAmount, CompositionBackdropBrush backdropBrush = null)
```  

Using this method, you can apply a BackdropBrush with a custom shape to a visual. You can provide a **Color** to blend with the BackdropBrush, the amount by which the BackdropBrush should be blurred and an optional **CompositionBackdropBrush**. If no **CompositionBackdropBrush** is provided by the user then this method creates one.

**Note** : _Create only one instance of **CompositionBackdropBrush** and reuse it within your application. It provides a better performance._

<img src="https://cloud.githubusercontent.com/assets/7021835/16091854/562d255c-32ea-11e6-8952-424a513741ea.gif" />

## 3. Loading Images on Visual using `ICompositionSurfaceImage`
**ICompositionSurfaceImage** is an interface which encapsulates a **CompositionDrawingSurface** onto which an image can be loaded by providing a **Uri**. You can then use the **CompositionDrawingSurface** to create a **CompositionSurfaceBrush** which can be applied to any **Visual**.

**ICompositionGenerator** provides the following API which allows you to create a **CompositionSurfaceImage**

```C#
Task<ICompositionSurfaceImage> CreateSurfaceImageAsync(Uri uri, Size size, 
    CompositionSurfaceImageOptions options);
```

This API requires the **Uri** of the image to be loaded, the **size** of the CompositionDrawingSurface (_usually the same size as the **Visual** on which it is finally applied_) and the **CompositionSurfaceImageOptions**.

### CompositionSurfaceImageOptions

The **CompositionSurfaceImageOptions** class encapsulates a set of properties which influence the rendering of the image on the *CompositionSurfaceImage*. The following table shows the list of these properties.  


| Property | Type | Description | Possible Values |
|---|---|---|---|
| **`Stretch`**| `Stretch` | Describes how image is resized to fill its allocated space. | **`None`**, **`Uniform`**, **`Fill`**, **`UniformToFill`** |
| **`HorizontalAlignment`** | `AlignmentX` | Describes how image is positioned horizontally in the **`CompositionSurfaceImage`**. | **`Left`**, **`Center`**, **`Right`** |
| **`VerticalAlignment`** | `AlignmentY` | Describes how image is positioned vertically in the **`CompositionSurfaceImage`**. | **`Top`**, **`Center`**, **`Bottom`** |
| **`Opacity`** | `float` | Specifies the opacity of the rendered image. | **`0 - 1f`** inclusive |
| **`Interpolation`** | `CanvasImageInterpolation` | Specifies the interpolation used to render the image on the **`CompositionSurfaceImage`**.  | **`NearestNeighbor`**, **`Linear`**, **`Cubic`**, **`MultiSampleLinear`**, **`Anisotropic`**, **`HighQualityCubic`** |
| **`SurfaceBackgroundColor`** | `Color` | Specifies the color which will be used to fill the **`CompositionSurfaceImage`** before rendering the image. | All possible values that can be created. |

### Example

The following example shows how you can load an image onto a **Visual**


```C#
var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
var generator = CompositionGeneratorFactory.GetCompositionGenerator(compositor);

var visual = compositor.CreateSpriteVisual();
visual.Size = new Vector2(this.ActualWidth.Single(), this.ActualHeight.Single());
visual.Offset = Vector3.Zero;

var options = new CompositionSurfaceImageOptions(Stretch.Uniform, AlignmentX.Center, AlignmentY.Center, 0.9f,
                CanvasImageInterpolation.Cubic)
    {
        SurfaceBackgroundColor = Colors.Maroon
    };

var surfaceImage =
    await generator.CreateSurfaceImageAsync(new Uri("ms-appx:///Assets/Images/Image3.jpg"), visual.Size.ToSize(), options);

visual.Brush = compositor.CreateSurfaceBrush(_surfaceImage.Surface);
```

Once you create a **CompositionSurfaceBrush** from the **ICompositionSurfaceImage** and apply it to a **Visual**, you can use the following **ICompositionSurfaceImage** APIs to resize, provide a new Uri or change the rendering options

```C#
Task RedrawAsync();

Task RedrawAsync(CompositionSurfaceImageOptions options);

Task RedrawAsync(Uri uri, CompositionSurfaceImageOptions options);

Task RedrawAsync(Uri uri, Size size, CompositionSurfaceImageOptions options);

Task ResizeAsync(Size size);

Task ResizeAsync(Size size, CompositionSurfaceImageOptions options);
```

Once you call any of the above methods, the Visual's brush is also updated.

## 4. Creating the Reflection of a `ContainerVisual`
**CompositionProToolkit** provides the following API which allows you to create the reflection of any **ContainerVisual**

```C#
Task CreateReflectionAsync(ContainerVisual visual, float reflectionDistance = 0f,
    float reflectionLength = 0.7f, ReflectionLocation location = ReflectionLocation.Bottom);
```

The parameters required for this API are
- **visual** - A **ContainerVisual** whose reflection has to be created.
- **reflectionDistance** - The distance between the visual and its reflection.
- **reflectionLength** - The normalized length (**0 - 1f**, inclusive) of the visual that must be visible in the reflection. _Default value is **0.7f**_.
- **location** - Specifies the location of the reflection with respect to the visual - Bottom, Top, Left or Right. _Default value is **ReflectionLocation.Bottom**_.

This API will create a reflection even if an effect is applied to the Visual.

If the visual has multiple other visuals in its visual tree, then the entire visual tree is reflected.


# CompositionProToolkit Controls

## 1. FluidProgressRing
**FluidProgressRing** is a concept design for a modern version of the **ProgressRing** control in UWP.The **ProgressRing** control has remained the same since Windows 8 and it is high time it got a refresh. The **FluidProgressRing** consists of a set of small circles (**nodes**) rotating about a common center. Each node rotates until it hits the adjacent node (then it slows down to a stop). The adjacent node then rotates and hits the next node and this process continues. The animations are done using the **Windows.UI.Composition APIs** and provide a smooth look and feel.

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



# Updates Chronology
## v0.3.0
(**Friday, July 15, 2016**) - Added `FluidWrapPanel` Control. **SampleGallery** added.

## v0.2.0
(**Friday, July 1, 2016**) - Added `FluidProgressRing` Control..

## v0.1.1
(**Wednesday, June 15, 2016**) - Added Compositor Extension method to create Masked Backdrop Brush..

## v0.1.0
(**Wednesday, June 1, 2016**) - Initial Version.


