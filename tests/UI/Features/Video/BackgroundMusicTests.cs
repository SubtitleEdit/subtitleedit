using Nikse.SubtitleEdit.Features.Video.BackgroundMusic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Video;

/// <summary>Video &gt; More &gt; Generate background music (ACE-Step 1.5 through audio.cpp).</summary>
public class BackgroundMusicTests
{
    private static MusicGenerationRequest MakeRequest() => new()
    {
        Prompt = "calm piano, instrumental",
        Bpm = 72,
        DurationSeconds = 60,
        Seed = 1234,
        OutputFileName = "/tmp/out.wav",
    };

    private static string ValueAfter(List<string> args, string flag)
    {
        var index = args.IndexOf(flag);
        Assert.True(index >= 0 && index + 1 < args.Count, flag + " missing");
        return args[index + 1];
    }

    [Fact]
    public void BuildArguments_InstrumentalText2MusicWithTempoAndSeed()
    {
        var args = AceStepAudioCpp.BuildArguments(MakeRequest(), "/models/ace.gguf", "metal", floatOutput: false);

        Assert.Equal("gen", ValueAfter(args, "--task"));
        Assert.Equal("ace_step", ValueAfter(args, "--family"));
        Assert.Equal("/models/ace.gguf", ValueAfter(args, "--model"));
        Assert.Equal("metal", ValueAfter(args, "--backend"));
        Assert.Equal("text2music", ValueAfter(args, "--task-route"));
        Assert.Equal("calm piano, instrumental", ValueAfter(args, "--text"));
        Assert.Equal("[Instrumental]", ValueAfter(args, "--lyrics"));
        Assert.Equal("60", ValueAfter(args, "--duration-seconds"));
        Assert.Equal("1234", ValueAfter(args, "--seed"));
        Assert.Equal("/tmp/out.wav", ValueAfter(args, "--out"));
        Assert.Contains("bpm=72", args);
        Assert.Contains("--log", args);
        Assert.Contains(args, a => a.StartsWith("negative_prompt=") && a.Contains("vocals"));

        // Older runtimes reject unknown flags.
        Assert.DoesNotContain("--out-format", args);
    }

    [Fact]
    public void BuildArguments_FloatOutputWhenSupported()
    {
        var args = AceStepAudioCpp.BuildArguments(MakeRequest(), "/models/ace.gguf", "cuda", floatOutput: true);

        Assert.Equal("float32", ValueAfter(args, "--out-format"));
    }

    private static List<MusicGenerationProgress> Feed(AceStepProgressParser parser, IEnumerable<string> lines) =>
        lines.Select(parser.Parse).Where(p => p != null).Select(p => p!).ToList();

    [Fact]
    public void ProgressParser_WalksThePhasesAndCountsDecodeChunks()
    {
        var parser = new AceStepProgressParser(60);
        var lines = new List<string>
        {
            "family=ace_step",
            "[TRACE ts=20260917-150055] runtime.model.family ace_step",
            "[TIMING ts=20260917-150113] ace_step.session.ensure_planner_ms 0.001375",
            "[TIMING ts=20260917-150137] ace_step.planner.total_ms 23838.441",
            "[TIMING ts=20260917-150137] ace_step.session.ensure_pre_dit_ms 0.000666",
            "[TRACE ts=20260917-150138] ace_step.diffusion.context_frames 1500",
            "[TIMING ts=20260917-150138] ace_step.session.ensure_diffusion_ms 0.000292",
            "[TIMING ts=20260917-150146] ace_step.session.ensure_vae_decoder_ms 0.000333",
            "[TRACE ts=20260917-150146] ace_step.vae.decode.chunk_frames 128",
            "[TRACE ts=20260917-150146] ace_step.vae.decode.overlap_frames 32",
        };
        // 1500 latent frames, 128-frame chunks stepping by 64: 24 chunks, one timing line each.
        lines.AddRange(Enumerable.Repeat("[TIMING ts=20260917-150148] ace_step.vae.decode.total_ms 1533.894333", 24));

        var updates = Feed(parser, lines);

        Assert.Equal(MusicGenerationPhase.Composing, updates[0].Phase);
        Assert.Contains(updates, u => u.Phase == MusicGenerationPhase.Generating);
        var decoding = updates.Where(u => u.Phase == MusicGenerationPhase.Decoding).ToList();
        Assert.Equal(25, decoding.Count); // phase start + 24 chunks
        Assert.True(decoding.Select(d => d.Percent).SequenceEqual(decoding.Select(d => d.Percent).OrderBy(p => p)), "progress never goes backwards");
        Assert.InRange(decoding[12].Percent, 70, 85); // halfway through decoding
        Assert.Equal(99, decoding[^1].Percent, 3);
    }

