using System.IO;
using System.Threading.Tasks;

namespace ZipBlobStorage.Repository
{
    public interface IAzureStorageRepository
    {
        Task UploadZipAsync(Stream stream, string fileName, string mimeType);

        Task<byte[]> LoadImageAsBytesAsync(string fileName);

        Stream DownloadZip(string fileName);
    }
}