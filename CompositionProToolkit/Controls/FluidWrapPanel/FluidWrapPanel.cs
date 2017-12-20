// Copyright (c) 2017 Ratish Philip 
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
// CompositionProToolkit v0.7.0
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using CompositionProToolkit.Expressions;

namespace CompositionProToolkit.Controls
{
    /// <summary>
    /// A wrap panel which allows you to rearrange the children simply by 
    /// dragging and placing them in the desired location. The remaining 
    /// children will automatically reposition themselves to accommodate the 
    /// dragged item in the new location. The children can be instances of 
    /// any class which derives from UIElement (or its subclasses). 
    /// </summary>
    public sealed class FluidWrapPanel : Panel
    {
        #region Constants

        private const double NormalScale = 1.0;
        private const double DragScaleDefault = 1.2;
        private const double NormalOpacity = 1.0;
        private const double DragOpacityDefault = 0.7;
        private const double OpacityMin = 0.1d;
        private const double DefaultItemWidth = 10.0;
        private const double DefaultItemHeight = 10.0;
        private const int ZIndexNormal = 0;
        private const int ZIndexIntermediate = 1;
        private const int ZIndexDrag = 10;
        private static TimeSpan DefaultInitializationAnimationDuration = TimeSpan.FromMilliseconds(300);
        private static TimeSpan DefaultFluidAnimationDuration = TimeSpan.FromMilliseconds(570);
        private static TimeSpan DefaultOpacityAnimationDuration = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan DefaultScaleAnimationDuration = TimeSpan.FromMilliseconds(400);

        #endregion

        #region Structures

        /// <summary>
        /// Structure to store the bit-normalized dimensions
        /// of the FluidWrapPanel's children.
        /// </summary>
        private struct BitSize
        {
            internal int Width;
            internal int Height;
        }

        /// <summary>
        /// Structure to store the location and the bit-normalized
        /// dimensions of the FluidWrapPanel's children.
        /// </summary>
        private struct BitInfo
        {
            internal long Row;
            internal long Col;
            internal int Width;
            internal int Height;

            /// <summary>
            /// Checks if the bit-normalized width and height
            /// are equal to 1.
            /// </summary>
            /// <returns>True if yes otherwise False</returns>
            internal bool IsUnitSize()
            {
                return (Width == 1) && (Height == 1);
            }

            /// <summary>
            /// Checks if the given location is within the 
            /// bit-normalized bounds
            /// </summary>
            /// <param name="row">Row</param>
            /// <param name="col">Column</param>
            /// <returns>True if yes otherwise False</returns>
            internal bool Contains(long row, long col)
            {
                return (row >= Row) && (row < Row + Height) &&
                       (col >= Col) && (col < Col + Width);
            }
        }

        #endregion

        #region Fields

        private Point _dragStartPoint;
        private UIElement _dragElement;
        private bool _isOptimized;
        private Size _panelSize;
        private int _cellsPerLine;
        private UIElement _lastExchangedElement;
        private int _maxCellRows;
        private int _maxCellCols;
        private Dictionary<UIElement, BitInfo> _fluidBits;

        private Compositor _compositor;
        private Dictionary<UIElement, Visual> _fluidVisuals;
        private ImplicitAnimationCollection _implicitAnimationCollection;
        private ImplicitAnimationCollection _implicitDragAnimationCollection;
        private List<UIElement> _uninitializedFluidItems;

		#endregion

		#region Dependency Properties

		#region DefaultInitializationAnimationDuration

	    public static readonly DependencyProperty InitializationAnimationDurationProperty = DependencyProperty.Register(
		    "InitializationAnimationDuration", typeof(TimeSpan), typeof(FluidWrapPanel), new PropertyMetadata(DefaultInitializationAnimationDuration));

	    public TimeSpan InitializationAnimationDuration
		{
		    get { return (TimeSpan) GetValue(InitializationAnimationDurationProperty); }
		    set { SetValue(InitializationAnimationDurationProperty, value); }
	    }

		#endregion

		#region DefaultOpacityAnimationDuration

		public static readonly DependencyProperty OpacityAnimationDurationProperty = DependencyProperty.Register(
		    "OpacityAnimationDuration", typeof(TimeSpan), typeof(FluidWrapPanel), new PropertyMetadata(DefaultOpacityAnimationDuration));

	    public TimeSpan OpacityAnimationDuration
		{
		    get { return (TimeSpan) GetValue(OpacityAnimationDurationProperty); }
		    set { SetValue(OpacityAnimationDurationProperty, value); }
	    }

		#endregion

		#region FluidAnimationDuration

		public static readonly DependencyProperty FluidAnimationDurationProperty = DependencyProperty.Register(
		    "FluidAnimationDuration", typeof(TimeSpan), typeof(FluidWrapPanel), new PropertyMetadata(DefaultFluidAnimationDuration));

	    public TimeSpan FluidAnimationDuration
	    {
		    get { return (TimeSpan) GetValue(FluidAnimationDurationProperty); }
		    set { SetValue(FluidAnimationDurationProperty, value); }
	    }

	    #endregion

        #region DragOpacity

