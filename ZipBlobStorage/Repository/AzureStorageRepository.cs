using System;
using System.Data.SqlClient;
using System.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ZipBlobStorage.Repository
{
    public class AzureStorageRepository : IAzureStorageRepository
    {
        private readonly CloudBlobClient client;

        private CloudBlobContainer _container;

        public AzureStorageRepository()
        {
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
            client = account.CreateCloudBlobClient();
        }

        public void UploadZip(Stream stream, string fileName, string mimeType)
        {
            _container = client.GetContainerReference("zip");
            _container.CreateIfNotExists();
            var blob = PrepareBlob(fileName, mimeType);
            stream.Seek(0, SeekOrigin.Begin);
            blob.UploadFromStream(stream);
        }

        public Stream DownloadZip(string fileName)
        {
            _container = client.GetContainerReference("zip");
            var blob = _container.GetBlockBlobReference(fileName);
            var resultStream = new MemoryStream();
            blob.DownloadToStream(resultStream);
            return resultStream;
        }

        private CloudBlockBlob PrepareBlob(string fileName, string mimeType)
        {
            var blob = _container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = mimeType;
            return blob;
        }
    }
}