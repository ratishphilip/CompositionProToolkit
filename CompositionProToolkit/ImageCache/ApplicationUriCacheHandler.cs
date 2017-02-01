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
using System.Threading.Tasks;

namespace CompositionProToolkit
{
    /// <summary>
    /// This class handles the caching of files which are present in 
    /// the Application package, Application data or any other location
    /// related to the Application whose Uri starts with 'ms-' like
    /// 'ms-appx', 'ms-appdata' for example.
    /// </summary>
    internal class ApplicationUriCacheHandler : ICacheHandler
    {
        #region Constants

        private const string UriScheme = "ms-";

        #endregion

        #region APIs

        /// <summary>
        /// Checks if this handler can cache the given object
        /// </summary>
        /// <param name="objectToCache">Object to cache</param>
        /// <returns>True if it can cache, otherwise False</returns>
        public bool CanCache(object objectToCache)
        {
            // Is the objectToCache is a valid Uri?
            var uri = objectToCache as Uri;
            if (uri != null)
            {
                return !String.IsNullOrWhiteSpace(uri.Scheme) &&
                   uri.Scheme.StartsWith(UriScheme);
            }

            // Is the objectToCache a string representing a Uri?
            var uriString = objectToCache as string;
            if (uriString != null)
            {
                // Try creating the Uri from the uriString
                if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                {
                    return !String.IsNullOrWhiteSpace(uri.Scheme) &&
                   uri.Scheme.StartsWith(UriScheme);
                }
            }

            return false;
        }

        /// <summary>
        /// Caches the given object to the Application's ImageCache
        /// and returns the uri of the cached file.
        /// </summary>
        /// <param name="objectToCache">Object to cache</param>
        /// <param name="cacheFileName">Name of the cache file</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns>Uri</returns>
        public Task<Uri> GetCachedUriAsync(object objectToCache, string cacheFileName,
            CacheProgressHandler progressHandler = null)
        {
            // Check if the objectToCache is a valid Uri
            var uri = objectToCache as Uri;
            if (uri == null)
            {
                // Is the objectToCache a string representing a Uri?
                var uriString = objectToCache as string;
                if (uriString != null)
                {
                    // Try creating the Uri from the uriString
                    if (!(Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri)))
                    {
                        return null;
                    }
                }
            }

            // Caching indicates only 80% of the task completed i.e. 80% progress,
            // Progress will be 100% only when the image is loaded successfully on the ImageFrame
            progressHandler?.Invoke(80);
            // Since the given Uri is contained within the Application data or package, there
            // is no need to cache it. Just return the same uri.
            return Task.Run(() => uri);
        }

        #endregion
    }
}
