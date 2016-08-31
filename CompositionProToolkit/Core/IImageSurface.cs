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
// CompositionProToolkit v0.4.5
// 

using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace CompositionProToolkit
{
    /// <summary>
    /// Interface for rendering an image onto a ICompositionSurface
    /// </summary>
    public interface IImageSurface : IRenderSurface
    {
        #region Properties

        /// <summary>
        /// Uri of the image to be loaded onto the ImageSurface
        /// </summary>
        Uri Uri { get; }
        /// <summary>
        /// Describes the image's resize and alignment options in the allocated space.
        /// </summary>
        ImageSurfaceOptions Options { get; }

        #endregion

        #region APIs

        /// <summary>
        /// Redraws the ImageSurface with the given image options
        /// </summary>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        void Redraw(ImageSurfaceOptions options);

        /// <summary>
        /// Redraws the ImageSurface by loading image from the new Uri and applying the image options.
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded on to the image surface.</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        Task RedrawAsync(Uri uri, ImageSurfaceOptions options);

        /// <summary>
        /// Resizes the ImageSurface with the given size and redraws the ImageSurface by loading 
        /// image from the new Uri.
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded onto the ImageSurface.</param>
        /// <param name="size">New size of the ImageSurface</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        Task RedrawAsync(Uri uri, Size size, ImageSurfaceOptions options);

        /// <summary>
        /// Resizes the ImageSurface to the new size.
        /// </summary>
        /// <param name="size">New size of the ImageSurface</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        void Resize(Size size, ImageSurfaceOptions options);

        #endregion
    }
}
