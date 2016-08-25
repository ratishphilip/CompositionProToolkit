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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;

namespace CompositionProToolkit
{
    public delegate void CacheProgressHandler(int progress);

    /// <summary>
    /// This class is responsible for caching images which can be
    /// obtained through a Uri or which are represented by a StorageFile.
    /// The images are cached in the Application's temporary folder.
    /// </summary>
    public static class ImageCache
    {
        #region Constants

        private const string DefaultCacheFolderName = "ImageCache";

        #endregion

        #region Fields

        private static readonly SemaphoreSlim CacheFolderSemaphore = new SemaphoreSlim(1);
        private static readonly Dictionary<string, Task<Uri>> ConcurrentUriTasks = new Dictionary<string, Task<Uri>>();
        private static readonly Dictionary<string, Task<Uri>> ConcurrentFileTasks = new Dictionary<string, Task<Uri>>();
        private static readonly List<IUriCacheHandler> UriCacheHandlers = new List<IUriCacheHandler>();
        private static readonly List<IStorageFileCacheHandler> FileCacheHandlers = new List<IStorageFileCacheHandler>();
        private static StorageFolder _cacheFolder;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the life duration of every cache entry.
        /// </summary>
        internal static TimeSpan CacheDuration { get; set; }

        /// <summary>
        /// Gets the Cache Folder name
        /// </summary>
        internal static string CacheFolderName => DefaultCacheFolderName;

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Static Constructor
        /// </summary>
        static ImageCache()
        {
            // Default cache duration is a day
            CacheDuration = TimeSpan.FromHours(24);
            // Uri Cache Handlers
            UriCacheHandlers.Add(new ApplicationUriCacheHandler());
            UriCacheHandlers.Add(new HttpUriCacheHandler());
            // StorageFile Cache Handlers
            FileCacheHandlers.Add(new StorageFileCacheHandler());
        }

        #endregion

        #region Internal APIs

        /// <summary>
        /// Adds IUriCacheHandlers to the UriCacheHandlers
        /// </summary>
        /// <param name="cacheHandlers">IUriCacheHandlers</param>
        internal static void AddUriCacheHandlers(IEnumerable<IUriCacheHandler> cacheHandlers)
        {
            if (cacheHandlers == null)
                return;

            UriCacheHandlers.AddRange(cacheHandlers);
        }

        /// <summary>
        /// Adds IStorageFileCacheHandler to the FileCacheHandlers
        /// </summary>
        /// <param name="cacheHandlers">IStorageFileCacheHandlers</param>
        internal static void AddFileCacheHandlers(IEnumerable<IStorageFileCacheHandler> cacheHandlers)
        {
            if (cacheHandlers == null)
                return;

            FileCacheHandlers.AddRange(cacheHandlers);
        }

        /// <summary>
        /// Clears the UriCacheHandlers
        /// </summary>
        internal static void ClearUriCacheHandlers()
        {
            UriCacheHandlers.Clear();
        }

        /// <summary>
        /// Clears the FileCacheHandlers
        /// </summary>
        internal static void ClearFileCacheHandlers()
        {
            FileCacheHandlers.Clear();
        }

        /// <summary>
        /// Creates a Uri from the Hashed value of the Uri's AbsoluteUri
        /// </summary>
        /// <param name="uri">Uri to hash</param>
        /// <returns>Hashed Uri</returns>
        internal static Uri GetHashedUri(Uri uri)
        {
            if (uri == null)
                return null;

            // Get the hash file name of the Uri
            var hashFileName = GetHashedFileName(uri);

            return new Uri($"ms-appdata:///temp/{CacheFolderName}/{hashFileName}");
        }

        #endregion

        #region APIs

        /// <summary>
        /// Caches the image obtained from the given Uri to the ImageCache
        /// and provides the Uri to the cached file.
        /// </summary>
        /// <param name="uri">Uri of the image</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns>Uri of the cached file</returns>
        public static async Task<Uri> GetCachedUriAsync(Uri uri, CacheProgressHandler progressHandler = null)
        {
            if (uri == null)
                return null;

            Task<Uri> task;

            // Get the hashed value of the Uri
            var hashKey = GetHashedFileName(uri);

            // Check if another task requesting the the same Uri exists
            lock (ConcurrentUriTasks)
            {
                if (ConcurrentUriTasks.ContainsKey(hashKey))
                {
                    task = ConcurrentUriTasks[hashKey];
                }
                else
                {
                    var cacheHandler = UriCacheHandlers.FirstOrDefault(h => h.CanCache(uri.Scheme));
                    if (cacheHandler == null)
                        return null;

                    task = cacheHandler.GetCachedUriAsync(uri, hashKey, progressHandler);
                    ConcurrentUriTasks.Add(hashKey, task);
                }
            }

            // Wait for the task to complete
            try
            {
                return await task;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                lock (ConcurrentUriTasks)
                {
                    if (ConcurrentUriTasks.ContainsKey(hashKey))
                    {
                        ConcurrentUriTasks.Remove(hashKey);
                    }
                }
            }
        }

