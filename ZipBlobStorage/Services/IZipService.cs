using System;
using System.Threading.Tasks;

using ZipBlobStorage.Models;

namespace ZipBlobStorage.Services
{
    public interface IZipService
    {
        Task UploadFile(RequestModel archiveModel);

        Task UnZipArchive(string zipName);
    }
}