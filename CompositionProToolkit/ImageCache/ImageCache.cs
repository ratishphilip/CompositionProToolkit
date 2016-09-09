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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

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
        private static readonly Dictionary<string, Task<Uri>> ConcurrentCacheTasks = new Dictionary<string, Task<Uri>>();
        private static readonly List<ICacheHandler> CacheHandlers = new List<ICacheHandler>();
        private static readonly HashAlgorithmProvider AlgorithmProvider;
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

            // Cache Handlers
            CacheHandlers.Add(new ApplicationUriCacheHandler());
            CacheHandlers.Add(new HttpUriCacheHandler());
            CacheHandlers.Add(new StorageFileCacheHandler());
            CacheHandlers.Add(new StreamCacheHandler());

            // Algorithm Provider
            AlgorithmProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
        }

        #endregion

        #region Internal APIs

        /// <summary>
        /// Adds collection of ICacheHandlers to the CacheHandlers
        /// </summary>
        /// <param name="cacheHandlers">Collection of ICacheHandlers</param>
        internal static void AddCacheHandlers(IEnumerable<ICacheHandler> cacheHandlers)
        {
            if (cacheHandlers == null)
                return;

            CacheHandlers.AddRange(cacheHandlers);
        }

        /// <summary>
        /// Clears the CacheHandlers
        /// </summary>
        internal static void ClearCacheHandlers()
        {
            CacheHandlers.Clear();
        }

        /// <summary>
        /// Creates a Uri from the Hashed value of the given object.
        /// The object must be one of the following types:
        /// 1. Uri
        /// 2. StorageFile
        /// 3. IRandomAccessStream
        /// </summary>
        /// <param name="objectToHash">Object to hash</param>
        /// <returns>Hashed Uri</returns>
        internal static async Task<Uri> GetHashedUriAsync(object objectToHash)
        {
            if (objectToHash == null)
                return null;

            var fileName = await GetHashedFileNameAsync(objectToHash);
            return (fileName == null) ? null : new Uri($"ms-appdata:///temp/{CacheFolderName}/{fileName}");
        }

        #endregion

        #region APIs

        /// <summary>
        /// Caches the given object to the Application's ImageCache
        /// and returns the uri of the cached file.
        /// </summary>
        /// <param name="objectToCache">Object to cache</param>
        /// <param name="progressHandler">Delegate for handling progress</param>
        /// <returns>Uri of the cached file</returns>
        public static async Task<Uri> GetCachedUriAsync(object objectToCache,
            CacheProgressHandler progressHandler = null)
        {
            if (objectToCache == null)
                return null;

            Task<Uri> task;

            // Get the hashed value of the object
            var hashKey = await GetHashedFileNameAsync(objectToCache);

            // Check if another task, requesting the cached file of 
            // the same object, exists
            lock (ConcurrentCacheTasks)
            {
                if (ConcurrentCacheTasks.ContainsKey(hashKey))
                {
                    task = ConcurrentCacheTasks[hashKey];
                }
                else
                {
                    var cacheHandler = CacheHandlers.FirstOrDefault(h => h.CanCache(objectToCache));
                    if (cacheHandler == null)
                        return null;

                    task = cacheHandler.GetCachedUriAsync(objectToCache, hashKey, progressHandler);
                    ConcurrentCacheTasks.Add(hashKey, task);
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
                lock (ConcurrentCacheTasks)
                {
                    if (ConcurrentCacheTasks.ContainsKey(hashKey))
                    {
                        ConcurrentCacheTasks.Remove(hashKey);
                    }
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the hashed file name for the given object. Supported object types
        /// include - Uri, StorageFile and IRandomAccessStream
        /// </summary>
        /// <param name="objectToHash">Object to be hashed.</param>
        /// <returns>Hashed file name</returns>
        private static async Task<string> GetHashedFileNameAsync(object objectToHash)
        {
            // Is the object a Uri?
            var uri = objectToHash as Uri;
            if (uri != null)
            {
                return $"{ComputeHash(uri.AbsoluteUri)}.jpg";
            }

            // Is the object a string representing a Uri?
            var uriString = objectToHash as string;
            if (uriString != null)
            {
                // Try creating the Uri from the uriString
                if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                {
                    return $"{ComputeHash(uri.AbsoluteUri)}.jpg";
                }
            }

            // Is the object a StorageFile?
            var file = objectToHash as StorageFile;
            if (file != null)
            {
                return $"{ComputeHash(file.Path)}.jpg";
            }

            // Is the object a readable IRandomAccessStream?
            var stream = objectToHash as IRandomAccessStream;
            if ((stream != null) && (stream.CanRead))
            {
                return $"{await ComputeHashAsync(stream)}.jpg";
            }

            return null;
        }

        ///// <summary>
        ///// Generates the name of the cache file by hashing
        ///// the AbsoluteUri of the Uri.
        ///// </summary>
        ///// <param name="uri">Uri</param>
        ///// <returns>Hashed file name</returns>
        //private static string GetHashedFileName(Uri uri)
        //{
        //    return $"{ComputeHash(uri.AbsoluteUri)}.jpg";
        //}

        ///// <summary>
        ///// Generates the name of the cache file by hashing
        ///// the Path of the StorageFile.
        ///// </summary>
        ///// <param name="file">StorageFile</param>
        ///// <returns>Hashed file name</returns>
        //private static string GetHashedFileName(StorageFile file)
        //{
        //    return $"{ComputeHash(file.Path)}.jpg";
        //}

        //private static async Task<string> GetHashedFileNameAsync(IRandomAccessStream stream)
        //{
        //    return $"{await ComputeHashAsync(stream)}.jpg";

        //    //IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);
        //    //buffer = await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
        //    //var bufferHash = _algorithmProvider.HashData(buffer);
        //    //var hash = CryptographicBuffer.EncodeToHexString(bufferHash).ToLower();


        //    //var dr = new DataReader(stream.GetInputStreamAt(0));
        //    //var bytes = new byte[stream.Size];
        //    //await dr.LoadAsync((uint)stream.Size);
        //    //dr.ReadBytes(bytes);


        //    //// Convert stream to IBuffer
        //    //using (var memoryStream = new MemoryStream())
        //    //{

        //    //    memoryStream.Capacity = (int)stream.Size;
        //    //    var iBuffer = memoryStream.GetWindowsRuntimeBuffer();
        //    //    await stream.ReadAsync(iBuffer, (uint)stream.Size, InputStreamOptions.None).AsTask().ConfigureAwait(false);
        //    //}
        //}

        /// <summary>
        /// Computes the SHA1 hash of the given string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Hashed string</returns>
        private static string ComputeHash(string input)
        {
            // Convert string to IBuffer
            var buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            // Calculate the hash
            var bufferHash = AlgorithmProvider.HashData(buffer);
            // Encode to hexadecimal lowercase characters
            var hash = CryptographicBuffer.EncodeToHexString(bufferHash).ToLower();

            return hash;
        }

        /// <summary>
        /// Computes the SHA1 hash of the given IRandomAccessStream
        /// </summary>
        /// <param name="stream">IRandomAccessStream</param>
        /// <returns>Hashed string</returns>
        private static async Task<string> ComputeHashAsync(IRandomAccessStream stream)
        {
            // Copy the stream to IBuffer
            IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);
            buffer = await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
            // Calculate the hash
            var bufferHash = AlgorithmProvider.HashData(buffer);
            // Encode to hexadecimal lowercase characters
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
