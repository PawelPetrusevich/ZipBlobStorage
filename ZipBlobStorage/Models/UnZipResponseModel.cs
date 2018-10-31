using System.Collections.Generic;

namespace ZipBlobStorage.Models
{
    public class UnZipResponseModel
    {
        public string StockId { get; set; }

        public List<string> Image { get; set; }
    }
}