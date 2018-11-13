using System.IO;
using System.Threading.Tasks;

namespace ZipBlobStorage.Repository
{
    public interface IAzureStorageRepository
    {
        Task UploadZipAsync(Stream stream, string fileName, string mimeType, string containerName);

        Task UploadImageAsync(Stream stream, string fileName, string mimeType, string containerName);

        Task LoadImageAsync(string fileName, Stream entryStream);

        void DownloadZip(string fileName, string containerName, Stream stream);
    }
}