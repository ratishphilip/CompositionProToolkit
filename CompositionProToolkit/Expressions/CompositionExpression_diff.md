# Property Sets

## Using Expression Builder

```C#
var propSetCenterPoint = _propertySet.GetReference().GetVector3Property("CenterPointOffset");
var propSetRotation = _propertySet.GetReference().GetScalarProperty("Rotation");
var orbitExpression = redSprite.GetReference().Offset + propSetCenterPoint + 
    EF.Vector3(
        EF.Cos(EF.ToRadians(propSetRotation)) * 150,
        EF.Sin(EF.ToRadians(propSetRotation)) * 75, 
        0);

blueSprite.StartAnimation("Offset", orbitExpression);
```

## Using CompositionExpression

```C#
var orbitAnimation = compositor.CreateVector3ExpressionAnimation();
orbitAnimation.Expression = c => redSprite.Offset + _propertySet.GetVector3("CenterPointOffset") +
                                 new Vector3(c.Cos(c.ToRadians(_propertySet.GetScalar("Rotation"))) * 150,
                                             c.Sin(c.ToRadians(_propertySet.GetScalar("Rotation"))) * 75,
                                             0);
// Start the expression animation!
blueSprite.StartAnimation(() => blueSprite.Offset, orbitAnimation);
```

## Using Expression Builder

```C#
var linear = compositor.CreateLinearEasingFunction();
var rotAnimation = compositor.CreateScalarKeyFrameAnimation();
rotAnimation.InsertKeyFrame(1.0f, 360f, linear);
rotAnimation.Duration = TimeSpan.FromMilliseconds(4000);
rotAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
_propertySet.StartAnimation("Rotation", rotAnimation);

var offsetAnimation = compositor.CreateScalarKeyFrameAnimation();
offsetAnimation.InsertKeyFrame(0f, 50f);
offsetAnimation.InsertKeyFrame(.5f, 150f);
offsetAnimation.InsertKeyFrame(1f, 50f);
offsetAnimation.Duration = TimeSpan.FromMilliseconds(4000);
offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
redSprite.StartAnimation("Offset.Y", offsetAnimation);
```

## Using CompositionExpression

```C#
var rotationAnimation = compositor.GenerateScalarKeyFrameAnimation()
                                  .HavingDuration(TimeSpan.FromMilliseconds(4000))
                                  .RepeatsForever();

rotationAnimation.InsertKeyFrame(1, 360, linear);
_propertySet.StartAnimation(() => _propertySet.GetScalar("Rotation"), rotationAnimation);

var offsetAnimation = compositor.GenerateScalarKeyFrameAnimation()
                                .HavingDuration(TimeSpan.FromMilliseconds(4000))
                                .RepeatsForever();
offsetAnimation.InsertKeyFrames(new KeyFrame<float>(0, 50),
                                new KeyFrame<float>(0.5f, 150),
                                new KeyFrame<float>(1f, 50));

redSprite.StartAnimation(() => redSprite.Offset.Y, offsetAnimation);
```

# PointerRotate

## Using Expression Builder

```C#
var hoverPosition = _hoverPositionPropertySet.GetSpecializedReference<PointerPositionPropertySetReferenceNode>().Position;
var angleExpressionNode =
    EF.Conditional(
        hoverPosition == new Vector3(0, 0, 0),
        ExpressionValues.CurrentValue.CreateScalarCurrentValue(),
        35 * ((EF.Clamp(EF.Distance(center, hoverPosition), 0, distanceToCenter) % distanceToCenter) / distanceToCenter));

_tiltVisual.StartAnimation("RotationAngleInDegrees", angleExpressionNode);

var axisAngleExpressionNode = EF.Vector3(
    -(hoverPosition.Y - center.Y) * EF.Conditional(hoverPosition.Y == center.Y, 0, 1),
    (hoverPosition.X - center.X) * EF.Conditional(hoverPosition.X == center.X, 0, 1),
    0);

_tiltVisual.StartAnimation("RotationAxis", axisAngleExpressionNode);
```

