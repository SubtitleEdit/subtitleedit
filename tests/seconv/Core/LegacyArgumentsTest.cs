using Xunit;

namespace SeConvTests.Core;

public class LegacyArgumentsTest
{
    [Fact]
    public void ConvertLegacyArguments_DropsConvertVerb()
    {
        // SE 4.x: `SubtitleEdit /convert sub.srt sami /overwrite`. The verb has no modern
        // equivalent option; it used to be rewritten to the nonexistent "--convert", which
        // consumed the pattern slot and failed the whole invocation.
        var result = SeConv.Program.ConvertLegacyArguments(["/convert", "sub.srt", "sami", "/overwrite"]);

        Assert.Equal(["sub.srt", "sami", "--overwrite"], result);
    }

    [Fact]
    public void ConvertLegacyArguments_DropsConvertVerbCaseInsensitive()
    {
        var result = SeConv.Program.ConvertLegacyArguments(["/Convert", "sub.srt", "sami"]);

        Assert.Equal(["sub.srt", "sami"], result);
    }

    [Fact]
    public void ConvertLegacyArguments_RewritesSlashOptionsAndKeepsPaths()
    {
        var result = SeConv.Program.ConvertLegacyArguments(["/encoding:utf-8", "/etc/subs/a.srt", "/?"]);

        Assert.Equal(["--encoding:utf-8", "/etc/subs/a.srt", "/?"], result);
    }
}
