// Copyright (c) Ratish Philip 
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
// 
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software. 
// 
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE. 
//
// This file is part of the CompositionProToolkit project: 
// https://github.com/ratishphilip/CompositionProToolkit
//
// CompositionProToolkit v0.8.0
// 

using System;
using System.Numerics;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using CompositionProToolkit.Expressions;
using CompositionProToolkit.Win2d;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit.Controls
{
    /// <summary>
    /// Represents a switch that can be toggled between two states.
    /// </summary>
    [WebHostHidden]
    [TemplatePart(Name = "PART_Border", Type = typeof(Border))]
    public class FluidToggleSwitch : ToggleButton
    {
        #region Constants

        private const float DefaultBaseLength = 256f;
        private const float DefaultBaseRadius = 128f;
        private const float DefaultTrackWidth = 240f;
        private const float DefaultTrackHeight = 144f;
        private const float DefaultTrackCornerRadius = 72f;
        private const float DefaultThumbRadius = 64f;
        private const float DefaultShadowBlurRadius = 16f;

        private const float ActiveTrackDarkFactor = 0.3f;
        private const float InactiveTrackDarkFactor = 0.15f;
        private const float DisabledTrackDarkFactor = 0.1f;
        private const float DisabledThumbLightFactor = 0.5f;
        private const float EnabledShadowDarkFactor = 0.8f;
        private const float DiasbledShadowLightFactor = 0.2f;
        private const float BloomStrokeDarkFactor = 0.4f;
        private const float DropShadowOpacity = 0.99f;
        private const float BaseBloomTargetScaleFactor = 1.1f;
        private const float TrackBloomTargetScaleFactor = 1.1f;

        private static readonly Vector2 DefaultBaseSize = new Vector2(DefaultBaseLength, DefaultBaseLength);
        private static readonly Vector2 DefaultTrackSize = new Vector2(DefaultTrackWidth, DefaultTrackHeight);
        private static readonly Vector2 DefaultThumbSize = new Vector2(DefaultThumbRadius * 2, DefaultThumbRadius * 2);
        private static readonly Vector3 DefaultTrackOffset = new Vector3(8f, 56f, 0f);
        private static readonly Vector3 DefaultThumbCheckedOffset = new Vector3(112f, 64f, 0f);
        private static readonly Vector3 DefaultThumbUncheckedOffset = new Vector3(16f, 64f, 0f);
        private static readonly Vector3 DefaultShadowCheckedOffset = new Vector3(8f, 8f, 0f);
        private static readonly Vector3 DefaultShadowUncheckedOffset = new Vector3(-8f, 8f, 0f);
        private static readonly Color DefaultActiveColor = CanvasObject.CreateColor("#4cd964");
        private static readonly Color DefaultInactiveColor = CanvasObject.CreateColor("#dfdfdf");
        private static readonly Color DefaultDisabledColor = CanvasObject.CreateColor("#eaeaea");
        private static readonly TimeSpan DefaultAnimationDuration = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan DefaultTrackBloomAnimationDuration = TimeSpan.FromMilliseconds(280);

        #endregion

        #region Fields

        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private Color _baseActiveColor;
        private Color _baseInactiveColor;
        private Color _baseDisabledColor;

        private Color _trackActiveColor;
        private Color _trackInactiveColor;
        private Color _trackDisabledColor;

        private Color _thumbActiveColor;
        private Color _thumbInactiveColor;
        private Color _thumbDisabledColor;

        private SpriteVisual _baseVisual;
        private SpriteVisual _trackVisual;
        private SpriteVisual _thumbVisual;
        private SpriteVisual _baseBloomVisual;
        private SpriteVisual _trackBloomVisual;

        private CompositionColorBrush _baseColorBrush;
        private CompositionColorBrush _trackColorBrush;
        private IMaskSurface _baseMask;
        private IMaskSurface _trackMask;
        private IGeometrySurface _thumbSurface;
        private CanvasGeometry _baseGeometry;
        private CanvasGeometry _trackGeometry;
        private IGeometrySurface _baseBloomSurface;
        private IGeometrySurface _trackBloomSurface;
        private CompositionSurfaceBrush _baseBloomBrush;
        private CompositionSurfaceBrush _trackBloomBrush;
        private CompositionMaskBrush _baseBloomMaskBrush;
        private CompositionMaskBrush _trackBloomMaskBrush;

        private Vector3 _thumbCheckedOffset;
        private Vector3 _thumbUncheckedOffset;
        private Vector3 _shadowCheckedOffset;
        private Vector3 _shadowUncheckedOffset;

        private ImplicitAnimationCollection _thumbImplicitAnimation;
        private ImplicitAnimationCollection _thumbShadowImplicitAnimation;
        private ImplicitAnimationCollection _colorImplicitAnimation;
        private KeyFrameAnimation<Vector2> _bloomAnimation;
        private KeyFrameAnimation<Vector2> _trackBloomAnimation;
        private DropShadow _dropShadow;

        private Border _outerBorder;

        #endregion

        #region Dependency Properties

        #region ActiveColor

        /// <summary>
        /// ActiveColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActiveColorProperty =
            DependencyProperty.Register("ActiveColor", typeof(Color), typeof(FluidToggleSwitch),
                new PropertyMetadata(DefaultActiveColor, OnActiveColorChanged));

        /// <summary>
        /// Gets or sets the ActiveColor property. This dependency property 
        /// indicates the color of the FluidToggleSwitch in Checked state.
        /// </summary>
        public Color ActiveColor
        {
            get => (Color)GetValue(ActiveColorProperty);
            set => SetValue(ActiveColorProperty, value);
        }

        /// <summary>
        /// Handles changes to the ActiveColor property.
        /// </summary>
        /// <param name="d">FluidToggleSwitch</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnActiveColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleSwitch = (FluidToggleSwitch)d;
            toggleSwitch.OnActiveColorChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the ActiveColor property.
        /// </summary>
        private void OnActiveColorChanged()
        {
            UpdateColors();
            InvalidateArrange();
        }

        #endregion

        #region InactiveColor

        /// <summary>
        /// InactiveColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty InactiveColorProperty =
            DependencyProperty.Register("InactiveColor", typeof(Color), typeof(FluidToggleSwitch),
                new PropertyMetadata(DefaultInactiveColor, OnInactiveColorChanged));

        /// <summary>
        /// Gets or sets the InactiveColor property. This dependency property 
        /// indicates the color of the FluidToggleSwitch in Checked state.
        /// </summary>
        public Color InactiveColor
        {
            get => (Color)GetValue(InactiveColorProperty);
            set => SetValue(InactiveColorProperty, value);
        }

        /// <summary>
        /// Handles changes to the InactiveColor property.
        /// </summary>
        /// <param name="d">FluidToggleSwitch</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnInactiveColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleSwitch = (FluidToggleSwitch)d;
            toggleSwitch.OnInactiveColorChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the InactiveColor property.
        /// </summary>
        private void OnInactiveColorChanged()
        {
            UpdateColors();
            InvalidateArrange();
        }

        #endregion

        #region DisabledColor

        /// <summary>
        /// DisabledColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty DisabledColorProperty =
            DependencyProperty.Register("DisabledColor", typeof(Color), typeof(FluidToggleSwitch),
                new PropertyMetadata(DefaultDisabledColor, OnDisabledColorChanged));

        /// <summary>
        /// Gets or sets the DisabledColor property. This dependency property 
        /// indicates the color of the FluidToggleSwitch in Checked state.
        /// </summary>
        public Color DisabledColor
        {
            get => (Color)GetValue(DisabledColorProperty);
            set => SetValue(DisabledColorProperty, value);
        }

        /// <summary>
        /// Handles changes to the DisabledColor property.
        /// </summary>
        /// <param name="d">FluidToggleSwitch</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDisabledColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleSwitch = (FluidToggleSwitch)d;
            toggleSwitch.OnDisabledColorChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the DisabledColor property.
        /// </summary>
        private void OnDisabledColorChanged()
        {
            UpdateColors();
            InvalidateArrange();
        }

        #endregion

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        public FluidToggleSwitch()
        {
            // Set the default Style Key
            DefaultStyleKey = typeof(FluidToggleSwitch);
            // Subscribe to the changes of the IsEnabled dependency property
            RegisterPropertyChangedCallback(IsEnabledProperty, OnIsEnabledChanged);
            // Subscribe to the changes of the IsChecked dependency property
            RegisterPropertyChangedCallback(IsCheckedProperty, OnIsCheckedChanged);

            UpdateColors();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Handler method when the Template is applied to the FluidToggleSwitch
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _outerBorder = GetTemplateChild("PART_Border") as Border;
        }

        /// <summary>
        /// Handler method for the Arrange Layout phase.
        /// </summary>
        /// <param name="finalSize">Final size of the control.</param>
        /// <returns>Size</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);

            // If compositor is null, it means none of the visuals
            // have been initialized yet.
            if (_compositor == null)
            {
                InitComposition();
            }

            // Update the visuals based on the current control Size and Padding
            var padding = Padding.CollapseThickness();
            var baseLength = (float)Math.Max(0, Math.Min(finalSize.Width - padding.Width,
                finalSize.Height - padding.Height));
            var offsetX = (float)((finalSize.Width - baseLength) / 2);
            var offsetY = (float)((finalSize.Height - baseLength) / 2);
            // Calculate the scale factor
            var scale = baseLength / DefaultBaseLength;

            //
            // Base Visual
            //
            _baseVisual.Size = DefaultBaseSize * scale;
            var baseColor = IsEnabled ? (IsChecked == true ? _baseActiveColor : _baseInactiveColor) : _baseDisabledColor;
            var baseRadius = DefaultBaseRadius * scale;
            _baseColorBrush.ImplicitAnimations = null;
            _baseColorBrush.Color = baseColor;
            _baseColorBrush.ImplicitAnimations = _colorImplicitAnimation;
            _baseGeometry = CanvasGeometry.CreateCircle(_generator.Device, baseRadius, baseRadius, baseRadius);
            _baseMask.Redraw(_baseVisual.Size.ToSize(), _baseGeometry);

            //
            // Base Bloom
            //
            _baseBloomVisual.Size = _baseVisual.Size;
            // Update the fill color and geometry
            var baseBloomColor = (IsChecked == true ? _baseInactiveColor : _baseActiveColor);
            var baseBloomGeometry = _baseGeometry/*.Transform(Matrix3x2.CreateScale(BaseBloomScaleFactor, _baseBloomVisual.Size * 0.5f))*/;
            _baseBloomSurface.Redraw(_baseBloomVisual.Size.ToSize(),
                baseBloomGeometry,
                //new CanvasStroke(generator.Device, currentBaseBloomColor.DarkerBy(0.2f)), 
                baseBloomColor);
            // Update the brush center
            _baseBloomBrush.CenterPoint = _baseBloomVisual.Size * 0.5f;

            //
            // Track Visual
            //
            _trackVisual.Size = DefaultTrackSize * scale;
            var trackColor = IsEnabled ? (IsChecked == true ? _trackActiveColor : _trackInactiveColor) : _trackDisabledColor;
            var trackCornerRadius = DefaultTrackCornerRadius * scale;
            _trackColorBrush.ImplicitAnimations = null;
            _trackColorBrush.Color = trackColor;
            _trackColorBrush.ImplicitAnimations = _colorImplicitAnimation;
            _trackGeometry = CanvasGeometry.CreateRoundedRectangle(_generator.Device,
                0,
                0,
                DefaultTrackWidth * scale,
                DefaultTrackHeight * scale,
                trackCornerRadius,
                trackCornerRadius);
            _trackMask.Redraw(_trackVisual.Size.ToSize(), _trackGeometry);
            _trackVisual.Offset = DefaultTrackOffset * scale;

            //
            // Track Bloom
            //
            _trackBloomVisual.Size = _trackVisual.Size;
            // Update the fill color and geometry
            var trackBloomColor = (IsChecked == true ? _trackInactiveColor : _trackActiveColor);
            var trackBloomGeometry = _trackGeometry/*.Transform(Matrix3x2.CreateScale(TrackBloomScaleFactor,
                _trackBloomVisual.Size * 0.5f))*/;
            _trackBloomSurface.Redraw(_trackBloomVisual.Size.ToSize(),
                trackBloomGeometry,
                new CanvasStroke(_generator.Device, trackBloomColor.DarkerBy(BloomStrokeDarkFactor), 3f),
                trackBloomColor);
            // Update the brush center
            _trackBloomBrush.CenterPoint = _trackBloomVisual.Size * 0.5f;

            //
            // Thumb
            //
            _thumbVisual.Size = DefaultThumbSize * scale;
            var thumbColor = IsEnabled ? (IsChecked == true ? _thumbActiveColor : _thumbInactiveColor) : _thumbDisabledColor;
            var thumbRadius = DefaultThumbRadius * scale;
            _thumbSurface.Redraw(_thumbVisual.Size.ToSize(),
                CanvasGeometry.CreateCircle(_generator.Device, thumbRadius, thumbRadius, thumbRadius),
                thumbColor);
            _thumbCheckedOffset = DefaultThumbCheckedOffset * scale;
            _thumbUncheckedOffset = DefaultThumbUncheckedOffset * scale;
            _thumbVisual.ImplicitAnimations = null;
            _thumbVisual.Offset = IsChecked == true ? _thumbCheckedOffset : _thumbUncheckedOffset;
            _thumbVisual.ImplicitAnimations = _thumbImplicitAnimation;

            //
            // Thumb Shadow
            //
            _shadowCheckedOffset = DefaultShadowCheckedOffset * scale;
            _shadowUncheckedOffset = DefaultShadowUncheckedOffset * scale;
            _dropShadow.BlurRadius = DefaultShadowBlurRadius * scale;
            _dropShadow.ImplicitAnimations = null;
            _dropShadow.Color = IsEnabled ?
                trackColor.DarkerBy(EnabledShadowDarkFactor) :
                trackColor.LighterBy(DiasbledShadowLightFactor);
            _dropShadow.Offset = IsChecked == true ? _shadowCheckedOffset : _shadowUncheckedOffset;
            _dropShadow.ImplicitAnimations = _thumbShadowImplicitAnimation;
            _dropShadow.Mask = _thumbVisual.Brush;

            // Update the base visual's offset
            _baseVisual.Offset = new Vector3(offsetX, offsetY, 0);

            // Adjust the corner radius of the border so that the hit area
            // of the FluidToggleSwitch is correct.
            if (_outerBorder != null)
            {
                _outerBorder.CornerRadius = new CornerRadius(_baseVisual.Size.X / 2f);
            }

            return size;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler method which is invoked when the IsEnabled property value changes
        /// </summary>
        /// <param name="sender">FluidToggleSwitch</param>
        /// <param name="dp">Dependency Property that has changed</param>
        private void OnIsEnabledChanged(DependencyObject sender, DependencyProperty dp)
        {
            InvalidateArrange();
        }

        /// <summary>
        /// Handler method which is invoked when the IsEnabled property value changes
        /// </summary>
        /// <param name="sender">FluidToggleSwitch</param>
        /// <param name="dp">Dependency Property that has changed</param>
        private void OnIsCheckedChanged(DependencyObject sender, DependencyProperty dp)
        {
            _thumbVisual.Offset = IsChecked == true ? _thumbCheckedOffset : _thumbUncheckedOffset;
            _dropShadow.Offset = IsChecked == true ? _shadowCheckedOffset : _shadowUncheckedOffset;

            var thumbColor = IsEnabled ? (IsChecked == true ? _thumbActiveColor : _thumbInactiveColor) : _thumbDisabledColor;
            _thumbSurface.Redraw(thumbColor);

            if (IsEnabled)
            {
                var baseColor = IsChecked == true ? _baseActiveColor : _baseInactiveColor;
                var trackColor = IsChecked == true ? _trackActiveColor : _trackInactiveColor;
                _dropShadow.Color = trackColor.DarkerBy(EnabledShadowDarkFactor);

                _compositor.CreateScopedBatch(CompositionBatchTypes.Animation,
                    () =>
                    {
                        _baseBloomSurface.Redraw(/*new CanvasStroke(generator.Device, baseColor.DarkerBy(0.2f)),*/ baseColor);
                        _baseBloomVisual.Brush = _baseBloomMaskBrush;
                        _baseBloomBrush.StartAnimation(() => _baseBloomBrush.ScaleXY(), _bloomAnimation);

                        _trackBloomSurface.Redraw(new CanvasStroke(_generator.Device, trackColor.DarkerBy(BloomStrokeDarkFactor), 3f), trackColor);
                        _trackBloomVisual.Brush = _trackBloomMaskBrush;
                        _trackBloomBrush.StartAnimation(() => _trackBloomBrush.ScaleXY(), _trackBloomAnimation);
                    },
                    () =>
                    {
                        _baseColorBrush.ImplicitAnimations = null;
                        _baseColorBrush.Color = baseColor;
                        _baseColorBrush.ImplicitAnimations = _colorImplicitAnimation;
                        _baseBloomVisual.Brush = null;
                        _baseBloomBrush.Scale = Vector2.One;

                        _trackColorBrush.ImplicitAnimations = null;
                        _trackColorBrush.Color = trackColor;
                        _trackColorBrush.ImplicitAnimations = _colorImplicitAnimation;
                        _trackBloomVisual.Brush = null;
                        _trackBloomBrush.Scale = Vector2.One;
                    });
            }
            else
            {
                _baseColorBrush.Color = _baseDisabledColor;
                _trackColorBrush.Color = _trackDisabledColor;
                _dropShadow.Color = _trackDisabledColor.LighterBy(DiasbledShadowLightFactor);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Updates the color components based on the Active, Inactive
        /// and Disabled colors.
        /// </summary>
        private void UpdateColors()
        {
            // Active
            _baseActiveColor = ActiveColor;
            _trackActiveColor = _baseActiveColor.DarkerBy(ActiveTrackDarkFactor);
            _thumbActiveColor = Colors.White;
            // Inactive
            _baseInactiveColor = InactiveColor;
            _trackInactiveColor = _baseInactiveColor.DarkerBy(InactiveTrackDarkFactor);
            _thumbInactiveColor = _thumbActiveColor;
            // Disabled
            _baseDisabledColor = DisabledColor;
            _trackDisabledColor = _baseDisabledColor.DarkerBy(DisabledTrackDarkFactor);
            _thumbDisabledColor = _baseDisabledColor.LighterBy(DisabledThumbLightFactor);
        }

        /// <summary>
        /// Initializes the Composition components of the FluidToggleSwitch
        /// </summary>
        private void InitComposition()
        {
            _compositor = Window.Current.Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            //
            // Base Visual
            //
            _baseVisual = _compositor.CreateSpriteVisual();
            _baseVisual.Size = DefaultBaseSize;
            _baseColorBrush = _compositor.CreateColorBrush(IsChecked == true ? _baseActiveColor : _baseInactiveColor);
            _baseGeometry = CanvasGeometry.CreateCircle(_generator.Device,
                DefaultBaseRadius,
                DefaultBaseRadius,
                DefaultBaseRadius);
            _baseMask = _generator.CreateMaskSurface(DefaultBaseSize.ToSize(), _baseGeometry);
            var baseMaskBrush = _compositor.CreateMaskBrush();
            baseMaskBrush.Mask = _compositor.CreateSurfaceBrush(_baseMask);
            baseMaskBrush.Source = _baseColorBrush;
            _baseVisual.Brush = baseMaskBrush;

            //
            // Base Bloom Visual
            //
            _baseBloomVisual = _compositor.CreateSpriteVisual();
            _baseBloomVisual.Size = _baseVisual.Size;
            var bloomColor = (IsChecked == true ? _baseInactiveColor : _baseActiveColor);
            _baseBloomSurface = _generator.CreateGeometrySurface(_baseBloomVisual.Size.ToSize(),
                _baseGeometry/*.Transform(Matrix3x2.CreateScale(BaseBloomScaleFactor, _baseBloomVisual.Size * 0.5f))*/,
                //new CanvasStroke(generator.Device, bloomColor.DarkerBy(0.2f)),
                bloomColor);
            _baseBloomBrush = _compositor.CreateSurfaceBrush(_baseBloomSurface);
            _baseBloomBrush.CenterPoint = _baseBloomVisual.Size * 0.5f;
            _baseBloomMaskBrush = _compositor.CreateMaskBrush();
            _baseBloomMaskBrush.Mask = baseMaskBrush.Mask;
            _baseBloomMaskBrush.Source = _baseBloomBrush;

            _baseVisual.Children.InsertAtTop(_baseBloomVisual);

            //
            // Track Visual
            //
            _trackVisual = _compositor.CreateSpriteVisual();
            _trackVisual.Size = DefaultTrackSize;
            _trackColorBrush = _compositor.CreateColorBrush(IsChecked == true ? _trackActiveColor : _trackInactiveColor);
            _trackGeometry = CanvasGeometry.CreateRoundedRectangle(_generator.Device,
                0,
                0,
                DefaultTrackWidth,
                DefaultTrackHeight,
                DefaultTrackCornerRadius,
                DefaultTrackCornerRadius);
            _trackMask = _generator.CreateMaskSurface(DefaultTrackSize.ToSize(), _trackGeometry);
            var trackMaskBrush = _compositor.CreateMaskBrush();
            trackMaskBrush.Mask = _compositor.CreateSurfaceBrush(_trackMask);
            trackMaskBrush.Source = _trackColorBrush;
            _trackVisual.Brush = trackMaskBrush;
            _trackVisual.Offset = DefaultTrackOffset;

            //
            // Track Bloom Visual
            //
            _trackBloomVisual = _compositor.CreateSpriteVisual();
            _trackBloomVisual.Size = _trackVisual.Size;
            bloomColor = IsChecked == true ? _trackInactiveColor : _trackActiveColor;
            _trackBloomSurface =
                _generator.CreateGeometrySurface(_trackBloomVisual.Size.ToSize(),
                    _trackGeometry/*.Transform(Matrix3x2.CreateScale(TrackBloomScaleFactor,
                        _trackBloomVisual.Size * 0.5f))*/,
                    new CanvasStroke(_generator.Device, bloomColor.DarkerBy(BloomStrokeDarkFactor), 3f),
                    bloomColor);
            _trackBloomBrush = _compositor.CreateSurfaceBrush(_trackBloomSurface);
            _trackBloomBrush.CenterPoint = _trackBloomVisual.Size * 0.5f;
            _trackBloomMaskBrush = _compositor.CreateMaskBrush();
            _trackBloomMaskBrush.Mask = trackMaskBrush.Mask;
            _trackBloomMaskBrush.Source = _trackBloomBrush;

            _trackVisual.Children.InsertAtTop(_trackBloomVisual);

            //
            // Thumb Visual
            //
            _thumbVisual = _compositor.CreateSpriteVisual();
            _thumbVisual.Size = DefaultThumbSize;
            _thumbSurface = _generator.CreateGeometrySurface(DefaultThumbSize.ToSize(),
                CanvasGeometry.CreateCircle(_generator.Device,
                    DefaultThumbRadius,
                    DefaultThumbRadius,
                    DefaultThumbRadius),
                IsChecked == true ? _thumbActiveColor : _trackInactiveColor);
            _thumbVisual.Brush = _compositor.CreateSurfaceBrush(_thumbSurface);
            _thumbVisual.Offset = IsChecked == true ? DefaultThumbCheckedOffset : DefaultThumbUncheckedOffset;

            //
            // Thumb Drop Shadow
            //
            _dropShadow = _compositor.CreateDropShadow();
            _dropShadow.BlurRadius = DefaultShadowBlurRadius;
            _dropShadow.Offset = IsChecked == true ? DefaultShadowCheckedOffset : DefaultShadowUncheckedOffset;
            var currentTrackColor = (IsChecked == true ? _trackActiveColor : _trackInactiveColor);
            _dropShadow.Color = IsEnabled
                ? currentTrackColor.DarkerBy(EnabledShadowDarkFactor)
                : currentTrackColor.LighterBy(DiasbledShadowLightFactor);
            _dropShadow.Opacity = DropShadowOpacity;
            _dropShadow.Mask = _thumbVisual.Brush;
            _thumbVisual.Shadow = _dropShadow;

            // Add Track and Thumb to the Base
            _baseVisual.Children.InsertAtTop(_trackVisual);
            _baseVisual.Children.InsertAtTop(_thumbVisual);

            ElementCompositionPreview.SetElementChildVisual(this, _baseVisual);

            //
            // ImplicitAnimations
            //

            // For Thumb
            var thumbOffsetAnimation = _compositor.GenerateVector3KeyFrameAnimation()
                .ForTarget(() => _thumbVisual.Offset)
                .HavingDuration(DefaultAnimationDuration);

            thumbOffsetAnimation.InsertFinalValueKeyFrame(1f, _compositor.CreateEaseOutBackEasingFunction());
            _thumbImplicitAnimation = _compositor.CreateImplicitAnimationCollection();
            _thumbImplicitAnimation.Add(() => _thumbVisual.Offset, thumbOffsetAnimation);

            _thumbVisual.ImplicitAnimations = _thumbImplicitAnimation;

            // For Thumb Shadow
            var shadowColorAnimation = _compositor.GenerateColorKeyFrameAnimation()
                .ForTarget(() => _dropShadow.Color)
                .HavingDuration(DefaultAnimationDuration);
            shadowColorAnimation.InsertFinalValueKeyFrame(1f, _compositor.CreateEaseInCircleEasingFunction());

            _thumbShadowImplicitAnimation = _compositor.CreateImplicitAnimationCollection();
            _thumbShadowImplicitAnimation.Add(() => _dropShadow.Offset, thumbOffsetAnimation);
            _thumbShadowImplicitAnimation.Add(() => _dropShadow.Color, shadowColorAnimation);
            _dropShadow.ImplicitAnimations = _thumbShadowImplicitAnimation;

            // For Base and Track Color
            _colorImplicitAnimation = _compositor.CreateImplicitAnimationCollection();
            var colorAnimation = _compositor.GenerateColorKeyFrameAnimation()
                .ForTarget(() => _baseColorBrush.Color)
                .HavingDuration(DefaultAnimationDuration);
            colorAnimation.InsertFinalValueKeyFrame(1f, _compositor.CreateEaseInCubicEasingFunction());
            _colorImplicitAnimation.Add(() => _baseColorBrush.Color, colorAnimation);

            _baseColorBrush.ImplicitAnimations = _colorImplicitAnimation;
            _trackColorBrush.ImplicitAnimations = _colorImplicitAnimation;

            //
            // Animations
            // 

            // Base Color Bloom Animation
            _bloomAnimation = _compositor.GenerateVector2KeyFrameAnimation()
                .HavingDuration(DefaultAnimationDuration);

            _bloomAnimation.InsertKeyFrames(new KeyFrame<Vector2>(0, Vector2.Zero),
                new KeyFrame<Vector2>(1f, new Vector2(BaseBloomTargetScaleFactor), _compositor.CreateEaseInCubicEasingFunction()));

            // Track Color Bloom Animation
            _trackBloomAnimation = _compositor.GenerateVector2KeyFrameAnimation()
                .HavingDuration(DefaultTrackBloomAnimationDuration);

            _trackBloomAnimation.InsertKeyFrames(new KeyFrame<Vector2>(0, Vector2.Zero),
                new KeyFrame<Vector2>(1f, new Vector2(TrackBloomTargetScaleFactor), _compositor.CreateLinearEasingFunction()));
        }

        #endregion
    }
}