    [Fact]
    public void ProgressParser_PhaseNeverMovesBack()
    {
        var parser = new AceStepProgressParser(30);

        var updates = Feed(parser, new[]
        {
            "[TIMING ts=1] ace_step.session.ensure_vae_decoder_ms 0.1",
            "[TIMING ts=2] ace_step.session.ensure_planner_ms 0.1",
        });

        Assert.All(updates, u => Assert.Equal(MusicGenerationPhase.Decoding, u.Phase));
    }

    [Theory]
    [InlineData("[TIMING ts=20260917-150223] session.wall_ms 70115.983042", "session.wall_ms", "70115.983042")]
    [InlineData("[TRACE ts=x] ace_step.vae.decode.chunk_frames 128", "ace_step.vae.decode.chunk_frames", "128")]
    [InlineData("family=ace_step", null, null)]
    [InlineData("[broken", null, null)]
    public void SplitLogLine(string line, string? key, string? value)
    {
        var (k, v) = AceStepProgressParser.SplitLogLine(line);

        Assert.Equal(key, k);
        Assert.Equal(value, v);
    }

    private static GeneratedMusic MakeGenerated(int generateSeconds, int preferredSeconds) => new()
    {
        Clip = new Nikse.SubtitleEdit.UiLogic.Media.MusicAudio(8000, 1, new float[8000]),
        Loop = new Nikse.SubtitleEdit.UiLogic.Media.MusicLoop(),
        Prompt = "calm piano",
        Bpm = 90,
        GenerateSeconds = generateSeconds,
        PreferredSeconds = preferredSeconds,
        Seed = 1,
    };

    [Fact]
    public void GeneratedMusic_MadeInTheDialog_IsReusedForAShorterSpeechTarget()
    {
        // dialog without a video: 60 s generated; the TTS run with 30 s of speech would ask for 40
        var music = MakeGenerated(generateSeconds: 60, preferredSeconds: 60);

        Assert.True(music.Matches(" calm piano ", 90, 60, targetSeconds: 30));
    }

    [Fact]
    public void GeneratedMusic_MadeForAShortVideo_IsRecognisedWhenTheDialogReopens()
    {
        var music = MakeGenerated(generateSeconds: 40, preferredSeconds: 60); // 30 s video

        Assert.True(music.Matches("calm piano", 90, 60));
        Assert.True(music.Matches("calm piano", 90, 60, targetSeconds: 30));
        Assert.False(music.Matches("calm piano", 90, 60, targetSeconds: 300)); // a long target wants the full 60 s
    }

    [Fact]
    public void GeneratedMusic_ChangedSettings_DoNotMatch()
    {
        var music = MakeGenerated(generateSeconds: 60, preferredSeconds: 60);

        Assert.False(music.Matches("calm piano", 100, 60, 30));
        Assert.False(music.Matches("upbeat", 90, 60, 30));
        Assert.False(music.Matches("calm piano", 90, 120, 30));
    }

    [Fact]
    public void AddToVideo_RemoveExistingAudio_MapsVideoAndMusicOnly()
    {
        var args = BackgroundMusicViewModel.BuildAddToVideoArguments("/v/in.mp4", "/t/music.wav", "/v/out.mp4", mix: false, originalVolumePercent: 100);

        Assert.Contains("-map 0:v -map 1:a:0", args);
        Assert.Contains("-c:v copy", args);
        Assert.Contains("-c:a aac", args);
        Assert.DoesNotContain("amix", args);
        Assert.EndsWith("\"/v/out.mp4\"", args);
    }

