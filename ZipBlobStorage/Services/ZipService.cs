using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task UploadFile(RequestModel filePaths)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var filePath in filePaths.Images)
                    {
                        var fileName = CreateFileName(filePath);
                        var entry = zip.CreateEntry(fileName, CompressionLevel.Optimal);
                        using (var binaryWriter = new BinaryWriter(entry.Open()))
                        {
                            var fileInByte = await _azureStorageRepository.LoadImageAsBytesAsync(filePath);
                            binaryWriter.Write(fileInByte);
                        }
                    }
                }

                var zipFileName = CreateZipFileName(filePaths.DealershipId);

                await _azureStorageRepository.UploadZipAsync(memoryStream, zipFileName, MimeMapping.GetMimeMapping(zipFileName)).ConfigureAwait(false);
            }
        }

        // TODO unzip
        public void OpenZip()
        {
            using (var stream = _azureStorageRepository.DownloadZip("TODO"))
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

        // TODO unzip
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
            return $"{fileName.Split('.').First()}/{Guid.NewGuid()}.{fileName.Split('.').Last()}";
        }

        private string CreateZipFileName(string dealershipId)
        {
            return $"{dealershipId}-{Guid.NewGuid()}.zip";
        }
    }
}