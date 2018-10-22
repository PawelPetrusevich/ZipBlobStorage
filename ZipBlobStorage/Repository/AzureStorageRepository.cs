using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

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

        public async Task UploadZipAsync(Stream stream, string fileName, string mimeType)
        {
            _container = client.GetContainerReference("zip");
            _container.CreateIfNotExists();
            var blob = PrepareBlob(fileName, mimeType);
            stream.Seek(0, SeekOrigin.Begin);
            await blob.UploadFromStreamAsync(stream);
        }

        public async Task<byte[]> LoadImageAsBytesAsync(string fileName)
        {
            _container = client.GetContainerReference("images");
            var blob = _container.GetBlockBlobReference(fileName);
            blob.FetchAttributes();
            var length = blob.Properties.Length;
            var byteArray = new byte[length];
            await blob.DownloadToByteArrayAsync(byteArray, 0);
            return byteArray;
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