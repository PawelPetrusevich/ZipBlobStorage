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

        private const string ZIP_FILE_NAME = "compressedFile.zip";

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

                _azureStorageRepository.UploadZip(memoryStream, ZIP_FILE_NAME, MimeMapping.GetMimeMapping(".zip"));
            }
        }

        public void OpenZip()
        {
            using (var stream = _azureStorageRepository.DownloadZip(ZIP_FILE_NAME))
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
                using (FileStream stream = new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{entry.FullName}", FileMode.Create))
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