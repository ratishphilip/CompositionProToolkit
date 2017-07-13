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
// CompositionProToolkit v0.7.0
// 

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas;

namespace CompositionProToolkit
{
    /// <summary>
    /// Class for rendering an image onto a ICompositionSurface
    /// </summary>
    internal sealed class ImageSurface : IImageSurface
    {
        #region Fields

        private ICompositionGeneratorInternal _generator;
        private CompositionDrawingSurface _surface;
        private Uri _uri;
        private CanvasBitmap _canvasBitmap;
        private readonly object _surfaceLock;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the CompositionGenerator
        /// </summary>
        public ICompositionGenerator Generator => _generator;

        /// <summary>
        /// Gets the Surface of the SurfaceImage
        /// </summary>
        public ICompositionSurface Surface => _surface;

        /// <summary>
        /// Gets the Uri of the image to be loaded onto the SurfaceImage
        /// </summary>
        public Uri Uri => _uri;

        /// <summary>
        /// Gets the SurfaceImage Size
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// Gets the image's resize and alignment options in the allocated space.
        /// </summary>
        public ImageSurfaceOptions Options { get; private set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">ICompositionMaskGeneratorInternal object</param>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="size">Size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        public ImageSurface(ICompositionGeneratorInternal generator, Uri uri, Size size, ImageSurfaceOptions options)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator), "CompositionGenerator cannot be null!");

            _generator = generator;
            _surfaceLock = new object();
            // Create the Surface of the SurfaceImage
            _surface = _generator.CreateDrawingSurface(_surfaceLock, size);
            Size = _surface?.Size ?? new Size(0, 0);
            _uri = uri;
            _canvasBitmap = null;
            // Set the image options
            Options = options;
            // Subscribe to DeviceReplaced event
            _generator.DeviceReplaced += OnDeviceReplaced;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Redraws the SurfaceImage
        /// </summary>
        /// <returns>Task</returns>
        public void Redraw()
        {
            // Reload the SurfaceImage
            RedrawSurfaceImageInternal();
        }

        /// <summary>
        /// Redraws the SurfaceImage with the given image options
        /// </summary>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        public void Redraw(ImageSurfaceOptions options)
        {
            // Set the image options
            Options = options;
            // Redraw the SurfaceImage
            RedrawSurfaceImageInternal();
        }

        /// <summary>
        /// Redraws the SurfaceImage by loading image from the new Uri
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded on to the image surface.</param>
        /// <returns>Task</returns>
        public Task RedrawAsync(Uri uri)
        {
            // If the given Uri differs from the previously stored Uri
            // dispose the existing canvasBitmap
            if (!_uri.IsEqualTo(uri))
            {
                _canvasBitmap?.Dispose();
                _canvasBitmap = null;
            }

            // Set the new Uri of the image to be loaded
            _uri = uri;
            // Reload the SurfaceImage
            return RedrawSurfaceImageInternalAsync();
        }

        /// <summary>
        /// Redraws the SurfaceImage by loading image from the new Uri and image options
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded on to the image surface.</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        public Task RedrawAsync(Uri uri, ImageSurfaceOptions options)
        {
            // If the given Uri differs from the previously stored Uri
            // dispose the existing canvasBitmap
            if (!_uri.IsEqualTo(uri))
            {
                _canvasBitmap?.Dispose();
                _canvasBitmap = null;
            }

            // Set the new Uri of the image to be loaded
            _uri = uri;
            // Set the image options
            Options = options;
            // Reload the SurfaceImage
            return RedrawSurfaceImageInternalAsync();
        }

        /// <summary>
        /// Resizes the SurfaceImage with the given size and redraws the SurfaceImage by loading 
        /// image from the new Uri.
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        public async Task RedrawAsync(Uri uri, Size size, ImageSurfaceOptions options)
        {
            // If the given Uri differs from the previously stored Uri
            // dispose the existing canvasBitmap
            if (!_uri.IsEqualTo(uri))
            {
                _canvasBitmap?.Dispose();
                _canvasBitmap = null;
            }

            // Set the image options
            Options = options;
            // Resize the surface only if AutoResize option is disabled
            if (!Options.AutoResize)
            {
                // resize the SurfaceImage
                _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
                // Set the size
                Size = _surface?.Size ?? new Size(0, 0);
            }
            // Set the new Uri of the image to be loaded
            _uri = uri;
            // Reload the SurfaceImage
            await RedrawSurfaceImageInternalAsync();
        }

        /// <summary>
        /// Resizes the SurfaceImage to the new size.
        /// </summary>
        /// <param name="size">New size of the SurfaceImage</param>
        public void Resize(Size size)
        {
            // Resize the surface only if AutoResize option is disabled
            if (Options.AutoResize)
                return;

            // resize the SurfaceImage
            _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
            // Set the size
            Size = _surface?.Size ?? new Size(0, 0);
            // resize the SurfaceImage
            RedrawSurfaceImageInternal();
        }

        /// <summary>
        /// Resizes the SurfaceImage to the new size.
        /// </summary>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        public void Resize(Size size, ImageSurfaceOptions options)
        {
            // Set the image options
            Options = options;
            // Resize the surface only if AutoResize option is disabled
            if (!Options.AutoResize)
            {
                // resize the SurfaceImage
                _generator.ResizeDrawingSurface(_surfaceLock, _surface, size);
                // Set the size
                Size = _surface?.Size ?? new Size(0, 0);
            }
            // resize the SurfaceImage
            RedrawSurfaceImageInternal();
        }

        /// <summary>
        /// Disposes the resources used by the SurfaceImage
        /// </summary>
        public void Dispose()
        {
            _surface?.Dispose();
            if (_generator != null)
                _generator.DeviceReplaced -= OnDeviceReplaced;
            _canvasBitmap?.Dispose();
            _canvasBitmap = null;
            _surface = null;
            _generator = null;
            _uri = null;
            Options = null;
        }

        #endregion

        #region Internal APIs

        /// <summary>
        /// Redraws the SurfaceImage asynchronously
        /// by loading the image from the Uri
        /// </summary>
        /// <returns>Task</returns>
        internal Task RedrawAsync()
        {
            // Reload the SurfaceImage
            return RedrawSurfaceImageInternalAsync();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the DeviceReplaced event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">object</param>
        private async void OnDeviceReplaced(object sender, object e)
        {
            // Recreate the ImageSurface
            _surface = _generator.CreateDrawingSurface(_surfaceLock, Size);
            // Reload the SurfaceImage
            await RedrawSurfaceImageInternalAsync();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper class to redraw the SurfaceImage synchronously
        /// </summary>
        private void RedrawSurfaceImageInternal()
        {
            // Resize the surface image
            _generator.RedrawImageSurface(_surfaceLock, _surface, Options, _canvasBitmap);
            // If AutoResize is allowed and the image is successfully loaded into the canvasBitmap, 
            // then update the Size property of the surface as the surface has been resized to match the canvasBitmap size
            if (Options.AutoResize)
            {
                // If the image is successfully loaded into the canvasBitmap, then update the Size property
                // of the surface as the surface has been resized to match the canvasBitmap size
                Size = _canvasBitmap?.Size ?? new Size(0, 0);
            }
        }

        /// <summary>
        /// Helper class to redraw the SurfaceImage asynchronously
        /// </summary>
        /// <returns>Task</returns>
        private async Task RedrawSurfaceImageInternalAsync()
        {
            // Cache the canvasBitmap to avoid reloading of the same image during Resize/Redraw operations
            _canvasBitmap = await _generator.RedrawImageSurfaceAsync(_surfaceLock, _surface, _uri, Options, _canvasBitmap);
            // If AutoResize is allowed and the image is successfully loaded into the canvasBitmap, 
            // then update the Size property of the surface as the surface has been resized to match the canvasBitmap size
            if (Options.AutoResize)
            {
                // If the image is successfully loaded into the canvasBitmap, then update the Size property
                // of the surface as the surface has been resized to match the canvasBitmap size
                Size = _canvasBitmap?.Size ?? new Size(0, 0);
            }
        }

        #endregion
    }
}
