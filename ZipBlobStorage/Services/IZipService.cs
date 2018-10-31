using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ZipBlobStorage.Models;

namespace ZipBlobStorage.Services
{
    public interface IZipService
    {
        Task UploadFile(RequestModel archiveModel, string functionPhysicPath);

        Task<IEnumerable<UnZipResponseModel>> UnZipArchive(string zipName);
    }
}