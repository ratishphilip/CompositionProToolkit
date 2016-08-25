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
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace CompositionProToolkit
{
    /// <summary>
    /// This class handles the caching of files which are present in 
    /// the Application package, Application data or any other location
    /// related to the Application whose Uri starts with 'ms-' like
    /// 'ms-appx', 'ms-appdata' for example.
    /// </summary>
    internal class ApplicationUriCacheHandler : IUriCacheHandler
    {
        #region Constants

        private const string UriScheme = "ms-";

        #endregion

        #region APIs

        /// <summary>
        /// Checks if this handler can cache the Uri
        /// with the given Uri scheme.
        /// </summary>
        /// <param name="uriScheme">Uri Scheme</param>
        /// <returns>True if it can cache, otherwise False</returns>
        public bool CanCache(string uriScheme)
        {
            return !String.IsNullOrWhiteSpace(uriScheme) &&
                   uriScheme.StartsWith(UriScheme);
        }

        /// <summary>
        /// Since the given Uri is contained within the Application data or package, there
        /// is no need to cache it. Just return the same uri.
        /// </summary>
        /// <param name="uri">Uri to cache</param>
        /// <param name="cacheFileName">Hash of the Uri</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns>Uri</returns>
        public Task<Uri> GetCachedUriAsync(Uri uri, string cacheFileName, CacheProgressHandler progressHandler = null)
        {
            // Caching indicates only half of the task completed i.e. 50% progress,
            // Progress will be 100% only when the image is loaded successfully on the 
            // CompositionImageFrame
            progressHandler?.Invoke(50);
            return Task.Run(() => uri);
        }

        #endregion
    }

    /// <summary>
    /// This class handles the caching of files which are available on the
    /// web throught the 'http' or 'https' protocol. It uses the Http client 
    /// to download the file and cache it.
    /// </summary>
    internal class HttpUriCacheHandler : IUriCacheHandler
    {
        #region Constants

        private readonly string[] _uriSchemes = new[] { "http", "https" };

        #endregion

        #region APIs

        /// <summary>
        /// Checks if this handler can cache the Uri
        /// with the given Uri scheme.
        /// </summary>
        /// <param name="uriScheme">Uri Scheme</param>
        /// <returns>True if it can cache, otherwise False</returns>
        public bool CanCache(string uriScheme)
        {
            // Accept only http or https
            return !string.IsNullOrWhiteSpace(uriScheme) &&
                   _uriSchemes.Contains(uriScheme.ToLower().Trim());
        }

        /// <summary>
        /// Caches the given Uri to the Application's ImageCache
        /// and returns the uri of the cached file.
        /// </summary>
        /// <param name="uri">Uri to cache</param>
        /// <param name="cacheFileName">Name of the cache file.</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns>Uri</returns>
        public async Task<Uri> GetCachedUriAsync(Uri uri, string cacheFileName,
            CacheProgressHandler progressHandler = null)
        {
            if (uri == null)
                return null;

            // Calculate the expiry date
            var expirationDate = DateTime.Now.Subtract(ImageCache.CacheDuration);
            // Get the cache folder location
            var cacheFolder = await ImageCache.GetCacheFolderAsync();
            if (cacheFolder == null)
                return null;

            // Report Progress
            progressHandler?.Invoke(-1);

            // Get the cache file corresponding to the cacheFileName
            var cacheFile = await cacheFolder.TryGetItemAsync(cacheFileName) as StorageFile;

            // Has the cache file expired or does it not exist?
            if (await cacheFile.IsNullOrExpired(expirationDate))
            {
                // Report Progress
                progressHandler?.Invoke(0);

                // Create/Recreate the cache file
                cacheFile = await cacheFolder.CreateFileAsync(cacheFileName, CreationCollisionOption.ReplaceExisting);

                try
                {
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
                                    if (totalContentLength <= 0UL)
                                    {
                                        // Report Progress
                                        progressHandler?.Invoke(-1);
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
                                            // We will report a progress percent between 0%-50% because caching represents half
                                            // the task of displaying the image. The other half requires successful loading 
                                            // of the cached image.
                                            var progress = (int)Math.Round((totalBytesRead * 50) / (double)totalContentLength);
                                            if (progress != prevProgress)
                                            {
                                                // Report Progress
                                                progressHandler?.Invoke(progress);
                                                prevProgress = progress;
                                            }
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
                    await cacheFile.DeleteAsync();

                    // Report Progress
                    progressHandler?.Invoke(-1);

                    return null;
                }
            }

            // Report Progress
            // Caching indicates only half of the task completed i.e. 50% progress,
            // Progress will be 100% only when the image is loaded successfully on the 
            // CompositionImageFrame
            progressHandler?.Invoke(50);

            // Now that we have a valid cached file for the Uri, return the Uri of the cached inputFile
            return new Uri($"ms-appdata:///temp/{ImageCache.CacheFolderName}/{cacheFileName}");
        }

        #endregion
    }

    /// <summary>
    /// This class is responsible for caching a StorageFile in the ImageCache
    /// </summary>
    internal class StorageFileCacheHandler : IStorageFileCacheHandler
    {
        #region APIs

        /// <summary>
        /// Checks if this handler can cache the given StorageFile
        /// </summary>
        /// <param name="file">StorageFile</param>
        /// <returns>True if it can cache, otherwise False</returns>
        public bool CanCache(StorageFile file)
        {
            // For now, this class should be able to cache all StorageFiles
            return true;
        }

        /// <summary>
        /// Caches the given Uri to the Application's ImageCache
        /// and returns the uri of the cached file.
        /// </summary>
        /// <param name="inputFile">StorageFile to be cached</param>
        /// <param name="cacheFileName">Name of the cache file.</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns></returns>
        public async Task<Uri> GetCachedUriAsync(StorageFile inputFile, string cacheFileName,
            CacheProgressHandler progressHandler = null)
        {
            if (inputFile == null)
                return null;

            // Calculate the expiry date
            var expirationDate = DateTime.Now.Subtract(ImageCache.CacheDuration);
            // Get the cache folder location
            var cacheFolder = await ImageCache.GetCacheFolderAsync();
            if (cacheFolder == null)
                return null;

            // Report Progress
            progressHandler?.Invoke(-1);

            // Get the cache file corresponding to the cacheFileName
            var cacheFile = await cacheFolder.TryGetItemAsync(cacheFileName) as StorageFile;

            // Has the cache file expired or does it not exist?
            if (await cacheFile.IsNullOrExpired(expirationDate))
            {
                try
                {
                    // Report Progress
                    progressHandler?.Invoke(10);

                    // Copy the storage file to the cacheFolder. If the file already exists, overwrite it
                    await inputFile.CopyAsync(cacheFolder, cacheFileName, NameCollisionOption.ReplaceExisting);

                    // Report Progress
                    // Caching indicates only half of the task completed i.e. 50% progress,
                    // Progress will be 100% only when the image is loaded successfully on the 
                    // CompositionImageFrame
                    progressHandler?.Invoke(50);
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
            progressHandler?.Invoke(100);

            // Now that we have a valid cached file for the Uri, return the Uri of the cached inputFile
            return new Uri($"ms-appdata:///temp/{ImageCache.CacheFolderName}/{cacheFileName}");
        }

        #endregion
    }
}
