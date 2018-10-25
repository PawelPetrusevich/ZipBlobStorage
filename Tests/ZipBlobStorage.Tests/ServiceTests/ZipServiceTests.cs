using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using FluentAssertions;

using Moq;

using NUnit.Framework;

using ZipBlobStorage.Models;
using ZipBlobStorage.Repository;
using ZipBlobStorage.Services;
using ZipBlobStorage.Tests.Extension;

namespace ZipBlobStorage.Tests.ServiceTests
{
    [TestFixture]
    public class ZipServiceTests
    {

        [Test]
        [AutoMoqData]
        public void ZipService_UploadFile_ArchiveInfo_NullException(
            [Frozen] Mock<IAzureStorageRepository> _azureRepository,
            [Frozen] Mock<IFtpRepository> _ftpRepository,
            RequestModel archiveInfo,
            ZipService zipService)
        {
            Func<Task> action = async () => { await zipService.UploadFile(null); };

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [AutoMoqData]
        public void ZipService_UploadFile_ArchiveInfo_ImageListEmptyException(
            [Frozen] Mock<IAzureStorageRepository> _azureRepository,
            [Frozen] Mock<IFtpRepository> _ftpRepository,
            RequestModel archiveInfo,
            ZipService zipService)
        {
            archiveInfo.Images = null;
            Func<Task> action = async () => { await zipService.UploadFile(archiveInfo); };
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [AutoMoqData]
        public async Task ZipService_UploadFile(
            [Frozen] Mock<IAzureStorageRepository> _azureRepository,
            [Frozen] Mock<IFtpRepository> _ftpRepository,
            RequestModel archiveInfo,
            ZipService zipService)
        {
            await zipService.UploadFile(archiveInfo);

            //_azureRepository.Verify(mock => mock.UploadZipAsync(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _azureRepository.Verify(mock => mock.LoadImageAsync(It.IsAny<string>(), It.IsAny<Stream>()), Times.Exactly(archiveInfo.Images.Length));
            _ftpRepository.Verify(mock => mock.UploadOnFtp(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
        }


    }
}