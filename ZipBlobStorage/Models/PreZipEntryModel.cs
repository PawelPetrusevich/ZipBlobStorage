using System.IO;
using System.Linq;

namespace ZipBlobStorage.Models
{
    public class PreZipEntryModel
    {
        public Stream FileStream { get; set; }

        public string FileName { get; set; }

        public PreZipEntryModel()
        {
        }

        public PreZipEntryModel(FileStream fileStream)
        {
            FileStream = fileStream;
            FileName = fileStream.Name.Split('\\').Last();
        }

        ~PreZipEntryModel()
        {
            FileStream.Dispose();
        }
    }
}