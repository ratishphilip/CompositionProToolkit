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

using Microsoft.Xaml.Interactivity;
using System;
using System.Linq;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace CompositionProToolkit.Controls
{
    /// <summary>
    /// Defines the Drag Behavior in the FluidWrapPanel using the Mouse/Touch/Pen
    /// </summary>
    public class FluidPointerDragBehavior : Behavior<UIElement>
    {
        #region Enums

        /// <summary>
        /// The various types of Pointers that can participate in FluidDrag
        /// </summary>
        [Flags]
        public enum DragButtonType
        {
            /// <summary>
            /// Mouse Left Button
            /// </summary>
            MouseLeftButton = 1,
            /// <summary>
            /// Mouse Middle Button
            /// </summary>
            MouseMiddleButton = 2,
            /// <summary>
            /// Mouse Right Button
            /// </summary>
            MouseRightButton = 4,
            /// <summary>
            /// Pen
            /// </summary>
            Pen = 8,
            /// <summary>
            /// Touch
            /// </summary>
            Touch = 16
        }

        #endregion

        #region Fields

        private FluidWrapPanel _parentFwPanel;

        #endregion

        #region Dependency Properties

        #region DragButton

        /// <summary>
        /// DragButton Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragButtonProperty =
            DependencyProperty.Register("DragButton", typeof(DragButtonType), typeof(FluidPointerDragBehavior),
                new PropertyMetadata(DragButtonType.MouseLeftButton));

        /// <summary>
        /// Gets or sets the DragButton property. This dependency property 
        /// indicates which Mouse button should participate in the drag interaction.
        /// </summary>
        public DragButtonType DragButton
        {
            get => (DragButtonType)GetValue(DragButtonProperty);
            set => SetValue(DragButtonProperty, value);
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>
        /// When the Behavior is attached to the UIElement
        /// </summary>
        protected override void OnAttached()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;

            // Subscribe to the Loaded event
            ((FrameworkElement)AssociatedObject).Loaded += OnAssociatedObjectLoaded;
        }

        /// <summary>
        /// When the Behavior is detached from the UIElement
        /// </summary>
        protected override void OnDetaching()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;

            ((FrameworkElement)AssociatedObject).Loaded -= OnAssociatedObjectLoaded;

            AssociatedObject.PointerPressed -= OnPointerPressed;
            AssociatedObject.PointerMoved -= OnPointerMoved;
            AssociatedObject.PointerReleased -= OnPointerReleased;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for the Load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            // Get the parent FluidWrapPanel 
            _parentFwPanel = AssociatedObject.GetAncestors().FirstOrDefault(x => x is FluidWrapPanel) as FluidWrapPanel;
            if (_parentFwPanel == null)
                throw new ArgumentNullException("No FluidWrapPanel found! FluidPointerDragBehavior should be applied " +
                                                 "to a UIElement which has a FluidWrapPanel as parent or ancestor!", innerException: null);

            // Subscribe to the Pointer Pressed/Moved/Released events
            AssociatedObject.PointerPressed += OnPointerPressed;
            AssociatedObject.PointerMoved += OnPointerMoved;
            AssociatedObject.PointerReleased += OnPointerReleased;
        }

        /// <summary>
        /// Handler for Pointer Pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!IsValidPointer(e.GetCurrentPoint(AssociatedObject), e))
                return;

            _parentFwPanel?.BeginFluidDrag(AssociatedObject, e);
        }

        /// <summary>
        /// Handler for Pointer Moved event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsValidPointer(e.GetCurrentPoint(AssociatedObject), e))
                return;

            _parentFwPanel?.OnFluidDrag(AssociatedObject, e);
        }

        /// <summary>
        /// Handler for Pointer Released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!IsValidPointer(e.GetCurrentPoint(AssociatedObject), e, true))
                return;

            _parentFwPanel?.EndFluidDrag(AssociatedObject, e);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Checks if the correct pointer has raised the event
        /// </summary>
        /// <param name="ptrPt">PointerPoint</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        /// <param name="isReleased">Flag to indicate if the pointer is released.</param>
        /// <returns></returns>
        private bool IsValidPointer(PointerPoint ptrPt, PointerRoutedEventArgs e, bool isReleased = false)
        {
            return (((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse) &&
                     (((DragButton & DragButtonType.MouseLeftButton) == DragButtonType.MouseLeftButton) && (ptrPt.Properties.IsLeftButtonPressed || isReleased)) ||
                     (((DragButton & DragButtonType.MouseRightButton) == DragButtonType.MouseRightButton) && (ptrPt.Properties.IsRightButtonPressed || isReleased)) ||
                     (((DragButton & DragButtonType.MouseMiddleButton) == DragButtonType.MouseMiddleButton) && (ptrPt.Properties.IsMiddleButtonPressed || isReleased)))) ||
                   ((e.Pointer.PointerDeviceType == PointerDeviceType.Pen) && ((DragButton & DragButtonType.Pen) == DragButtonType.Pen)) ||
                   ((e.Pointer.PointerDeviceType == PointerDeviceType.Touch) && ((DragButton & DragButtonType.Touch) == DragButtonType.Touch));
        }

        #endregion
    }
}
