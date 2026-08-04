using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The consent / AI-marking attestations SE sends to the crispasr TTS server, and the
/// Video → Text to speech → "accept voice cloning" setting that gates them.
///
/// Verified against a real crispasr 0.8.23 server (f5-tts backend, POST /v1/audio/speech):
/// a request whose <c>voice</c> ends in <c>.wav</c> plus <c>"spoken_disclaimer": false</c> and no
/// <c>marking_attestation</c> comes back HTTP 400 <c>marking_attestation_required</c>; the same
/// request with the field renders normally.
/// </summary>
[Collection(TtsSettingsCollection.Name)]
public class CrispAsrTtsProvenanceTests
{
    [Fact]
    public void AddSpeechAttestations_WhenAccepted_SendsConsentMarkingAndSkipsDisclaimer()
    {
        using var _ = new AcceptVoiceCloningScope(true);
        var payload = new Dictionary<string, object>();

        CrispAsrTtsProvenance.AddSpeechAttestations(payload);

        Assert.Equal(CrispAsrTtsProvenance.ConsentAttestation, payload["consent_attestation"]);
        Assert.Equal(CrispAsrTtsProvenance.MarkingAttestation, payload["marking_attestation"]);
        Assert.Equal(false, payload["spoken_disclaimer"]);
    }

    [Fact]
    public void AddSpeechAttestations_WhenNotAccepted_SendsNothing()
    {
        // No attestation means the server keeps its own defaults: the audible AI disclaimer stays
        // on and an actual clone request is refused with HTTP 400 consent_required. That refusal
        // is the point of turning the setting off.
        using var _ = new AcceptVoiceCloningScope(false);
        var payload = new Dictionary<string, object>();

        CrispAsrTtsProvenance.AddSpeechAttestations(payload);

        Assert.Empty(payload);
    }

    [Fact]
    public void AddSpeechAttestations_LeavesExistingFieldsAlone()
    {
        using var _ = new AcceptVoiceCloningScope(true);
        var payload = new Dictionary<string, object>
        {
            ["input"] = "hello",
            ["voice"] = "Arnold.wav",
        };

        CrispAsrTtsProvenance.AddSpeechAttestations(payload);

        Assert.Equal("hello", payload["input"]);
        Assert.Equal("Arnold.wav", payload["voice"]);
    }

    [Fact]
    public void AddServerMarkingArgs_WhenNotAccepted_AddsNothing()
    {
        using var _ = new AcceptVoiceCloningScope(false);
        var args = new Collection<string> { "--server" };

        CrispAsrTtsProvenance.AddServerMarkingArgs(args, "/does/not/matter/crispasr");

        Assert.Equal(new[] { "--server" }, args);
    }

    [Fact]
    public void AddServerMarkingArgs_WithNoInstalledExecutable_AddsNothing()
    {
        // crispasr aborts on an unknown argument, so the opt-out flags must never be passed to a
        // binary we cannot confirm understands them.
        using var _ = new AcceptVoiceCloningScope(true);
        var args = new Collection<string> { "--server" };

        CrispAsrTtsProvenance.AddServerMarkingArgs(args, Path.Combine(Path.GetTempPath(), "no-such-crispasr"));

        Assert.Equal(new[] { "--server" }, args);
    }

    [Fact]
    public void AddServerMarkingArgs_WhenSupported_AddsAllThreeFlags()
    {
        using var _ = new AcceptVoiceCloningScope(true);
        using var stub = FakeCrispAsr.WithHelpText(
            "--no-watermark disable it\n--no-c2pa disable that\n--accept-marking-responsibility attest");
        var args = new Collection<string> { "--server" };

        CrispAsrTtsProvenance.AddServerMarkingArgs(args, stub.Path);

        Assert.Equal(
            new[] { "--server", "--no-watermark", "--no-c2pa", "--accept-marking-responsibility" },
            args);
    }

    [Fact]
    public void SupportsMarkingOptOut_WithMissingExecutable_IsFalse()
    {
        Assert.False(CrispAsrTtsProvenance.SupportsMarkingOptOut(
            Path.Combine(Path.GetTempPath(), "no-such-crispasr")));
    }

    [Fact]
    public void SupportsMarkingOptOut_WithHelpListingAllThreeFlags_IsTrue()
    {
        using var stub = FakeCrispAsr.WithHelpText(
            "--no-watermark x\n--no-c2pa y\n--accept-marking-responsibility z");

        Assert.True(CrispAsrTtsProvenance.SupportsMarkingOptOut(stub.Path));
    }

    [Fact]
    public void SupportsMarkingOptOut_WithV0820StyleHelp_IsFalse()
    {
        // The regression this replaced a hash check for: crispasr v0.8.20 documents --no-watermark
        // but neither --no-c2pa nor --accept-marking-responsibility, and passing the full set to it
        // makes the server print usage and exit instead of starting.
        using var stub = FakeCrispAsr.WithHelpText(
            "--no-spoken-disclaimer skip it\n--no-watermark disable the watermark");

        Assert.False(CrispAsrTtsProvenance.SupportsMarkingOptOut(stub.Path));
    }

    [Fact]
    public void SupportsMarkingOptOut_WithAnExecutableThatIsNotCrispAsr_IsFalse()
    {
        // Fails closed: a binary that cannot be run, or prints nothing useful, keeps the marking on.
        using var stub = FakeCrispAsr.WithHelpText(string.Empty);

        Assert.False(CrispAsrTtsProvenance.SupportsMarkingOptOut(stub.Path));
    }

