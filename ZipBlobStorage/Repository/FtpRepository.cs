using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ZipBlobStorage.Repository
{
    public class FtpRepository : IFtpRepository
    {
        private const string FTP_URL = "ftp url";

        private const string FTP_USER_NAME = "user name";

        private const string FTP_PASSWORD = "user password";


        public async Task<FtpStatusCode> UploadOnFtp(string fileName, Stream fileStream)
        {
            FtpWebRequest ftpClient = (FtpWebRequest)WebRequest.Create(FTP_URL + fileName);
            ftpClient.Credentials = new NetworkCredential(FTP_USER_NAME, FTP_PASSWORD);
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