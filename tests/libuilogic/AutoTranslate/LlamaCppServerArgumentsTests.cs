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

    private static readonly LlamaCppModel NoThinkingModel = new(
        "Test no-thinking", "test-nothink.gguf", "1 GB", "https://example.com/test-nothink.gguf",
        NoThinking: true);

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

    /// <summary>
    /// Gemma 4 thinks by default, and a thinking model puts its answer in
    /// "message.reasoning_content" while "message.content" - the only field the translate and
    /// review clients read - stays empty, so the line comes back untranslated.
    /// </summary>
    [Fact]
    public void NoThinking_TurnsReasoningOff()
    {
        var args = Build(NoThinkingModel, string.Empty, argsOnly: false);

        Assert.Equal("off", args[args.IndexOf("--reasoning") + 1]);
        // Gemma 4 keeps its embedded Jinja template - the flag must not drag a template override in.
        Assert.DoesNotContain("--no-jinja", args);
        Assert.DoesNotContain("--chat-template", args);
    }

    [Fact]
    public void NoThinking_IsNotAddedForOtherModels()
    {
        Assert.DoesNotContain("--reasoning", Build(PlainModel, string.Empty, argsOnly: false));
    }

    [Fact]
    public void ArgumentsOnly_LeavesReasoningToTheUser()
    {
        Assert.DoesNotContain("--reasoning", Build(NoThinkingModel, "-ngl 30", argsOnly: true));
    }

    /// <summary>Every curated Gemma 4 entry must carry it - that is the whole bug.</summary>
    [Fact]
    public void CuratedGemma4Models_AllDisableThinking()
    {
        var gemma4 = LlamaCppServerManager.TranslateModels
            .Concat(LlamaCppServerManager.ReviewModels)
            .Concat(LlamaCppServerManager.OcrModels)
            .Where(m => LlamaCppServerManager.IsGemma4FileName(m.FileName))
            .ToList();

        Assert.NotEmpty(gemma4);
        Assert.All(gemma4, m => Assert.True(m.NoThinking, m.DisplayName + " must set NoThinking"));
    }

    /// <summary>
    /// A controlled A/B (seconv EN-&gt;DA, 10 reps x 20 lines on the Q8_0 9B, 200 lines/variant)
    /// measured the old "chatml" + "--no-jinja" override leaking raw &lt;think&gt; text into 2.5%
    /// of lines, while NoThinking alone stayed at 0% on both the 9B and 4B quants - and stacking
    /// both overrides was worse than either alone (6.5% on 9B, 16.7% on 4B). Every curated Qwen
    /// 3/3.5/3.6 entry must therefore use NoThinking and must not also force the chatml template.
    /// </summary>
    [Fact]
    public void CuratedQwenModels_AllDisableThinkingAndNeverForceChatml()
    {
        var qwen = LlamaCppServerManager.TranslateModels
            .Concat(LlamaCppServerManager.ReviewModels)
            .Concat(LlamaCppServerManager.OcrModels)
            .Where(m => m.FileName.Contains("qwen", System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.NotEmpty(qwen);
        Assert.All(qwen, m => Assert.True(m.NoThinking, m.DisplayName + " must set NoThinking"));
        Assert.All(qwen, m => Assert.Null(m.ChatTemplate));
        Assert.All(qwen, m => Assert.False(m.NoJinja));
    }
}
