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
using CompositionProToolkit.Expressions.Templates;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CompositionProToolkit.Controls
{
    /// <summary>
    /// A 3d ProgressRing control
    /// </summary>
    [WebHostHidden]
    [TemplatePart(Name = "PART_RootGrid", Type = typeof(Grid))]
    public class ProgressRing3d : Control
    {
        #region Enums

        /// <summary>
        /// Shape of the Node in the ProgressRing3d
        /// </summary>
        public enum NodeShapeType
        {
            /// <summary>
            /// Circular shaped nodes
            /// </summary>
            Circle,
            /// <summary>
            /// Square shaped nodes
            /// </summary>
            Square
        }

        #endregion

        #region Constants

        private const float WidthFactor = 0.95f;

        #endregion

        #region Fields

        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private Matrix4x4 _projectionMatrix;
        private Grid _rootGrid;
        private Visual _rootVisual;
        private double _depth;
        private Vector3 _center;
        private Color _nodeColor;
        private List<SpriteVisual> _nodes;
        private CompositionPropertySet _nodeSet;
        private CompositionSurfaceBrush _nodeBrush;
        private bool _nodesInitialized;
        private SpriteVisual _outerVisual;

        // Size of each node
        private readonly Vector2 _nodeSize = new Vector2(6, 6);
        private readonly UISettings _uiSettings = new UISettings();
        private readonly TimeSpan _defaultAnimationDuration = TimeSpan.FromSeconds(6.5f);

        #endregion

        #region Dependency Properties

        #region NodeColor

        /// <summary>
        /// NodeColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty NodeColorProperty =
            DependencyProperty.Register("NodeColor", typeof(Color), typeof(ProgressRing3d),
                new PropertyMetadata(Colors.Blue, OnNodeColorChanged));

        /// <summary>
        /// Gets or sets the NodeColor property. This dependency property 
        /// indicates the color of the nodes in the ProgressRing3d.
        /// </summary>
        public Color NodeColor
        {
            get => (Color)GetValue(NodeColorProperty);
            set => SetValue(NodeColorProperty, value);
        }

        /// <summary>
        /// Handles changes to the NodeColor property.
        /// </summary>
        /// <param name="d">ProgressRing3d</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnNodeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (ProgressRing3d)d;
            target.OnNodeColorChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the NodeColor property.
        /// </summary>
        private void OnNodeColorChanged()
        {
            if (SyncAccentColor)
                return;

            _nodeColor = NodeColor;
            // Refresh Layout
            if (_nodesInitialized)
            {
                UpdateNodes();
            }
            else
            {
                InvalidateArrange();
            }
        }

        #endregion

        #region NodeShape

        /// <summary>
        /// NodeShape Dependency Property
        /// </summary>
        public static readonly DependencyProperty NodeShapeProperty =
            DependencyProperty.Register("NodeShape", typeof(NodeShapeType), typeof(ProgressRing3d),
                new PropertyMetadata(NodeShapeType.Circle, OnNodeShapeChanged));

        /// <summary>
        /// Gets or sets the NodeShape property. This dependency property 
        /// indicates the shape of the node.
        /// </summary>
        public NodeShapeType NodeShape
        {
            get => (NodeShapeType)GetValue(NodeShapeProperty);
            set => SetValue(NodeShapeProperty, value);
        }

        /// <summary>
        /// Handles changes to the NodeShape property.
        /// </summary>
        /// <param name="d">ProgressRing3d</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnNodeShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (ProgressRing3d)d;
            target.OnNodeShapeChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the NodeShape property.
        /// </summary>
        private void OnNodeShapeChanged()
        {
            // Refresh Layout
            if (_nodesInitialized)
            {
                UpdateNodes();
            }
            else
            {
                InvalidateArrange();
            }
        }

        #endregion

        #region SyncAccentColor

        /// <summary>
        /// SyncAccentColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty SyncAccentColorProperty =
            DependencyProperty.Register("SyncAccentColor", typeof(bool), typeof(ProgressRing3d),
                new PropertyMetadata(true, OnSyncAccentColorChanged));

        /// <summary>
        /// Gets or sets the SyncAccentColor property. This dependency property 
        /// indicates whether the NodeColor should be synced with the SystemAccent color.
        /// If SyncAccentColor is set to true, the NodeColor property will be ignored.
        /// </summary>
        public bool SyncAccentColor
        {
            get => (bool)GetValue(SyncAccentColorProperty);
            set => SetValue(SyncAccentColorProperty, value);
        }

        /// <summary>
        /// Handles changes to the SyncAccentColor property.
        /// </summary>
        /// <param name="d">ProgressRing3d</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSyncAccentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (ProgressRing3d)d;
            target.OnSyncAccentColorChanged();
        }

        /// <summary>
        /// Provides the class instance an opportunity to handle changes to the SyncAccentColor property.
        /// </summary>
        private void OnSyncAccentColorChanged()
        {
            _nodeColor = SyncAccentColor ? (Color)Application.Current.Resources["SystemAccentColor"] : NodeColor;
            // Refresh Layout
            if (_nodesInitialized)
            {
                UpdateNodes();
            }
            else
            {
                InvalidateArrange();
            }
        }

        #endregion

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        public ProgressRing3d()
        {
            // Set the style key
            DefaultStyleKey = typeof(ProgressRing3d);
            // Define the projection matrix
            _projectionMatrix = new Matrix4x4(1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, -1 * (1 / (float)_depth),
                0, 0, 0, 1);

            _uiSettings.ColorValuesChanged += OnColorValuesChanged;
            _nodes = new List<SpriteVisual>();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called when the Template is first applied to the control
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Get the Root Grid within which the composition elements will be hosted
            _rootGrid = GetTemplateChild("PART_RootGrid") as Grid;
            if (_rootGrid != null)
            {
                _rootVisual = ElementCompositionPreview.GetElementVisual(_rootGrid);
            }
        }

        /// <summary>
        /// Handler for the Arrange Layout phase
        /// </summary>
        /// <param name="finalSize">Final size of the ProgressRing3d control</param>
        /// <returns>Size</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);

            // Is this the first time the method is being invoked?
            if (_compositor == null)
            {
                InitializeComposition();
            }

            var padding = Padding.CollapseThickness();
            var rootSize = new Vector2((float)(size.Width - padding.Width), (float)(size.Height - padding.Height));
            _center = new Vector3(rootSize * 0.5f, 0f);
            // Outer visual size will be a square size with side length equal to the (width of rootSize * WidthFactor)
            var outerVisualSize = new Vector2(rootSize.X * WidthFactor, rootSize.X * WidthFactor);

            var outerVisualOffset = new Vector3((rootSize.X - outerVisualSize.X) / 2, (rootSize.Y - outerVisualSize.Y) / 2, 0);

            // OuterVisual is the parent of all the nodes
            _outerVisual.Size = outerVisualSize;
            _outerVisual.Offset = outerVisualOffset;
            _outerVisual.CenterPoint = new Vector3(outerVisualSize * 0.5f, 0);

            var radius = (outerVisualSize.X - _nodeSize.X) / 2f;
            var center = new Vector3(outerVisualSize.X / 2, outerVisualSize.Y / 2, 0);
            _nodeSet.InsertScalar("radius", radius);
            _nodeSet.InsertVector3("center", center);

            _depth = outerVisualSize.X * 2f;
            // Update the perspective transform of the RootGrid
            UpdateParentTransform();

            return size;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when the System Accent color changes
        /// </summary>
        /// <param name="sender">UISettings</param>
        /// <param name="args">event arguments</param>
        private async void OnColorValuesChanged(UISettings sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!SyncAccentColor)
                    return;

                _nodeColor = (Color)Application.Current.Resources["SystemAccentColor"];
                if (_nodesInitialized)
                {
                    UpdateNodes();
                }
                else
                {
                    InvalidateArrange();
                }
            });
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Initializes the composition elements, animations etc.
        /// </summary>
        private void InitializeComposition()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            _nodeSet = _compositor.CreatePropertySet();
            _nodeSet.InsertScalar("angle", 0f);
            _nodeSet.InsertScalar("offsetAngle", 180f);
            _nodeSet.InsertScalar("radius", 0f);
            _nodeSet.InsertVector3("center", Vector3.Zero);

            // Animation for the angle
            var angleAnim = _compositor.GenerateScalarKeyFrameAnimation()
                                       .HavingDuration(_defaultAnimationDuration)
                                       .RepeatsForever();
            angleAnim.InsertKeyFrame(1f, -360f, _compositor.CreateLinearEasingFunction());

            // Animation for each of the nodes
            var offsetAnim = _compositor.CreateVector3ExpressionAnimation();
            offsetAnim.Expression = c =>
                new Vector3(_nodeSet.Get<Vector3>("center").X +
                                _nodeSet.Get<float>("radius") * c.Cos((_nodeSet.Get<float>("angle") +
                                                                       _nodeSet.Get<float>("offsetAngle") +
                                                                       new VisualTarget().Properties.Get<float>("angleOffset")) * Scalar.DegreesToRadians),
                            _nodeSet.Get<Vector3>("center").Y +
                                _nodeSet.Get<float>("radius") * c.Sin((_nodeSet.Get<float>("angle") +
                                                                       _nodeSet.Get<float>("offsetAngle") +
                                                                       new VisualTarget().Properties.Get<float>("angleOffset")) * Scalar.DegreesToRadians),
                            0);


            _outerVisual = _compositor.CreateSpriteVisual();
            _outerVisual.RotationAxis = new Vector3(1, 0, 0);
            _outerVisual.RotationAngleInDegrees = 90;

            // Create the nodes
            for (var i = 0; i < 50; i += 10)
            {
                var node = CreateNode(i);
                _outerVisual.Children.InsertAtTop(node);
                node.StartAnimation(() => node.Offset, offsetAnim);
                _nodes.Add(node);
            }

            _nodeColor = SyncAccentColor ? (Color)Application.Current.Resources["SystemAccentColor"] : NodeColor;

            UpdateNodes();

            _nodeSet.StartAnimation("angle", angleAnim);

            if (_rootGrid != null)
            {
                _rootVisual = ElementCompositionPreview.GetElementVisual(_rootGrid);
                ElementCompositionPreview.SetElementChildVisual(_rootGrid, _outerVisual);
            }

            _nodesInitialized = true;
        }

        /// <summary>
        /// Updates the perspective transform of the parent RootGrid
        /// </summary>
        private void UpdateParentTransform()
        {
            if (_rootVisual == null)
                return;

            _projectionMatrix.M34 = -1 / (float)_depth;
            _rootVisual.TransformMatrix = Matrix4x4.CreateTranslation(-_center.X, -_center.Y, -_center.Z) *
                                          _projectionMatrix *
                                          Matrix4x4.CreateTranslation(_center.X, _center.Y, _center.Z);
        }

        /// <summary>
        /// Creates a node
        /// </summary>
        /// <param name="angleOffset">angular offset of the node</param>
        /// <returns>SpriteVisual</returns>
        private SpriteVisual CreateNode(float angleOffset)
        {
            var node = _compositor.CreateSpriteVisual();
            node.AnchorPoint = new Vector2(0.5f);
            node.Size = _nodeSize;
            node.Brush = _nodeBrush;
            node.CenterPoint = new Vector3(node.Size * 0.5f, 0);
            node.RotationAxis = new Vector3(1, 0, 0);
            node.RotationAngleInDegrees = 90;
            node.Properties.InsertScalar("angleOffset", angleOffset);

            return node;
        }

        /// <summary>
        /// Updates the Geometry and fill color of the nodes
        /// </summary>
        private void UpdateNodes()
        {
            CanvasGeometry nodeGeometry;
            switch (NodeShape)
            {
                case NodeShapeType.Square:
                    nodeGeometry = CanvasGeometry.CreateRectangle(_generator.Device, 0, 0, _nodeSize.X, _nodeSize.Y);
                    break;
                default:
                    nodeGeometry = CanvasGeometry.CreateCircle(_generator.Device, _nodeSize * 0.5f, _nodeSize.X / 2f);
                    break;
            }

            var nodeGeometrySurface = _generator.CreateGeometrySurface(_nodeSize.ToSize(), nodeGeometry, _nodeColor);
            _nodeBrush = _compositor.CreateSurfaceBrush(nodeGeometrySurface);
            foreach (var node in _nodes)
            {
                node.Brush = _nodeBrush;
            }
        }

        #endregion
    }
}
