using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace ZipBlobStorage.Repository
{
    public class FtpRepository : IFtpRepository
    {
#if DEBUG
        private readonly Lazy<string> ftpUrl = new Lazy<string>(() => ConfigurationManager.AppSettings["FtpUrl"]);

        private readonly Lazy<string> ftpUserName = new Lazy<string>(() => ConfigurationManager.AppSettings["FtpUserName"]);

        private readonly Lazy<string> ftpUserPassword = new Lazy<string>(() => ConfigurationManager.AppSettings["FtpUserPassword"]);
#else
        private readonly Lazy<string> ftpUrl = new Lazy<string>(() => Environment.GetEnvironmentVariable("FtpUrl"));

        private readonly Lazy<string> ftpUserName = new Lazy<string>(() => Environment.GetEnvironmentVariable("FtpUserName"));

        private readonly Lazy<string> ftpUserPassword = new Lazy<string>(() => Environment.GetEnvironmentVariable("FtpUserPassword"));

#endif


        public async Task<FtpStatusCode> UploadOnFtp(string fileName, Stream fileStream)
        {
            FtpWebRequest ftpClient = (FtpWebRequest)WebRequest.Create(ftpUrl.Value + fileName);
            ftpClient.Credentials = new NetworkCredential(ftpUserName.Value, ftpUserPassword.Value);
            ftpClient.Method = WebRequestMethods.Ftp.UploadFile;
            ftpClient.UseBinary = true;
            ftpClient.KeepAlive = true;
            using (var ftpStream = ftpClient.GetRequestStream())
            {
                await fileStream.CopyToAsync(ftpStream);
            }

            using (FtpWebResponse response = (FtpWebResponse)await ftpClient.GetResponseAsync())
            {
                return response.StatusCode;
            }
        }
    }
}