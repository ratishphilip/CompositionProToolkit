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

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CompositionProToolkit.Win2d
{
    /// <summary>
    /// Contains all the Regular Expressions which are
    /// used for parsing the Path data to CanvasGeometry
    /// </summary>
    internal static class RegexFactory
    {
        #region Regex Constants

        // Whitespace
        public const string Spacer = @"\s*";
        // Whitespace or comma
        public const string SoC = @"(?:\s+|\s*,\s*)";
        // Whitespace or comma or a minus sign (look ahead)
        public const string Sep = @"(?:\s+|\s*,\s*|(?=-))";
        // Numbers
        public const string PositiveInteger = @"[0-9]+";
        public const string Float = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
        public const string PositiveFloat = @"[+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
        // Position
        public static readonly string Pos = $"{Float}{Sep}{Float}";

        // Path Elements
        public static readonly string MoveTo = $"(?<MoveTo>[Mm]{Spacer}{Pos}(?:{Sep}{Pos})*{Spacer})";
        public static readonly string Line = $"(?<Line>[Ll]{Spacer}{Pos}(?:{Sep}{Pos})*{Spacer})";
        public static readonly string HorizontalLine = $"(?<HorizontalLine>[Hh]{Spacer}{Float}(?:{Sep}{Float})*{Spacer})";
        public static readonly string VerticalLine = $"(?<VerticalLine>[Vv]{Spacer}{Float}(?:{Sep}{Float})*{Spacer})";
        public static readonly string QuadraticBezier = $"(?<QuadraticBezier>[Qq]{Spacer}{Pos}{Sep}{Pos}(?:{Sep}{Pos}{Sep}{Pos})*{Spacer})";
        public static readonly string SmoothQuadraticBezier = $"(?<SmoothQuadraticBezier>[Tt]{Spacer}{Pos}(?:{Sep}{Pos})*{Spacer})";
        public static readonly string CubicBezier = $"(?<CubicBezier>[Cc]{Spacer}{Pos}{Sep}{Pos}{Sep}{Pos}(?:{Sep}{Pos}{Sep}{Pos}{Sep}{Pos})*{Spacer})";
        public static readonly string SmoothCubicBezier = $"(?<SmoothCubicBezier>[Ss]{Spacer}{Pos}{Sep}{Pos}(?:{Sep}{Pos}{Sep}{Pos})*{Spacer})";
        public static readonly string Arc = $"(?<Arc>[Aa]{Spacer}{PositiveFloat}{Sep}{PositiveFloat}{Sep}{Float}{SoC}[01]{SoC}[01]{Sep}{Pos}" +
                                            $"(?:{Sep}{PositiveFloat}{Sep}{PositiveFloat}{Sep}{Float}{SoC}[01]{SoC}[01]{Sep}{Pos})*{Spacer})";
        public static readonly string ClosePath = $"(?<ClosePath>[Zz]{Spacer})";

        // CanvasPathFigure
        public static readonly string CanvasPathFigureRegexString =
            $"{MoveTo}" +                   // M x,y
            $"(" +
            $"{Line}+|" +                   // L x,y
            $"{HorizontalLine}+|" +         // H x
            $"{VerticalLine}+|" +            // V y
            $"{QuadraticBezier}+|" +        // Q x1,y1 x,y
            $"{SmoothQuadraticBezier}+|" +  // T x,y
            $"{CubicBezier}+|" +            // C x1,y1 x2,y2 x,y
            $"{SmoothCubicBezier}+|" +      // S x2,y2 x,y
            $"{Arc}+|" +                    // A radX, radY, angle, isLargeArc, sweepDirection, x, y
            $")+" +
            $"{ClosePath}?";                // Close Path (Optional)

        public static readonly string FillRule = $"{Spacer}(?<FillRule>[Ff]{Spacer}[01])";
        public static readonly string PathFigure = $"{Spacer}(?<PathFigure>{CanvasPathFigureRegexString})";
        public static readonly string EllipseFigure = $"{Spacer}(?<EllipseFigure>[Oo]{Spacer}{PositiveFloat}{Sep}{PositiveFloat}{Sep}{Pos}" +
                                                      $"(?:{Sep}{PositiveFloat}{Sep}{PositiveFloat}{Sep}{Pos})*)";
        public static readonly string PolygonFigure = $"{Spacer}(?<PolygonFigure>[Pp]{Spacer}{PositiveInteger}{Sep}{PositiveFloat}{Sep}{Pos}" +
                                                      $"(?:{Sep}{PositiveInteger}{Sep}{PositiveFloat}{Sep}{Pos})*)";

        // CanvasPathGeometry
        public static readonly string CanvasPathGeometryRegexString =
            $"{FillRule}?" +            // F0 or F1
            $"(" +
            $"{PathFigure}+|" +         // Path Figure
            $"{EllipseFigure}+|" +      // Ellipse Figure
            $"{PolygonFigure}+" +       // Polygon Figure
            $")+";

        // Regex strings used to extract the attributes of the Path Elements
        // MoveTo
        public static readonly string MoveToAttributes = $"(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string MoveToRegexString = $"{Spacer}(?<Main>(?<Command>[Mm]){Spacer}{MoveToAttributes})(?<Additional>{Sep}{Pos})*";
        // Line
        public static readonly string LineAttributes = $"(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string LineRegexString = $"{Spacer}(?<Main>(?<Command>[Ll]){Spacer}{LineAttributes})(?<Additional>{Sep}{Pos})*";
        // Horizontal Line
        public static readonly string HorizontalLineAttributes = $"(?<X>{Float})";
        public static readonly string HorizontalLineRegexString = $"{Spacer}(?<Main>(?<Command>[Hh]){Spacer}{HorizontalLineAttributes})(?<Additional>{Sep}{Float})*";
        // Vertical Line
        public static readonly string VerticalLineAttributes = $"(?<X>{Float})";
        public static readonly string VerticalLineRegexString = $"{Spacer}(?<Main>(?<Command>[Vv]){Spacer}{VerticalLineAttributes})(?<Additional>{Sep}{Float})*";
        // Quadratic Bezier
        public static readonly string QuadraticBezierAttributes = $"(?<X1>{Float}){Sep}(?<Y1>{Float}){Sep}(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string QuadraticBezierRegexString = $"{Spacer}(?<Main>(?<Command>[Qq]){Spacer}{QuadraticBezierAttributes})(?<Additional>{Sep}{Pos}{Sep}{Pos})*";
        // Smooth Quadratic Bezier
        public static readonly string SmoothQuadraticBezierAttributes = $"(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string SmoothQuadraticBezierRegexString = $"{Spacer}(?<Main>(?<Command>[Tt]){Spacer}{SmoothQuadraticBezierAttributes})" +
                                                                         $"(?<Additional>{Sep}{Pos})*";
        // Cubic Bezier
        public static readonly string CubicBezierAttributes = $"(?<X1>{Float}){Sep}(?<Y1>{Float}){Sep}(?<X2>{Float}){Sep}(?<Y2>{Float}){Sep}" +
                                                               $"(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string CubicBezierRegexString = $"{Spacer}(?<Main>(?<Command>[Cc]){Spacer}{CubicBezierAttributes})" +
                                                               $"(?<Additional>{Sep}{Pos}{Sep}{Pos}{Sep}{Pos})*";
        // Smooth Cubic Bezier
        public static readonly string SmoothCubicBezierAttributes = $"(?<X2>{Float}){Sep}(?<Y2>{Float}){Sep}(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string SmoothCubicBezierRegexString = $"{Spacer}(?<Main>(?<Command>[Ss]){Spacer}{SmoothCubicBezierAttributes})" +
                                                                     $"(?<Additional>{Sep}{Pos}{Sep}{Pos})*";
        // Arc
        public static readonly string ArcAttributes = $"(?<RadiusX>{PositiveFloat}){Sep}(?<RadiusY>{PositiveFloat}){Sep}(?<Angle>{Float}){SoC}" +
                                                      $"(?<IsLargeArc>[01]){SoC}(?<SweepDirection>[01]){Sep}(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string ArcRegexString = $"{Spacer}(?<Main>(?<Command>[Aa]){Spacer}{ArcAttributes})" +
                                                       $"(?<Additional>{Sep}{PositiveFloat}{Sep}{PositiveFloat}{Sep}{Float}{SoC}[01]{SoC}[01]{Sep}{Pos})*";
        // Close Path
        public static readonly string ClosePathRegexString = $"{Spacer}(?<Main>(?<Command>[Zz])){Spacer}";
        // Fill Rule
        public static readonly string FillRuleRegexString = $"{Spacer}(?<Main>(?<Command>[Ff]){Spacer}(?<FillValue>[01]))";
        // Path Figure
        public static readonly string PathFigureRegexString = $"{Spacer}(?<Main>{PathFigure})";
        // Ellipse Figure
        public static readonly string EllipseFigureAttributes = $"(?<RadiusX>{PositiveFloat}){Sep}(?<RadiusY>{PositiveFloat}){Sep}" +
                                                                $"(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string EllipseFigureRegexString = $"{Spacer}(?<Main>(?<Command>[Oo]){Spacer}{EllipseFigureAttributes})" +
                                                                 $"(?<Additional>{Sep}{PositiveFloat}{Sep}{PositiveFloat}{Sep}{Pos})*";
        // Polygon Figure
        public static readonly string PolygonFigureAttributes = $"(?<Sides>{PositiveInteger}){Sep}(?<Radius>{Float}){Sep}(?<X>{Float}){Sep}(?<Y>{Float})";
        public static readonly string PolygonFigureRegexString = $"{Spacer}(?<Main>(?<Command>[Pp]){Spacer}{PolygonFigureAttributes})" +
                                                                 $"(?<Additional>{Sep}{PositiveInteger}{Sep}{PositiveFloat}{Sep}{Pos})*";

        #endregion

        #region Fields

        private static readonly Dictionary<PathFigureType, Regex> PathFigureRegexes;
        private static readonly Dictionary<PathFigureType, Regex> PathFigureAttributeRegexes;
        private static readonly Dictionary<PathElementType, Regex> PathElementRegexes;
        private static readonly Dictionary<PathElementType, Regex> PathElementAttributeRegexes;

        #endregion

        #region Properties

        /// <summary>
        /// Regex to perform validation of Path data
        /// </summary>
        public static Regex ValidationRegex { get; }
        /// <summary>
        /// Regex for the CanvasPathGeometry
        /// </summary>
        public static Regex CanvasPathGeometryRegex { get; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Static ctor
        /// </summary>
        static RegexFactory()
        {
            PathFigureRegexes = new Dictionary<PathFigureType, Regex>
            {
                [PathFigureType.FillRule] = new Regex(FillRuleRegexString, RegexOptions.Compiled),
                [PathFigureType.PathFigure] = new Regex(PathFigureRegexString, RegexOptions.Compiled),
                [PathFigureType.EllipseFigure] = new Regex(EllipseFigureRegexString, RegexOptions.Compiled),
                [PathFigureType.PolygonFigure] = new Regex(PolygonFigureRegexString, RegexOptions.Compiled)
            };

            PathFigureAttributeRegexes = new Dictionary<PathFigureType, Regex>
            {
                // Not Applicable for FillRuleElement
                [PathFigureType.FillRule] = null,
                // Not Applicable for CanvasPathFigure
                [PathFigureType.PathFigure] = null,
                [PathFigureType.EllipseFigure] = new Regex($"{Sep}{EllipseFigureAttributes}", RegexOptions.Compiled),
                [PathFigureType.PolygonFigure] = new Regex($"{Sep}{PolygonFigureAttributes}", RegexOptions.Compiled)
            };

            PathElementRegexes = new Dictionary<PathElementType, Regex>
            {
                [PathElementType.MoveTo] = new Regex(MoveToRegexString, RegexOptions.Compiled),
                [PathElementType.Line] = new Regex(LineRegexString, RegexOptions.Compiled),
                [PathElementType.HorizontalLine] = new Regex(HorizontalLineRegexString, RegexOptions.Compiled),
                [PathElementType.VerticalLine] = new Regex(VerticalLineRegexString, RegexOptions.Compiled),
                [PathElementType.QuadraticBezier] = new Regex(QuadraticBezierRegexString, RegexOptions.Compiled),
                [PathElementType.SmoothQuadraticBezier] = new Regex(SmoothQuadraticBezierRegexString, RegexOptions.Compiled),
                [PathElementType.CubicBezier] = new Regex(CubicBezierRegexString, RegexOptions.Compiled),
                [PathElementType.SmoothCubicBezier] = new Regex(SmoothCubicBezierRegexString, RegexOptions.Compiled),
                [PathElementType.Arc] = new Regex(ArcRegexString, RegexOptions.Compiled),
                [PathElementType.ClosePath] = new Regex(ClosePathRegexString, RegexOptions.Compiled)
            };

            PathElementAttributeRegexes = new Dictionary<PathElementType, Regex>
            {
                [PathElementType.MoveTo] = new Regex($"{Sep}{MoveToAttributes}", RegexOptions.Compiled),
                [PathElementType.Line] = new Regex($"{Sep}{LineAttributes}", RegexOptions.Compiled),
                [PathElementType.HorizontalLine] = new Regex($"{Sep}{HorizontalLineAttributes}", RegexOptions.Compiled),
                [PathElementType.VerticalLine] = new Regex($"{Sep}{VerticalLineAttributes}", RegexOptions.Compiled),
                [PathElementType.QuadraticBezier] = new Regex($"{Sep}{QuadraticBezierAttributes}", RegexOptions.Compiled),
                [PathElementType.SmoothQuadraticBezier] = new Regex($"{Sep}{SmoothQuadraticBezierAttributes}", RegexOptions.Compiled),
                [PathElementType.CubicBezier] = new Regex($"{Sep}{CubicBezierAttributes}", RegexOptions.Compiled),
                [PathElementType.SmoothCubicBezier] = new Regex($"{Sep}{SmoothCubicBezierAttributes}", RegexOptions.Compiled),
                [PathElementType.Arc] = new Regex($"{Sep}{ArcAttributes}", RegexOptions.Compiled),
                // Not Applicable for ClosePathElement as it has no attributes
                [PathElementType.ClosePath] = null
            };

            ValidationRegex = new Regex(@"\s+");
            CanvasPathGeometryRegex = new Regex(CanvasPathGeometryRegexString, RegexOptions.Compiled);
        }

        #endregion

        #region APIs

        /// <summary>
        /// Get the Regex for the given PathFigureType
        /// </summary>
        /// <param name="figureType">PathFigureType</param>
        /// <returns>Regex</returns>
        internal static Regex GetRegex(PathFigureType figureType)
        {
            return PathFigureRegexes[figureType];
        }

        /// <summary>
        /// Get the Regex for the given PathElementType
        /// </summary>
        /// <param name="elementType">PathElementType</param>
        /// <returns>Regex</returns>
        internal static Regex GetRegex(PathElementType elementType)
        {
            return PathElementRegexes[elementType];
        }

        /// <summary>
        /// Get the Regex for extracting attributes of the given PathFigureType
        /// </summary>
        /// <param name="figureType">PathFigureType</param>
        /// <returns>Regex</returns>
        internal static Regex GetAttributesRegex(PathFigureType figureType)
        {
            return PathFigureAttributeRegexes[figureType];
        }

        /// <summary>
        /// Get the Regex for extracting attributes of the given PathElementType
        /// </summary>
        /// <param name="elementType">PathElementType</param>
        /// <returns>Regex</returns>
        internal static Regex GetAttributesRegex(PathElementType elementType)
        {
            return PathElementAttributeRegexes[elementType];
        }

        #endregion
    }
}
