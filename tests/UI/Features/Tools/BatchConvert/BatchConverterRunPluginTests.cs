using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Plugins;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterRunPluginTests
{
    // Batch convert "Run plugin": a batch-capable plugin gets a "batch" request per file, and its
    // returned subtitle is what gets saved. Errors fail the file.

    private sealed class FakePluginRunner : IPluginRunner
    {
        private readonly Func<PluginRequest, PluginRunResult> _run;
        public List<PluginRequest> Requests { get; } = new();

        public FakePluginRunner(Func<PluginRequest, PluginRunResult> run)
        {
            _run = run;
        }

        public Task<PluginRunResult> RunAsync(InstalledPlugin plugin, PluginRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_run(request));
        }
    }

    private static InstalledPlugin MakePlugin() => new()
    {
        Manifest = new PluginManifest { Name = "Batch test plugin", Batch = true },
        FolderPath = Path.GetTempPath(),
        ManifestPath = Path.Combine(Path.GetTempPath(), "plugin.json"),
        LaunchPath = "fake",
    };

    private static async Task<Subtitle> RunConvertAsync(FakePluginRunner runner, InstalledPlugin? plugin)
    {
        var dir = Directory.CreateTempSubdirectory("se-runplugin-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.srt");
            await File.WriteAllTextAsync(inputFile, "1\n00:00:01,000 --> 00:00:04,000\nHello world\n\n2\n00:00:05,000 --> 00:00:07,000\nSecond line\n");

            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);

            var subtitle = Subtitle.Parse(inputFile);
            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new SubRip().Name, subtitle);

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputFolder,
                Overwrite = true,
                TargetFormatName = SubRip.NameOfFormat,
            };
            config.RunPlugin.IsActive = true;
            config.RunPlugin.PluginName = "Batch test plugin";
            config.RunPlugin.Plugin = plugin;

            var converter = new BatchConverter(null!, null!, null!, runner);
            converter.Initialize(config);
            await converter.Convert(item, CancellationToken.None);

            var outputFile = Path.Combine(outputFolder, "input.srt");
            Assert.True(File.Exists(outputFile), "converted file was not written");
            return Subtitle.Parse(outputFile);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RunPlugin_SendsBatchRequest_AndSavesReturnedParagraphs()
    {
        var runner = new FakePluginRunner(request => PluginRunResult.Ok(new PluginResponse
        {
            Status = PluginConstants.StatusOk,
            Subtitle = new PluginSubtitle
            {
                Paragraphs = request.Subtitle.Paragraphs!
                    .Select(p => new PluginParagraph { StartMs = p.StartMs, EndMs = p.EndMs, Text = p.Text.ToUpperInvariant() })
                    .ToList(),
            },
        }));

        var result = await RunConvertAsync(runner, MakePlugin());

        var request = Assert.Single(runner.Requests);
        Assert.Equal(PluginConstants.RequestTypeBatch, request.RequestType);
        Assert.Empty(request.SelectedIndices);
        Assert.Equal(SubRip.NameOfFormat, request.Subtitle.Format);
        Assert.EndsWith("input.srt", request.Subtitle.FileName);
        Assert.Equal(2, request.Subtitle.Paragraphs!.Count);

        Assert.Equal(new[] { "HELLO WORLD", "SECOND LINE" }, result.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public async Task RunPlugin_NativeResponse_IsParsed()
    {
        var runner = new FakePluginRunner(_ => PluginRunResult.Ok(new PluginResponse
        {
            Status = PluginConstants.StatusOk,
            Subtitle = new PluginSubtitle
            {
                Format = SubRip.NameOfFormat,
                Native = "1\n00:00:02,000 --> 00:00:03,000\nOnly line\n",
            },
        }));

        var result = await RunConvertAsync(runner, MakePlugin());

        var p = Assert.Single(result.Paragraphs);
        Assert.Equal("Only line", p.Text);
        Assert.Equal(2000, p.StartTime.TotalMilliseconds);
    }

    [Fact]
    public async Task RunPlugin_OkWithoutSubtitle_KeepsLines()
    {
        var runner = new FakePluginRunner(_ => PluginRunResult.Ok(new PluginResponse { Status = PluginConstants.StatusOk }));

        var result = await RunConvertAsync(runner, MakePlugin());

        Assert.Equal(new[] { "Hello world", "Second line" }, result.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public async Task RunPlugin_ErrorStatus_FailsFileWithPluginMessage()
    {
        var runner = new FakePluginRunner(_ => PluginRunResult.Ok(new PluginResponse
        {
            Status = PluginConstants.StatusError,
            Message = "Not an EBU file",
        }));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => RunConvertAsync(runner, MakePlugin()));
        Assert.Equal("Not an EBU file", exception.Message);
    }

    [Fact]
    public async Task RunPlugin_MissingPlugin_FailsFile()
    {
        var runner = new FakePluginRunner(_ => throw new InvalidOperationException("must not run"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => RunConvertAsync(runner, null));
        Assert.Contains("Batch test plugin", exception.Message);
        Assert.Empty(runner.Requests);
    }

    [Fact]
    public async Task RunPlugin_DoesNotPersistReturnedSettings()
    {
        var name = "Batch test plugin";
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.Plugins.Settings.Remove(name);
        var runner = new FakePluginRunner(_ => PluginRunResult.Ok(new PluginResponse
        {
            Status = PluginConstants.StatusOk,
            Settings = System.Text.Json.JsonDocument.Parse("{\"x\":1}").RootElement.Clone(),
        }));

        await RunConvertAsync(runner, MakePlugin());

        Assert.False(Nikse.SubtitleEdit.Logic.Config.Se.Settings.Plugins.Settings.ContainsKey(name));
    }
}
