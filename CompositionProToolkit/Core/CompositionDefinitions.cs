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
// CompositionProToolkit v0.4.6
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
    /// influence the rendering of the image on the ImageSurface
    /// </summary>
    public class ImageSurfaceOptions
    {
        #region Properties

        /// <summary>
        /// Specifies whether the surface should resize itself
        /// automatically to match the loaded image size
        /// </summary>
        public bool AutoResize { get; set; }
        /// <summary>
        /// Describes how image is resized to fill its allocated space.
        /// NOTE: This property is taken into consideration only if AutoResize is False.
        /// </summary>
        public Stretch Stretch { get; set; }
        /// <summary>
        /// Describes how image is positioned horizontally in 
        /// the ImageSurface
        /// NOTE: This property is taken into consideration only if AutoResize is False.
        /// </summary>
        public AlignmentX HorizontalAlignment { get; set; }
        /// <summary>
        /// Describes how image is positioned vertically in 
        /// the ImageSurface
        /// NOTE: This property is taken into consideration only if AutoResize is False.
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
        /// in case the image is not rendered
        /// </summary>
        public Color SurfaceBackgroundColor { get; set; }

        #endregion

        #region Static Properties

        /// <summary>
        /// Default ImageSurfaceOptions when AutoResize is True
        /// Uniform Stretch and Center alignment
        /// </summary>
        public static ImageSurfaceOptions Default =>
                    new ImageSurfaceOptions()
                    {
                        AutoResize = true,
                        Interpolation = CanvasImageInterpolation.HighQualityCubic,
                        Opacity = 1f,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = AlignmentX.Center,
                        VerticalAlignment = AlignmentY.Center,
                        SurfaceBackgroundColor = Colors.Transparent
                    };

        /// <summary>
        /// Default ImageSurfaceOptions when AutoResize is False
        /// Uniform Stretch and Center alignment
        /// </summary>
        public static ImageSurfaceOptions DefaultOptimized =>
                    new ImageSurfaceOptions()
                    {
                        AutoResize = false,
                        Interpolation = CanvasImageInterpolation.HighQualityCubic,
                        Opacity = 1f,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = AlignmentX.Center,
                        VerticalAlignment = AlignmentY.Center,
                        SurfaceBackgroundColor = Colors.Transparent
                    };

        #endregion
    }
}
