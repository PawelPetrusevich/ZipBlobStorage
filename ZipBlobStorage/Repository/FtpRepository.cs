using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace ZipBlobStorage.Repository
{
    public class FtpRepository : IFtpRepository
    {
        private readonly Lazy<string> ftpUrl = new Lazy<string>(() => Environment.GetEnvironmentVariable("FtpUrl"));

        private readonly Lazy<string> ftpUserName = new Lazy<string>(() => Environment.GetEnvironmentVariable("FtpUserName"));

        private readonly Lazy<string> ftpUserPassword = new Lazy<string>(() => Environment.GetEnvironmentVariable("FtpUserPassword"));



        public async Task<FtpStatusCode> UploadOnFtp(string fileName, Stream fileStream)
        {
            string extension = Path.GetExtension(fileName).Replace(".", string.Empty);
            string name = Path.GetFileNameWithoutExtension(fileName);

            var tempZipName = CreateTempZipName(name, extension);
            var rightZipName = CreateRightZipName(name, extension);


            var result = await UploadZip(ftpUrl.Value, tempZipName, fileStream);
            if (result != FtpStatusCode.ClosingData)
            {
                return result;
            }

            result = await RenameZip(ftpUrl.Value, tempZipName, rightZipName);

            return result;
        }

        private async Task<FtpStatusCode> RenameZip(string url, string oldFileName, string newFileName)
        {
            var ftpClient = CreateFtpRequest(url, oldFileName);
            ftpClient.Method = WebRequestMethods.Ftp.Rename;
            ftpClient.RenameTo = newFileName;

            using (var response = (FtpWebResponse)await ftpClient.GetResponseAsync())
            {
                return response.StatusCode;
            }
        }

        private async Task<FtpStatusCode> UploadZip(string url, string tempZipName, Stream fileStream)
        {
            var ftpClient = CreateFtpRequest(url, tempZipName);
            ftpClient.Method = WebRequestMethods.Ftp.UploadFile;
            ftpClient.ContentLength = fileStream.Length;
            using (var ftpStream = ftpClient.GetRequestStream())
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                await fileStream.CopyToAsync(ftpStream);
            }

            using (FtpWebResponse response = (FtpWebResponse)await ftpClient.GetResponseAsync())
            {
                return response.StatusCode;
            }
        }

        private FtpWebRequest CreateFtpRequest(string url, string fileName)
        {
            FtpWebRequest ftpClient = (FtpWebRequest)WebRequest.Create(url + fileName);
            //ftpClient.Credentials = new NetworkCredential(ftpUserName.Value, ftpUserPassword.Value);
            ftpClient.UseBinary = true;
            ftpClient.UsePassive = true;
            ftpClient.KeepAlive = true;
            return ftpClient;
        }


        private string CreateTempZipName(string name, string extension)
        {
            return $"{name}._{extension}";
        }

        private string CreateRightZipName(string name, string extension)
        {
            return $"{name}.{extension}";
        }
    }
}