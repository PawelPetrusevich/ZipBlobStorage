using System.IO;
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
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req,
            [Inject(typeof(IZipService))]IZipService zipService,
            TraceWriter log,
            ExecutionContext context)
        {
            // {
            //    "DealershipId":"TODO",
            //    "Images":["download.jpg","download(1).jpg","download(2).jpg"]
            // }

            var zipName = "TODO.zip";

            var result = await req.Content.ReadAsAsync<RequestModel>();

            var path = Path.Combine(context.FunctionAppDirectory, zipName);

            await zipService.UploadFile(result, path).ConfigureAwait(false);

            //await zipService.UnZipArchive(zipName);

            return null;
        }
    }
}