## Using CompositionExpression

```C#
var hover = new PointerPositionPropertySetReference("hover");

var rotationAnimation = _compositor.CreateScalarExpressionAnimation();
rotationAnimation.Expression = c => hover.Position == new Vector3(0, 0, 0)
    ? c.CurrentValue
    : 35 * ((c.Clamp(c.Distance(center, hover.Position), 0, distanceToCenter) % distanceToCenter) /
            distanceToCenter);
rotationAnimation.SetReference("hover", _hoverPositionPropertySet);
_tiltVisual.StartAnimation(() => _tiltVisual.RotationAngleInDegrees, rotationAnimation);

var axisAnimation = _compositor.CreateVector3ExpressionAnimation();
axisAnimation.Expression = c => new Vector3(-(hover.Position.Y -center.Y) * (hover.Position.Y == center.Y? 0 : 1),
    (hover.Position.X - center.X) * (hover.Position.X == center.X ? 0 : 1),
    0);
axisAnimation.SetReference("hover", _hoverPositionPropertySet);
_tiltVisual.StartAnimation(() => _tiltVisual.RotationAxis, axisAnimation);
```

# Gears

## Using Expression Builder

```C#
private void ConfigureGearAnimation(Visual currentGear, Visual previousGear)
{
    // If rotation expression is null then create an expression of a gear rotating the opposite direction

    _rotateExpression = _rotateExpression ?? -previousGear.GetReference().RotationAngleInDegrees;

    // Start the animation based on the Rotation Angle in Degrees.
    currentGear.StartAnimation("RotationAngleInDegrees", _rotateExpression);
}

private void StartGearMotor(double secondsPerRotation)
{
    // Start the first gear (the red one)
    if (_gearMotionScalarAnimation == null)
    {
        _gearMotionScalarAnimation = _compositor.CreateScalarKeyFrameAnimation();
        var linear = _compositor.CreateLinearEasingFunction();

        var startingValue = ExpressionValues.StartingValue.CreateScalarStartingValue();
        _gearMotionScalarAnimation.InsertExpressionKeyFrame(0.0f, startingValue);
        _gearMotionScalarAnimation.InsertExpressionKeyFrame(1.0f, startingValue + 360f, linear);

        _gearMotionScalarAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
    }

    _gearMotionScalarAnimation.Duration = TimeSpan.FromSeconds(secondsPerRotation);
    // Start the first gear (the red one)
    _gearVisuals.First().StartAnimation("RotationAngleInDegrees", _gearMotionScalarAnimation);
}
```

## Using CompositionExpression

```C#
private void ConfigureGearAnimation(int current, int previous)
{
    var rotationAnimation = _compositor.CreateScalarExpressionAnimation();
    rotationAnimation.Expression = c => -_gearVisuals[previous].RotationAngleInDegrees;

    _gearVisuals[current].StartAnimation(() => _gearVisuals[current].RotationAngleInDegrees, rotationAnimation);
}

KeyFrameAnimation<float> _gearAnimation;
        
private void StartGearMotor(double secondsPerRotation)
{
    if (_gearAnimation == null)
    {
        _gearAnimation = _compositor.GenerateScalarKeyFrameAnimation()
                                    .RepeatsForever();

        var linear = _compositor.CreateLinearEasingFunction();

        _gearAnimation.InsertStartingValueKeyFrame(0);
        _gearAnimation.InsertExpressionKeyFrame(1, c => c.StartingValue + 360f, linear);
    }

    _gearAnimation.HavingDuration(TimeSpan.FromSeconds(secondsPerRotation));
    // Start the first gear (the red one)
    _gearVisuals[0].StartAnimation(() => _gearVisuals[0].RotationAngleInDegrees, _gearAnimation);
}
```

