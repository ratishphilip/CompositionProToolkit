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
// CompositionProToolkit v0.6.0
// 

using Windows.UI;
using Windows.UI.Composition;

namespace CompositionProToolkit.Expressions.Templates
{
    /// <summary>
    /// Base Template class for CompositionColorBrush
    /// </summary>
    public abstract class ColorBrushTemplate : ExpressionTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name">Template name</param>
        internal ColorBrushTemplate(string name) : base(name)
        {
            Type = typeof(CompositionColorBrush);
        }

        /// <summary>
        /// The color used to fill a Windows.UI.Composition.SpriteVisual.
        /// </summary>
        public Color Color { get; }
    }

    /// <summary>
    /// Target Template for CompositionColorBrush. This provides a reference to
    /// the CompositionColorBrush object to which an Expression is connected.
    /// </summary>
    public sealed class ColorBrushTarget : ColorBrushTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ColorBrushTarget() : base(string.Empty)
        {

        }
    }

    /// <summary>
    /// Reference Template for CompositionColorBrush. When you use this object to define
    /// the reference in the Expression of any animation, you must call the 
    /// SetReference() method of the ExpressionAnimation to set the reference 
    /// to actual CompositionColorBrush object.
    /// </summary>
    public sealed class ColorBrushReference : ColorBrushTemplate
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="referenceName">Reference name</param>
        public ColorBrushReference(string referenceName) : base(referenceName)
        {

        }
    }
}