    [Fact]
    public void SupportsMarkingOptOut_WhenHelpCannotBeRead_IsFalse()
    {
        using var stub = FakeCrispAsr.WithHelpText(null);

        Assert.False(CrispAsrTtsProvenance.SupportsMarkingOptOut(stub.Path));
    }

    [Fact]
    public void SupportsMarkingOptOut_ProbesEachBinaryOnce()
    {
        var calls = 0;
        using var stub = FakeCrispAsr.WithHelpText(
            "--no-watermark x\n--no-c2pa y\n--accept-marking-responsibility z",
            onProbe: () => calls++);

        Assert.True(CrispAsrTtsProvenance.SupportsMarkingOptOut(stub.Path));
        Assert.True(CrispAsrTtsProvenance.SupportsMarkingOptOut(stub.Path));

        Assert.Equal(1, calls);
    }

    [Fact]
    public void AcceptVoiceCloning_DefaultsToOn()
    {
        Assert.True(new SeVideoTextToSpeech().AcceptVoiceCloning);
    }

    [Fact]
    public async Task RealHelpProbe_WithLargeStderrAndEmptyStdout_DoesNotDeadlock()
    {
        // #12878: the real probe read stdout to EOF *before* touching stderr. crispasr writes its
        // whole ~25 KB usage dump to stderr and nothing at all to stdout, so the child blocked once
        // the pipe filled, stdout never reached EOF, and the probe hung forever - on the UI thread,
        // freezing the app on every CrispASR TTS generation. Every other test here stubs
        // HelpTextProvider, which is exactly why this went unnoticed; this one runs the real thing.
        //
        // 1 MB is past any platform's pipe buffer (4 KB on Windows, 64 KB on Unix), so the old
        // sequential read deadlocks here regardless of where the suite runs.
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("Needs a shebang stub; Process.Start cannot launch a .cmd with UseShellExecute=false.");
        }

        const int payloadBytes = 1024 * 1024;
        using var stub = ShellStub.WritingToStdErr(payloadBytes);

        var probe = Task.Run(() => CrispAsrTtsProvenance.HelpTextProvider(stub.Path));
        var finished = await Task.WhenAny(
            probe,
            Task.Delay(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));

        Assert.True(ReferenceEquals(finished, probe), "the --help probe deadlocked");
        Assert.Equal(payloadBytes, (await probe)?.Length);
    }
}

/// <summary>
/// A real executable stub (shebang script) that, given <c>--help</c>, floods stderr and writes
/// nothing to stdout - the shape of output that deadlocked the probe in #12878. Unix only; see the
/// skip in the test that uses it.
/// </summary>
internal sealed class ShellStub : IDisposable
{
    public string Path { get; }

    private ShellStub(int stdErrBytes)
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "crispasr-flood-" + Guid.NewGuid().ToString("N") + ".sh");

        // Generate the payload rather than embedding it, so the script stays small and the byte
        // count exact. Nothing is written to stdout - that is the half of the shape that matters.
        File.WriteAllText(
            Path,
            "#!/bin/sh\n"
            + $"awk 'BEGIN {{ for (i = 0; i < {stdErrBytes}; i++) printf \"x\" }}' 1>&2\n");
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(
                Path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    public static ShellStub WritingToStdErr(int stdErrBytes) => new(stdErrBytes);

    public void Dispose()
    {
        try
        {
            File.Delete(Path);
        }
        catch
        {
            // temp file - nothing depends on it being gone
        }
    }
}

/// <summary>
/// Stands in for an installed crispasr binary: a real file on disk (so the existence and
/// cache-key checks are exercised) whose <c>--help</c> output is whatever the test says, without
/// spawning a stub executable — writing one portable across macOS/Linux and the Windows CI is not
/// worth it when the decision under test is "does this help text list the flags".
/// </summary>
internal sealed class FakeCrispAsr : IDisposable
{
    private readonly Func<string, string?> _originalProvider;

    public string Path { get; }

    private FakeCrispAsr(string? helpText, Action? onProbe)
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "crispasr-stub-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(Path, "stub");

        _originalProvider = CrispAsrTtsProvenance.HelpTextProvider;
        CrispAsrTtsProvenance.ClearSupportCache();
        CrispAsrTtsProvenance.HelpTextProvider = requested =>
        {
            onProbe?.Invoke();
            return requested == Path ? helpText : _originalProvider(requested);
        };
    }

    public static FakeCrispAsr WithHelpText(string? helpText, Action? onProbe = null)
        => new(helpText, onProbe);

    public void Dispose()
    {
        CrispAsrTtsProvenance.HelpTextProvider = _originalProvider;
        CrispAsrTtsProvenance.ClearSupportCache();

        try
        {
            File.Delete(Path);
        }
        catch
        {
            // temp file — nothing depends on it being gone
        }
    }
}

/// <summary>
/// Restores <see cref="SeVideoTextToSpeech.AcceptVoiceCloning"/> after a test flips it.
/// </summary>
internal sealed class AcceptVoiceCloningScope : IDisposable
{
    private readonly bool _original;

    public AcceptVoiceCloningScope(bool accepted)
    {
        _original = Se.Settings.Video.TextToSpeech.AcceptVoiceCloning;
        Se.Settings.Video.TextToSpeech.AcceptVoiceCloning = accepted;
    }

    public void Dispose()
    {
        Se.Settings.Video.TextToSpeech.AcceptVoiceCloning = _original;
    }
}

/// <summary>
/// Groups every test that reads or writes the shared <see cref="Se.Settings"/> TTS state so xUnit
/// runs them one at a time instead of racing on the static.
/// </summary>
[CollectionDefinition(Name)]
public class TtsSettingsCollection
{
    public const string Name = "TtsSettings";
}
