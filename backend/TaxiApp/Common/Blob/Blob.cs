using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Diagnostics;
using System.Drawing;

namespace Common.Blob
{
    public class Blob
    {
        private const string connectionString = "UseDevelopmentStorage=true";
        private const string containerName = "taxi-blob-container";

        // blobname - guid + ".jpg"

        // Vraća URL koji se čuva u User.PhotoUrl
        // Postavi container na public
        public string UploadImage(Bitmap image, string blobName)
        {
            try
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobStorage = cloudStorageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobStorage.GetContainerReference(containerName);
                container.CreateIfNotExistsAsync();

                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Save the image to the memory stream
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                    // Reset the position to the beginning of the stream before uploading
                    memoryStream.Position = 0;

                    // Set the content type
                    blob.Properties.ContentType = "image/png";

                    // Upload the image from the memory stream
                    blob.UploadFromStreamAsync(memoryStream);

                    // Return the URI of the uploaded blob
                    return blob.Uri.ToString();
                }
            }
            catch (StorageException ex)
            {
                Trace.WriteLine($"StorageException: {ex.Message}");
                throw; // Optional: Rethrow the exception to propagate it further
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception: {ex.Message}");
                throw; // Optional: Rethrow the exception to propagate it further
            }
        }
    }
}
