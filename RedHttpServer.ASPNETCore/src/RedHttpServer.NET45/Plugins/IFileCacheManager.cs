using System.Collections.Generic;

namespace RHttpServer
{
    /// <summary>
    ///     Interface for classes used for Json serialization and deserialization
    /// </summary>
    public interface IFileCacheManager
    {
        /// <summary>
        ///     File types that are allowed to be cached.
        ///     All extension must be lowercase
        /// </summary>
        HashSet<string> CacheAllowedFileExtension { get; }

        /// <summary>
        ///     The current size of the cache in bytes
        /// </summary>
        long Size { get; }

        /// <summary>
        ///     Method to call when it is needed to empty the page cache.
        ///     Useful if the page files on the harddrive is being modified on runtime.
        /// </summary>
        void EmptyCache();

        /// <summary>
        ///     Checks whether a file can be cached using current settings
        /// </summary>
        /// <param name="filesizeBytes">The lenght of the file</param>
        /// <param name="filename">The filename with extention, can contain path</param>
        /// <returns></returns>
        bool CanAdd(long filesizeBytes, string filename);

        /// <summary>
        ///     Returns to file content to out parameter if found
        /// </summary>
        /// <param name="filepath">The path of the file and key to retrieving it</param>
        /// <param name="content">The content of the file</param>
        /// <returns>False if not found in cache</returns>
        bool TryGetFile(string filepath, out byte[] content);

        /// <summary>
        ///     Attempts to add the file, as it may have been added by another thread.
        /// </summary>
        /// <param name="filepath">The path the file should be located at</param>
        /// <param name="content">The content of the file</param>
        /// <returns>Returns false if file already added</returns>
        bool TryAdd(string filepath, byte[] content);

        /// <summary>
        ///     Attempts to add the file, as it may have been added by another thread.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns>Returns false if file already added</returns>
        bool TryAddFile(string filepath);

        /// <summary>
        ///     Set the max sizes for individual files and
        /// </summary>
        /// <param name="maxFileSizeBytes">Default cache manager uses 0x4000 bytes as default (16 kb)</param>
        /// <param name="maxCacheSizeBytes">Default cache manager uses 0x3200000 bytes as default(50 mb)</param>
        void Configure(int maxFileSizeBytes, long maxCacheSizeBytes);
    }
}