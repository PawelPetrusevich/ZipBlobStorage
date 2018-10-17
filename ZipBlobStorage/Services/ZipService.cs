using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

using ZipBlobStorage.Models;
using ZipBlobStorage.Repository;

namespace ZipBlobStorage.Services
{
    public class ZipService : IZipService
    {
        private readonly IAzureStorageRepository _azureStorageRepository;

        public ZipService(IAzureStorageRepository azureStorageRepository)
        {
            this._azureStorageRepository = azureStorageRepository;
        }

        public void UploadFile(IEnumerable<PreZipEntryModel> entryModels)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var preZipEntryModel in entryModels)
                    {
                        var fileName = CreateFileName(preZipEntryModel.FileName.Split('\\').Last());
                        var entry = zip.CreateEntry(fileName, CompressionLevel.Optimal);
                        using (var binaryWriter = new BinaryWriter(entry.Open()))
                        {
                            var fileInByte = new byte[preZipEntryModel.FileStream.Length];
                            preZipEntryModel.FileStream.Read(fileInByte, 0, (int)preZipEntryModel.FileStream.Length);
                            binaryWriter.Write(fileInByte);
                        }
                    }
                }

                _azureStorageRepository.UploadFile(memoryStream, "compressedFile.zip", MimeMapping.GetMimeMapping(".zip"));

                using (var writer = new FileStream(@"C:\Users\pavel_petrusevich\Desktop\thumb.zip", FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(writer);
                }
            }
        }

        public void OpenZip()
        {
            using (FileStream stream = new FileStream(@"C:\Users\pavel_petrusevich\Desktop\thumb.zip", FileMode.Open))
            {
                using (var zipFiles = new ZipArchive(stream))
                {
                    foreach (var zipFilesEntry in zipFiles.Entries)
                    {
                        CreateFile(zipFilesEntry);
                    }
                }
            }
        }

        private void CreateFile(ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            {
                using (FileStream stream = new FileStream(@"C:\Users\pavel_petrusevich\Desktop\" + Guid.NewGuid() + ".jpg", FileMode.Create))
                {
                    entryStream.CopyTo(stream);
                }
            }
        }


        private string CreateFileName(string fileName)
        {
            return $"{fileName.Split('.').First()}-{Guid.NewGuid()}.{fileName.Split('.').Last()}";
        }
    }
}