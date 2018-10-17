using System.IO;

namespace ZipBlobStorage.Repository
{
    public interface IAzureStorageRepository
    {
        void UploadFile(Stream stream, string fileName, string mimeType);
    }
}