        /// <summary>
        /// DragOpacity Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragOpacityProperty =
            DependencyProperty.Register("DragOpacity", typeof(double), typeof(FluidWrapPanel),
                new PropertyMetadata(DragOpacityDefault));

        /// <summary>
        /// Gets or sets the DragOpacity property. This dependency property 
        /// indicates the opacity of the child being dragged.
        /// </summary>
        public double DragOpacity
        {
            get { return (double)GetValue(DragOpacityProperty); }
            set { SetValue(DragOpacityProperty, CoerceDragOpacity(value)); }
        }

        /// <summary>
        /// Coerces the FluidDrag Opacity to an acceptable value
        /// </summary>
        /// <param name="opacity">Value</param>
        /// <returns>Coerced Value</returns>
        private static double CoerceDragOpacity(double opacity)
        {
            if (opacity < OpacityMin)
            {
                opacity = OpacityMin;
            }
            else if (opacity > NormalOpacity)
            {
                opacity = NormalOpacity;
            }

            return opacity;
        }

        #endregion

        #region DragScale

        /// <summary>
        /// DragScale Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragScaleProperty =
            DependencyProperty.Register("DragScale", typeof(double), typeof(FluidWrapPanel),
                new PropertyMetadata(DragScaleDefault));

        /// <summary>
        /// Gets or sets the DragScale property. This dependency property 
        /// indicates the factor by which the child should be scaled when it is dragged.
        /// </summary>
        public double DragScale
        {
            get { return (double)GetValue(DragScaleProperty); }
            set { SetValue(DragScaleProperty, value); }
        }

        #endregion

        #region FluidItems

        /// <summary>
        /// FluidItems Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyProperty FluidItemsProperty =
            DependencyProperty.Register("FluidItems", typeof(ObservableCollection<UIElement>), typeof(FluidWrapPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the FluidItems property. This dependency property 
        /// indicates the observable list of FluidWrapPanel's children.
        /// NOTE: This property can be set internally only (or by a class deriving from this class)
        /// </summary>
        public ObservableCollection<UIElement> FluidItems
        {
            get { return (ObservableCollection<UIElement>)GetValue(FluidItemsProperty); }
            private set { SetValue(FluidItemsProperty, value); }
        }

        #endregion

        #region IsComposing

        /// <summary>
        /// IsComposing Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsComposingProperty =
            DependencyProperty.Register("IsComposing", typeof(bool), typeof(FluidWrapPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the IsComposing property. This dependency property 
        /// indicates if the FluidWrapPanel is in Composing mode.
        /// </summary>
        public bool IsComposing
        {
            get { return (bool)GetValue(IsComposingProperty); }
            set { SetValue(IsComposingProperty, value); }
        }

        #endregion

        #region ItemHeight

        /// <summary>
        /// ItemHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(FluidWrapPanel),
                new PropertyMetadata(DefaultItemHeight, OnItemHeightChanged));

        private static void OnItemHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as FluidWrapPanel;
            panel?.InvalidateMeasure();
        }

        /// <summary>
        /// Gets or sets the ItemHeight property. This dependency property 
        /// indicates the height of each item.
        /// </summary>
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, CoerceItemHeight(value)); }
        }

        /// <summary>
        /// Coerces the ItemHeight to a valid positive value
        /// </summary>
        /// <param name="height">Height</param>
        /// <returns>Coerced Value</returns>
        private static double CoerceItemHeight(double height)
        {
            return height < 0.0 ? 0.0 : height;
        }

        #endregion

        #region ItemWidth

