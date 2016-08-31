<p align="center">
<img align="center" src="https://cloud.githubusercontent.com/assets/7021835/18138158/41f5d4aa-6f60-11e6-8373-9e1085130bff.png" alt="CompositionProToolkit.Expressions logo">
</p>

__CompositionProToolkit.Expressions__ namespace consists of a collection of Extension methods and Helper classes which make it easier to use <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.aspx">Windows.UI.Composition</a> features. They include methods for creating statically typed **CompositionAnimation** expressions, **CompositionPropertySet** extension methods, helper methods for creating **ScopedBatchSets** etc.

# Table of Contents

- [CompositionProToolkit.Expressions Internals](#compositionprotoolkit.expressions-internals)
  - [CompositionPropertySet extensions](#1-compositionpropertyset-extensions)
  - [Creating statically typed CompositionAnimation Expressions](#2-creating-statically-typed-compositionanimation-expressions)
  - [Using Lambda Expressions for StartAnimation & StopAnimation](#3-using-lambda-expressions-for-startanimation--stopanimation)
  - [Using Arrays within Lambda Expression for defining animations](#4-using-arrays-within-lambda-expression-for-defining-animations)
  - [Using Lambda Expression for creating EffectFactory and animating CompositionEffectBrushes](#5-using-lambda-expression-for-creating-effectfactory-and-animating-compositioneffectbrushes)
  - [KeyFrame&lt;T&gt;](#6-keyframet)
  - [KeyFrameAnimation&lt;T&gt;](#7-keyframeanimationt)
  - [ScopedBatchHelper](#8-scopedbatchhelper)
  - [Converting from `double` to `float`](#9-converting-from-double-to-float)
- [Updates Chronology](#updates-chronology)
- [Credits](#credits)

# CompositionProToolkit.Expressions Internals

## 1. CompositionPropertySet extensions
The <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.compositionpropertyset.aspx">__CompositionPropertySet__</a> class is like a dictionary which stores key-value pairs. As of now, the values can be of type __bool__, __float__, __Color__, __Matrix3x2__, __Matrix4x4__, __Quaternion__, __Scalar__, __Vector2__, __Vector3__ and __Vector4__. To store and retrieve, __CompositionPropertySet__ has separate __Insert*xxx*__ and __TryGet*xxx*__ methods for each type.  
__CompositionProToolkit.Expressions__ provides generic extension methods __Insert<T>__ and __Get<T>__ which makes things simpler.

```C#
public static void Insert<T>(this CompositionPropertySet propertySet, string key, object input);
public static T Get<T>(this CompositionPropertySet propertySet, string key);
```

## 2. Creating statically typed CompositionAnimation Expressions
According to MSDN, <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.expressionanimation.aspx">__ExpressionAnimation__</a> and <a href="">__KeyFrameAnimation__</a> use a _mathematical expression_ to specify how the animated value should be calculated each frame. The expressions can reference properties from composition objects. _Currently, the mathematical expression is provided in the form of a **string**_. Expression animations work by parsing the mathematical expression string and internally converting it to a list of operations to produce an output value.  
Well, using a __string__ for creating an expression increases the chance of introducing errors (spelling, type-mismatch to name a few...). These errors will not be picked up during compile time and can be difficult to debug during runtime too.  
To mitigate this issue, we can use lambda expressions which are statically typed and allow the common errors to be caught during compile time.

**CompositionProToolkit.Expressions** provides the following extension methods which allow the user to provide lambda expressions

```C#
public static ExpressionAnimation CreateExpressionAnimation<T>(this Compositor compositor,
            Expression<CompositionLambda<T>> expression);
            
public static Dictionary<string, object> SetExpression<T>(this ExpressionAnimation animation,
			Expression<CompositionLambda<T>> expression);
	
public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, 
	float normalizedProgressKey, 
	Expression<CompositionLambda<T>> expression, CompositionEasingFunction easingFunction = null);
```

Each of these methods have a parameter of type **Expression&lt;CompositionLambda&lt;T&gt;&gt;** which defines the actual lambda expression. These extension methods parse the lambda expression and convert them to appropriate mathematical expression string and link to the symbols used in the lambda expression by calling the appropriate __Set*xxx*Parameter__ internally.  

**CompositionLambda&lt;T&gt;** is a delegate which is defined as

```C#
public delegate T CompositionLambda<T>(CompositionExpressionContext<T> ctx);
```

**CompositionExpressionContext&lt;T&gt;** is a generic class which defines a set of dummy helper functions (all the <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.expressionanimation.aspx">__helper methods__</a> supported by ExpressionAnimation). These methods are primarily used to create the lambda expression. This class also defines the **StartingValue** and **FinalValue** properties for use within **CompositionLambda** expressions.

**CompositionProToolkit.Expressions** also provides the following extension methods which allow the user to provide a key-value pair (or a set of key-value pairs) as parameter(s) for the **CompositionAnimation**

```C#
public static bool SetParameter<T>(this T animation, string key, object input) 
    where T : CompositionAnimation 
{
}

public static T SetParameters<T>(this T animation, Dictionary<string, object> parameters) 
    where T : CompositionAnimation 
{
}
```

### Examples
The following examples show how expressions are currently provided in string format to ExpressionAnimation. These examples also show how, using **CompositionProToolkit.Expressions**, **Expression&lt;CompositionLambda&lt;T&gt;&gt;** can be created (for the same scenario) and provided to the ExpressionAnimation.

#### Example 1

**Without using CompositionProToolkit.Expressions**

```C#
Point position = new Point(0,0);
Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
CompositionPropertySet scrollProperties = 
	ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

position.X += scrollViewer.HorizontalOffset;
position.Y += scrollViewer.VerticalOffset;
offsetAnimation.Duration = totalDuration;

// Create expression string
string expression = 
	"Vector3(scrollingProperties.Translation.X, scrollingProperties.Translation.Y, 0) + itemOffset";

// Set the expression
offsetAnimation.InsertExpressionKeyFrame(1f, expression);

// Set the parameters
offsetAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);
offsetAnimation.SetVector3Parameter("itemOffset", new Vector3((float) position.X, (float) position.Y, 0));
```

**Using CompositionProToolkit.Expressions**

```C#
Point position = new Point(0,0);
Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
CompositionPropertySet scrollProperties = 
	ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

position.X += scrollViewer.HorizontalOffset;
position.Y += scrollViewer.VerticalOffset;
var itemOffset = new Vector3(position.X.Single(), position.Y.Single(), 0);

offsetAnimation.Duration = totalDuration;

// Create the CompositionLambda Expression
Expression<CompositionLambda<Vector3>> expression =
	c => c.Vector3(scrollProperties.Get<TranslateTransform>("Translation").X.Single(),
		 	scrollProperties.Get<TranslateTransform>("Translation").Y.Single(), 0) + itemOffset;
		 
// Set the Expression
offsetAnimation.InsertExpressionKeyFrame(1f, expression);
```

#### Example 2

**Without using CompositionProToolkit.Expressions**

```C#
ScrollViewer myScrollViewer = ThumbnailList.GetFirstDescendantOfType<ScrollViewer>();
var scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(myScrollViewer);
			
ExpressAnimation parallaxExpression = compositor.CreateExpressionAnimation();
parallaxExpression.SetScalarParameter("StartOffset", 0.0f);
parallaxExpression.SetScalarParameter("ParallaxValue", 0.5f);
parallaxExpression.SetScalarParameter("ItemHeight", 0.0f);
parallaxExpression.SetReferenceParameter("ScrollManipulation", scrollProperties);
parallaxExpression.Expression = "(ScrollManipulation.Translation.Y + StartOffset - (0.5 * ItemHeight)) * 
	ParallaxValue - (ScrollManipulation.Translation.Y + StartOffset - (0.5 * ItemHeight))";
```

**Using CompositionProToolkit.Expressions**

```C#
ScrollViewer myScrollViewer = ThumbnailList.GetFirstDescendantOfType<ScrollViewer>();
var scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(myScrollViewer);
			
ExpressAnimation parallaxExpression = compositor.CreateExpressionAnimation();

var StartOffset = 0.0f;
var ParallaxValue = 0.5f;
var ItemHeight = 0.0f;
// Create the Expression
Expression<CompositionLambda<float>> expr = c =>
		((scrollProperties.Get<TranslateTransform>("Translation").Y + StartOffset - (0.5 * ItemHeight)) *
		 ParallaxValue - (scrollProperties.Get<TranslateTransform>("Translation").Y + StartOffset - 
		 (0.5 * ItemHeight))).Single();
// Set the Expression
parallaxExpression.SetExpression(expr);
```

#### Example 3 

This example shows how to provide _this.StartingValue_ and _this.FinalValue_ in a **CompositionLambda** Expression  

**Without using CompositionProToolkit.Expressions**

```C#
var scaleKeyFrameAnimation = _compositor.CreateVector3KeyFrameAnimation();
scaleKeyFrameAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
scaleKeyFrameAnimation.Duration = TimeSpan.FromSeconds(3);

var rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
rotationAnimation.InsertExpressionKeyFrame(1.0f, "this.StartingValue + 45.0f");
rotationAnimation.Duration = TimeSpan.FromSeconds(3);
```

**Using CompositionProToolkit.Expressions**

```C#
Expression<CompositionLambda<float>> expr1 = c => c.FinalValue;

var scaleKeyFrameAnimation = _compositor.CreateVector3KeyFrameAnimation();
scaleKeyFrameAnimation.InsertExpressionKeyFrame(1.0f, expr1);
scaleKeyFrameAnimation.Duration = TimeSpan.FromSeconds(3);

Expression<CompositionLambda<float>> expr2 = c => c.StartingValue + 45.0f;

var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
rotationAnimation.InsertExpressionKeyFrame(1.0f, expr2);
rotationAnimation.Duration = TimeSpan.FromSeconds(3);
```

#### Example 4

The following table shows few examples of **Expression&lt;CompositionLambda&lt;T&gt;&gt;** expressed as **String**

| T | Expression&lt;CompositionLambda&lt;T&gt;&gt; | String |
|-----|-----|-----|
|`float` | `c => c.StartingValue`| `"this.StartingValue"` |
|`float` | `c => c.FinalValue`| `"this.FinalValue"` |
|`float` | `c => c.StartingValue + (45.0).Single()`| `"this.StartingValue + 45"` |
|`Vector3` | `c => c.Vector3(propSet.Get<TranslateTransform>("Translation").X, propSet.Get<TranslateTransform>("Translation").Y, 0)` <br /> _[**propSet** is of type **CompositionPropertySet**]_ | `"Vector3(propSet.Translation.X, propSet.Translation.Y, 0)` |
|`Vector3` | `c => c.Vector3(propSet.Properties.Get<TranslateTransform>("Translation").X, propSet.Properties.Get<TranslateTransform>("Translation").Y, 0)`<br /> _[**propSet** is of type **CompositionPropertySet**]_ | `"Vector3(propSet.Translation.X, propSet.Translation.Y, 0)` |

## 3. Using Lambda Expressions for StartAnimation & StopAnimation

In order to avoid providing the property names as string for **StartAnimation** and **StopAnimation** methods of **CompositionObject**, **CompositionProToolkit.Expressions** provides the following extension methods

```C#

public static void StartAnimation(this CompositionObject compositionObject,
            Expression<Func<object>> expression, CompositionAnimation animation);

public static void StopAnimation(this CompositionObject compositionObject,
            Expression<Func<object>> expression);

public static string ScaleXY(this CompositionObject compositionObject);
```

The method **ScaleXY** is a dummy method to specify that the animation has to be executed on both the **Scale.X** as well as the **Scale.Y** properties of **CompositionObject** simultaneously.

### Examples

The following examples show how lamda expression can be used in the **StartAnimation** and **StopAnimation** extension methods.

#### Example 1

**Without using CompositionProToolkit.Expressions**

```C#
rootVisual.StartAnimation("Opacity", fadeInAnimation);
rootVisual.StartAnimation("RotationAxis.X", rotateAnimation);
rootVisual.StartAnimation("Scale.XY", scaleAnimation);
rootVisual.StopAnimation("Offset", offsetAnimation);
```

**Using CompositionProToolkit.Expressions**

```C#
rootVisual.StartAnimation(() => rootVisual.Opacity, fadeInAnimation);
rootVisual.StartAnimation(() => rootVisual.RotationAxis.X, rotateAnimation);
rootVisual.StartAnimation(() => rootVisual.ScaleXY(), scaleAnimation);
rootVisual.StopAnimation(() => rootVisual.Offset, offsetAnimation);
```
## 4. Using Arrays within Lambda Expression for defining animations
You can use arrays within the **CompositionLambda<T>** expressions for defining the mathematical expression for animations.

### Example

```C#
private void Page_Loaded(object sender, RoutedEventArgs e)
{
    _gearVisuals = new Visual[Container.Children.Count()];
    
    for (int i = 0; i < _gearVisuals.Length; i++)
    {
        AddGear(i, Container.Children.ElementAt(i) as Image);

        if (i == 0)
        {
            _compositor = _gearVisuals[0].Compositor;
        }
        else
        {
            ConfigureGearAnimation(i, i-1);
        }
    }
}

private void ConfigureGearAnimation(int current, int previous)
{
    Expression<CompositionLambda<float>> expr = c => -_gearVisuals[previous].RotationAngleInDegrees;
    _rotationExpression = _compositor.CreateExpressionAnimation(expr);
    
    _gearVisuals[current].StartAnimation(() => _gearVisuals[current].RotationAngleInDegrees, _rotationExpression);
}
```
## 5. Using Lambda Expression for creating EffectFactory and animating CompositionEffectBrushes
You can use lambda expressions to specify the animatable properties while creating the EffectFactory and to animate CompositionEffectBrushes.  

The following extension method is defined for the **Compositor** for creating the EffectFactory

```C#
public static CompositionEffectFactory CreateEffectFactory(this Compositor compositor,
            IGraphicsEffect graphicsEffect, params Expression<Func<object>>[] animatablePropertyExpressions);
```

### Example

**Without using CompositionProToolkit.Expressions**

```C#
private ArithmeticCompositeEffect _graphicsEffect;

_graphicsEffect = new ArithmeticCompositeEffect
{
    Name = "Arithmetic",
    Source1 = new CompositionEffectSourceParameter("ImageSource"),
    Source1Amount = .25f,
    Source2 = new Transform2DEffect
    {
        Name = "LightMapTransform",
        Source = new CompositionEffectSourceParameter("LightMap")
    },
    Source2Amount = 0,
    MultiplyAmount = 1
};

var effectFactory = _compositor.CreateEffectFactory(_graphicsEffect, 
						new [] { "LightMapTransform.TransformMatrix" });

var brush = effectFactory.CreateBrush();

brush.StartAnimation("LightMapTransform.TransformMatrix", _transformExpression);
```

**Using CompositionProToolkit.Expressions**

```C#
private ArithmeticCompositeEffect _graphicsEffect;

_graphicsEffect = new ArithmeticCompositeEffect
{
    Name = "Arithmetic",
    Source1 = new CompositionEffectSourceParameter("ImageSource"),
    Source1Amount = .25f,
    Source2 = new Transform2DEffect
    {
        Name = "LightMapTransform",
        Source = new CompositionEffectSourceParameter("LightMap")
    },
    Source2Amount = 0,
    MultiplyAmount = 1
};

var effectFactory = _compositor.CreateEffectFactory(_graphicsEffect, 
						() => ((Transform2DEffect)_graphicsEffect.Source2).TransformMatrix);

var brush = effectFactory.CreateBrush();

brush.StartAnimation(() => ((Transform2DEffect)_graphicsEffect.Source2).TransformMatrix, 
						_transformExpression);
```

## 6. KeyFrame&lt;T&gt;
**KeyFrame&lt;T&gt;** is a generic class which encapsulates the values required to define a KeyFrame within a **KeyFrameAnimation**. It has the following properties

| Property | Type | Description |
|---|---|---|
| **Key** | `float` | The time the key frame should occur at, expressed as a percentage of the animation Duration. Allowed value is from 0.0 to 1.0. |
| **Value** | `T` | The expression used to calculate the value of the KeyFrame. |
| **Easing** | `CompositionEasingFunction` | The easing function to use when interpolating between frames. |

## 7. KeyFrameAnimation&lt;T&gt;

According to [MSDN](https://msdn.microsoft.com/en-us/windows/uwp/graphics/composition-animation), KeyFrame Animations are time-based animations that use one or more key frames to specify how the animated value should change over time. The frames represent markers, allowing you to define what the animated value should be at a specific time. To construct a KeyFrame Animation, use one of the constructor methods of the **Compositor** class that correlates to the structure type of the property you wish to animate.

- **CreateColorKeyFrameAnimation**
- **CreateQuaternionKeyFrameAnimation**
- **CreateScalarKeyFrameAnimation**
- **CreateVector2KeyFrameAnimation**
- **CreateVector3KeyFrameAnimation**
- **CreateVector4KeyFrameAnimation**

**KeyFrameAnimation&lt;T&gt;** is a generic class which encapsulates a **KeyFrameAnimation** object and provides a unified set of properties and methods which cater to the various animation classes deriving from **KeyFrameAnimation**.

### Properties

| Property | Type | Description |
|---|---|---|
| **Animation** | `KeyFrameAnimation` | The encapsulated KeyFrameAnimation object. |
| **DelayTime** | `TimeSpan` | The duration by which the animation should be delayed |
| **Direction** | `AnimationDirection` | Direction of the Animation |
| **Duration** | `TimeSpan` | The duration of the animation. Minimum allowed value is 1ms and maximum allowed value is 24 days. |
| **IterationBehavior** | `AnimationIterationBehavior` | The iteration behavior for the key frame animation. |
| **IterationCount** | `int` | The number of times to repeat the key frame animation. A value of -1 causes the animation to repeat indefinitely.|
| **KeyFrameCount** | `int` | The number of key frames in the KeyFrameAnimation. |
| **StopBehavior** | `AnimationStopBehavior` | Specifies how to set the property value when StopAnimation is called. |
| **Target** | `String` | Specifies the target for the animation. |


### APIs
The following APIs facilitate the setting of keyframe(s) on the encapsulated **KeyFrameAnimation** object.

```C#
public void InsertKeyFrame(float normalizedProgressKey, T value, 
				 	    CompositionEasingFunction easingFunction = null);
public void InsertKeyFrame(KeyFrame<T> keyFrame)
public void InsertKeyFrames(params KeyFrame<T>[] keyFrames)
public void InsertExpressionKeyFrame(float normalizedProgressKey, Expression<CompositionLambda<T>> expression,
                                     CompositionEasingFunction easingFunction = null)
```

### Chaining APIs
The following APIs help you to set the properties of the encapsulated **KeyFrameAnimation** object. These method return the **KeyFrameAnimation&lt;T&gt;** objects itself (on which these methods are called), thus allowing these methods to be *chained*.


```C#
// Sets the Animation Direction of encapsulated KeyFrameAnimation object.
public KeyFrameAnimation<T> InTheDirection(AnimationDirection direction);

// Sets the duration by which the animation, defined by
public KeyFrameAnimation<T> DelayBy(TimeSpan delayTime);

// Sets the animation Duration of encapsulated KeyFrameAnimation object.
public KeyFrameAnimation<T> HavingDuration(TimeSpan duration);

// Sets the number of times the animation, defined by the encapsulated
// KeyFrameAnimation object, should be repeated.
public KeyFrameAnimation<T> Repeats(int count);

// Causes the animation defined by the encapsulated KeyFrameAnimation
// object to repeat indefinitely
public KeyFrameAnimation<T> RepeatsForever();

// Specifies how to set the property value when StopAnimation is called on
// the encapsulated KeyFrameAnimation object.
public KeyFrameAnimation<T> OnStop(AnimationStopBehavior stopBehavior);

// Sets the target for the encapsulated KeyFrameAnimation object
public KeyFrameAnimation<T> ForTarget(Expression<Func<object>> targetExpression);
```

The following extension method is defined for the **Compositor** to create a **KeyFrameAnimation&lt;T&gt;** object

```C#
// Creates an instance of KeyFrameAnimation<T>
public static KeyFrameAnimation<T> CreateKeyFrameAnimation<T>(this Compositor compositor);
```

The following extension methods have been added to **Compositor** to create **StartingValue** and **FinalValue** Expressions.

```C#
// Creates a CompositionLambda expression for 'c => c.StartingValue' for the given type
public static Expression<CompositionLambda<T>> CreateStartingValueExpression<T>(this Compositor compositor);
// Creates a CompositionLambda expression for 'c => c.FinalValue' for the given type
public static Expression<CompositionLambda<T>> CreateFinalValueExpression<T>(this Compositor compositor)
```

### Example

**Without using CompositionProToolkit.Expressions**
```C#
CubicBezierEasingFunction easeIn = _compositor.CreateCubicBezierEasingFunction(new Vector2(0.0f, 0.51f), 
                                                                               new Vector2(1.0f, 0.51f));
                                                                               
// Example 1
var enterAnimation = _compositor.CreateScalarKeyFrameAnimation();
enterAnimation.InsertKeyFrame(0.33f, 1.25f, easeIn);
enterAnimation.InsertKeyFrame(0.66f, 0.75f, easeIn);
enterAnimation.InsertKeyFrame(1.0f, 1.0f, easeIn);
enterAnimation.DelayTime = TimeSpan.FromMilliseconds(500);
enterAnimation.Duration = TimeSpan.FromMilliseconds(5000);
enterAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

spriteVisual.StartAnimation("Scale.XY", enterAnimation);

// Example 2
var exitAnimation = _compositor.CreateVector2KeyFrameAnimation();
exitAnimation.InsertKeyFrame(1.0f, new Vector2(0, 0));
exitAnimation.Duration = TimeSpan.FromMilliseconds(750);
exitAnimation.IterationBehavior = AnimationIterationBehavior.Count;
exitAnimation.IterationCount = 1;

spriteVisual2.StartAnimation("Offset", exitAnimation);

// Example 3 - ImplicitAnimations
var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
offsetAnimation.Target = "Offset";
offsetAnimation.InsertExpressionKeyFrame(1f, "this.FinalValue");
offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);

var implicitAnimationCollection = _compositor.CreateImplicitAnimationCollection();
implicitAnimationCollection["Offset"] = offsetAnimation;

spriteVisual3.ImplicitAnimations = implicitAnimationCollection;
```

**Using CompositionProToolkit.Expressions**
```C#
CubicBezierEasingFunction easeIn = _compositor.CreateCubicBezierEasingFunction(new Vector2(0.0f, 0.51f), 
                                                                               new Vector2(1.0f, 0.51f));

// Example 1
var enterAnimation =_compositor.CreateKeyFrameAnimation<float>()
                               .HavingDuration(TimeSpan.FromSeconds(5))
                               .DelayBy(TimeSpan.FromMilliseconds(500))
                               .RepeatsForever();

enterAnimation.InsertKeyFrames(new KeyFrame<float>(0.33f, 1.25f, easeIn), 
                               new KeyFrame<float>(0.66f, 0.75f, easeIn), 
                               new KeyFrame<float>(1.0f, 1.0f, easeIn));
                               
spriteVisual.StartAnimation(() => spriteVisual.ScaleXY(), enterAnimation.Animation);

// Example 2
var exitAnimation = _compositor.CreateKeyFrameAnimation<Vector2>()
                               .HavingDuration(TimeSpan.FromMilliseconds(750))
                               .Repeats(1);

exitAnimation.InsertKeyFrame(1.0f, new Vector2(0, 0));

spriteVisual2.StartAnimation(() => spriteVisual2.Offset, exitAnimation.Animation);

// Example 3 - ImplicitAnimations
var vector3Expr = _compositor.CreateFinalValueExpression<Vector3>();
var offsetAnimation = _compositor.CreateKeyFrameAnimation<Vector3>()
                                 .HavingDuration(TimeSpan.FromMilliseconds(500))
                                 .ForTarget(() => spriteVisual3.Offset);
                                 
offsetAnimation.InsertExpressionKeyFrame(1f, vector3Expr);

var implicitAnimationCollection = _compositor.CreateImplicitAnimationCollection();
implicitAnimationCollection["Offset"] = offsetAnimation.Animation;

spriteVisual3.ImplicitAnimations = implicitAnimationCollection;
```

**NOTE:** _While using **KeyFrameAnimation&lt;T&gt;**, whenever you call the **StartAnimation** or **StopAnimation** method on any object deriving from **CompositionObject**, make sure that you pass the **Animation** property of **KeyFrameAnimation&lt;T&gt;** object as an argument. (If you provide the **KeyFrameAnimation&lt;T&gt;** object as argument, **it will not compile!**)_

## 8. ScopedBatchHelper

This class contains a static method **CreateScopedBatch** creates a scoped batch and handles the subscribing and unsubscribing process of the **Completed** event internally.

__API__:

```C#
public static void CreateScopedBatch(Compositor compositor, 
                                     CompositionBatchTypes batchType, 
                                     Action action, 
                                     Action postAction = null)
```
__Example usage__:

```C#
ScopedBatchHelper.CreateScopedBatch(_compositor, CompositionBatchTypes.Animation,
       () => // Action
       {
           transitionVisual.StartAnimation(() => transitionVisual.ScaleXY(), _scaleUpAnimation);
       },
       () => // Post Action
       {
           BackBtn.IsEnabled = true;
       });
```
## 9. Converting from `double` to `float`
Most of the values which is calculated or derived from the properties of **UIElement** (and its derived classes) are of type **double**. But most of the classes in **Sytem.Numerics** and **Windows.UI.Composition** namespaces require the values to be of type **float**. If you find it tedious adding a `(float)` cast before each and every variable of type **double**, you can call the **.Single** extension method for **System.Double** instead, which converts the **double** into **float**. Ensure that the value of the double variable is between **System.Single.MinValue** and **System.Single.MaxValue** otherwise **ArgumentOutOfRangeException** will be thrown.  

**Note**: _Conversion of a value from **double** to **float** will reduce the precision of the value._  

**Example**
```C#
double width = window.Width;
double height = window.Height;
Vector2 size = new Vector2(width.Single(), height.Single());
```

# Updates Chronology

## v0.4.5
(**Wenesday, August 31, 2016**) - Merged `CompositionExpressionToolkit` project with `CompositionProToolkit` project.

# Credits
The **CompositionExpressionEngine** is based on the <a href="https://github.com/albahari/ExpressionFormatter">ExpressionFormatter</a> project by **Joseph Albahari** (*the legend behind the*  ***LinqPad*** *tool*). 

*Thank you Joseph Albahari for being generous and making the ExpressionFormatter code open source!*

**CompositionProToolkit.Expressions** is also influenced from the <a href="https://github.com/aL3891/CompositionAnimationToolkit">CompositionAnimationToolkit</a> by Allan Lindqvist.
