using System.Collections.ObjectModel;
using System.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Translating one file can take minutes, and before #13706 batch convert showed nothing while it
/// ran - the row just sat there, so a slow engine was indistinguishable from a stalled one. The
/// converter now feeds <see cref="DoAutoTranslate.Progress"/> into the item status the same way the
/// OCR runners do, which is only useful if it actually reaches the item and does not flood the UI
/// with one notification per line on long files.
/// </summary>
public class BatchConverterAutoTranslateProgressTests
{
    /// <summary>
    /// Translates one row per call so progress is reported per line - the same path the "advanced"
    /// local-LLM engines take (<see cref="IBatchContextTranslator"/>), which is the slow case the
    /// issue was about.
    /// </summary>
    private sealed class OneLineAtATimeTranslator : IAutoTranslator, IBatchContextTranslator
    {
        public string Name => "Test translator";
        public string Url => string.Empty;
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1000;

        public void Initialize()
        {
        }

        public List<TranslationPair> GetSupportedSourceLanguages() => new();

        public List<TranslationPair> GetSupportedTargetLanguages() => new();

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            return Task.FromResult("[t] " + text);
        }

        public Task<int> TranslateBatchAsync(ObservableCollection<TranslateRow> rows, int index, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            rows[index].TranslatedText = "[t] " + rows[index].Text;
            return Task.FromResult(1);
        }
    }

    private static async Task<List<string>> ConvertAndCollectStatusesAsync(int lineCount)
    {
        var dir = Directory.CreateTempSubdirectory("se-translate-progress-test");
        try
        {
            var subtitle = new Subtitle();
            for (var i = 0; i < lineCount; i++)
            {
                subtitle.Paragraphs.Add(new Paragraph("Line " + (i + 1), i * 2000, i * 2000 + 1500));
            }

            subtitle.Renumber();

            var inputFile = Path.Combine(dir.FullName, "input.srt");
            await File.WriteAllTextAsync(inputFile, subtitle.ToText(new SubRip()), TestContext.Current.CancellationToken);

            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);

            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new SubRip().Name, subtitle);

            var statuses = new List<string>();
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BatchConvertItem.Status))
                {
                    statuses.Add(item.Status);
                }
            };

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputFolder,
                Overwrite = true,
                TargetFormatName = SubRip.NameOfFormat,
            };
            config.AutoTranslate.IsActive = true;
            config.AutoTranslate.Translator = new OneLineAtATimeTranslator();

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            return statuses;
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Translating_ReportsPercentOnTheItem()
    {
        var statuses = await ConvertAndCollectStatusesAsync(4);

        var progress = statuses.Where(s => s.StartsWith("Translating: ", StringComparison.Ordinal)).ToList();
        Assert.Equal(new[] { "Translating: 25%", "Translating: 50%", "Translating: 75%", "Translating: 100%" }, progress);
    }

    [Fact]
    public async Task Translating_DoesNotRepeatTheSamePercent()
    {
        // 200 lines is well past one notification per whole percent, so without the guard this
        // would raise 200 property changes for 100 distinct values.
        var statuses = await ConvertAndCollectStatusesAsync(200);

        var progress = statuses.Where(s => s.StartsWith("Translating: ", StringComparison.Ordinal)).ToList();
        Assert.Equal(progress.Distinct().Count(), progress.Count);
        Assert.Equal("Translating: 100%", progress.Last());
    }

    [Fact]
    public async Task Translating_DoesNotLeaveTheRowShowingHundredPercent()
    {
        // Translating is one step of many, so the row must not sit on a finished-looking
        // "Translating: 100%" while the rest of the convert functions and the save run.
        var statuses = await ConvertAndCollectStatusesAsync(4);

        var lastProgressIndex = statuses.FindLastIndex(s => s.StartsWith("Translating: ", StringComparison.Ordinal));
        Assert.True(lastProgressIndex >= 0, "no translate progress was reported");
        Assert.NotEqual(statuses.Count - 1, lastProgressIndex);
        Assert.Equal("-", statuses[lastProgressIndex + 1]);
    }

    [Fact]
    public async Task NoTranslation_ReportsNoTranslateProgress()
    {
        var dir = Directory.CreateTempSubdirectory("se-translate-progress-off-test");
        try
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 1500));
            subtitle.Renumber();

            var inputFile = Path.Combine(dir.FullName, "input.srt");
            await File.WriteAllTextAsync(inputFile, subtitle.ToText(new SubRip()), TestContext.Current.CancellationToken);

            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);

            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new SubRip().Name, subtitle);
            var statuses = new List<string>();
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BatchConvertItem.Status))
                {
                    statuses.Add(item.Status);
                }
            };

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputFolder,
                Overwrite = true,
                TargetFormatName = SubRip.NameOfFormat,
            };

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            Assert.DoesNotContain(statuses, s => s.StartsWith("Translating: ", StringComparison.Ordinal));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
