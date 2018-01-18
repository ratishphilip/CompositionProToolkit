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

using System.Numerics;
using Windows.UI.Composition;

namespace CompositionProToolkit.Expressions.Templates
{
    /// <summary>
    /// Base Template class for CompositionSurfaceBrush
    /// </summary>
    public abstract class SurfaceBrushTemplate:ExpressionTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name">Template name</param>
        internal SurfaceBrushTemplate(string name) : base(name)
        {
            Type = typeof(CompositionSurfaceBrush);
        }

        /// <summary>
        /// Controls the positioning of the vertical axis of content with respect to the
        /// vertical axis of the Windows.UI.Composition.SpriteVisual. The value is clamped
        /// from 0.0f to 1.0f with 0.0f representing the left vertical edge and 1.0f representing
        /// the right vertical edge of the Windows.UI.Composition.SpriteVisual. By default
        /// this is set to 0.0f.
        /// </summary>
        public float HorizontalAlignmentRatio { get; }
        /// <summary>
        /// Controls the positioning of the horizontal axis of content with respect to the
        /// horizontal axis of the Windows.UI.Composition.SpriteVisual. The value is clamped
        /// from 0.0f to 1.0f with 0.0f representing the top horizontal edge and 1.0f representing
        /// the bottom horizontal edge of the Windows.UI.Composition.SpriteVisual. By default
        /// this is set to 0.0f.
        /// </summary>
        public float VerticalAlignmentRatio { get; }
        /// <summary>
        /// Inset from the bottom edge of the source content that specifies the thickness
        /// of the bottom row. Defaults to 0.0f.
        /// </summary>
        public float BottomInset { get; }
        /// <summary>
        /// Inset from the left edge of the source content that specifies the thickness of
        /// the left column. Defaults to 0.0f.
        /// </summary>
        public float LeftInset { get; }
        /// <summary>
        /// Inset from the right edge of the source content that specifies the thickness of
        /// the right column. Defaults to 0.0f.
        /// </summary>
        public float RightInset { get; }
        /// <summary>
        /// Inset from the top edge of the source content that specifies the thickness
        /// of the top row. Defaults to 0.0f.
        /// </summary>
        public float TopInset { get; }
        /// <summary>
        /// The angle of rotation of the brush in radians.
        /// </summary>
        public float RotationAngle { get; }
        /// <summary>
        /// The angle of rotation of the brush in degrees.
        /// </summary>
        public float RotationAngleInDegrees { get; }
        /// <summary>
        /// The point on the brush to be positioned at the brush's offset. Value is normalized
        /// with respect to the size of the SpriteVisual.
        /// </summary>
        public Vector2 AnchorPoint { get; }
        /// <summary>
        /// The point about which the brush is rotated and scaled.
        /// </summary>
        public Vector2 CenterPoint { get; }
        /// <summary>
        /// The point about which the brush is rotated and scaled.
        /// </summary>
        public Vector2 Offset { get; }
        /// <summary>
        /// The scale to apply to the brush.
        /// </summary>
        public Vector2 Scale { get; }
        /// <summary>
        /// The transformation matrix to apply to the brush.
        /// </summary>
        public Matrix3x2 TransformMatrix { get; }
    }

    /// <summary>
    /// Target Template for CompositionSurfaceBrush. This provides a reference to
    /// the CompositionSurfaceBrush object to which an Expression is connected.
    /// </summary>
    public sealed class SurfaceBrushTarget : SurfaceBrushTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SurfaceBrushTarget() : base(string.Empty)
        {

        }
    }

    /// <summary>
    /// Reference Template for CompositionSurfaceBrush. When you use this object to define
    /// the reference in the Expression of any animation, you must call the 
    /// SetReference() method of the ExpressionAnimation to set the reference
    /// to actual CompositionSurfaceBrush object.
    /// </summary>
    public sealed class SurfaceBrushReference : SurfaceBrushTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="referenceName">Reference name</param>
        public SurfaceBrushReference(string referenceName) : base(referenceName)
        {

        }
    }
}
