using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
{
    public class TarFileTest
    {

        [Fact]
        public void TarFileReadTest()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample.tar");
            using (var tr = new TarReader(fileName))
            {
                Assert.True(tr.Files.Count(p => p.FileSizeInBytes > 0) == 13);
                Assert.True(tr.Files[0].FileSizeInBytes == 629);
                Assert.True(tr.Files[0].FileName == "BmpReader.cs");
                Assert.True(tr.Files[12].FileSizeInBytes == 4078);
                Assert.True(tr.Files[12].FileName == "XSub.cs");
            }
        }

    }
}
