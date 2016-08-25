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
// CompositionProToolkit v0.4.3
// 

using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace CompositionProToolkit
{
    /// <summary>
    /// Interface for CacheHandlers which can cache
    /// the content specified by a Uri.
    /// </summary>
    internal interface IUriCacheHandler
    {
        /// <summary>
        /// Checks if this handler can cache the Uri
        /// with the given Uri scheme.
        /// </summary>
        /// <param name="uriScheme">Uri Scheme</param>
        /// <returns>True if it can cache, otherwise False</returns>
        bool CanCache(string uriScheme);

        /// <summary>
        /// Caches the given Uri to the Application's ImageCache
        /// and returns the uri of the cached file.
        /// </summary>
        /// <param name="uri">Uri to cache</param>
        /// <param name="cacheFileName">Name of the cache file.</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns>Uri</returns>
        Task<Uri> GetCachedUriAsync(Uri uri, string cacheFileName, CacheProgressHandler progressHandler = null);
    }

    /// <summary>
    /// Interface for CacheHandlers which can cache
    /// the content specified by a StorageFile.
    /// </summary>
    internal interface IStorageFileCacheHandler
    {
        /// <summary>
        /// Checks if this handler can cache the given StorageFile
        /// </summary>
        /// <param name="file">StorageFile</param>
        /// <returns>True if it can cache, otherwise False</returns>
        bool CanCache(StorageFile file);

        /// <summary>
        /// Caches the given StorageFile to the Application's ImageCache
        /// and returns the uri of the cached file.
        /// </summary>
        /// <param name="inputFile">StorageFile to be cached</param>
        /// <param name="cacheFileName">Name of the cache file.</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns>Uri</returns>
        Task<Uri> GetCachedUriAsync(StorageFile inputFile, string cacheFileName, CacheProgressHandler progressHandler = null);
    }
}
