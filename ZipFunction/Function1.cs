using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

using ZipBlobStorage.Models;
using ZipBlobStorage.Repository;
using ZipBlobStorage.Services;


namespace ZipFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // {
            //    "DealershipId":"TODO",
            //    "Images":["download.jpg","download(1).jpg","download(2).jpg"]
            // }

            var result = await req.Content.ReadAsAsync<RequestModel>();

            var zipService = new ZipService(new AzureStorageRepository());

            await zipService.UploadFile(result).ConfigureAwait(false);

            var zipName = "TODO.zip";

            await zipService.UnZipArchive(zipName);

            return null;
        }
    }
}
