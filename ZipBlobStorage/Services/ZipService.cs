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
        private const string IN_CONTAINER_NAME = "in-container";

        private const string OUT_CONTAINER_NAME = "out-container";

        private readonly IAzureStorageRepository _azureStorageRepository;

        public ZipService(IAzureStorageRepository azureStorageRepository)
        {
            this._azureStorageRepository = azureStorageRepository;
        }

        public async Task UploadFile(RequestModel archiveModel)
        {
            if (archiveModel == null)
            {
                throw new ArgumentNullException($"{nameof(archiveModel)} can not be null.");
            }

            if (archiveModel.Images == null || !archiveModel.Images.Any())
            {
                throw new ArgumentException($"{nameof(archiveModel)} can not be empty.");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var filePath in archiveModel.Images)
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

                var zipFileName = CreateZipFileName(archiveModel.DealershipId);

                await _azureStorageRepository.UploadZipAsync(memoryStream, zipFileName, MimeMapping.GetMimeMapping(zipFileName), IN_CONTAINER_NAME).ConfigureAwait(false);
            }
        }

        // TODO unzip
        public async Task UnZipArchive(string zipName)
        {
            using (var stream = _azureStorageRepository.DownloadZip(zipName, IN_CONTAINER_NAME))
            {
                using (var zipFiles = new ZipArchive(stream))
                {
                    if (zipFiles.Entries == null || zipFiles.Entries.Any())
                    {
                        throw new Exception($"{nameof(zipName)} is empty.");
                    }

                    foreach (var zipFilesEntry in zipFiles.Entries)
                    {
                        await CreateFile(zipFilesEntry);
                    }
                }
            }
        }

        // TODO unzip
        private async Task CreateFile(ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            {
                await _azureStorageRepository.UploadImageAsync(entryStream, entry.Name, MimeMapping.GetMimeMapping(entry.Name), OUT_CONTAINER_NAME).ConfigureAwait(false);
            }
        }


        private string CreateFileName(string fileName)
        {
            return $"{fileName.Split('.').First()}/{Guid.NewGuid()}.{fileName.Split('.').Last()}";
        }

        private string CreateZipFileName(string dealershipId)
        {
            return $"{dealershipId}.zip";
        }
    }
}