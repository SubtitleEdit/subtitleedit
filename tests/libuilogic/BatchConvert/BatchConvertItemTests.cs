using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace LibUiLogicTests.BatchConvert;

public class BatchConvertItemTests
{
    [Fact]
    public void DefaultCtor_InitializesStringFieldsToEmpty()
    {
        var item = new BatchConvertItem();
        Assert.Equal(string.Empty, item.FileName);
        Assert.Equal(string.Empty, item.Format);
        Assert.Equal(string.Empty, item.Status);
        Assert.Equal(string.Empty, item.DisplaySize);
        Assert.Equal(string.Empty, item.OutputFileName);
        Assert.Equal(string.Empty, item.LanguageCode);
        Assert.Equal(string.Empty, item.TrackNumber);
        Assert.Null(item.Subtitle);
        Assert.Null(item.ImageSubtitle);
    }

    [Fact]
    public void FullCtor_PopulatesFieldsAndFormatsDisplaySize()
    {
        var subtitle = new Subtitle();
        var item = new BatchConvertItem("file.srt", 1024, "SubRip", subtitle);

        Assert.Equal("file.srt", item.FileName);
        Assert.Equal(1024, item.Size);
        Assert.Equal("SubRip", item.Format);
        Assert.Equal("-", item.Status);
        Assert.Same(subtitle, item.Subtitle);
        Assert.False(string.IsNullOrEmpty(item.DisplaySize));
    }

    [Fact]
    public void Status_Setter_RaisesPropertyChanged()
    {
        var item = new BatchConvertItem();
        var changes = new List<string?>();
        item.PropertyChanged += (_, e) => changes.Add(e.PropertyName);

        item.Status = "running";

        Assert.Contains("Status", changes);
        Assert.Equal("running", item.Status);
    }
}
