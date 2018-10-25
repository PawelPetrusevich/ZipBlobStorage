using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ZipBlobStorage.Repository
{
    public interface IFtpRepository
    {
        Task<FtpStatusCode> UploadOnFtp(string fileName, Stream fileStream);
    }
}