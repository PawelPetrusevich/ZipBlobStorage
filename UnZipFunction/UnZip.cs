using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using ZipBlobStorage.Services;

using ZipFunction;

namespace UnZipFunction
{
    public static class UnZip
    {
        [FunctionName("UnZip")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req,
            [Inject(typeof(IZipService))]IZipService zipService,
            TraceWriter log,
            ExecutionContext context)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string filePath = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "fileName", true) == 0)
                .Value;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"{nameof(filePath)} is empty.");
            }

            var responseModel = await zipService.UnZipArchive(filePath);

            return req.CreateResponse(HttpStatusCode.OK, responseModel);
        }
    }
}
