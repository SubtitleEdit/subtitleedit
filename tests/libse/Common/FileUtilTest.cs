using Nikse.SubtitleEdit.Core.Common;
using System.Text;

namespace LibSETests.Common;

public class FileUtilTest
{
    // The UTF-16LE BOM is FF FE; ReadAllLinesShared used to test FE FF (the UTF-16BE BOM),
    // so the BOM was never stripped and U+FEFF leaked into the first line.
    [Fact]
    public void ReadAllLinesSharedUtf16LeWithBomStripsBom()
    {
        var path = Path.GetTempFileName();
        try
        {
            var bytes = Encoding.Unicode.GetPreamble()
                .Concat(Encoding.Unicode.GetBytes("Hello" + Environment.NewLine + "World"))
                .ToArray();
            File.WriteAllBytes(path, bytes);

            var lines = FileUtil.ReadAllLinesShared(path, Encoding.Unicode);

            Assert.Equal("Hello", lines[0]);
            Assert.Equal("World", lines[1]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
