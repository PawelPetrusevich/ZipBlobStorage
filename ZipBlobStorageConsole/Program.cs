using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ZipBlobStorage.Models;
using ZipBlobStorage.Repository;
using ZipBlobStorage.Services;

namespace ZipBlobStorageConsole
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var zipService = new ZipService(new AzureStorageRepository());
            Console.WriteLine("1-Upload");
            Console.WriteLine("2-Download");

            int x = int.Parse(Console.ReadKey().KeyChar.ToString());

            switch (x)
            {
                case 1:
                    OpenFileDialog dialog = new OpenFileDialog();
                    string path = string.Empty;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        path = dialog.FileName;
                    }

                    List<PreZipEntryModel> collection = new List<PreZipEntryModel>();

                    FileStream reader = new FileStream(path, FileMode.Open);

                    collection.Add(new PreZipEntryModel(reader));

                    zipService.UploadFile(collection);
                    break;

                case 2:
                    zipService.OpenZip();
                    break;
            }
        }
    }
}
