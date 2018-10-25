using System;
using System.Collections.Generic;
using System.Configuration;
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
        private readonly Lazy<string> containerForInName = new Lazy<string>(() => ConfigurationManager.AppSettings["ContainerForInName"]);

        private readonly Lazy<string> containerForOutName = new Lazy<string>(()=> ConfigurationManager.AppSettings["ContainerForOutName"]);

        private readonly IAzureStorageRepository _azureStorageRepository;

        private readonly IFtpRepository _ftpRepository;


        public ZipService(IAzureStorageRepository azureStorageRepository, IFtpRepository ftpRepository)
        {
            this._azureStorageRepository = azureStorageRepository;
            _ftpRepository = ftpRepository;
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

                        using (var entryStream = entry.Open())
                        {
                            await _azureStorageRepository.LoadImageAsync(filePath, entryStream);
                        }
                    }
                }

                var zipFileName = CreateZipFileName(archiveModel.DealershipId);

                //await _ftpRepository.UploadOnFtp(zipFileName, memoryStream);

                await _azureStorageRepository.UploadZipAsync(memoryStream, zipFileName, MimeMapping.GetMimeMapping(zipFileName), containerForInName.Value).ConfigureAwait(false);
            }
        }

        // TODO unzip
        public async Task UnZipArchive(string zipName)
        {
            using (var stream = _azureStorageRepository.DownloadZip(zipName, containerForInName.Value))
            {
                using (var zipFiles = new ZipArchive(stream))
                {
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
                await _azureStorageRepository.UploadImageAsync(entryStream, entry.Name, MimeMapping.GetMimeMapping(entry.Name), containerForOutName.Value).ConfigureAwait(false);
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