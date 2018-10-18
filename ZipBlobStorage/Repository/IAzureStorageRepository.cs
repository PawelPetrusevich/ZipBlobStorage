using System.IO;

namespace ZipBlobStorage.Repository
{
    public interface IAzureStorageRepository
    {
        void UploadZip(Stream stream, string fileName, string mimeType);

        Stream DownloadZip(string fileName);
    }
}