using Nikse.SubtitleEdit.Logic.Media;
using System.Reflection;

namespace UITests.Logic.Media;

public class FileHelperTests
{
    [Fact]
    public void AddMissingExtension_AppendsExtensionOnlyWhenFileNameHasNoExtension()
    {
        var method = typeof(FileHelper).GetMethod("AddMissingExtension", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        Assert.Equal("subtitle.srt", method!.Invoke(null, ["subtitle", ".srt"]));
        Assert.Equal("subtitle.ass", method.Invoke(null, ["subtitle.ass", ".srt"]));
    }
}