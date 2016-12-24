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
// CompositionProToolkit v0.5.0
// 

using System.Text;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit.Win2d
{
    /// <summary>
    /// Helper class for Win2d
    /// </summary>
    public static class CanvasGeometryParser
    {
        #region APIs

        /// <summary>
        /// Parses the Path data in string format and converts it to CanvasGeometry.
        /// </summary>
        /// <param name="resourceCreator">ICanvasResourceCreator</param>
        /// <param name="pathData">Path data</param>
        /// <param name="logger">(Optional) For logging purpose. To log the set of  
        /// CanvasPathBuilder commands, used for creating the CanvasGeometry, in 
        /// string format.</param>
        /// <returns>CanvasGeometry</returns>
        public static CanvasGeometry Parse(ICanvasResourceCreator resourceCreator, string pathData,
            StringBuilder logger = null)
        {
            // Create the CanvasPathGeometry from the path data
            var pathGeometry = new CanvasPathGeometry(pathData);

            // Log command
            logger?.AppendLine("using (var pathBuilder = new CanvasPathBuilder(resourceCreator))");
            logger?.AppendLine("{");

            // Get the CanvasGeometry from the CanvasPathGeometry
            var geometry = pathGeometry.CreateGeometry(resourceCreator, logger);

            // Log command
            logger?.AppendLine("}");

            return geometry;
        }

        #endregion
    }
}
