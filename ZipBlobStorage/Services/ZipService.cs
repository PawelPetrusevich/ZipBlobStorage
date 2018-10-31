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
#if DEBUG
        private readonly Lazy<string> containerForInName = new Lazy<string>(() => ConfigurationManager.AppSettings["ContainerForInName"]);

        private readonly Lazy<string> containerForOutName = new Lazy<string>(() => ConfigurationManager.AppSettings["ContainerForOutName"]);
#else

        private readonly Lazy<string> containerForInName = new Lazy<string>(() => Environment.GetEnvironmentVariable("ContainerForInName"));

        private readonly Lazy<string> containerForOutName = new Lazy<string>(() => Environment.GetEnvironmentVariable("ContainerForOutName"));

#endif

        private readonly IAzureStorageRepository _azureStorageRepository;

        private readonly IFtpRepository _ftpRepository;


        public ZipService(IAzureStorageRepository azureStorageRepository, IFtpRepository ftpRepository)
        {
            this._azureStorageRepository = azureStorageRepository;
            _ftpRepository = ftpRepository;
        }

        public async Task UploadFile(RequestModel archiveModel, string functionPhysicPath)
        {
            if (archiveModel == null)
            {
                throw new ArgumentNullException($"{nameof(archiveModel)} can not be null.");
            }

            if (archiveModel.Images == null || !archiveModel.Images.Any())
            {
                throw new ArgumentException($"{nameof(archiveModel)} can not be empty.");
            }

            if (string.IsNullOrWhiteSpace(functionPhysicPath))
            {
                throw new ArgumentNullException($"{nameof(functionPhysicPath)} can not be null or empty.");
            }

            using (var memoryStream = new FileStream(functionPhysicPath, FileMode.Create))
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

        public string CombineFilePath(string functionPhysicPath)
        {
            var dirInfo = new DirectoryInfo(functionPhysicPath);

            dirInfo.CreateSubdirectory("TempData");

            return Path.Combine(functionPhysicPath, "TempData");
        }

        /// <summary>
        /// Unzip zip-file from blob to blob.
        /// </summary>
        /// <param name="zipName">Zip-file name.</param>
        /// <returns>Response model.</returns>
        public async Task<IEnumerable<UnZipResponseModel>> UnZipArchive(string zipName)
        {
            var responseModels = new List<UnZipResponseModel>();
            using (var stream = _azureStorageRepository.DownloadZip(zipName, containerForInName.Value))
            {
                using (var zipFiles = new ZipArchive(stream))
                {
                    foreach (var zipFilesEntry in zipFiles.Entries)
                    {
                        var fileName = CreateImageName(zipFilesEntry);
                        AddInfoToResponseMessage(zipFilesEntry, responseModels, fileName);
                        await CreateFile(zipFilesEntry, fileName);
                    }
                }
            }

            return responseModels;
        }


        /// <summary>
        /// Add info about file to the response model.
        /// </summary>
        /// <param name="entry">
        /// Zip entry.
        /// </param>
        /// <param name="responseModels">
        /// Response model.
        /// </param>
        /// <param name="fileGuid">
        /// The file Name.
        /// </param>
        private void AddInfoToResponseMessage(ZipArchiveEntry entry, List<UnZipResponseModel> responseModels, string fileName)
        {
            var stockId = entry.FullName.Split('_').First();

            if (!responseModels.Any(x => x.StockId.Equals(stockId)))
            {
                responseModels.Add(new UnZipResponseModel
                {
                    StockId = stockId,
                    Image = new List<string>()
                });
            }

            responseModels.Single(x => x.StockId.Equals(stockId)).Image.Add(fileName);
        }

        // TODO file name pattern
        public string CreateImageName(ZipArchiveEntry entry)
        {
            var stockId = entry.FullName.Split('_').First();
            return $"{stockId}/{stockId}_1_{Guid.NewGuid()}";
        }


        private async Task CreateFile(ZipArchiveEntry entry, string fileName)
        {
            using (var entryStream = entry.Open())
            {
                await _azureStorageRepository.UploadImageAsync(entryStream, fileName, MimeMapping.GetMimeMapping(entry.Name), containerForOutName.Value).ConfigureAwait(false);
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