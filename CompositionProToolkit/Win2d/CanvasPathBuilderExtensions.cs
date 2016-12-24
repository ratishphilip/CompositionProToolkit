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
using System.Numerics;
using CompositionProToolkit.Common;
using Microsoft.Graphics.Canvas.Geometry;

namespace CompositionProToolkit.Win2d
{
    /// <summary>
    /// Defines extension method for CanvasPathBuilder
    /// </summary>
    public static class CanvasPathBuilderExtensions
    {
        /// <summary>
        /// Adds a circle figure to the path.
        /// </summary>
        /// <param name="pathBuilder">CanvasPathBuilder</param>
        /// <param name="center">Center location of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        public static void AddCircleFigure(this CanvasPathBuilder pathBuilder, Vector2 center, float radius)
        {
            pathBuilder.AddEllipseFigure(center.X, center.Y, radius, radius);
        }

        /// <summary>
        /// Adds a circle figure to the path.
        /// </summary>
        /// <param name="pathBuilder">CanvasPathBuilder</param>
        /// <param name="x">X coordinate of the center location of the circle.</param>
        /// <param name="y">Y coordinate of the center location of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        public static void AddCircleFigure(this CanvasPathBuilder pathBuilder, float x, float y, float radius)
        {
            pathBuilder.AddEllipseFigure(x, y, radius, radius);
        }

        /// <summary>
        /// Adds an ellipse figure to the path.
        /// </summary>
        /// <param name="pathBuilder">CanvasPathBuilder</param>
        /// <param name="center">Center location of the ellipse.</param>
        /// <param name="radiusX">Radius of the ellipse on the X-axis.</param>
        /// <param name="radiusY">Radius of the ellipse on the Y-axis.</param>
        public static void AddEllipseFigure(this CanvasPathBuilder pathBuilder, Vector2 center, float radiusX,
            float radiusY)
        {
            pathBuilder.AddEllipseFigure(center.X, center.Y, radiusX, radiusY);
        }

        /// <summary>
        /// Adds an ellipse figure to the path.
        /// </summary>
        /// <param name="pathBuilder">CanvasPathBuilder</param>
        /// <param name="x">X coordinate of the center location of the ellipse.</param>
        /// <param name="y">Y coordinate of the center location of the ellipse.</param>
        /// <param name="radiusX">Radius of the ellipse on the X-axis.</param>
        /// <param name="radiusY">Radius of the ellipse on the Y-axis.</param>
        public static void AddEllipseFigure(this CanvasPathBuilder pathBuilder, float x, float y,
            float radiusX, float radiusY)
        {
            if (radiusX < 0f)
            {
                throw new ArgumentException("radiusX cannot be a negative number.", nameof(radiusX));
            }

            if (radiusY < 0f)
            {
                throw new ArgumentException("radiusY cannot be a negative number.", nameof(radiusY));
            }

            try
            {
                pathBuilder.BeginFigure(x + radiusX, y);
            }
            catch (ArgumentException)
            {
                // An ArgumentException will be raised if another figure was already begun( and not ended)
                // before calling AddEllipseFigure() method.
                throw new InvalidOperationException("A call to CanvasPathBuilder.AddEllipseFigure occurred, " +
                                                    "when another figure was already begun. Please call CanvasPathBuilder.EndFigure method, " +
                                                    "before calling CanvasPathBuilder.AddEllipseFigure, to end the previous figure.");
            }

            // First Semi-Ellipse
            pathBuilder.AddArc(new Vector2(x - radiusX, y), radiusX, radiusY, Float.Pi, CanvasSweepDirection.Clockwise, CanvasArcSize.Large);
            // Second Semi-Ellipse
            pathBuilder.AddArc(new Vector2(x + radiusX, y), radiusX, radiusY, Float.Pi, CanvasSweepDirection.Clockwise, CanvasArcSize.Large);
            // End Figure
            pathBuilder.EndFigure(CanvasFigureLoop.Closed);
        }

        /// <summary>
        /// Adds a n-sided polygon figure to the path.
        /// </summary>
        /// <param name="pathBuilder">CanvasPathBuilder</param>
        /// <param name="numSides">Number of sides of the polygon.</param>
        /// <param name="center">Center location of the polygon.</param>
        /// <param name="radius">Radius of the circle cirumscribing the polygon i.e. the distance
        /// of each of the vertices of the polygon from the center.</param>
        public static void AddPolygonFigure(this CanvasPathBuilder pathBuilder, int numSides, Vector2 center,
            float radius)
        {
            pathBuilder.AddPolygonFigure(numSides, center.X, center.Y, radius);
        }

        /// <summary>
        /// Adds a n-sided polygon figure to the path.
        /// </summary>
        /// <param name="pathBuilder">CanvasPathBuilder</param>
        /// <param name="numSides">Number of sides of the polygon.</param>
        /// <param name="x">X coordinate of the center location of the polygon.</param>
        /// <param name="y">Y coordinate of the center location of the polygon.</param>
        /// <param name="radius">Radius of the circle cirumscribing the polygon i.e. the distance
        /// of each of the vertices of the polygon from the center.</param>
        public static void AddPolygonFigure(this CanvasPathBuilder pathBuilder, int numSides, float x, float y,
            float radius)
        {
            if (radius < 0f)
            {
                throw new ArgumentException("radius cannot be a negative number.", nameof(radius));
            }

            if (numSides < 3)
            {
                throw new ArgumentException("A polygon should have at least 3 sides", nameof(numSides));
            }

            // Calculate the first vertex location based on the number of sides
            var angle = Float.TwoPi / numSides;
            var startAngle = numSides % 2 == 1 ? Float.PiByTwo : Float.PiByTwo - (angle / 2f);

            var startX = x + (float)(radius * Math.Cos(startAngle));
            var startY = y - (float)(radius * Math.Sin(startAngle));

            try
            {
                pathBuilder.BeginFigure(startX, startY);
            }
            catch (ArgumentException)
            {
                // An ArgumentException will be raised if another figure was already begun( and not ended)
                // before calling AddPolygonFigure() method.
                throw new InvalidOperationException("A call to CanvasPathBuilder.AddPolygonFigure occurred, " +
                                                    "when another figure was already begun. Please call CanvasPathBuilder.EndFigure method, " +
                                                    "before calling CanvasPathBuilder.AddPolygonFigure, to end the previous figure.");
            }

            // Add lines to the remaining vertices
            for (var i = 1; i < numSides; i++)
            {
                var posX = x + (float)(radius * Math.Cos(startAngle + i * angle));
                var posY = y - (float)(radius * Math.Sin(startAngle + i * angle));
                pathBuilder.AddLine(posX, posY);
            }

            // Add a line to the first vertex so that the lines join properly
            pathBuilder.AddLine(startX, startY);

            // End the Figure
            pathBuilder.EndFigure(CanvasFigureLoop.Closed);
        }

    }
}
