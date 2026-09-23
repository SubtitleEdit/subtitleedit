using System.Runtime.CompilerServices;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// #15206: the batch list is reused between runs, so a CPU run's "Converted" statuses stayed
/// on the rows when a following CUDA run failed, and a row that failed on the second run was
/// still shown - and counted in the summary - as converted.
/// </summary>
public class SpeechToTextBatchStatusTests : IDisposable
{
    private readonly string _file;

    public SpeechToTextBatchStatusTests()
    {
        _file = Path.Combine(Path.GetTempPath(), "se-stt-status-" + Guid.NewGuid().ToString("N") + ".wav");
        File.WriteAllText(_file, "x");
    }

    public void Dispose()
    {
        File.Delete(_file);
    }

    private SpeechToTextJobItem Item(string status)
    {
        // Media info is not read by the status helpers.
        var mediaInfo = (FfmpegMediaInfo)RuntimeHelpers.GetUninitializedObject(typeof(FfmpegMediaInfo));
        return new SpeechToTextJobItem(_file, status, mediaInfo);
    }

    [Fact]
    public void ResetBatchStatuses_ClearsStatusesFromEarlierRun()
    {
        var items = new[] { Item(Se.Language.General.Converted), Item(Se.Language.General.Error), Item(string.Empty) };

        SpeechToTextViewModel.ResetBatchStatuses(items);

        Assert.All(items, p => Assert.Equal(string.Empty, p.Status));
    }

    [Fact]
    public void ApplyBatchItemResult_NullSubtitle_OverwritesEarlierConverted()
    {
        var item = Item(Se.Language.General.Converted);

        SpeechToTextViewModel.ApplyBatchItemResult(item, null);

        Assert.Equal(Se.Language.General.Error, item.Status);
    }

    [Fact]
    public void ApplyBatchItemResult_EmptySubtitle_IsError()
    {
        var item = Item(Se.Language.General.Converted);

        SpeechToTextViewModel.ApplyBatchItemResult(item, new Subtitle());

        Assert.Equal(Se.Language.General.Error, item.Status);
    }

    [Fact]
    public void ApplyBatchItemResult_SubtitleWithText_IsConverted()
    {
        var item = Item(string.Empty);
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 1000));

        SpeechToTextViewModel.ApplyBatchItemResult(item, subtitle);

        Assert.Equal(Se.Language.General.Converted, item.Status);
    }
}
