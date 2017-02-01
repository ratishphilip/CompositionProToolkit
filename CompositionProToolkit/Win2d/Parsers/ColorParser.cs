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
using CompositionProToolkit.Win2d.Core;

namespace CompositionProToolkit.Win2d.Parsers
{
    /// <summary>
    /// Parser for Color
    /// </summary>
    internal static class ColorParser
    {
        /// <summary>
        /// Converts the hexadecimal color string in #RRGGBB or #AARRGGBB format
        /// to the corresponding Color object. The '#' character is optional.
        /// </summary>
        /// <param name="hexColor">Hexadecimal color string</param>
        /// <returns>Color</returns>
        internal static Color Parse(string hexColor)
        {
            var match = RegexFactory.HexadecimalColorRegex.Match(hexColor);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid Hexadecimal string!", nameof(hexColor));
            }

            return GetColor(match);
        }

        /// <summary>
        /// Attempts to convert the hexadecimal color string in #RRGGBB or #AARRGGBB format
        /// to the corresponding Color object. The '#' character is optional.
        /// </summary>
        /// <param name="hexColor">Hexadecimal color string</param>
        /// <param name="color">Output Color object</param>
        /// <returns>True if successful, otherwise False</returns>
        internal static bool TryParse(string hexColor, out Color color)
        {
            var match = RegexFactory.HexadecimalColorRegex.Match(hexColor);
            if (!match.Success)
            {
                return false;
            }

            color = GetColor(match);
            return true;
        }

        /// <summary>
        /// Converts a Vector4 High Dynamic Range Color
        /// to Color. Negative components of the Vector4 will be
        /// sanitized by taking the absolute value of the component.
        /// The HDR Color components should have value in the range
        /// between 0 and 1, inclusive. If they are more than 1, they
        /// will be clamped at 1.
        /// </summary>
        /// <param name="hdrColor">High Dynamic Range Color</param>
        /// <returns>Color</returns>
        internal static Color Parse(Vector4 hdrColor)
        {
            // Vector4's X, Y, Z, W components match to
            // Color's   R, G, B, A components respectively
            var r = (byte)Math.Min(Math.Abs(hdrColor.X) * 255f, 255f);
            var g = (byte)Math.Min(Math.Abs(hdrColor.Y) * 255f, 255f);
            var b = (byte)Math.Min(Math.Abs(hdrColor.Z) * 255f, 255f);
            var a = (byte)Math.Min(Math.Abs(hdrColor.W) * 255f, 255f);

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Parses and constructs a Color object from the specified
        /// Match object.
        /// </summary>
        /// <param name="match">Match object</param>
        /// <returns>Color</returns>
        private static Color GetColor(Match match)
        {
            // Alpha component
            byte alpha = 255;
            var alphaStr = match.Groups["Alpha"].Value;
            if (!String.IsNullOrWhiteSpace(alphaStr))
            {
                alpha = (byte)Convert.ToInt32(alphaStr, 16);
            }
            // Red component
            var red = (byte)Convert.ToInt32(match.Groups["Red"].Value, 16);
            // Green component
            var green = (byte)Convert.ToInt32(match.Groups["Green"].Value, 16);
            // Blue component
            var blue = (byte)Convert.ToInt32(match.Groups["Blue"].Value, 16);

            return Color.FromArgb(alpha, red, green, blue);
        }
    }
}