        /// <summary>
        /// ItemWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(double), typeof(FluidWrapPanel),
                new PropertyMetadata(DefaultItemWidth, OnItemWidthChanged));

        private static void OnItemWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as FluidWrapPanel;
            panel?.InvalidateMeasure();
        }

        /// <summary>
        /// Gets or sets the ItemWidth property. This dependency property 
        /// indicates the width of each item.
        /// </summary>
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, CoerceItemWidth(value)); }
        }

        /// <summary>
        /// Coerces the ItemWidth to a valid positive value
        /// </summary>
        /// <param name="width">width</param>
        /// <returns>Coerced Value</returns>
        private static double CoerceItemWidth(double width)
        {
            return width < 0.0 ? 0.0 : width;
        }

        #endregion

        #region ItemsSource

        /// <summary>
        /// ItemsSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(FluidWrapPanel),
                new PropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        /// Gets or sets the ItemsSource property. This dependency property 
        /// indicates the bindable collection.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (ObservableCollection<UIElement>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ItemsSource property.
        /// </summary>
        /// <param name="d">FluidWrapPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (FluidWrapPanel)d;
            var newItemsSource = panel.ItemsSource;
            panel.OnItemsSourceChanged(newItemsSource);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemsSource property.
        /// </summary>
        /// <param name="newItemsSource">New Value</param>
        private void OnItemsSourceChanged(IEnumerable newItemsSource)
        {
            // Clear the previous items in the Children property
            ClearItemsSource();

            // Add the new children
            foreach (UIElement child in newItemsSource)
            {
                Children.Add(child);
            }

            // Refresh Layout
            InvalidateMeasure();
        }

        #endregion

        #region OptimizeChildPlacement

        /// <summary>
        /// OptimizeChildPlacement Dependency Property
        /// </summary>
        public static readonly DependencyProperty OptimizeChildPlacementProperty =
            DependencyProperty.Register("OptimizeChildPlacement", typeof(bool), typeof(FluidWrapPanel),
                new PropertyMetadata(true, OnOptimizeChildPlacementChanged));

        private static void OnOptimizeChildPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as FluidWrapPanel;
            panel?.InvalidateMeasure();
        }

        /// <summary>
        /// Gets or sets the OptimizeChildPlacement property. This dependency property 
        /// indicates whether the placement of the children is optimized. 
        /// If set to true, the child is placed at the first available position from 
        /// the beginning of the FluidWrapPanel. 
        /// If set to false, each child occupies the same (or greater) row and/or column
        /// than the previous child.
        /// </summary>
        public bool OptimizeChildPlacement
        {
            get { return (bool)GetValue(OptimizeChildPlacementProperty); }
            set { SetValue(OptimizeChildPlacementProperty, value); }
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Orientation Dependency Property
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(FluidWrapPanel),
                new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        /// <summary>
        /// Gets or sets the Orientation property. This dependency property 
        /// indicates the orientation of arrangement of items in the panel.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Orientation property.
        /// </summary>
        /// <param name="d">FluidWrapPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (FluidWrapPanel)d;
            panel.OnOrientationChanged();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Orientation property.
        /// </summary>
        private void OnOrientationChanged()
        {
            // Refresh the layout
            InvalidateMeasure();
        }

        #endregion

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Default Ctor
        /// </summary>
        public FluidWrapPanel()
        {
            FluidItems = new ObservableCollection<UIElement>();
            _fluidBits = new Dictionary<UIElement, BitInfo>();

            _fluidVisuals = new Dictionary<UIElement, Visual>();
            _uninitializedFluidItems = new List<UIElement>();
            _lastExchangedElement = null;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Adds a new UIElement to the existing FluidWrapPanel Children
        /// </summary>
        /// <param name="child">UIElement to add</param>
        /// <returns>Task</returns>
        public async Task AddChildAsync(FrameworkElement child)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Add the child
                Children.Add(child);
                // Refresh the layout
                InvalidateMeasure();
            });
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Handles the Measure pass during Layout
        /// </summary>
        /// <param name="availableSize">Available size</param>
        /// <returns>Total Size required to accommodate all the Children</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // Compositor will be null the very first time
            if (_compositor == null)
            {
                InitializeComposition();
            }

            // Clear any previously uninitialized items
            _uninitializedFluidItems.Clear();

            var availableItemSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

            // Iterate through all the UIElements in the Children collection
            foreach (var child in Children.Where(c => c != null))
            {
                // Ask the child how much size it needs
                child.Measure(availableItemSize);
                // Check if the child is already added to the fluidElements collection
                if (FluidItems.Contains(child))
                    continue;

                // If the FluidItems collection does not contain this child it means it is newly
                // added to the FluidWrapPanel and is not initialized yet
                // Add the child to the fluidElements collection
                FluidItems.Add(child);
                // Add the child to the UninitializedFluidItems
                _uninitializedFluidItems.Add(child);
                // Get the visual of the child
                var visual = ElementCompositionPreview.GetElementVisual(child);

                visual.ImplicitAnimations = _implicitAnimationCollection;
                visual.CenterPoint = new Vector3((float)(child.DesiredSize.Width / 2), (float)(child.DesiredSize.Height / 2), 0);
                visual.Offset = new Vector3((float)-child.DesiredSize.Width, (float)-child.DesiredSize.Height, 0);
                _fluidVisuals[child] = visual;
            }

            // Unit size of a cell
            var cellSize = new Size(ItemWidth, ItemHeight);

            if ((availableSize.Width < 0.0d) || (availableSize.Width.IsZero())
                || (availableSize.Height < 0.0d) || (availableSize.Height.IsZero())
                || !FluidItems.Any())
            {
                return cellSize;
            }

            // Calculate how many unit cells can fit in the given width (or height) when the 
            // Orientation is Horizontal (or Vertical)
            _cellsPerLine = CalculateCellsPerLine(availableSize, cellSize, Orientation);

            // Convert the children's dimensions from Size to BitSize
            var childData = FluidItems.Select(child => new BitSize
            {
                Width = Math.Max(1, (int)Math.Floor((child.DesiredSize.Width / cellSize.Width) + 0.5)),
                Height = Math.Max(1, (int)Math.Floor((child.DesiredSize.Height / cellSize.Height) + 0.5))
            }).ToList();

            // If all the children have the same size as the cellSize then use optimized code
            // when a child is being dragged
            _isOptimized = !childData.Any(c => (c.Width != 1) || (c.Height != 1));

            int matrixWidth;
            int matrixHeight;
            if (Orientation == Orientation.Horizontal)
            {
                // If the maximum width required by a child is more than the calculated cellsPerLine, then
                // the matrix width should be the maximum width of that child
                matrixWidth = Math.Max(childData.Max(s => s.Width), _cellsPerLine);
                // For purpose of calculating the true size of the panel, the height of the matrix must
                // be set to the cumulative height of all the children
                matrixHeight = childData.Sum(s => s.Height);
            }
            else
            {
                // For purpose of calculating the true size of the panel, the width of the matrix must
                // be set to the cumulative width of all the children
                matrixWidth = childData.Sum(s => s.Width);
                // If the maximum height required by a child is more than the calculated cellsPerLine, then
                // the matrix height should be the maximum height of that child
                matrixHeight = Math.Max(childData.Max(s => s.Height), _cellsPerLine);
            }

            // Create FluidBitMatrix to calculate the size required by the panel
            var matrix = new FluidBitMatrix(matrixHeight, matrixWidth, Orientation);

            var startIndex = 0L;

            foreach (var child in childData)
            {
                var width = child.Width;
                var height = child.Height;

                MatrixCell cell;
                if (matrix.TryFindRegion(startIndex, width, height, out cell))
                {
                    matrix.SetRegion(cell, width, height);
                }
                else
                {
                    // If code reached here, it means that the child is too big to be accommodated
                    // in the matrix. Normally this should not occur!
                    throw new InvalidOperationException("Measure Pass: Unable to accommodate child in the panel!");
                }

                if (!OptimizeChildPlacement)
                {
                    // Update the startIndex so that the next child occupies a location which has 
                    // the same (or greater) row and/or column as this child
                    startIndex = (Orientation == Orientation.Horizontal) ? cell.Row : cell.Col;
                }
            }

            // Calculate the true size of the matrix
            var matrixSize = matrix.GetFilledMatrixDimensions();
            // Calculate the size required by the panel
            return new Size(matrixSize.Width * cellSize.Width, matrixSize.Height * cellSize.Height);
        }

        /// <summary>
        /// Handles the Arrange pass during Layout
        /// </summary>
        /// <param name="finalSize">Final Size of the control</param>
        /// <returns>Total size occupied by all the Children</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var cellSize = new Size(ItemWidth, ItemHeight);

            if ((finalSize.Width < 0.0d) || (finalSize.Width.IsZero())
                || (finalSize.Height < 0.0d) || (finalSize.Height.IsZero()))
            {
                finalSize = cellSize;
            }

            // Final size of the FluidWrapPanel
            _panelSize = finalSize;

            if (!FluidItems.Any())
            {
                return finalSize;
            }

            // Create the animation for the uinitialized children
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Duration = DefaultInitializationAnimationDuration;

            // Calculate how many unit cells can fit in the given width (or height) when the 
            // Orientation is Horizontal (or Vertical)
            _cellsPerLine = CalculateCellsPerLine(finalSize, cellSize, Orientation);
            // Convert the children's dimensions from Size to BitSize
            var childData = FluidItems.ToDictionary(child => child, child => new BitSize
            {
                Width = Math.Max(1, (int)Math.Floor((child.DesiredSize.Width / cellSize.Width) + 0.5)),
                Height = Math.Max(1, (int)Math.Floor((child.DesiredSize.Height / cellSize.Height) + 0.5))
            });

            // If all the children have the same size as the cellSize then use optimized code
            // when a child is being dragged
            _isOptimized = !childData.Values.Any(c => (c.Width != 1) || (c.Height != 1));

            // Calculate matrix dimensions
            int matrixWidth;
            int matrixHeight;
            if (Orientation == Orientation.Horizontal)
            {
                // If the maximum width required by a child is more than the calculated cellsPerLine, then
                // the matrix width should be the maximum width of that child
                matrixWidth = Math.Max(childData.Values.Max(s => s.Width), _cellsPerLine);
                // For purpose of calculating the true size of the panel, the height of the matrix must
                // be set to the cumulative height of all the children
                matrixHeight = childData.Values.Sum(s => s.Height);
            }
            else
            {
                // For purpose of calculating the true size of the panel, the width of the matrix must
                // be set to the cumulative width of all the children
                matrixWidth = childData.Values.Sum(s => s.Width);
                // If the maximum height required by a child is more than the calculated cellsPerLine, then
                // the matrix height should be the maximum height of that child
                matrixHeight = Math.Max(childData.Values.Max(s => s.Height), _cellsPerLine);
            }

            // Create FluidBitMatrix to calculate the size required by the panel
            var matrix = new FluidBitMatrix(matrixHeight, matrixWidth, Orientation);

            var startIndex = 0L;
            _fluidBits.Clear();

            foreach (var child in childData)
            {
                var width = child.Value.Width;
                var height = child.Value.Height;

                MatrixCell cell;
                if (matrix.TryFindRegion(startIndex, width, height, out cell))
                {
                    // Set the bits
                    matrix.SetRegion(cell, width, height);
                    // Arrange the child
                    child.Key.Arrange(new Rect(0, 0, child.Key.DesiredSize.Width, child.Key.DesiredSize.Height));
                    // Convert MatrixCell location to actual location
                    var pos = new Vector3((float)(cell.Col * cellSize.Width), (float)(cell.Row * cellSize.Height), 0);
                    // Get the Bit Information for this child
                    BitInfo info;
                    info.Row = cell.Row;
                    info.Col = cell.Col;
                    info.Width = width;
                    info.Height = height;
                    _fluidBits.Add(child.Key, info);

                    // If this child is not being dragged, then set the Offset of its visual
                    if (!ReferenceEquals(child.Key, _dragElement))
                    {
                        var visual = _fluidVisuals[child.Key];
                        // Is the child unitialized?
                        if (_uninitializedFluidItems.Contains(child.Key))
                        {
                            // Use explicit animation to position the uninitialized child to the new location
                            // because implicit property animations do not run the first time 
                            // a Visual shows up on screen
                            offsetAnimation.InsertKeyFrame(1f, pos);
                            visual.StartAnimation(() => visual.Offset, offsetAnimation);
                        }
                        else
                        {
                            // Child has been already initialized. Set the Offset directly. The ImplicitAnimations
                            // of the Child's visual will take care of animating it to the new location
                            _fluidVisuals[child.Key].Offset = pos;
                        }
                    }
                }
                else
                {
                    // If code reached here, it means that the child is too big to be accommodated
                    // in the matrix. Normally this should not occur!
                    throw new InvalidOperationException("Arrange Pass: Unable to accommodate child in the panel!");
                }

                if (!OptimizeChildPlacement)
                {
                    // Update the startIndex so that the next child occupies a location which has 
                    // the same (or greater) row and/or column as this child
                    startIndex = (Orientation == Orientation.Horizontal) ? cell.Row : cell.Col;
                }
            }

            // All the uninitialized fluid items have been initialized, so clear the list
            _uninitializedFluidItems.Clear();

            // Calculate the maximum cells along the width and height of the FluidWrapPanel
            _maxCellRows = (int)Math.Max(1, Math.Floor(_panelSize.Height / ItemHeight));
            _maxCellCols = (int)Math.Max(1, Math.Floor(_panelSize.Width / ItemWidth));

            return finalSize;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Initialize all Composition related stuff here (Compositor, Animations etc)
        /// </summary>
        private void InitializeComposition()
        {
            var rootVisual = ElementCompositionPreview.GetElementVisual(this);
            // Compositor
            _compositor = rootVisual.Compositor;

            // Final Value Expressions
            var vector3Expr = _compositor.CreateFinalValueExpression<Vector3>();
            var scalarExpr = _compositor.CreateFinalValueExpression<float>();

            // Offset Animation
            var offsetAnimation = _compositor.CreateKeyFrameAnimation<Vector3>()
                                             .HavingDuration(FluidAnimationDuration)
                                             .ForTarget(() => rootVisual.Offset);
            offsetAnimation.InsertExpressionKeyFrame(1f, vector3Expr);

            // Opacity Animation
            var opacityAnimation = _compositor.CreateKeyFrameAnimation<float>()
                                              .HavingDuration(DefaultOpacityAnimationDuration)
                                              .ForTarget(() => rootVisual.Opacity);
            opacityAnimation.InsertExpressionKeyFrame(1f, scalarExpr);

            // Scale Animation
            var scaleAnimation = _compositor.CreateKeyFrameAnimation<Vector3>()
                                            .HavingDuration(FluidAnimationDuration)
                                            .ForTarget(() => rootVisual.Scale);
            scaleAnimation.InsertExpressionKeyFrame(1f, vector3Expr);

            // ImplicitAnimation
            _implicitAnimationCollection = _compositor.CreateImplicitAnimationCollection();
            _implicitAnimationCollection["Offset"] = offsetAnimation.Animation;
            _implicitAnimationCollection["Opacity"] = opacityAnimation.Animation;
            _implicitAnimationCollection["Scale"] = scaleAnimation.Animation;

            // ImplicitDragAnimation
            _implicitDragAnimationCollection = _compositor.CreateImplicitAnimationCollection();
            _implicitDragAnimationCollection["Opacity"] = opacityAnimation.Animation;
            _implicitDragAnimationCollection["Scale"] = scaleAnimation.Animation;
        }

        /// <summary>
        /// Removes all the children from the FluidWrapPanel
        /// </summary>
        private void ClearItemsSource()
        {
            FluidItems.Clear();
            Children.Clear();
        }

        /// <summary>
        /// Provides the index of the child corresponding to the given point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Index of the child</returns>
        private int GetIndexFromPoint(Point point)
        {
            if ((point.X < 0.00D) || (point.X > _panelSize.Width) ||
                (point.Y < 0.00D) || (point.Y > _panelSize.Height) ||
                !FluidItems.Any())
                return -1;

            // Get the row and column of the cell corresponding 
            // to this location
            var row = (int)(point.Y / ItemHeight);
            var column = (int)(point.X / ItemWidth);

            // Get the index for the cell based on Orientation
            var result = -1;
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    result = (_cellsPerLine * row) + column;
                    break;
                case Orientation.Vertical:
                    result = (_cellsPerLine * column) + row;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets the list of children overlapped by the given element when it is
        /// moved to the given cell location.
        /// </summary>
        /// <param name="element">UIElement</param>
        /// <param name="cell">Cell location</param>
        /// <returns>List of overlapped UIElements</returns>
        private List<UIElement> GetOverlappedChildren(UIElement element, MatrixCell cell)
        {
            var result = new List<UIElement>();
            var info = _fluidBits[element];

            for (var row = 0; row < info.Height; row++)
            {
                for (var col = 0; col < info.Width; col++)
                {
                    var item = _fluidBits.Where(t => t.Value.Contains(cell.Row + row, cell.Col + col)).Select(t => t.Key).FirstOrDefault();
                    if ((item != null) && !ReferenceEquals(item, element) && (!result.Contains(item)))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the list of cell locations which are vacated when the given element is
        /// moved to the given cell location.
        /// </summary>
        /// <param name="element">UIElement</param>
        /// <param name="cell">Cell location</param>
        /// <returns>List of cell locations</returns>
        private List<MatrixCell> GetVacatedCells(UIElement element, MatrixCell cell)
        {
            var result = new List<MatrixCell>();

            var info = _fluidBits[element];
            var baseRow = info.Row;
            var baseCol = info.Col;
            var width = info.Width;
            var height = info.Height;

            var minRow = cell.Row;
            var maxRow = minRow + height;
            var minCol = cell.Col;
            var maxCol = minCol + width;

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var row = baseRow + i;
                    var col = baseCol + j;

                    var isInside = (row >= minRow) && (row < maxRow) &&
                                   (col >= minCol) && (col < maxCol);

                    if (!isInside)
                        result.Add(new MatrixCell(row, col));
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the given UIElement can fit in the given cell location
        /// </summary>
        /// <param name="element">UIElement</param>
        /// <param name="cell">Cell location</param>
        /// <returns>True if the UIElement fits otherwise False.</returns>
        private bool IsValidCellPosition(UIElement element, MatrixCell cell)
        {
            if (!cell.IsValid())
                return false;

            var info = _fluidBits[element];

            return (cell.Row + info.Height <= _maxCellRows) &&
                   (cell.Col + info.Width <= _maxCellCols);
        }

        /// <summary>
        /// Gets the top left cell location corresponding to the given position
        /// </summary>
        /// <param name="info">Bit Information</param>
        /// <param name="position">Position where the user clicked w.r.t. the UIElement being dragged</param>
        /// <param name="positionInParent">Position where the user clicked w.r.t. the FluidWrapPanel</param>
        /// <returns></returns>
        private MatrixCell GetCellFromPoint(BitInfo info, Point position, Point positionInParent)
        {
            var row = (int)(positionInParent.Y / ItemHeight);
            var col = (int)(positionInParent.X / ItemWidth);

            // If the item is not having unit size, then calculate the top left cell location
            if (!info.IsUnitSize())
            {
                row -= (int)(position.Y / ItemHeight);
                col -= (int)(position.X / ItemWidth);
            }

            // Bounds check
            if ((row < 0) ||
                (row > _maxCellRows) ||
                (col < 0) ||
                (col > _maxCellCols))
            {
                return MatrixCell.InvalidCell;
            }

            return new MatrixCell(row, col);
        }

        #endregion

        #region FluidDrag Helpers

        /// <summary>
        /// Handler for the event when the user starts dragging the dragElement.
        /// </summary>
        /// <param name="child">UIElement being dragged</param>
        /// <param name="position">Position in the child where the user clicked</param>
        /// <param name="pointer">Pointer</param>
        internal void BeginFluidDrag(UIElement child, Point position, Pointer pointer)
        {
            if ((child == null) || (!IsComposing))
                return;

			
	        // Capture further pointer events
	        child.CapturePointer(pointer);
	        UIElement childOfPanel = findDirectChildOfPanel(child);
			
	        _dragElement = childOfPanel;
	        _dragElement.SetValue(Canvas.ZIndexProperty, ZIndexDrag);

	        var positionInDragElement = child.TransformToVisual(_dragElement).TransformPoint(position);


			var visual = _fluidVisuals[_dragElement];
            visual.Opacity = (float)DragOpacity;
            visual.CenterPoint = new Vector3((float)positionInDragElement.X, (float)positionInDragElement.Y, 0);
            visual.Scale = new Vector3((float)DragScale, (float)DragScale, 1);
            visual.ImplicitAnimations = _implicitDragAnimationCollection;

            // Set the starting position of the drag
            _dragStartPoint = new Point(positionInDragElement.X, positionInDragElement.Y);
        }

	    private UIElement findDirectChildOfPanel(UIElement child)
	    {
		    UIElement directChild = null;
		    FrameworkElement ancestor = child as FrameworkElement;
			while (ancestor != null)
			{
				//if (ancestor is ListBoxItem)
				//{
				//	_parentLbItem = ancestor as ListBoxItem;
				//}

				if (ancestor is FluidWrapPanel)
				{
					// No need to go further up
					return directChild;
				}

				directChild = ancestor;

				// Find the visual ancestor of the current item
				ancestor = VisualTreeHelper.GetParent(ancestor) as FrameworkElement;
			}

		    return directChild;
	    }

	    /// <summary>
        /// Handler for the event when the user drags the dragElement.
        /// </summary>
        /// <param name="child">UIElement being dragged</param>
        /// <param name="position">Position where the user clicked w.r.t. the UIElement being dragged</param>
        /// <param name="positionInParent">Position where the user clicked w.r.t. the FluidWrapPanel</param>
        internal void OnFluidDrag(UIElement child, Point position, Point positionInParent)
        {
            if ((child == null) || (!IsComposing) || (_dragElement == null))
                return;


            // Set the offset of the _dragElement's visual
            var visual = _fluidVisuals[_dragElement];
            visual.Offset = new Vector3((float)(positionInParent.X - _dragStartPoint.X),
                                        (float)(positionInParent.Y - _dragStartPoint.Y),
                                        0);

            // Are all the children are of unit cell size?
            if (_isOptimized)
            {
                // Get the index of the dragElement
                var dragCellIndex = FluidItems.IndexOf(_dragElement);

                // Get the index in the fluidElements list corresponding to the current mouse location
                var index = GetIndexFromPoint(positionInParent);

                // If no valid cell index is obtained (happens when the dragElement is dragged outside
                // the FluidWrapPanel), add the child to the end of the fluidElements list.
                if ((index == -1) || (index >= FluidItems.Count))
                {
                    index = FluidItems.Count - 1;
                }

                // If both indices are same no need to process further
                if (index == dragCellIndex)
                    return;

                // Move the dragElement to the new index
                FluidItems.RemoveAt(dragCellIndex);
                FluidItems.Insert(index, _dragElement);

                // Refresh the FluidWrapPanel
                InvalidateArrange();
            }
            // Children are not having unit cell size
            else
            {
                // Refresh the FluidWrapPanel only if the dragElement
                // can be successfully placed in the new location
                if (TryFluidDrag(position, positionInParent))
                {
                    InvalidateArrange();
                }
            }
        }

        /// <summary>
        /// Handles the situation when the user drags a dragElement which does not have 
        /// unit size dimension. It checks if the dragElement can fit in the new location and
        /// the rest of the children can be rearranged successfully in the remaining space.
        /// </summary>
        /// <param name="position">Position of the pointer within the dragElement</param>
        /// <param name="positionInParent">Position of the pointer w.r.t. the FluidWrapPanel</param>
        /// <returns>True if successful otherwise False</returns>
        private bool TryFluidDrag(Point position, Point positionInParent)
        {
            // Get the index of the dragElement
            var dragCellIndex = FluidItems.IndexOf(_dragElement);

            // Convert the current location to MatrixCell which indicates the top left cell of the dragElement
            var currentCell = GetCellFromPoint(_fluidBits[_dragElement], position, positionInParent);

            // Check if the item being dragged can fit in the new cell location
            if (!IsValidCellPosition(_dragElement, currentCell))
                return false;

            // Get the list of cells vacated when the dragElement moves to the new cell location
            var vacatedCells = GetVacatedCells(_dragElement, currentCell);
            // If none of the cells are vacated, no need to proceed further
            if (vacatedCells.Count == 0)
            {
                _lastExchangedElement = null;
                return false;
            }

            // Get the list of children overlapped by the 
            var overlappedChildren = GetOverlappedChildren(_dragElement, currentCell);
            var dragInfo = _fluidBits[_dragElement];
            // If there is only one overlapped child and its dimension matches the 
            // dimensions of the dragElement, then exchange their indices
            if (overlappedChildren.Count == 1)
            {
                var element = overlappedChildren[0];
                var info = _fluidBits[element];
                var dragCellCount = info.Width * info.Height;
                if ((info.Width == dragInfo.Width) && (info.Height == dragInfo.Height))
                {
                    // If user moves the dragElement back to the lastExchangedElement's position, then it can
                    // be exchanged again only if the dragElement has vacated all the cells occupied by it in 
                    // the previous location.
                    if (ReferenceEquals(element, _lastExchangedElement) && (vacatedCells.Count != dragCellCount))
                    {
                        return false;
                    }

                    // Exchange the dragElement and the overlapped element
                    _lastExchangedElement = element;
                    var index = FluidItems.IndexOf(element);
                    // To prevent an IndexOutOfRangeException during the exchange
                    // Remove the item with higher index first followed by the lower index item and then
                    // Insert the items in the lower index first and then in the higher index
                    if (index > dragCellIndex)
                    {
                        FluidItems.RemoveAt(index);
                        FluidItems.RemoveAt(dragCellIndex);
                        FluidItems.Insert(dragCellIndex, element);
                        FluidItems.Insert(index, _dragElement);
                    }
                    else
                    {
                        FluidItems.RemoveAt(dragCellIndex);
                        FluidItems.RemoveAt(index);
                        FluidItems.Insert(index, _dragElement);
                        FluidItems.Insert(dragCellIndex, element);
                    }

                    return true;
                }
            }

            // Since there are multiple overlapped children, we need to rearrange all the children
            // Create a temporary matrix to check if all the children are placed successfully
            // when the dragElement is moved to the new cell location
            var tempMatrix = new FluidBitMatrix(_maxCellRows, _maxCellCols, Orientation);

            // First set the cells corresponding to dragElement's cells in new location
            tempMatrix.SetRegion(currentCell, dragInfo.Width, dragInfo.Height);
            // Try to fit the remaining items
            var startIndex = 0L;
            var tempFluidBits = new Dictionary<UIElement, BitInfo>();
            // Add the new bit information for dragElement
            dragInfo.Row = currentCell.Row;
            dragInfo.Col = currentCell.Col;
            tempFluidBits[_dragElement] = dragInfo;

            // Try placing the rest of the children in the matrix
            foreach (var item in _fluidBits.Where(t => !ReferenceEquals(t.Key, _dragElement)))
            {
                var width = item.Value.Width;
                var height = item.Value.Height;

                MatrixCell cell;
                if (tempMatrix.TryFindRegion(startIndex, width, height, out cell))
                {
                    // Set the bits
                    tempMatrix.SetRegion(cell, width, height);
                    // Capture the bit information
                    BitInfo newinfo;
                    newinfo.Row = cell.Row;
                    newinfo.Col = cell.Col;
                    newinfo.Width = width;
                    newinfo.Height = height;
                    tempFluidBits.Add(item.Key, newinfo);
                }
                else
                {
                    // No suitable location was found to fit the current item. So the children cannot be
                    // successfully placed after moving dragElement to new cell location. So dragElement
                    // will not be moved.
                    return false;
                }

                // Update the startIndex so that the next child occupies a location the same (or greater)
                // row and/or column as this child
                if (!OptimizeChildPlacement)
                {
                    startIndex = (Orientation == Orientation.Horizontal) ? cell.Row : cell.Col;
                }
            }

            // All the children have been successfully readjusted, so now 
            // Re-Index the children based on the panel's orientation
            var tempFluidItems = new List<UIElement>();
            if (Orientation == Orientation.Horizontal)
            {
                for (var row = 0; row < _maxCellRows; row++)
                {
                    for (var col = 0; col < _maxCellCols; col++)
                    {
                        var item = tempFluidBits.Where(t => t.Value.Contains(row, col))
                                                .Select(t => t.Key).FirstOrDefault();
                        if ((item != null) && (!tempFluidItems.Contains(item)))
                        {
                            tempFluidItems.Add(item);
                        }
                    }
                }
            }
            else
            {
                for (var col = 0; col < _maxCellCols; col++)
                {
                    for (var row = 0; row < _maxCellRows; row++)
                    {
                        var item = tempFluidBits.Where(t => t.Value.Contains(row, col))
                                                .Select(t => t.Key).FirstOrDefault();
                        if ((item != null) && (!tempFluidItems.Contains(item)))
                        {
                            tempFluidItems.Add(item);
                        }
                    }
                }
            }

            // Update the new indices in FluidItems
            FluidItems.Clear();
            foreach (var fluidItem in tempFluidItems)
            {
                FluidItems.Add(fluidItem);
            }

            // Clean up
            tempFluidItems.Clear();
            tempFluidBits.Clear();

            return true;
        }

        /// <summary>
        /// Handler for the event when the user stops dragging the dragElement and releases it.
        /// </summary>
        /// <param name="child">UIElement being dragged</param>
        /// <param name="position">Position where the user clicked w.r.t. the UIElement being dragged</param>
        /// <param name="positionInParent">Position where the user clicked w.r.t. the FluidWrapPanel</param>
        /// <param name="pointer">Pointer</param>
        internal void EndFluidDrag(UIElement child, Point position, Point positionInParent, Pointer pointer)
        {
            if ((child == null) || (!IsComposing) || (_dragElement == null))
                return;

            // Set the offset of the _dragElement's visual
            var visual = _fluidVisuals[_dragElement];
            visual.ImplicitAnimations = _implicitAnimationCollection;
            visual.Opacity = 1f;
            visual.Scale = Vector3.One;
            visual.Offset = new Vector3((float)(positionInParent.X - _dragStartPoint.X),
                                                            (float)(positionInParent.Y - _dragStartPoint.Y),
                                                            0);
            //visual.CenterPoint = new Vector3((float)(ItemWidth / 2), (float)(ItemHeight / 2), 0);

            // Z-Index is set to 1 so that during the animation it does not go below other elements.
            _dragElement.SetValue(Canvas.ZIndexProperty, ZIndexIntermediate);
            // Release the pointer capture
            child.ReleasePointerCapture(pointer);

            _dragElement = null;
            _lastExchangedElement = null;

            InvalidateArrange();
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Calculates the number of child items that can be accommodated in a single line
        /// </summary>
        private static int CalculateCellsPerLine(Size panelSize, Size cellSize, Orientation panelOrientation)
        {
            var count = (panelOrientation == Orientation.Horizontal) ? panelSize.Width / cellSize.Width :
                panelSize.Height / cellSize.Height;
            return Math.Max(1, (int)Math.Floor(count));
        }

        #endregion
    }
}