    [Fact]
    public void AddToVideo_Mix_KeepsFirstAudioTrackAtVolume()
    {
        var args = BackgroundMusicViewModel.BuildAddToVideoArguments("/v/in.mkv", "/t/music.wav", "/v/out.mkv", mix: true, originalVolumePercent: 80);

        Assert.Contains("[0:a:0]volume=0.80[orig]", args);
        Assert.Contains("amix=inputs=2:duration=first:normalize=0", args);
        Assert.Contains("-map 0:v -map \"[aout]\"", args);
    }

    [Fact]
    public void AddToVideo_Webm_UsesOpus()
    {
        var args = BackgroundMusicViewModel.BuildAddToVideoArguments("/v/in.webm", "/t/music.wav", "/v/out.webm", mix: false, originalVolumePercent: 100);

        Assert.Contains("-c:a libopus", args);
    }

    [Fact]
    public void MixUnderSpeech_FormatsEveryBranchAndPadsTheSidechain()
    {
        var args = BackgroundMusicGenerator.BuildMixUnderSpeechArguments("/t/speech.wav", "/t/music.wav", "/t/out.wav", 30);

        // ffmpeg 4.x: "The following filters could not choose their formats: sidechaincompress".
        Assert.Contains("[s0]aformat=sample_fmts=fltp:sample_rates=48000:channel_layouts=stereo[speech]", args);
        Assert.Contains("[k0]aformat=sample_fmts=fltp:sample_rates=48000:channel_layouts=stereo,apad[key]", args);
        Assert.Contains("volume=0.30,aformat=sample_fmts=fltp:sample_rates=48000:channel_layouts=stereo[music]", args);
        // Without apad the music stopped where the speech ended.
        Assert.Contains("[music][key]sidechaincompress", args);
        Assert.Contains("amix=inputs=2:duration=longest:normalize=0[out]", args);
        Assert.EndsWith("\"/t/out.wav\"", args);
    }

    [Fact]
    public void Presets_UniqueKeysAndTalkingHeadSuffix()
    {
        var presets = BackgroundMusicPreset.GetAll();

        Assert.Equal(presets.Count, presets.Select(p => p.Key).Distinct().Count());
        Assert.Contains(presets, p => p.Key == "cooking");
        Assert.Single(presets, p => p.IsCustom);
        var named = presets.Select(p => p.DisplayName).ToList();
        Assert.Equal(named.OrderBy(n => n, System.StringComparer.CurrentCultureIgnoreCase), named);
        foreach (var preset in presets.Where(p => !p.IsCustom))
        {
            Assert.EndsWith(BackgroundMusicPreset.PromptSuffix, preset.Prompt);
            Assert.InRange(preset.Bpm, 40, 200);
            Assert.False(string.IsNullOrWhiteSpace(preset.DisplayName));
        }
    }

    [Fact]
    public void Settings_DefaultToCookingPresetAndReplacingTheAudio()
    {
        var settings = new SeVideoBackgroundMusic();

        Assert.Equal("cooking", settings.Preset);
        Assert.True(settings.RemoveExistingAudioTracks);
        Assert.Equal(100, settings.MusicVolumePercent);
        Assert.Equal(60, settings.GenerateSeconds);
        Assert.True(settings.UseRandomSeed);
    }

    [Fact]
    public void ModelFile_WrongSizeIsNotInstalled()
    {
        var fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"ace-step-{System.Guid.NewGuid():N}.gguf");
        System.IO.File.WriteAllBytes(fileName, new byte[] { 1, 2, 3 });
        try
        {
            Assert.False(AceStepAudioCpp.IsValidLocalModelFile(fileName));
            Assert.False(AceStepAudioCpp.IsValidLocalModelFile(fileName + ".missing"));
        }
        finally
        {
            System.IO.File.Delete(fileName);
        }
    }
}
