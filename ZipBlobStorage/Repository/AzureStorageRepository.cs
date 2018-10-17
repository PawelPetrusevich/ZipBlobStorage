using System;
using System.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ZipBlobStorage.Repository
{
    public class AzureStorageRepository : IAzureStorageRepository
    {
        private readonly CloudBlobContainer _container;


        public AzureStorageRepository()
        {
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudBlobClient client = account.CreateCloudBlobClient();
            _container = client.GetContainerReference("demo");
        }

        public void UploadFile(Stream stream, string fileName, string mimeType)
        {
            var blob = PrepareBlob(fileName, mimeType);
            stream.Position = 0;
            blob.UploadFromStream(stream);
        }

        private CloudBlockBlob PrepareBlob(string fileName, string mimeType)
        {
            var blob = _container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = mimeType;
            return blob;
        }
    }
}