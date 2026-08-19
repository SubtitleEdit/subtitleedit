using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using System.Collections.Generic;
using System.Linq;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// The llama-server launch line: SE's curated tuning, the user's own arguments, and the
/// "use only these parameters" opt-out from #13865.
/// </summary>
public class LlamaCppServerArgumentsTests
{
    private static readonly LlamaCppModel PlainModel = new("Test", "test.gguf", "1 GB", "https://example.com/test.gguf");

    private static readonly LlamaCppModel ChatTemplateModel = new(
        "Test chatml", "test-chatml.gguf", "1 GB", "https://example.com/test-chatml.gguf",
        ChatTemplate: "chatml", NoJinja: true);

    private static List<string> Build(LlamaCppModel model, string extraArgs, bool argsOnly, string? mmprojPath = null)
    {
        return LlamaCppServerManager.BuildServerArguments(model, "/models/test.gguf", mmprojPath, 1234, 8192, extraArgs, argsOnly);
    }

    [Fact]
    public void Default_AddsCuratedFlagsAndUserArgumentsLast()
    {
        var args = Build(ChatTemplateModel, "-ngl 30 --no-mmap", argsOnly: false);

        Assert.Equal("99", args[args.IndexOf("-ngl") + 1]);
        Assert.Contains("--swa-full", args);
        Assert.Contains("--cache-reuse", args);
        Assert.Contains("--no-jinja", args);
        Assert.Contains("--chat-template", args);

        // llama-server applies later arguments over earlier ones, so the user's -ngl must come
        // after SE's - that is what makes a repeated flag an override.
        Assert.True(args.LastIndexOf("-ngl") > args.IndexOf("-ngl"));
        Assert.Equal(new[] { "-ngl", "30", "--no-mmap" }, args.TakeLast(3));
    }

    [Fact]
    public void ArgumentsOnly_DropsCuratedFlagsButKeepsModelHostAndPort()
    {
        var args = Build(ChatTemplateModel, "-ngl 30 --jinja", argsOnly: true);

        Assert.Equal(
            new[] { "-m", "/models/test.gguf", "--host", "127.0.0.1", "--port", "1234", "-ngl", "30", "--jinja" },
            args);
    }

    [Fact]
    public void ArgumentsOnly_KeepsTheVisionProjector()
    {
        var args = Build(PlainModel, "-ngl 0", argsOnly: true, mmprojPath: "/models/mmproj.gguf");

        Assert.Equal(new[] { "--mmproj", "/models/mmproj.gguf" }, args.Skip(2).Take(2));
        Assert.DoesNotContain("--swa-full", args);
    }

    [Fact]
    public void Multimodal_SkipsCacheReuse()
    {
        var args = Build(PlainModel, string.Empty, argsOnly: false, mmprojPath: "/models/mmproj.gguf");

        Assert.DoesNotContain("--cache-reuse", args);
    }

    [Fact]
    public void QuotedUserArgument_SurvivesAsOneArgument()
    {
        var args = Build(PlainModel, "--override-kv \"key=str:some value\"", argsOnly: false);

        Assert.Equal(new[] { "--override-kv", "key=str:some value" }, args.TakeLast(2));
    }
}
