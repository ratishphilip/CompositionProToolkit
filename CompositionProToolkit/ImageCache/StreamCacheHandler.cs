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
using Windows.Storage;
using Windows.Storage.Streams;

namespace CompositionProToolkit
{
    /// <summary>
    /// This class is responsible for caching a IRandomAccessStream in the ImageCache
    /// </summary>
    internal class StreamCacheHandler : ICacheHandler
    {
        #region APIs

        /// <summary>
        /// Checks if this handler can cache the given object
        /// </summary>
        /// <param name="objectToCache">Object to cache</param>
        /// <returns>True if it can cache, otherwise False</returns>
        public bool CanCache(object objectToCache)
        {
            // Check if the objectToCache is a valid IRandomAccessStream
            var stream = objectToCache as IRandomAccessStream;
            // Stream should be readable
            return (stream != null) && stream.CanRead;
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
            // Check if the objectToCache is a valid IRandomAccessStream
            var stream = objectToCache as IRandomAccessStream;
            if ((stream == null) || !stream.CanRead)
                return null;

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
                    // Open the cache file for writing
                    using (var cacheStream = await cacheFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        // Start reading the content as a stream
                        using (var inputStream = stream.GetInputStreamAt(0))
                        {
                            ulong totalBytesRead = 0;
                            var prevProgress = -1;
                            var totalContentLength = stream.Size;
                            if (totalContentLength <= 0UL)
                            {
                                prevProgress = 0;
                            }
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
                catch (Exception)
                {
                    // In case any exception occurs during the copying of the StorageFile
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
