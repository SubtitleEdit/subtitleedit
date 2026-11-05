using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// A GPU build of crispasr uses the GPU unasked and exits when the model does not fit in VRAM,
/// instead of falling back. Each subtitle line is its own process, so the translator has to retry
/// on the CPU itself or every line fails (issue #16532).
/// </summary>
public class CrispAsrMadladCpuFallbackTests : IDisposable
{
    private readonly string _executable = Path.GetTempFileName();
    private readonly string _model = Path.GetTempFileName();
    private readonly List<IReadOnlyList<string>> _calls = new();

    public void Dispose()
    {
        File.Delete(_executable);
        File.Delete(_model);
    }

    private CrispAsrMadladTranslate MakeTranslator(Func<bool, CrispAsrMadladTranslate.CrispAsrResult> run)
    {
        var translator = new CrispAsrMadladTranslate();
        translator.Initialize(_executable, _model);
        translator.RunProcess = (_, arguments, _) =>
        {
            _calls.Add(arguments);
            return Task.FromResult(run(arguments.Contains("--no-gpu")));
        };
        return translator;
    }

    private static CrispAsrMadladTranslate.CrispAsrResult GpuOutOfMemory(bool useCpu)
    {
        return useCpu
            ? new CrispAsrMadladTranslate.CrispAsrResult(0, "Hallo", string.Empty)
            : new CrispAsrMadladTranslate.CrispAsrResult(13, string.Empty, "failed to allocate 2.04 GB on 'Vulkan0'");
    }

    [Fact]
    public async Task WorkingGpu_DoesNotPassNoGpu()
    {
        var translator = MakeTranslator(_ => new CrispAsrMadladTranslate.CrispAsrResult(0, "Hallo", string.Empty));

        Assert.Equal("Hallo", await translator.Translate("Hello", "en", "de", CancellationToken.None));

        Assert.DoesNotContain("--no-gpu", Assert.Single(_calls));
    }

    [Fact]
    public async Task GpuFailure_RetriesOnTheCpu()
    {
        var translator = MakeTranslator(GpuOutOfMemory);

        Assert.Equal("Hallo", await translator.Translate("Hello", "en", "de", CancellationToken.None));

        Assert.Equal(2, _calls.Count);
        Assert.DoesNotContain("--no-gpu", _calls[0]);
        Assert.Contains("--no-gpu", _calls[1]);
    }

    [Fact]
    public async Task GpuFailure_LaterLinesGoStraightToTheCpu()
    {
        var translator = MakeTranslator(GpuOutOfMemory);
        await translator.Translate("Hello", "en", "de", CancellationToken.None);
        _calls.Clear();

        await translator.Translate("Hello again", "en", "de", CancellationToken.None);

        Assert.Contains("--no-gpu", Assert.Single(_calls));
    }

    [Fact]
    public async Task NewRun_TriesTheGpuAgain()
    {
        var translator = MakeTranslator(GpuOutOfMemory);
        await translator.Translate("Hello", "en", "de", CancellationToken.None);
        _calls.Clear();
        translator.Initialize(_executable, _model);

        await translator.Translate("Hello", "en", "de", CancellationToken.None);

        Assert.DoesNotContain("--no-gpu", _calls[0]);
    }

    [Fact]
    public async Task FailureOnBoth_ReportsTheError()
    {
        var translator = MakeTranslator(_ => new CrispAsrMadladTranslate.CrispAsrResult(13, string.Empty, "broken model"));

        var exception = await Assert.ThrowsAsync<Exception>(() => translator.Translate("Hello", "en", "de", CancellationToken.None));

        Assert.Contains("broken model", exception.Message);
        Assert.Equal("broken model", translator.Error);
        Assert.Equal(2, _calls.Count);
    }

    [Fact]
    public async Task FailureOnBoth_KeepsTryingTheGpuFirst()
    {
        var translator = MakeTranslator(_ => new CrispAsrMadladTranslate.CrispAsrResult(13, string.Empty, "broken model"));
        await Assert.ThrowsAsync<Exception>(() => translator.Translate("Hello", "en", "de", CancellationToken.None));
        _calls.Clear();

        await Assert.ThrowsAsync<Exception>(() => translator.Translate("Hello", "en", "de", CancellationToken.None));

        Assert.DoesNotContain("--no-gpu", _calls[0]);
    }
}
