using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using ZipBlobStorage.Models;
using ZipBlobStorage.Repository;
using ZipBlobStorage.Services;

namespace ZipBlobStorage.Tests
{
    [TestFixture]
    public class ZipServiceTests
    {
        [Test]
        public void LoadTest()
        {
            var path = @"C:\Users\pavel_petrusevich\Desktop\download.jpg";
            var zipService = new ZipService(new AzureStorageRepository());

            List<PreZipEntryModel> collection = new List<PreZipEntryModel>();

            FileStream reader = new FileStream(path, FileMode.Open);

            collection.Add(new PreZipEntryModel(reader));

            zipService.UploadFile(collection);

            reader.Close();
        }

        [Test]
        public void OpenTest()
        {
            var zipService = new ZipService(new AzureStorageRepository());
            zipService.OpenZip();
        }
    }
}