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

using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit.Controls
{
    /// <summary>
    /// This class contains the definition for a new concept design
    /// for the ProgressRing control in UWP.
    /// </summary>
    public class FluidProgressRing : Control
    {
        #region Constants

        private const string AnimatedProperty = "RotationAngleInDegrees";

        #endregion

        #region Fields

        private readonly Compositor _compositor;
        private readonly ICompositionGenerator _generator;
        private IGeometrySurface _nodeMask;
        private ContainerVisual _container;

        private Vector2 _ringSize;
        private Vector2 _nodeSize;
        private float _ringRadius;
        private float _nodeRadius;
        private float _alpha;
        private float _beta;
        private float _gamma;
        private float _theta;
        private float _phi;
        private float _keyFrameSlice;
        private int _maxKeyFrames;
        private int _maxFrameBlocks;
        private int _maxFramesPerBlock;
        private bool _isAnimationStarted;

        private List<SpriteVisual> _nodes;
        private List<ScalarKeyFrameAnimation> _animations;
        private ScalarKeyFrameAnimation _containerAnimation;

        #endregion

        #region Dependency Properties

        #region MaxNodes

        /// <summary>
        /// MaxNodes Dependency Property
        /// </summary>
        public static readonly DependencyProperty MaxNodesProperty =
            DependencyProperty.Register("MaxNodes", typeof(int), typeof(FluidProgressRing),
                new PropertyMetadata(7, OnMaxNodesChanged));

        /// <summary>
        /// Gets or sets the MaxNodes property. This dependency property 
        /// indicates the maximum number of nodes that can be accommodated within
        /// the FluidProgressRing.
        /// </summary>
        public int MaxNodes
        {
            get { return (int)GetValue(MaxNodesProperty); }
            set { SetValue(MaxNodesProperty, value); }
        }

        /// <summary>
        /// Handles changes to the MaxNodes property.
        /// </summary>
        /// <param name="d">FluidProgressRing</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnMaxNodesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressRing = (FluidProgressRing)d;
            progressRing.OnMaxNodesChanged();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the MaxNodes property.
        /// </summary>
		void OnMaxNodesChanged()
        {
            // Refresh the layout
            InvalidateArrange();
        }

        #endregion

        #region ActiveNodes

        /// <summary>
        /// ActiveNodes Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActiveNodesProperty =
            DependencyProperty.Register("ActiveNodes", typeof(int), typeof(FluidProgressRing),
                new PropertyMetadata(6, OnActiveNodesChanged));

        /// <summary>
        /// Gets or sets the ActiveNodes property. This dependency property 
        /// indicates the number of stationary nodes in the FluidProgressRing.
        /// The FluidProgressRing will have an additional node which will be
        /// in motion. ActiveNodes should be less than or equal to MaxNodes.
        /// </summary>
        public int ActiveNodes
        {
            get { return (int)GetValue(ActiveNodesProperty); }
            set { SetValue(ActiveNodesProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ActiveNodes property.
        /// </summary>
        /// <param name="d">FluidProgressRing</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnActiveNodesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressRing = (FluidProgressRing)d;
            progressRing.OnActiveNodesChanged();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ActiveNodes property.
        /// </summary>
		void OnActiveNodesChanged()
        {
            // Refresh the layout
            InvalidateArrange();
        }

        #endregion

        #region NodeDuration

        /// <summary>
        /// NodeDuration Dependency Property
        /// </summary>
        public static readonly DependencyProperty NodeDurationProperty =
            DependencyProperty.Register("NodeDuration", typeof(TimeSpan), typeof(FluidProgressRing),
                new PropertyMetadata(TimeSpan.FromSeconds(0.5), OnNodeDurationChanged));

        /// <summary>
        /// Gets or sets the NodeDuration property. This dependency property 
        /// indicates the time it takes for a node to travel and hit the adjacent node.
        /// </summary>
        public TimeSpan NodeDuration
        {
            get { return (TimeSpan)GetValue(NodeDurationProperty); }
            set { SetValue(NodeDurationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the NodeDuration property.
        /// </summary>
        /// <param name="d">FluidProgressRing</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnNodeDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressRing = (FluidProgressRing)d;
            progressRing.OnNodeDurationChanged();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the NodeDuration property.
        /// </summary>
		void OnNodeDurationChanged()
        {
            // Refresh the layout
            InvalidateArrange();
        }

        #endregion

        #region RingDuration

        /// <summary>
        /// RingDuration Dependency Property
        /// </summary>
        public static readonly DependencyProperty RingDurationProperty =
            DependencyProperty.Register("RingDuration", typeof(TimeSpan), typeof(FluidProgressRing),
                new PropertyMetadata(TimeSpan.FromSeconds(5), OnRingDurationChanged));

        /// <summary>
        /// Gets or sets the RingDuration property. This dependency property 
        /// indicates the duration for one complete rotation of the FluidProgressRing.
        /// </summary>
        public TimeSpan RingDuration
        {
            get { return (TimeSpan)GetValue(RingDurationProperty); }
            set { SetValue(RingDurationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the RingDuration property.
        /// </summary>
        /// <param name="d">FluidProgressRing</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnRingDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressRing = (FluidProgressRing)d;
            progressRing.OnRingDurationChanged();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the RingDuration property.
        /// </summary>
		void OnRingDurationChanged()
        {
            // Refresh the layout
            InvalidateArrange();
        }

        #endregion

        #region NodeSizeFactor

        /// <summary>
        /// NodeSizeFactor Dependency Property
        /// </summary>
        public static readonly DependencyProperty NodeSizeFactorProperty =
            DependencyProperty.Register("NodeSizeFactor", typeof(double), typeof(FluidProgressRing),
                new PropertyMetadata(0.15, OnNodeSizeFactorChanged));

        /// <summary>
        /// Gets or sets the NodeSizeFactor property. This dependency property 
        /// indicates the ratio of the node diameter to the width of the FluidProgressControl.
        /// </summary>
        public double NodeSizeFactor
        {
            get { return (double)GetValue(NodeSizeFactorProperty); }
            set { SetValue(NodeSizeFactorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the NodeSizeFactor property.
        /// </summary>
        /// <param name="d">FluidProgressRing</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnNodeSizeFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressRing = (FluidProgressRing)d;
            progressRing.OnNodeSizeFactorChanged();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the NodeSizeFactor property.
        /// </summary>
		void OnNodeSizeFactorChanged()
        {
            // Refresh the layout
            InvalidateArrange();
        }

        #endregion

        #region NodeColor

        /// <summary>
        /// NodeColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty NodeColorProperty =
            DependencyProperty.Register("NodeColor", typeof(Color), typeof(FluidProgressRing),
                new PropertyMetadata(Colors.Blue, OnNodeColorChanged));

        /// <summary>
        /// Gets or sets the NodeColor property. This dependency property 
        /// indicates the color of the nodes.
        /// </summary>
        public Color NodeColor
        {
            get { return (Color)GetValue(NodeColorProperty); }
            set { SetValue(NodeColorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the NodeColor property.
        /// </summary>
        /// <param name="d">FluidProgressRing</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnNodeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressRing = (FluidProgressRing)d;
            progressRing.OnNodeColorChanged();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the NodeColor property.
        /// </summary>
		void OnNodeColorChanged()
        {
            // Refresh the layout
            InvalidateArrange();
        }

        #endregion

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        public FluidProgressRing()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _generator = _compositor.CreateCompositionGenerator();
            _isAnimationStarted = false;
            // Default Values
            Width = 70;
            Height = 70;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Handles the Arrange layout phase
        /// </summary>
        /// <param name="finalSize">Final Size of the control</param>
        /// <returns>Size</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if ((_compositor == null) || (_generator == null))
                return base.ArrangeOverride(finalSize);

            if (Double.IsInfinity(finalSize.Width) || Double.IsInfinity(finalSize.Height) ||
                Double.IsNaN(finalSize.Width) || Double.IsNaN(finalSize.Height))
                return base.ArrangeOverride(finalSize);

            // Stop the animations and dispose the previous nodes
            // and their animations
            if (_isAnimationStarted)
            {
                for (var i = 0; i < _nodes.Count; i++)
                {
                    _nodes[i].StopAnimation(AnimatedProperty);
                    _animations[i].Dispose();
                    _animations[i] = null;
                    _nodes[i].Dispose();
                    _nodes[i] = null;
                }

                _container.StopAnimation(AnimatedProperty);
                _containerAnimation.Dispose();
                _containerAnimation = null;
                _container.Dispose();
                _container = null;

                _animations.Clear();
                _nodes.Clear();

                _isAnimationStarted = false;
            }

            // Validate MaxNodes and ActiveNodes
            if ((MaxNodes <= 0) || (ActiveNodes <= 0))
                return finalSize;

            // Coerce ActiveNodes to MaxNodes if ActiveNodes > MaxNodes
            if (ActiveNodes > MaxNodes)
                ActiveNodes = MaxNodes;

            // Take the smallest of the width or height
            var sideLength = (float)Math.Min(finalSize.Width, finalSize.Height);
            // Size of the progress ring displayed
            _ringSize = new Vector2(sideLength, sideLength);
            var sideHalf = sideLength / 2f;
            // Radius of each node
            _nodeRadius = (float)NodeSizeFactor * sideHalf;
            // Size of each node
            _nodeSize = new Vector2(_nodeRadius * 2f, _nodeRadius * 2f);
            // Radius of the node
            _ringRadius = sideHalf - _nodeRadius;

            // Each animation will consist of '_maxFrameBlocks' number of 
            // FrameBlocks. Each FrameBlock will consist of keyframes which allow
            // the element being animated to move to the next slot and wait
            // at that slot until all other elements have moved to their next slot.
            // Each FrameBlock (except the last one) will have '_maxFramesPerBlock' 
            // number of keyframes. The last keyframe in the last FrameBlock
            // will always be (1f, "this.StartingValue + 360")

            // Total number of FrameBlocks in each animation
            _maxFrameBlocks = ActiveNodes;
            // Total keyframes in each FrameBlock
            _maxFramesPerBlock = ActiveNodes + 1;
            // Total keyframes in each animation
            _maxKeyFrames = _maxFrameBlocks * _maxFramesPerBlock;
            // Normalized Progress Key unit value for each keyframe
            _keyFrameSlice = 1f / _maxKeyFrames;

            // ========================================================================
            // NOTE:
            // gamma * maxNodes = 360
            // gamma = alpha + beta
            // beta = 2 * Asin(nodeRadius / ringRadius) * (180 / Math.PI)
            // invisibleNodes = MaxNodes - ActiveNodes
            // phi = (invisibleNodes * gamma)
            // theta = phi - beta
            // ========================================================================

            // gamma is the angle between adjacent nodes when maxNodes number of nodes are arranged in a circle
            _gamma = 360f / MaxNodes;
            // beta is the angle a node must travel after hitting the adjacent node
            _beta = 2f * (float)(Math.Asin(_nodeRadius / _ringRadius) * Scalar.RadiansToDegrees);
            // alpha is the smallest angle a node must travel before hitting the adjacent node
            _alpha = _gamma - _beta;
            // phi is the angle occupied by (MaxNodes - ActiveNodes) number of nodes
            _phi = (MaxNodes - ActiveNodes + 1) * _gamma;
            // theta is the largest angle a node must travel before hitting the adjacent node
            _theta = _phi - _beta;

            // Create the Animations
            _animations = CreateAnimations();

            // Create the Container
            _container = _compositor.CreateContainerVisual();
            _container.Size = _ringSize;
            _container.Offset = new Vector3(((float)finalSize.Width - sideLength) / 2f,
                                            ((float)finalSize.Height - sideLength) / 2f,
                                            0f);
            _container.CenterPoint = new Vector3(_ringSize.X / 2f, _ringSize.Y / 2f, 0f);

            // Create the Nodes
            var offset = new Vector3(_nodeRadius, _ringSize.Y / 2f, 0);
            var centerPoint = new Vector3(_ringSize.X / 2f - _nodeRadius, 0, 0);
            _nodes = new List<SpriteVisual>();
            var geometry = CanvasGeometry.CreateCircle(_generator.Device, new Vector2(_nodeRadius, _nodeRadius), _nodeRadius);
            // Create/Update the nodeMask
            var color = NodeColor;
            if (_nodeMask == null)
            {
                //Task.Run(async () =>
                //        {
                            _nodeMask = _generator.CreateGeometrySurface(_nodeSize.ToSize(), geometry, color);
                    //    })
                    //.Wait();
            }
            else
            {
                //Task.Run(async () =>
                //        {
                             _nodeMask.Redraw(_nodeSize.ToSize(), geometry, color);
                    //    })
                    //.Wait();
            }

            // Create the SurfaceBrush for the nodes
            var brush = _compositor.CreateSurfaceBrush(_nodeMask.Surface);

            var baseAngle = 0f;

            // Create the visuals for the nodes
            for (var i = 0; i < _maxFramesPerBlock; i++)
            {
                var node = _compositor.CreateSpriteVisual();
                node.Size = _nodeSize;
                node.AnchorPoint = new Vector2(0.5f);
                node.Offset = offset;
                node.CenterPoint = centerPoint;
                node.Brush = brush;
                node.RotationAngleInDegrees = baseAngle;
                if (i == 0)
                {
                    baseAngle += _phi;
                }
                else if (i == _maxFramesPerBlock - 2)
                {
                    baseAngle = -_beta;
                }
                else
                {
                    baseAngle += _gamma;
                }

                _nodes.Add(node);
                // Add the visual to the container
                _container.Children.InsertAtTop(node);
            }

            // Add the container to the Visual Tree
            ElementCompositionPreview.SetElementChildVisual(this, _container);

            // Start Node animations
            for (var i = 0; i < _maxFramesPerBlock; i++)
            {
                _nodes[i].StartAnimation(AnimatedProperty, _animations[i]);
            }

            // Start container animation
            _containerAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _containerAnimation.InsertExpressionKeyFrame(1f, "this.StartingValue + 360f", _compositor.CreateLinearEasingFunction());
            _containerAnimation.Duration = RingDuration;
            _containerAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            _container.StartAnimation(AnimatedProperty, _containerAnimation);

            _isAnimationStarted = true;

            return finalSize;
        }
        
        #endregion

        #region Helpers

        /// <summary>
        /// Creates the animations for each of the nodes
        /// </summary>
        /// <returns>List of ScalarKeyFrameAnimations</returns>
        private List<ScalarKeyFrameAnimation> CreateAnimations()
        {
            var result = new List<ScalarKeyFrameAnimation>();

            // Easing function
            var linear = _compositor.CreateLinearEasingFunction();

            // Each animation will consist of '_maxFrameBlocks' number of 
            // FrameBlocks. Each FrameBlock will consist of keyframes which allow
            // the element being animated to move to the next slot and wait
            // at that slot until all other elements have moved to their next slot.
            // Each FrameBlock (except the last one) will have '_maxFramesPerBlock' 
            // number of keyframes. The last keyframe in the last FrameBlock
            // will always be (1f, "this.StartingValue + 360")

            const string startingValue = "this.StartingValue";
            const string alpha = " + alpha";
            const string beta = " + beta";
            const string gamma = " + gamma";
            const string theta = " + theta";
            const string phi = " + phi";

            for (var animNumber = 0; animNumber < _maxFramesPerBlock; animNumber++)
            {
                var nodeAnimation = _compositor.CreateScalarKeyFrameAnimation();
                nodeAnimation.Duration = TimeSpan.FromMilliseconds(NodeDuration.TotalMilliseconds * _maxKeyFrames);
                nodeAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                var frameCount = 1;

                string sv;

                // First FrameBlock
                if (animNumber == 0)
                {
                    // First Keyframe in the FrameBlock the value will be 'startingValue + theta'
                    nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), $"{startingValue}{theta}", linear);

                    // For the rest of the frames in this FrameBlock the value will remain 'startingValue + phi'
                    sv = $"{startingValue}{phi}";
                    for (var i = 1; i < _maxFramesPerBlock; i++)
                    {
                        nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), sv, linear);
                    }
                }
                // Last FrameBlock
                else if (animNumber == _maxFramesPerBlock - 1)
                {
                    // First (animCount - 1) Keyframes in the FrameBlock will be 'startingValue + beta'
                    sv = $"{startingValue}{beta}";
                    for (var i = 0; i < animNumber; i++)
                    {
                        nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), sv, linear);
                    }

                    // For the frame number 'animCount', in this FrameBlock, the value will be 'sv + theta'
                    for (var i = animNumber; i < _maxFramesPerBlock; i++)
                    {
                        nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), $"{sv}{theta}", linear);
                    }
                }
                // Intermediate FrameBlocks
                else
                {
                    // First (animCount - 1) Keyframes in the FrameBlock will be 'startingValue'
                    sv = $"{startingValue}";
                    for (var i = 0; i < animNumber; i++)
                    {
                        nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), sv, linear);
                    }

                    // For the frame number 'animCount', in this FrameBlock, the value will be 'sv + alpha'
                    nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), $"{sv}{alpha}", linear);

                    // For the rest of the frames in this FrameBlock the value will remain 'sv + gamma'
                    sv = $"{sv}{gamma}";
                    for (var i = animNumber + 1; i < _maxFramesPerBlock; i++)
                    {
                        nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), sv, linear);
                    }
                }

                // Rest of the FrameBlocks
                for (var frameBlock = 1; frameBlock < _maxFrameBlocks; frameBlock++)
                {
                    // Calculate the number of frames to be added in this FrameBlock
                    // The last FrameBlock should have (_maxFramesPerBlock - 1) keyframes because we are adding 
                    // the last keyframe of this animation explicitly outside this loop 
                    var maxFrames = frameBlock < (_maxFrameBlocks - 1) ? _maxFramesPerBlock : _maxFramesPerBlock - 1;

                    // First FrameBlock
                    if (animNumber == 0)
                    {
                        // First Keyframe in the FrameBlock the value will be 'sv + alpha'
                        nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), $"{sv}{alpha}", linear);

                        // For the rest of the frames in this FrameBlock the value will remain 'sv + gamma'
                        sv = $"{sv}{gamma}";
                        for (var j = 1; j < maxFrames; j++)
                        {
                            nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), sv, linear);
                        }
                    }
                    // Last FrameBlock
                    else if (animNumber == _maxFramesPerBlock - 1)
                    {
                        sv = (frameBlock == 1) ? $"{sv}{phi}" : $"{sv}{gamma}";
                        // First (animCount - 1) Keyframes in the FrameBlock the value will be 'sv'
                        for (var i = 0; i < animNumber; i++)
                        {
                            nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), sv, linear);
                        }

                        // For the frame number 'animCount', in this FrameBlock, the value will be 'sv + alpha'
                        for (var i = animNumber; i < maxFrames; i++)
                        {
                            nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), $"{sv}{alpha}", linear);
                        }
                    }
                    // Intermediate FrameBlock
                    else
                    {
                        var isLargeAngle = (_maxFrameBlocks - animNumber) == frameBlock;

                        // First (animCount - 1) Keyframes in the FrameBlock the value will be 'sv'
                        for (var i = 0; i < animNumber; i++)
                        {
                            nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), sv, linear);
                        }

                        // For the frame number 'animCount', in this FrameBlock, the value will be 
                        // 'sv + theta' (if isLargeAngle == true) otherwise 'sv + alpha'
                        nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), isLargeAngle ? $"{sv}{theta}" : $"{sv}{alpha}", linear);

                        // For the rest of the keyframes in this FrameBlock, the value will be 
                        // 'sv + phi' (if isLargeAngle == true) otherwise 'sv + gamma'
                        sv = isLargeAngle ? $"{sv}{phi}" : $"{sv}{gamma}";
                        for (var i = animNumber + 1; i < maxFrames; i++)
                        {
                            nodeAnimation.InsertExpressionKeyFrame(_keyFrameSlice * (frameCount++), sv, linear);
                        }
                    }
                }

                // In the last keyframe the element being rotated should
                // return to its original position after making a full rotation
                // about its center.
                nodeAnimation.InsertExpressionKeyFrame(1f, "this.StartingValue + 360", linear);

                // Set the parameters
                nodeAnimation.SetScalarParameter("alpha", _alpha);
                nodeAnimation.SetScalarParameter("beta", _beta);
                nodeAnimation.SetScalarParameter("gamma", _gamma);
                nodeAnimation.SetScalarParameter("theta", _theta);
                nodeAnimation.SetScalarParameter("phi", _phi);

                result.Add(nodeAnimation);
            }

            return result;
        }

        #endregion
    }
}
