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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit.Win2d
{
    /// <summary>
    /// A class containing a collection of ICanvasPathFigure
    /// which can be used to construct a CanvasGeometry.
    /// </summary>
    internal class CanvasPathGeometry
    {
        #region Fields

        

        private readonly List<ICanvasPathElement> _figures;

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="pathData">Path data in text format</param>
        public CanvasPathGeometry(string pathData)
        {
            _figures = new List<ICanvasPathElement>();

            var matches = RegexFactory.CanvasPathGeometryRegex.Matches(pathData);
            // If no match is found or no captures in the match, then it means
            // that the path data is invalid.
            if ((matches == null) || (matches.Count == 0))
            {
                throw new ArgumentException("Invalid Path data!", nameof(pathData));
            }

            // If the match contains more than one captures, it means that there
            // are multiple FillRuleElements present in the path data. There can
            // be only one FillRuleElement in the path data (at the beginning).
            if (matches.Count > 1)
            {
                throw new ArgumentException("Multiple FillRule elements present in Path Data! " +
                                            "There should be only one FillRule within the Path Data. " +
                                            "You can either remove additional FillRule elements or split the Path Data " +
                                            "into multiple Path Data and call the Parse method on each of them.");
            }

            var figures = new List<ICanvasPathElement>();

            foreach (PathFigureType type in Enum.GetValues(typeof(PathFigureType)))
            {
                foreach (Capture figureCapture in matches[0].Groups[type.ToString()].Captures)
                {
                    var figureRootIndex = figureCapture.Index;
                    var regex = RegexFactory.GetRegex(type);
                    var figureMatch = regex.Match(figureCapture.Value);
                    var isRelative = false;
                    // Process the 'Main' Group which contains the Path Command and
                    // corresponding attributes
                    if (figureMatch.Groups["Main"].Captures.Count == 1)
                    {
                        var figure = PathElementFactory.CreatePathFigure(type, figureMatch, figureRootIndex);
                        figures.Add(figure);
                        isRelative = figure.IsRelative;
                    }

                    // Process the 'Additional' Group which contains just the attributes
                    figures.AddRange(from Capture capture in figureMatch.Groups["Additional"].Captures
                                     select PathElementFactory.CreateAdditionalPathFigure(type, capture, figureRootIndex + capture.Index, isRelative));
                }
            }
            
            // Sort the figures by their indices
            _figures.AddRange(figures.OrderBy(f => f.Index));
            if (_figures.Count > 0)
            {
                // Check if the first element in the _figures list is a FillRuleElement
                // which would indicate the fill rule to be followed while creating the 
                // path. If it is not present, then insert a default FillRuleElement at
                // the beginning.
                if ((_figures.ElementAt(0) as FillRuleElement) == null)
                {
                    _figures.Insert(0, PathElementFactory.CreateDefaultPathElement(PathFigureType.FillRule));
                }
            }

            // Perform validation to check if there are any invalid characters in the path data that were not captured
            var preValidationCount = RegexFactory.ValidationRegex.Replace(pathData, string.Empty).Length;

            var postValidationCount = _figures.Sum(x => x.ValidationCount);

            if (preValidationCount != postValidationCount)
            {
                throw new ArgumentException("Path data contains invalid characters!", nameof(pathData));
            }
        }

        #endregion

        #region APIs

        /// <summary>
        /// Creates a CanvasGeometry based on its collection of
        /// CanvasPathFigures.
        /// </summary>
        /// <param name="resourceCreator">ICanvasResourceCreator</param>
        /// <param name="logger">For logging purpose. To log the set of CanvasPathBuilder 
        /// commands, used for creating the CanvasGeometry, in string format.</param>
        /// <returns>CanvasGeometry</returns>
        internal CanvasGeometry CreateGeometry(ICanvasResourceCreator resourceCreator, StringBuilder logger)
        {
            if (_figures.Count == 0)
                return null;

            ICanvasPathElement lastElement = null;
            var currentPoint = Vector2.Zero;

            using (var pathBuilder = new CanvasPathBuilder(resourceCreator))
            {
                foreach (var pathFigure in _figures)
                {
                    currentPoint = pathFigure.CreatePath(pathBuilder, currentPoint, ref lastElement, logger);
                }

                return CanvasGeometry.CreatePath(pathBuilder);
            }
        }

        #endregion
    }
}
