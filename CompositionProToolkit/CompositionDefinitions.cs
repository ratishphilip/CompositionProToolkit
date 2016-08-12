// Copyright (c) 2016 Ratish Philip 
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
// CompositionProToolkit v0.4.1
// 

using Windows.UI;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;

namespace CompositionProToolkit
{
    /// <summary>
    /// Enum to define the location of the
    /// Reflection of a Visual.
    /// </summary>
    public enum ReflectionLocation
    {
        Bottom = 0,
        Top = 1,
        Left = 2,
        Right = 3
    }

    /// <summary>
    /// Class to define the various options that would
    /// influence the rendering of the image on the CompositionSurfaceImage
    /// </summary>
    public class CompositionSurfaceImageOptions
    {
        #region Properties

        /// <summary>
        /// Describes how image is resized to fill its allocated space.
        /// </summary>
        public Stretch Stretch { get; set; }
        /// <summary>
        /// Describes how image is positioned horizontally in 
        /// the CompositionSurfaceImage
        /// </summary>
        public AlignmentX HorizontalAlignment { get; set; }
        /// <summary>
        /// Describes how image is positioned vertically in 
        /// the CompositionSurfaceImage
        /// </summary>
        public AlignmentY VerticalAlignment { get; set; }
        /// <summary>
        /// Specifies the opacity of the rendered image
        /// </summary>
        public float Opacity { get; set; }
        /// <summary>
        /// Specifies the interpolation used to render the image
        /// </summary>
        public CanvasImageInterpolation Interpolation { get; set; }
        /// <summary>
        /// Color which will be used to fill the SurfaceImage 
        /// before rendering the image
        /// </summary>
        public Color SurfaceBackgroundColor { get; set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stretch">Stretch Mode</param>
        /// <param name="hAlign">Horizontal Alignment</param>
        /// <param name="vAlign">Vertical Alignment</param>
        /// <param name="opacity">Opacity at which image should be rendered</param>
        /// <param name="interpolation">Render Interpolation</param>
        public CompositionSurfaceImageOptions(Stretch stretch,
                    AlignmentX hAlign, AlignmentY vAlign, float opacity = 1f,
                    CanvasImageInterpolation interpolation = CanvasImageInterpolation.HighQualityCubic)
        {
            Stretch = stretch;
            HorizontalAlignment = hAlign;
            VerticalAlignment = vAlign;
            Opacity = opacity;
            Interpolation = interpolation;
            SurfaceBackgroundColor = Colors.Transparent;
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Default CompositionSurfaceImageOptions
        /// Uniform Stretch and Left-Top alignment
        /// </summary>
        public static CompositionSurfaceImageOptions Default =>
                    new CompositionSurfaceImageOptions(Stretch.Uniform, AlignmentX.Left, AlignmentY.Top);

        /// <summary>
        /// Uniform Stretch and Center alignment
        /// </summary>
        public static CompositionSurfaceImageOptions UniformCenter =>
            new CompositionSurfaceImageOptions(Stretch.Uniform, AlignmentX.Center, AlignmentY.Center);

        #endregion
    }
}