        /// <summary>
        /// Caches the image represented by the given StorageFile to
        ///  the ImageCache and provides the Uri to the cached file.
        /// </summary>
        /// <param name="file">StorageFile</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns>Uri of the cached file</returns>
        public static async Task<Uri> GetCachedUriAsync(StorageFile file, CacheProgressHandler progressHandler = null)
        {
            if (file == null)
                return null;

            Task<Uri> task;

            // Get the hashed value of the Uri
            var hashKey = GetHashedFileName(file);

            // Check if another task requesting the the same Uri exists
            lock (ConcurrentFileTasks)
            {
                if (ConcurrentFileTasks.ContainsKey(hashKey))
                {
                    task = ConcurrentFileTasks[hashKey];
                }
                else
                {
                    var cacheHandler = FileCacheHandlers.FirstOrDefault(h => h.CanCache(file));
                    if (cacheHandler == null)
                        return null;

                    task = cacheHandler.GetCachedUriAsync(file, hashKey, progressHandler);
                    ConcurrentFileTasks.Add(hashKey, task);
                }
            }

            // Wait for the task to complete
            try
            {
                return await task;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                lock (ConcurrentFileTasks)
                {
                    if (ConcurrentFileTasks.ContainsKey(hashKey))
                    {
                        ConcurrentFileTasks.Remove(hashKey);
                    }
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Generates the name of the cache file by hashing
        /// the AbsoluteUri of the Uri.
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns>Hashed file name</returns>
        private static string GetHashedFileName(Uri uri)
        {
            return $"{ComputeHash(uri.AbsoluteUri)}.jpg";
        }

        /// <summary>
        /// Generates the name of the cache file by hashing
        /// the Path of the StorageFile.
        /// </summary>
        /// <param name="file">StorageFile</param>
        /// <returns>Hashed file name</returns>
        private static string GetHashedFileName(StorageFile file)
        {
            return $"{ComputeHash(file.Path)}.jpg";
        }

        /// <summary>
        /// Computes the SHA1 hash of the given string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Hashed string</returns>
        private static string ComputeHash(string input)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var bufferHash = provider.HashData(buffer);
            var hash = CryptographicBuffer.EncodeToHexString(bufferHash).ToLower();
            return hash;
        }

        /// <summary>
        /// Gets the StorageFolder where the images are cached.
        /// </summary>
        /// <returns>StorageFolder</returns>
        internal static async Task<StorageFolder> GetCacheFolderAsync()
        {
            if (_cacheFolder != null)
                return _cacheFolder;

            await CacheFolderSemaphore.WaitAsync();
            try
            {
                _cacheFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(CacheFolderName,
                    CreationCollisionOption.OpenIfExists);
            }
            catch (Exception)
            {
                // Do not throw here because null cacheFolder will be
                // handled appropriately by the IUriCacheHandler or IStorageFileCacheHandler
            }
            finally
            {
                CacheFolderSemaphore.Release();
            }

            return _cacheFolder;
        }

        /// <summary>
        /// Clears the entire image cache.
        /// </summary>
        /// <param name="duration">Defines a timespan from now to select 
        /// which cache entries to delete.</param>
        /// <returns>Task</returns>
        internal static async Task ClearCacheAsync(TimeSpan? duration = null)
        {
            duration = duration ?? TimeSpan.FromSeconds(0);
            var expirationDate = DateTime.Now.Subtract(duration.Value);
            try
            {
                var folder = await GetCacheFolderAsync();
                if (folder == null)
                    return;

                foreach (var file in await folder.GetFilesAsync())
                {
                    try
                    {
                        if ((await file.GetBasicPropertiesAsync()).DateModified < expirationDate)
                        {
                            await file.DeleteAsync();
                        }
                    }
                    catch
                    {
                        // Just ignore errors for now
                    }
                }
            }
            catch
            {
                // Just ignore errors for now
            }
        }

        /// <summary>
        /// Extension method to check if the given StorageFile has not been modified
        /// in the given timespan from now.
        /// </summary>
        /// <param name="file">StorageFile</param>
        /// <param name="expirationDate">Defines a timespan from now to select 
        /// which cache entries to delete.</param>
        /// <returns></returns>
        internal static async Task<bool> IsNullOrExpired(this StorageFile file, DateTime expirationDate)
        {
            if (file == null)
                return true;

            var properties = await file.GetBasicPropertiesAsync();
            return properties.DateModified < expirationDate;
        }

        #endregion
    }
}
