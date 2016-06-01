# CompositionProToolkit
**CompositionProToolkit** is a collection of helper classes for Windows.UI.Composition. The main difference between **CompositionProToolkit** and [**CompositionProToolkit**](https://github.com/ratishphilip/CompositionExpressionToolkit) is that **CompositionProToolkit** has a dependency on **Win2D** library.

# CompositionProToolkit Internals

## 1. Creating custom shaped `Visual` using `CanvasGeometry`
As of now, you can customize the shape of Visuals by applying a mask on the Visual. The mask is defined using a **CompositionMaskBrush**. In the **CompositionMaskBrush** the `Mask` is defined by a **CompositionSurfaceBrush**. Into the **CompositionSurfaceBrush** an image, which defines the mask, is loaded. In this image, the areas which are to masked in the Visual are transparent whereas the areas to be shown in the Visual are white.  
Using **CompositionProToolkit** you can now define a mask for the **Visual** using **Win2D**'s **CanvasGeometry**. It provides two interfaces **ICompositionMask** and **ICompositionMaskGenerator** and a static class **CompositionMaskFactory** which provides an object implementing the **ICompositionMaskGenerator**. Using the **ICompositionMaskGenerator** an object implementing the **ICompositionMask** can be created. This object represents the mask that needs to be applied to the Visual using a **CompositionMaskBrush**.

### Example

The following code

```C#
// Get the Generator
ICompositionMaskGenerator generator = CompositionMaskFactory.GetCompositionMaskGenerator(compositor);

//Create the visual
SpriteVisual visual = compositor.CreateSpriteVisual();
visual.Size = new Vector2(400, 400);
visual.Offset = new Vector3(200, 0, 0);

// Create the combined geometry
var ellipse1 = CanvasGeometry.CreateEllipse(_generator.Device, 200, 200, 150, 75);
var ellipse2 = CanvasGeometry.CreateEllipse(_generator.Device, 200, 200, 75, 150);
var combinedGeometry = ellipse1.CombineWith(ellipse2, Matrix3x2.Identity, CanvasGeometryCombine.Union);

// Create the CompositionMask
ICompositionMask compositionMask = await generator.CreateMaskAsync(visual.Size.ToSize(), combinedGeometry);

// Create SurfaceBrush from CompositionMask
var mask = compositor.CreateSurfaceBrush(_compositionMask.Surface);
var source = compositor.CreateColorBrush(Colors.Blue);

// Create mask brush
var maskBrush = compositor.CreateMaskBrush();
maskBrush.Mask = mask;
maskBrush.Source = source;

visual.Brush = maskBrush;
```

creates the following output.

**ICompositionMask** provides a **RedrawAsync** API which allows you to update the geometry of the mask (and thus the shape of the Visual). Here is an example of a **CanvasAnimatedControl** having two visuals - A blue rectangular visual in the background and a red visual in the foreground. The red visual's mask is redrawn periodically to give an impression of animation. (_see the **Sample** project for more details on how it is implemented_)
