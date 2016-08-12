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

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using CompositionProToolkit.Common;
using Microsoft.Graphics.Canvas;

namespace CompositionProToolkit
{
    /// <summary>
    /// Class to define a CompositionSurface where an image can be rendered
    /// </summary>
    internal sealed class CompositionSurfaceImage : ICompositionSurfaceImage
    {
        #region Fields

        private ICompositionGeneratorInternal _generator;
        private CompositionDrawingSurface _surface;
        private Uri _uri;
        private CanvasBitmap _canvasBitmap;

        #endregion

        #region Properties

        /// <summary>
        /// SurfaceImage Generator
        /// </summary>
        public ICompositionGenerator Generator => _generator;

        /// <summary>
        /// Surface of the SurfaceImage
        /// </summary>
        public ICompositionSurface Surface => _surface;

        /// <summary>
        /// Uri of the image to be loaded onto the SurfaceImage
        /// </summary>
        public Uri Uri => _uri;

        /// <summary>
        /// SurfaceImage Size
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// Describes how the image is resized to fill its allocated space.
        /// </summary>
        public CompositionSurfaceImageOptions Options { get; private set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">ICompositionMaskGeneratorInternal object</param>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="size">Size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        public CompositionSurfaceImage(ICompositionGeneratorInternal generator, Uri uri, Size size, CompositionSurfaceImageOptions options)
        {
            _generator = generator;
            // Create the Surface of the SurfaceImage
            _surface = _generator.CreateDrawingSurface(size);
            Size = _surface?.Size ?? Size.Empty;
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
        public Task RedrawAsync()
        {
            // Reload the SurfaceImage
            return ReloadSurfaceImageWorkerAsync();
        }

        /// <summary>
        /// Redraws the SurfaceImage with the given image options
        /// </summary>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        public Task RedrawAsync(CompositionSurfaceImageOptions options)
        {
            // Set the image options
            Options = options;
            // Reload the SurfaceImage
            return ReloadSurfaceImageWorkerAsync();
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
            return ReloadSurfaceImageWorkerAsync();
        }

        /// <summary>
        /// Redraws the SurfaceImage by loading image from the new Uri and image options
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded on to the image surface.</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        public Task RedrawAsync(Uri uri, CompositionSurfaceImageOptions options)
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
            return ReloadSurfaceImageWorkerAsync();
        }

        /// <summary>
        /// Resizes the SurfaceImage with the given size and redraws the SurfaceImage by loading 
        /// image from the new Uri.
        /// </summary>
        /// <param name="uri">Uri of the image to be loaded onto the SurfaceImage.</param>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        public async Task RedrawAsync(Uri uri, Size size, CompositionSurfaceImageOptions options)
        {
            // If the given Uri differs from the previously stored Uri
            // dispose the existing canvasBitmap
            if (!_uri.IsEqualTo(uri))
            {
                _canvasBitmap?.Dispose();
                _canvasBitmap = null;
            }

            // Resize the SurfaceImage
            await ResizeInternalAsync(size, false);
            // Set the new Uri of the image to be loaded
            _uri = uri;
            // Set the image options
            Options = options;
            // Reload the SurfaceImage
            await ReloadSurfaceImageWorkerAsync();
        }

        /// <summary>
        /// Resizes the SurfaceImage to the new size.
        /// </summary>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <returns>Task</returns>
        public Task ResizeAsync(Size size)
        {
            // resize the SurfaceImage
            return ResizeInternalAsync(size, true);
        }

        /// <summary>
        /// Resizes the SurfaceImage to the new size.
        /// </summary>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <param name="options">Describes the image's resize and alignment options in the allocated space.</param>
        /// <returns>Task</returns>
        public Task ResizeAsync(Size size, CompositionSurfaceImageOptions options)
        {
            // Set the image options
            Options = options;
            // resize the SurfaceImage
            return ResizeInternalAsync(size, true);
        }

        /// <summary>
        /// Disposes the resources used by the SurfaceImage
        /// </summary>
        public void Dispose()
        {
            _surface?.Dispose();
            _generator.DeviceReplaced -= OnDeviceReplaced;
            _canvasBitmap?.Dispose();
            _canvasBitmap = null;
            _surface = null;
            _generator = null;
            _uri = null;
            Options = null;
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
            // Reload the SurfaceImage
            await ReloadSurfaceImageWorkerAsync();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Resizes the SurfaceImage to the new size.
        /// </summary>
        /// <param name="size">New size of the SurfaceImage</param>
        /// <param name="redraw">Flag to indicate whether the SurfaceImage should be redrawn</param>
        /// <returns>Task</returns>
        public async Task ResizeInternalAsync(Size size, bool redraw)
        {
            // resize the SurfaceImage
            _generator.ResizeDrawingSurface(_surface, size);
            Size = size;
            if (redraw)
            {
                // Reload the SurfaceImage
                await ReloadSurfaceImageWorkerAsync();
            }
        }

        /// <summary>
        /// Helper class to redraw the SurfaceImage
        /// </summary>
        /// <returns>Task</returns>
        private async Task ReloadSurfaceImageWorkerAsync()
        {
            // Cache the canvasBitmap to avoid reloading of the same image during Resize/Redraw operations
            _canvasBitmap = await _generator.ReloadSurfaceImageAsync(_surface, Size, _uri, Options, _canvasBitmap);
        }

        #endregion
    }
}
