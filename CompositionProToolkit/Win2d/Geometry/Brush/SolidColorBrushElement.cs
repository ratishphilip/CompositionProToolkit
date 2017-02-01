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
// CompositionProToolkit v0.5.1
// 

using System;
using System.Numerics;
using System.Text.RegularExpressions;
using Windows.UI;
using CompositionProToolkit.Win2d.Common;
using CompositionProToolkit.Win2d.Core;
using CompositionProToolkit.Win2d.Parsers;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace CompositionProToolkit.Win2d.Geometry.Brush
{
    /// <summary>
    /// Represents a CanvasSolidColorBrush
    /// </summary>
    internal sealed class SolidColorBrushElement : AbstractCanvasBrushElement
    {
        #region Fields

        private Color _color;

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="capture">Capture object</param>
        public SolidColorBrushElement(Capture capture)
        {
            // Set the default values
            _color = Colors.Transparent;
            _opacity = 1f;

            Initialize(capture);
        }

        #endregion

        #region APIs

        /// <summary>
        /// Creates the ICanvasBrush from the parsed data
        /// </summary>
        /// <param name="resourceCreator">ICanvasResourceCreator object</param>
        /// <returns>ICanvasBrush</returns>
        public override ICanvasBrush CreateBrush(ICanvasResourceCreator resourceCreator)
        {
            return new CanvasSolidColorBrush(resourceCreator, _color)
            {
                Opacity = _opacity
            };
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the Regex for extracting Brush Element Attributes
        /// </summary>
        /// <returns>Regex</returns>
        protected override Regex GetAttributesRegex()
        {
            return RegexFactory.GetAttributesRegex(BrushType.SolidColor);
        }

        /// <summary>
        /// Gets the Brush Element Attributes from the Match
        /// </summary>
        /// <param name="match">Match object</param>
        protected override void GetAttributes(Match match)
        {
            // Is it HexColor?
            if (match.Groups["Color"].Success)
            {
                // Convert the captured hexadecimal string to Color
                ColorParser.TryParse(match.Groups["Color"].Value, out _color);
            }
            //  or HdrColor
            else
            {
                float x = 0f, y = 0f, z = 0f, w = 0f;
                Single.TryParse(match.Groups["X"].Value, out x);
                Single.TryParse(match.Groups["Y"].Value, out y);
                Single.TryParse(match.Groups["Z"].Value, out z);
                Single.TryParse(match.Groups["W"].Value, out w);

                _color = ColorParser.Parse(new Vector4(x, y, z, w));
            }

            // Opacity (optional)
            var group = match.Groups["Opacity"];
            if (group.Success)
            {
                Single.TryParse(group.Value, out _opacity);
            }
        }

        #endregion
    }
}
