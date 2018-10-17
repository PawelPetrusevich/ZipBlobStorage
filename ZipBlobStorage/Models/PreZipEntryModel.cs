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

        public PreZipEntryModel(FileStream fileStrim)
        {
            FileStream = fileStrim;
            FileName = fileStrim.Name.Split('\\').Last();
        }

        ~PreZipEntryModel()
        {
            FileStream.Dispose();
        }
    }
}