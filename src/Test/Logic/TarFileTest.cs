using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using System.IO;

namespace Test.Logic
{
    [TestClass]
    [DeploymentItem("Files")]
    public class TarFileTest
    {

        [TestMethod]
        public void TarFileReadTest()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample.tar");
            var tr = new TarReader(fileName);
            Assert.IsTrue(tr.Files.Count == 13);
            Assert.IsTrue(tr.Files[0].FileSizeInBytes == 629);
            Assert.IsTrue(tr.Files[0].FileName == "BmpReader.cs");
            Assert.IsTrue(tr.Files[12].FileSizeInBytes == 4078);
            Assert.IsTrue(tr.Files[12].FileName == "XSub.cs");
        }

    }
}
