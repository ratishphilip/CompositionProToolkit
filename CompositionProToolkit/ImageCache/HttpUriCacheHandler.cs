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
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace CompositionProToolkit
{
    /// <summary>
    /// This class handles the caching of files which are available on the
    /// web throught the 'http' or 'https' protocol. It uses the Http client 
    /// to download the file and cache it.
    /// </summary>
    internal class HttpUriCacheHandler : ICacheHandler
    {
        #region Constants

        private readonly string[] _uriSchemes = new[] { "http", "https" };

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
                // Check if the objectToCache is a valid Uri with a Scheme
                // equal to either 'http' or 'https'
                return !String.IsNullOrWhiteSpace(uri.Scheme) &&
                   _uriSchemes.Contains(uri.Scheme.ToLower().Trim());
            }

            // Is the objectToCache a string representing a Uri?
            var uriString = objectToCache as string;
            if (uriString != null)
            {
                // Try creating the Uri from the uriString
                if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                {
                    // Check if the objectToCache is a valid Uri with a Scheme
                    // equal to either 'http' or 'https'
                    return !String.IsNullOrWhiteSpace(uri.Scheme) &&
                        _uriSchemes.Contains(uri.Scheme.ToLower().Trim());
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
        public async Task<Uri> GetCachedUriAsync(object objectToCache, string cacheFileName,
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

            // Calculate the expiry date
            var expirationDate = DateTime.Now.Subtract(ImageCache.CacheDuration);
            // Get the cache folder location
            var cacheFolder = await ImageCache.GetCacheFolderAsync();
            if (cacheFolder == null)
                return null;

            // Report Progress
            progressHandler?.Invoke(0);

            // Get the cache file corresponding to the cacheFileName
            var cacheFile = await cacheFolder.TryGetItemAsync(cacheFileName) as StorageFile;

            // Has the cache file expired or does it not exist?
            if (await cacheFile.IsNullOrExpired(expirationDate))
            {
                try
                {
                    // Create/Recreate the cache file
                    cacheFile = await cacheFolder.CreateFileAsync(cacheFileName, CreationCollisionOption.ReplaceExisting);

                    using (var httpClient = new HttpClient())
                    {
                        // Use HttpCompletionOption.ResponseHeadersRead for the GetAsync call so that it returns as 
                        // soon as response headers are received and not when the whole response is received
                        using (var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                        {
                            // If respose status is not success then raise exception
                            response.EnsureSuccessStatusCode();
                            // Open the cache file for writing
                            using (var cacheStream = await cacheFile.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                // Start reading the content as a stream
                                using (var inputStream = await response.Content.ReadAsInputStreamAsync())
                                {
                                    ulong totalBytesRead = 0;
                                    var prevProgress = -1;
                                    var totalContentLength = response.Content.Headers.ContentLength ?? 0UL;
                                    while (true)
                                    {
                                        // Read from the stream
                                        IBuffer buffer = new Windows.Storage.Streams.Buffer(2048);
                                        buffer = await inputStream.ReadAsync(buffer, buffer.Capacity,
                                                                             InputStreamOptions.None);

                                        if (buffer.Length == 0)
                                        {
                                            // There is nothing else to read
                                            break;
                                        }

                                        // The following code can be used to report progress
                                        totalBytesRead += buffer.Length;
                                        if (totalContentLength > 0UL)
                                        {
                                            // We will report a progress percent between 0%-80% because caching represents 80%
                                            // of the task of displaying the image. The other 20% requires successful loading 
                                            // of the cached image.
                                            var progress =
                                                (int)Math.Round((totalBytesRead * 80) / (double)totalContentLength);
                                            if (progress != prevProgress)
                                            {
                                                // Report Progress
                                                progressHandler?.Invoke(progress);
                                                prevProgress = progress;
                                            }
                                        }
                                        else
                                        {
                                            prevProgress = Math.Min(prevProgress + 1, 80);
                                            // Report Progress
                                            progressHandler?.Invoke(prevProgress);
                                        }

                                        // Write to cache file
                                        await cacheStream.WriteAsync(buffer);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // In case any exception occurs during the downloading or caching
                    // delete the cacheFile and return a 'null' Uri
                    cacheFile = await cacheFolder.TryGetItemAsync(cacheFileName) as StorageFile;
                    if (cacheFile != null)
                    {
                        await cacheFile.DeleteAsync();
                    }

                    // Report Progress
                    progressHandler?.Invoke(-1);

                    return null;
                }
            }

            // Report Progress
            // Caching indicates only half of the task completed i.e. 50% progress,
            // Progress will be 100% only when the image is loaded successfully on the ImageFrame
            progressHandler?.Invoke(80);

            // Now that we have a valid cached file for the Uri, return the Uri of the cached inputFile
            return new Uri($"ms-appdata:///temp/{ImageCache.CacheFolderName}/{cacheFileName}");
        }

        #endregion
    }
}
