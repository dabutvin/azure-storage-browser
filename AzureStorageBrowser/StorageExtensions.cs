using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageBrowser
{
    public static class StorageExtensions
    {
        /// <summary>
        /// Returns true if the CloudBlockBlob is an image
        /// </summary>
        public static bool IsImage(this CloudBlockBlob cloudBlockBlob)
        {
            var extension = Path.GetExtension(cloudBlockBlob.Name).ToLower();

            if (
                   extension == ".jpg"
                || extension == ".png"
                || extension == ".gif"
                || extension == ".jpeg")
            {
                return true;
            }

            var contentType = cloudBlockBlob.Properties.ContentType.ToLower();

            if (
                   contentType == "image/jpg"
                || contentType == "image/jpeg"
                || contentType == "image/pjpeg"
                || contentType == "image/gif"
                || contentType == "image/x-png"
                || contentType == "image/png"
            )
            {
                return true;
            }

            return false;
        }
    }
}
