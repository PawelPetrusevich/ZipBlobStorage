using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ZipBlobStorage.Repository
{
    public class AzureStorageRepository : IAzureStorageRepository
    {
#if DEBUG

        private readonly Lazy<string> containerForImagesName = new Lazy<string>(() => ConfigurationManager.AppSettings["ContainerForImagesName"]);

#else

        private readonly Lazy<string> containerForImagesName = new Lazy<string>(() => Environment.GetEnvironmentVariable("ContainerForImagesName"));

#endif


        private readonly CloudBlobClient client;

        private CloudBlobContainer _container;

        public AzureStorageRepository()
        {
#if DEBUG
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
#else
            CloudStorageAccount account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("BlobConnectionString"));
#endif
            client = account.CreateCloudBlobClient();
        }

        public async Task UploadZipAsync(Stream stream, string fileName, string mimeType, string containerName)
        {
            _container = client.GetContainerReference(containerName);
            _container.CreateIfNotExists();
            var blob = PrepareBlob(fileName, mimeType);
            stream.Seek(0, SeekOrigin.Begin);
            await blob.UploadFromStreamAsync(stream);
        }

        public async Task UploadImageAsync(Stream stream, string fileName, string mimeType, string containerName)
        {
            _container = client.GetContainerReference(containerName);
            _container.CreateIfNotExists();
            var blob = PrepareBlob(fileName, mimeType);
            await blob.UploadFromStreamAsync(stream);
        }

        public async Task LoadImageAsync(string fileName, Stream entryStream)
        {
            _container = client.GetContainerReference(containerForImagesName.Value);
            var blob = _container.GetBlockBlobReference(fileName);
            blob.FetchAttributes();

            await blob.DownloadToStreamAsync(entryStream);
        }

        public void DownloadZip(string fileName, string containerName, Stream stream)
        {
            _container = client.GetContainerReference(containerName);
            var blob = _container.GetBlockBlobReference(fileName);
            blob.DownloadToStream(stream);
        }

        private CloudBlockBlob PrepareBlob(string fileName, string mimeType)
        {
            var blob = _container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = mimeType;
            return blob;
        }
    }
}