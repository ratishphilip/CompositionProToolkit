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

using Windows.UI.Composition;

namespace CompositionProToolkit.Expressions.Templates
{
    /// <summary>
    /// Base Template class for CompositionNineGridBrush
    /// </summary>
    public abstract class NineGridBrushTemplate : ExpressionTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name">Template name</param>
        internal NineGridBrushTemplate(string name) : base(name)
        {
            Type = typeof(CompositionNineGridBrush);
        }

        /// <summary>
        /// Inset from the bottom edge of the source content that specifies the thickness
        /// of the bottom row. Defaults to 0.0f.
        /// </summary>
        public float BottomInset { get; }
        /// <summary>
        /// Scale to be applied to BottomInset. Defaults to 1.0f.
        /// </summary>
        public float BottomInsetScale { get; }
        /// <summary>
        /// Inset from the left edge of the source content that specifies the thickness of
        /// the left column. Defaults to 0.0f.
        /// </summary>
        public float LeftInset { get; }
        /// <summary>
        /// Scale to be applied to LeftInset. Defaults to 1.0f.
        /// </summary>
        public float LeftInsetScale { get; }
        /// <summary>
        /// Inset from the right edge of the source content that specifies the thickness of
        /// the right column. Defaults to 0.0f.
        /// </summary>
        public float RightInset { get; }
        /// <summary>
        /// Scale to be applied to RightInset. Defaults to 1.0f.
        /// </summary>
        public float RightInsetScale { get; }
        /// <summary>
        /// Inset from the top edge of the source content that specifies the thickness
        /// of the top row. Defaults to 0.0f.
        /// </summary>
        public float TopInset { get; }
        /// <summary>
        /// Scale to be applied to TopInset. Defaults to 1.0f.
        /// </summary>
        public float TopInsetScale { get; }
    }

    /// <summary>
    /// Target Template for CompositionNineGridBrush. This provides a reference to
    /// the CompositionNineGridBrush object to which an Expression is connected.
    /// </summary>
    public sealed class NineGridBrushTarget : NineGridBrushTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        public NineGridBrushTarget() : base(string.Empty)
        {

        }
    }

    /// <summary>
    /// Reference Template for CompositionNineGridBrush. When you use this object to define
    /// the reference in the Expression of any animation, you must call the 
    /// SetReference() method of the ExpressionAnimation to set the reference
    /// to actual CompositionNineGridBrush object.
    /// </summary>
    public sealed class NineGridBrushReference : NineGridBrushTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="referenceName">Reference name</param>
        public NineGridBrushReference(string referenceName) : base(referenceName)
        {

        }
    }
}
