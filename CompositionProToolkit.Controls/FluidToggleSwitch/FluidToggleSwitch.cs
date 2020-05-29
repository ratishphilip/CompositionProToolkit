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
// CompositionProToolkit.Controls v1.0.1
// 

using CompositionProToolkit.Expressions;
using CompositionProToolkit.Win2d;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace CompositionProToolkit.Controls
{
    /// <summary>
    /// Represents a switch that can be toggled between two states.
    /// </summary>
    [ContentProperty(Name = ContentHeader)]
    [TemplatePart(Name = LayoutRoot, Type = typeof(Grid))]
    [TemplatePart(Name = InteractionGrid, Type = typeof(Grid))]
    [TemplatePart(Name = RenderGrid, Type = typeof(Grid))]
    [TemplatePart(Name = HeaderContentPresenter, Type = typeof(ContentPresenter))]
    public class FluidToggleSwitch : Control
    {
        #region Constants

        private const string ContentHeader = "Header";
        private const string LayoutRoot = "LayoutRoot";
        private const string InteractionGrid = "InteractionGrid";
        private const string RenderGrid = "RenderGrid";
        private const string HeaderContentPresenter = "HeaderContentPresenter";

        private const float DefaultBaseWidth = 206f;
        private const float DefaultBaseHeight = 128f;
        private const float DefaultBaseCornerRadius = 64f;
        private const float DefaultTrackWidth = 190f;
        private const float DefaultTrackHeight = 112f;
        private const float DefaultTrackCornerRadius = 56f;
        private const float DefaultThumbRadius = 48f;
        private const float DefaultInnerThumbRadius = 40f;

        private const float DefaultShadowBlurRadius = 20f;
        private const float ActiveTrackLightFactor = 0.0f;
        private const float ActiveTrackDarkFactor = 0.0f;
        private const float InactiveTrackLightFactor = 0.1f;
        private const float InactiveTrackDarkFactor = 0.01f;
        private const float DisabledTrackLightFactor = 0.2f;
        private const float DisabledTrackDarkFactor = 0.075f;
        private const float DisabledThumbDarkFactor = 0.4f;
        private const float InnerThumbDarkFactor = 0.02f;
        private const float EnabledShadowDarkFactor = 0.8f;
        private const float DisabledShadowDarkFactor = 0.6f;
        private const float DropShadowOpacity = 0.7f;

        private static readonly Vector2 DefaultBaseSize = new Vector2(DefaultBaseWidth, DefaultBaseHeight);
        private static readonly Vector2 DefaultTrackSize = new Vector2(DefaultTrackWidth, DefaultTrackHeight);
        private static readonly Vector2 DefaultThumbSize = new Vector2(DefaultThumbRadius * 2, DefaultThumbRadius * 2);
        private static readonly Vector2 DefaultInnerThumbSize = new Vector2(DefaultInnerThumbRadius * 2, DefaultInnerThumbRadius * 2);
        private static readonly Vector3 DefaultTrackOffset = new Vector3(8f, 8f, 0f);
        private static readonly Vector3 DefaultInnerThumbOffset = new Vector3(8f, 8f, 0f);
        private static readonly Vector3 DefaultThumbCheckedOffset = new Vector3(94f, 16f, 0f);
        private static readonly Vector3 DefaultThumbUncheckedOffset = new Vector3(16f, 16f, 0f);
        private static readonly Vector3 DefaultShadowCheckedOffset = new Vector3(-8f, 0f, 0f);
        private static readonly Vector3 DefaultShadowUncheckedOffset = new Vector3(8f, 0f, 0f);
        private static readonly Vector3 DefaultShadowDisabledOffset = new Vector3(0f, 0f, 0f);
        private static readonly Color DefaultActiveColor = CanvasObject.CreateColor("#007aff");
        //private static readonly Color DefaultActiveColor = CanvasObject.CreateColor("#4cd964");
        private static readonly Color DefaultInactiveColor = CanvasObject.CreateColor("#bfbfbf");
        private static readonly Color DefaultDarkThemeInactiveColor = CanvasObject.CreateColor("#4b4b4b");
        private static readonly Color DefaultDisabledColor = CanvasObject.CreateColor("#bfbfbf");
        private static readonly TimeSpan DefaultAnimationDuration = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan DefaultOnBloomAnimationDuration = TimeSpan.FromMilliseconds(320);
        private static readonly TimeSpan DefaultOffBloomAnimationDuration = TimeSpan.FromMilliseconds(280);

        #endregion

        #region Fields

        private Compositor _compositor;
        private ICompositionGenerator _generator;

        private bool _isDarkTheme = false;
        private Color _baseActiveColor;
        private Color _baseInactiveColor;
        private Color _baseDisabledColor;

        private Color _trackActiveColor;
        private Color _trackInactiveColor;
        private Color _trackDisabledColor;

        private Color _thumbActiveColor;
        private Color _thumbInactiveColor;
        private Color _thumbDisabledColor;

        private Color _innerThumbActiveColor;
        private Color _innerThumbInactiveColor;

        private SpriteVisual _rootVisual;
        private SpriteVisual _baseVisual;
        private SpriteVisual _trackVisual;
        private SpriteVisual _thumbVisual;
        private SpriteVisual _innerThumbVisual;
        private SpriteVisual _bloomVisual;
        private SpriteVisual _onBloomVisual;
        private SpriteVisual _offBloomVisual;

        private IGeometrySurface _baseSurface;
        private IGeometrySurface _trackSurface;
        private IGeometrySurface _thumbSurface;
        private IGeometrySurface _innerThumbSurface;
        private CanvasGeometry _baseGeometry;
        private CanvasGeometry _trackGeometry;
        private IGeometrySurface _onBloomSurface;
        private IGeometrySurface _offBloomSurface;

        private Vector3 _thumbCheckedOffset;
        private Vector3 _thumbUncheckedOffset;
        private Vector3 _shadowCheckedOffset;
        private Vector3 _shadowUncheckedOffset;
        private Vector3 _bloomCenterOffset;

        private ImplicitAnimationCollection _thumbImplicitAnimation;
        private ImplicitAnimationCollection _thumbShadowImplicitAnimation;
        private KeyFrameAnimation<Vector2> _onBloomAnimation;
        private KeyFrameAnimation<Vector2> _offBloomAnimation;
        private DropShadow _dropShadow;

        private Grid _rootGrid;
        private Grid _interactionGrid;
        private Grid _renderGrid;
        private ContentPresenter _headerContentPresenter;

        #endregion

        #region Events

        /// <summary>
        /// This event is fired when On/Off state changes for FluidToggleSwitch.
        /// </summary>
        public event RoutedEventHandler Toggled;

        #endregion

        #region Dependency Properties

        #region Header

        /// <summary>
        /// Header Dependency Property
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(FluidToggleSwitch),
                new PropertyMetadata(null, OnHeaderChanged));

        /// <summary>
        /// Gets or sets the content to display as the FluidToggleSwitch Header.
        /// </summary>
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// Handles changes to the Header property.
        /// </summary>
        /// <param name="d">FluidToggleSwitch</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FluidToggleSwitch)d;
            target.OnHeaderChanged();
        }

        /// <summary>
        /// Handles changes to the Header property.
        /// </summary>
        private void OnHeaderChanged()
        {
            if (_headerContentPresenter == null)
            {
                return;
            }

            _headerContentPresenter.Visibility = Header == null ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// HeaderTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(FluidToggleSwitch),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the template used for displaying the Header.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        #endregion

        #region OffContent

        /// <summary>
        /// OffContent Dependency Property
        /// </summary>
        public static readonly DependencyProperty OffContentProperty =
            DependencyProperty.Register("OffContent", typeof(object), typeof(FluidToggleSwitch),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the content to display when the ToggleSwitch is in Off state.
        /// </summary>
        public object OffContent
        {
            get => GetValue(OffContentProperty);
            set => SetValue(OffContentProperty, value);
        }

        #endregion

        #region OffContentTemplate

        /// <summary>
        /// OffContentTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty OffContentTemplateProperty =
            DependencyProperty.Register("OffContentTemplate", typeof(DataTemplate), typeof(FluidToggleSwitch),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the template used to display the Off content.
        /// </summary>
        public DataTemplate OffContentTemplate
        {
            get => (DataTemplate)GetValue(OffContentTemplateProperty);
            set => SetValue(OffContentTemplateProperty, value);
        }

        #endregion

        #region OnContent

        /// <summary>
        /// OnContent Dependency Property
        /// </summary>
        public static readonly DependencyProperty OnContentProperty =
            DependencyProperty.Register("OnContent", typeof(object), typeof(FluidToggleSwitch),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the content to display when the ToggleSwitch is in On state.
        /// </summary>
        public object OnContent
        {
            get => GetValue(OnContentProperty);
            set => SetValue(OnContentProperty, value);
        }

        #endregion

        #region OnContentTemplate

        /// <summary>
        /// OnContentTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty OnContentTemplateProperty =
            DependencyProperty.Register("OnContentTemplate", typeof(DataTemplate), typeof(FluidToggleSwitch),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the template used to display the On content.
        /// </summary>
        public DataTemplate OnContentTemplate
        {
            get => (DataTemplate)GetValue(OnContentTemplateProperty);
            set => SetValue(OnContentTemplateProperty, value);
        }

        #endregion

        #region ActiveColor

        /// <summary>
        /// ActiveColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActiveColorProperty =
            DependencyProperty.Register("ActiveColor", typeof(Color), typeof(FluidToggleSwitch),
                new PropertyMetadata(DefaultActiveColor, OnActiveColorChanged));

        /// <summary>
        /// Gets or sets the color of the FluidToggleSwitch in Checked state.
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
        /// Gets or sets the color of the FluidToggleSwitch in Checked state.
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

        #region DarkThemeInactiveColor

        /// <summary>
        /// DarkThemeInactiveColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty DarkThemeInactiveColorProperty =
            DependencyProperty.Register("DarkThemeInactiveColor", typeof(Color), typeof(FluidToggleSwitch),
                new PropertyMetadata(DefaultDarkThemeInactiveColor, OnDarkThemeInactiveColorChanged));

        /// <summary>
        /// Gets or sets the color of the FluidToggleSwitch in Checked state.
        /// </summary>
        public Color DarkThemeInactiveColor
        {
            get => (Color)GetValue(DarkThemeInactiveColorProperty);
            set => SetValue(DarkThemeInactiveColorProperty, value);
        }

        /// <summary>
        /// Handles changes to the DarkThemeInactiveColor property.
        /// </summary>
        /// <param name="d">FluidToggleSwitch</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDarkThemeInactiveColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleSwitch = (FluidToggleSwitch)d;
            toggleSwitch.OnDarkThemeInactiveColorChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the DarkThemeInactiveColor property.
        /// </summary>
        private void OnDarkThemeInactiveColorChanged()
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
        /// Gets or sets the color of the FluidToggleSwitch in Checked state.
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

        #region IsOn

        /// <summary>
        /// IsOn Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(FluidToggleSwitch),
                new PropertyMetadata(false, OnIsOnChanged));

        /// <summary>
        /// Gets or sets whether the FluidToggleSwitch is in On state.
        /// </summary>
        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        /// <summary>
        /// Handles changes to the IsOn property.
        /// </summary>
        /// <param name="d">FluidToggleSwitch</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || e.NewValue == e.OldValue)
            {
                return;
            }

            var target = (FluidToggleSwitch)d;
            target.OnToggleStateChanged();
        }

        #endregion

        #region TargetVisibility

        /// <summary>
        /// TargetVisibility Dependency Property
        /// </summary>
        public static readonly DependencyProperty TargetVisibilityProperty =
            DependencyProperty.Register("TargetVisibility", typeof(Visibility), typeof(FluidToggleSwitch),
                new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// <para>Converts the IsOn property to Visibility. This property can be bound to a control's
        /// Visibility property to make it Visible when the IsOn is true and Collapsed when IsOn is false.</para>
        /// <para>When IsOn is True, this property returns Visibility.Visible.</para>
        /// <para>When IsOn is False, this property returns Visibility.Collapsed.</para>
        /// </summary>
        public Visibility TargetVisibility
        {
            get => (Visibility)GetValue(TargetVisibilityProperty);
            private set => SetValue(TargetVisibilityProperty, value);
        }

        #endregion

        #region InverseTargetVisibility

        /// <summary>
        /// InverseTargetVisibility Dependency Property
        /// </summary>
        public static readonly DependencyProperty InverseTargetVisibilityProperty =
            DependencyProperty.Register("InverseTargetVisibility", typeof(Visibility), typeof(FluidToggleSwitch),
                new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// <para>Returns the inverse of the TargetVisibility property. This property can be bound to a control's
        /// Visibility property to make it Collapsed when the IsOn is true and Visible when IsOn is false.</para>
        /// <para>When IsOn is True, this property returns Visibility.Collapsed.</para>
        /// <para>When IsOn is False, this property returns Visibility.Visible.</para>
        /// </summary>
        public Visibility InverseTargetVisibility
        {
            get => (Visibility)GetValue(InverseTargetVisibilityProperty);
            private set => SetValue(InverseTargetVisibilityProperty, value);
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
            // Subscribe to the changes of the Requested dependency property
            RegisterPropertyChangedCallback(RequestedThemeProperty, OnRequestedThemeChanged);

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

            _rootGrid = GetTemplateChild("LayoutRoot") as Grid;
            _interactionGrid = GetTemplateChild("InteractionGrid") as Grid;
            _renderGrid = GetTemplateChild("RenderGrid") as Grid;
            _headerContentPresenter = GetTemplateChild("HeaderContentPresenter") as ContentPresenter;

            if (_renderGrid == null || _interactionGrid == null || _rootGrid == null)
                return;

            _interactionGrid.Tapped += OnFluidToggleSwitchTapped;

            // Set the visual state
            SetVisualState(false);

            // Invoke the Toggled event
            Toggled?.Invoke(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Handler method for the Arrange Layout phase.
        /// </summary>
        /// <param name="finalSize">Final size of the control.</param>
        /// <returns>Size</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);

            // If compositor is null, it means none of the visuals
            // have been initialized yet.
            if (_compositor == null)
            {
                InitComposition();
            }

            // Update the visuals based on the current control Size and Padding
            var width = (float)_renderGrid.Width;
            var scale = width / DefaultBaseWidth;
            var height = scale * DefaultBaseHeight;
            var baseSize = new Vector2(width, height);

            var offsetX = 0f;
            var offsetY = (float)((_renderGrid.Height - height) / 2);

            //
            // Root Visual
            //
            _rootVisual.Size = baseSize;
            _rootVisual.Offset = new Vector3(offsetX, offsetY, 0);

            //
            // Base Visual
            //
            _baseVisual.Size = baseSize;
            var baseColor = IsEnabled ? (IsOn ? _baseActiveColor : _baseInactiveColor) : _baseDisabledColor;
            var baseCornerRadius = DefaultBaseCornerRadius * scale;
            _baseGeometry = CanvasGeometry.CreateRoundedRectangle(_generator.Device,
                                                                  0,
                                                                  0,
                                                                  DefaultBaseWidth * scale,
                                                                  DefaultBaseHeight * scale,
                                                                  baseCornerRadius,
                                                                  baseCornerRadius);
            _baseSurface.Redraw(_baseVisual.Size.ToSize(), _baseGeometry, baseColor);

            //
            // Track Visual
            //
            _trackVisual.Size = DefaultTrackSize * scale;
            var trackColor = IsEnabled ? (IsOn ? _trackActiveColor : _trackInactiveColor) : _trackDisabledColor;
            var trackCornerRadius = DefaultTrackCornerRadius * scale;
            _trackGeometry = CanvasGeometry.CreateRoundedRectangle(_generator.Device,
                                                                   0,
                                                                   0,
                                                                   DefaultTrackWidth * scale,
                                                                   DefaultTrackHeight * scale,
                                                                   trackCornerRadius,
                                                                   trackCornerRadius);
            _trackSurface.Redraw(_trackVisual.Size.ToSize(), _trackGeometry, trackColor);
            _trackVisual.Offset = DefaultTrackOffset * scale;

            //
            // Bloom Visual
            //
            _bloomVisual.Size = baseSize;
            _bloomVisual.Brush = _compositor.CreateColorBrush(_trackActiveColor);
            _bloomVisual.Clip = _compositor.CreateGeometricClip(_baseGeometry);
            _bloomCenterOffset = (DefaultThumbCheckedOffset + new Vector3(DefaultThumbRadius, DefaultThumbRadius, 0)) * scale;
            var radius = DefaultThumbCheckedOffset.X * scale;

            _onBloomVisual.Size = new Vector2(radius * 2);
            _onBloomVisual.Offset = _bloomCenterOffset;
            var onBloomGeometry = CanvasGeometry.CreateCircle(_generator.Device, _onBloomVisual.Size * 0.5f, radius);
            _onBloomSurface.Redraw(_onBloomVisual.Size.ToSize(), onBloomGeometry, _trackInactiveColor);

            _offBloomVisual.Size = baseSize * 1.2f;
            _offBloomVisual.Offset = new Vector3(baseSize * 0.5f, 0);
            var offBloomGeometry = _baseGeometry.Transform(Matrix3x2.CreateScale(1.2f));
            _offBloomSurface.Redraw(_offBloomVisual.Size.ToSize(), offBloomGeometry, _trackInactiveColor);

            //
            // Inner Thumb
            //
            _innerThumbVisual.Size = DefaultInnerThumbSize * scale;
            var innerThumbColor = IsEnabled ? (IsOn ? _innerThumbActiveColor : _innerThumbInactiveColor) : _thumbDisabledColor;
            var innerThumbRadius = DefaultInnerThumbRadius * scale;
            _innerThumbSurface.Redraw(_innerThumbVisual.Size.ToSize(),
                CanvasGeometry.CreateCircle(_generator.Device, innerThumbRadius, innerThumbRadius, innerThumbRadius), innerThumbColor);
            _innerThumbVisual.Offset = DefaultInnerThumbOffset * scale;

            //
            // Thumb
            //
            _thumbVisual.Size = DefaultThumbSize * scale;
            var thumbColor = IsEnabled ? (IsOn ? _thumbActiveColor : _thumbInactiveColor) : _thumbDisabledColor;
            var thumbRadius = DefaultThumbRadius * scale;
            _thumbSurface.Redraw(_thumbVisual.Size.ToSize(),
                CanvasGeometry.CreateCircle(_generator.Device, thumbRadius, thumbRadius, thumbRadius), thumbColor);
            _thumbCheckedOffset = DefaultThumbCheckedOffset * scale;
            _thumbUncheckedOffset = DefaultThumbUncheckedOffset * scale;
            _thumbVisual.ImplicitAnimations = null;
            _thumbVisual.Offset = IsOn ? _thumbCheckedOffset : _thumbUncheckedOffset;
            _thumbVisual.ImplicitAnimations = _thumbImplicitAnimation;

            //
            // Thumb Shadow
            //
            _shadowCheckedOffset = DefaultShadowCheckedOffset * scale;
            _shadowUncheckedOffset = DefaultShadowUncheckedOffset * scale;
            _dropShadow.BlurRadius = DefaultShadowBlurRadius * scale;
            _dropShadow.ImplicitAnimations = null;
            _dropShadow.Color = IsEnabled ? trackColor.DarkerBy(EnabledShadowDarkFactor) : trackColor.DarkerBy(DisabledShadowDarkFactor);
            _dropShadow.Offset = IsOn ? _shadowCheckedOffset : _shadowUncheckedOffset;
            _dropShadow.ImplicitAnimations = _thumbShadowImplicitAnimation;


            // Attach Visual to the RenderGrid
            if (_renderGrid != null)
            {
                ElementCompositionPreview.SetElementChildVisual(_renderGrid, _rootVisual);
            }

            return _rootGrid.DesiredSize;
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
        /// Handler method which is invoked when the RequestedTheme property value changes
        /// </summary>
        /// <param name="sender">FluidToggleSwitch</param>
        /// <param name="dp">Dependency Property that has changed</param>
        private void OnRequestedThemeChanged(DependencyObject sender, DependencyProperty dp)
        {
            switch (RequestedTheme)
            {
                case ElementTheme.Dark:
                    _isDarkTheme = true;
                    break;
                default:
                    _isDarkTheme = false;
                    break;
            }

            UpdateColors();

            InvalidateArrange();
        }

        /// <summary>
        /// Handler method which is invoked when the FluidToggleSwitch is tapped to toggle its state
        /// </summary>
        /// <param name="sender">FluidToggleSwitch</param>
        /// <param name="e">TappedRoutedEventArgs</param>
        private void OnFluidToggleSwitchTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!IsEnabled)
                return;

            IsOn = !IsOn;
        }

        /// <summary>
        /// Handles changes to the IsOn property.
        /// </summary>
        private void OnToggleStateChanged()
        {
            bool stateChangedBeforeFirstRender = false;
            if (_compositor == null)
            {
                stateChangedBeforeFirstRender = IsOn;
                InitComposition();
            }

            _thumbVisual.Offset = IsOn ? _thumbCheckedOffset : _thumbUncheckedOffset;

            var thumbColor = IsEnabled ? (IsOn ? _thumbActiveColor : _thumbInactiveColor) : _thumbDisabledColor;
            _thumbSurface.Redraw(thumbColor);

            if (IsEnabled)
            {
                var baseColor = IsOn ? _baseActiveColor : _baseInactiveColor;
                var trackColor = IsOn ? _trackActiveColor : _trackInactiveColor;
                _dropShadow.Color = trackColor.DarkerBy(EnabledShadowDarkFactor);
                _dropShadow.Offset = IsOn ? _shadowCheckedOffset : _shadowUncheckedOffset;

                if (stateChangedBeforeFirstRender)
                {
                    _baseSurface.Redraw(baseColor);
                    _trackSurface.Redraw(trackColor);
                }
                else
                {
                    _compositor.CreateScopedBatch(CompositionBatchTypes.Animation,
                        () =>
                        {
                            _bloomVisual.Opacity = 1f;
                            if (IsOn)
                            {
                                _onBloomVisual.Opacity = 1;
                                _onBloomVisual.StartAnimation(() => _onBloomVisual.ScaleXY(), _onBloomAnimation);
                            }
                            else
                            {
                                _offBloomVisual.Scale = Vector3.Zero;
                                _offBloomVisual.Opacity = 1;
                                _offBloomVisual.StartAnimation(() => _offBloomVisual.ScaleXY(), _offBloomAnimation);
                            }
                        },
                        () =>
                        {
                            _baseSurface.Redraw(baseColor);
                            _trackSurface.Redraw(trackColor);
                            _bloomVisual.Opacity = 0f;
                            _onBloomVisual.Opacity = 0;
                            _offBloomVisual.Opacity = 0;
                        });
                }
            }
            else
            {
                _baseSurface.Redraw(_baseDisabledColor);
                _trackSurface.Redraw(_trackDisabledColor);
                _dropShadow.Color = _trackDisabledColor.DarkerBy(DisabledShadowDarkFactor);
                _dropShadow.Offset = DefaultShadowDisabledOffset;
            }

            SetVisualState();

            TargetVisibility = IsOn ? Visibility.Visible : Visibility.Collapsed;
            InverseTargetVisibility = IsOn ? Visibility.Collapsed : Visibility.Visible;

            // Invoke the Toggled event
            Toggled?.Invoke(this, new RoutedEventArgs());
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Sets the Visual state of the FluidToggleSwitch
        /// </summary>
        private void SetVisualState(bool animate = true)
        {
            if (_headerContentPresenter != null)
            {
                _headerContentPresenter.Visibility = Header == null ? Visibility.Collapsed : Visibility.Visible;
            }

            if (IsEnabled)
            {
                if (IsOn)
                {
                    VisualStateManager.GoToState(this, "On", animate);
                    if (OnContent != null)
                    {
                        VisualStateManager.GoToState(this, "OnContent", animate);
                    }
                }
                else
                {
                    VisualStateManager.GoToState(this, "Off", animate);
                    if (OffContent != null)
                    {
                        VisualStateManager.GoToState(this, "OffContent", animate);
                    }
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", animate);
            }
        }

        /// <summary>
        /// Updates the color components based on the Active, Inactive
        /// and Disabled colors.
        /// </summary>
        private void UpdateColors()
        {
            // Active
            _baseActiveColor = _isDarkTheme ? ActiveColor.LighterBy(ActiveTrackLightFactor) : ActiveColor.DarkerBy(ActiveTrackDarkFactor);
            _trackActiveColor = ActiveColor;
            _thumbActiveColor = Colors.White;
            _innerThumbActiveColor = _innerThumbInactiveColor = Colors.White.DarkerBy(InnerThumbDarkFactor);

            // Inactive
            _baseInactiveColor = _isDarkTheme ? DarkThemeInactiveColor.LighterBy(InactiveTrackLightFactor) : InactiveColor.DarkerBy(InactiveTrackDarkFactor);
            _trackInactiveColor = _isDarkTheme ? DarkThemeInactiveColor : InactiveColor;
            _thumbInactiveColor = _thumbActiveColor;

            // Disabled
            _baseDisabledColor = _isDarkTheme ? DisabledColor.LighterBy(DisabledTrackLightFactor) : DisabledColor.DarkerBy(DisabledTrackDarkFactor);
            _trackDisabledColor = DisabledColor;
            _thumbDisabledColor = Colors.White.DarkerBy(DisabledThumbDarkFactor);
        }

        /// <summary>
        /// Initializes the Composition components of the FluidToggleSwitch
        /// </summary>
        private void InitComposition()
        {
            _compositor = Window.Current.Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            // Update the colors
            UpdateColors();

            //
            // Root Visual
            //
            _rootVisual = _compositor.CreateSpriteVisual();
            _rootVisual.Size = DefaultBaseSize;

            //
            // Base Visual
            //
            _baseVisual = _compositor.CreateSpriteVisual();
            _baseVisual.Size = DefaultBaseSize;
            _baseGeometry = CanvasGeometry.CreateRoundedRectangle(_generator.Device,
                                                                  0,
                                                                  0,
                                                                  DefaultBaseWidth,
                                                                  DefaultBaseHeight,
                                                                  DefaultBaseCornerRadius,
                                                                  DefaultBaseCornerRadius);
            _baseSurface = _generator.CreateGeometrySurface(DefaultBaseSize.ToSize(),
                                                            _baseGeometry,
                                                            IsOn ? _baseActiveColor : _baseInactiveColor);
            _baseVisual.Brush = _compositor.CreateSurfaceBrush(_baseSurface);

            //
            // Track Visual
            //
            _trackVisual = _compositor.CreateSpriteVisual();
            _trackVisual.Size = DefaultTrackSize;
            _trackGeometry = CanvasGeometry.CreateRoundedRectangle(_generator.Device,
                                                                   0,
                                                                   0,
                                                                   DefaultTrackWidth,
                                                                   DefaultTrackHeight,
                                                                   DefaultTrackCornerRadius,
                                                                   DefaultTrackCornerRadius);
            _trackSurface = _generator.CreateGeometrySurface(DefaultTrackSize.ToSize(),
                                                             _trackGeometry,
                                                             IsOn ? _trackActiveColor : _trackInactiveColor);
            _trackVisual.Brush = _compositor.CreateSurfaceBrush(_trackSurface);
            _trackVisual.Offset = DefaultTrackOffset;

            //
            // Bloom Visual
            //
            _bloomVisual = _compositor.CreateSpriteVisual();
            _bloomVisual.Size = DefaultBaseSize;
            _bloomVisual.Brush = _compositor.CreateColorBrush(DefaultActiveColor);
            _bloomVisual.Clip = _compositor.CreateGeometricClip(_baseGeometry);
            _bloomCenterOffset = DefaultThumbCheckedOffset + new Vector3(DefaultThumbRadius, DefaultThumbRadius, 0);
            var radius = DefaultThumbCheckedOffset.X;

            _onBloomVisual = _compositor.CreateSpriteVisual();
            _onBloomVisual.Size = new Vector2(radius * 2);
            _onBloomVisual.AnchorPoint = Vector2.One * 0.5f;
            _onBloomVisual.Offset = _bloomCenterOffset;
            var onBloomGeometry = CanvasGeometry.CreateCircle(_generator.Device, _onBloomVisual.Size * 0.5f, radius);
            _onBloomSurface = _generator.CreateGeometrySurface(_onBloomVisual.Size.ToSize(), onBloomGeometry, DefaultInactiveColor);
            _onBloomVisual.Brush = _compositor.CreateSurfaceBrush(_onBloomSurface);

            _offBloomVisual = _compositor.CreateSpriteVisual();
            _offBloomVisual.Size = DefaultBaseSize * 1.2f;
            _offBloomVisual.AnchorPoint = Vector2.One * 0.5f;
            _offBloomVisual.Offset = new Vector3(DefaultBaseSize * 0.5f, 0);
            var offBloomGeometry = _baseGeometry.Transform(Matrix3x2.CreateScale(1.2f));
            _offBloomSurface = _generator.CreateGeometrySurface(_offBloomVisual.Size.ToSize(), offBloomGeometry, DefaultInactiveColor);
            _offBloomVisual.Brush = _compositor.CreateSurfaceBrush(_offBloomSurface);

            _bloomVisual.Children.InsertAtTop(_offBloomVisual);
            _bloomVisual.Children.InsertAtTop(_onBloomVisual);
            // Hide the bloom visual as it will be visible only during toggle
            _onBloomVisual.Opacity = 0f;
            _offBloomVisual.Opacity = 0f;
            _bloomVisual.Opacity = 0f;

            //
            // Inner Thumb Visual
            //
            _innerThumbVisual = _compositor.CreateSpriteVisual();
            _innerThumbVisual.Size = DefaultInnerThumbSize;
            _innerThumbSurface = _generator.CreateGeometrySurface(DefaultInnerThumbSize.ToSize(),
                CanvasGeometry.CreateCircle(_generator.Device,
                                            DefaultInnerThumbRadius,
                                            DefaultInnerThumbRadius,
                                            DefaultInnerThumbRadius),
                                            IsEnabled ? (IsOn ? _innerThumbActiveColor
                                                              : _innerThumbInactiveColor)
                                                      : _thumbDisabledColor);
            _innerThumbVisual.Brush = _compositor.CreateSurfaceBrush(_innerThumbSurface);
            _innerThumbVisual.Offset = DefaultInnerThumbOffset;

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
                                            IsOn ? _thumbActiveColor
                                                 : _thumbInactiveColor);
            _thumbVisual.Brush = _compositor.CreateSurfaceBrush(_thumbSurface);
            _thumbVisual.Offset = IsOn ? DefaultThumbCheckedOffset : DefaultThumbUncheckedOffset;
            _thumbVisual.Children.InsertAtTop(_innerThumbVisual);

            //
            // Thumb Drop Shadow
            //
            _dropShadow = _compositor.CreateDropShadow();
            _dropShadow.BlurRadius = DefaultShadowBlurRadius;
            _dropShadow.Offset = IsOn ? DefaultShadowCheckedOffset : DefaultShadowUncheckedOffset;
            var currentTrackColor = IsOn ? _trackActiveColor : _trackInactiveColor;
            _dropShadow.Color = IsEnabled ? currentTrackColor.DarkerBy(EnabledShadowDarkFactor) : currentTrackColor.LighterBy(DisabledShadowDarkFactor);
            _dropShadow.Opacity = DropShadowOpacity;
            //_dropShadow.Mask = _thumbVisual.Brush;
            _dropShadow.SourcePolicy = CompositionDropShadowSourcePolicy.InheritFromVisualContent;
            _thumbVisual.Shadow = _dropShadow;


            // Add Base,Track, Bloom and Thumb to the Root
            _rootVisual.Children.InsertAtTop(_baseVisual);
            _rootVisual.Children.InsertAtTop(_trackVisual);
            _rootVisual.Children.InsertAtTop(_bloomVisual);
            _rootVisual.Children.InsertAtTop(_thumbVisual);

            // Attach Visual to the RenderGrid
            if (_renderGrid != null)
            {
                ElementCompositionPreview.SetElementChildVisual(_renderGrid, _rootVisual);
            }

            //
            // ImplicitAnimations
            //

            // For Thumb
            var offsetAnimation = _compositor.GenerateVector3KeyFrameAnimation()
                                             .ForTarget(() => _thumbVisual.Offset)
                                             .HavingDuration(DefaultAnimationDuration);

            offsetAnimation.InsertFinalValueKeyFrame(1f, _compositor.CreateEaseOutExponentialEasingFunction());
            _thumbImplicitAnimation = _compositor.CreateImplicitAnimationCollection();
            _thumbImplicitAnimation.Add(() => _thumbVisual.Offset, offsetAnimation);

            _thumbVisual.ImplicitAnimations = _thumbImplicitAnimation;

            // For Thumb Shadow
            var shadowColorAnimation = _compositor.GenerateColorKeyFrameAnimation()
                                                  .ForTarget(() => _dropShadow.Color)
                                                  .HavingDuration(DefaultAnimationDuration);
            shadowColorAnimation.InsertFinalValueKeyFrame(1f, _compositor.CreateEaseInCircleEasingFunction());

            _thumbShadowImplicitAnimation = _compositor.CreateImplicitAnimationCollection();
            _thumbShadowImplicitAnimation.Add(() => _dropShadow.Offset, offsetAnimation);
            _thumbShadowImplicitAnimation.Add(() => _dropShadow.Color, shadowColorAnimation);
            _dropShadow.ImplicitAnimations = _thumbShadowImplicitAnimation;

            //
            // Animations
            // 

            // On Bloom Animation
            _onBloomAnimation = _compositor.GenerateVector2KeyFrameAnimation()
                                           .HavingDuration(DefaultOnBloomAnimationDuration);

            _onBloomAnimation.InsertKeyFrames(
                new KeyFrame<Vector2>(0f, Vector2.One),
                new KeyFrame<Vector2>(1f, Vector2.Zero, _compositor.CreateEaseOutSineEasingFunction()));

            // Off Bloom Animation
            _offBloomAnimation = _compositor.GenerateVector2KeyFrameAnimation()
                                            .HavingDuration(DefaultOffBloomAnimationDuration);

            _offBloomAnimation.InsertKeyFrames(
                new KeyFrame<Vector2>(0f, Vector2.Zero),
                new KeyFrame<Vector2>(1f, Vector2.One, _compositor.CreateLinearEasingFunction()));

            TargetVisibility = IsOn ? Visibility.Visible : Visibility.Collapsed;
            InverseTargetVisibility = IsOn ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion
    }
}
