using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.ExportDialogueList;

namespace UITests.Features.Files.ExportDialogueList;

public class DialogueListExporterTests
{
    private static List<DialogueListRow> Rows() => new()
    {
        new DialogueListRow(1, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(3500), "<i>Hello</i>\nworld", "Hej\nverden", "Anna", "Default", 0, false),
        new DialogueListRow(2, TimeSpan.FromMilliseconds(4000), TimeSpan.FromMilliseconds(6020), "Second line", string.Empty, "Bob", "Default", 0, true),
    };

    private static DialogueListOptions Options(DialogueListTimeCodeFormat format, params DialogueListColumn[] columns) => new()
    {
        Columns = columns.ToList(),
        TimeCodeFormat = format,
    };

    [Fact]
    public void Frames_TimeCodes_At25Fps()
    {
        var cells = DialogueListExporter.MakeCells(Rows(), Options(DialogueListTimeCodeFormat.Frames,
            DialogueListColumn.Start, DialogueListColumn.End, DialogueListColumn.Duration, DialogueListColumn.Gap), 25);

        Assert.Equal(new object?[] { "00:00:01:00", "00:00:03:13", "00:00:02:13", "00:00:00:13" }, cells[0]);
        Assert.Equal(new object?[] { "00:00:04:00", "00:00:06:01", "00:00:02:01", null }, cells[1]); // no gap after the last line
    }

    [Fact]
    public void Milliseconds_And_Seconds()
    {
        var ms = DialogueListExporter.MakeCells(Rows(), Options(DialogueListTimeCodeFormat.Milliseconds, DialogueListColumn.Start, DialogueListColumn.End), 25);
        Assert.Equal(new object?[] { "00:00:01,000", "00:00:03,500" }, ms[0]);

        var seconds = DialogueListExporter.MakeCells(Rows(), Options(DialogueListTimeCodeFormat.Seconds, DialogueListColumn.Duration), 25);
        Assert.Equal(2.5, seconds[0][0]); // a number, so Excel can calculate with it
    }

    [Fact]
    public void Text_LineBreaks_And_Formatting()
    {
        var options = Options(DialogueListTimeCodeFormat.Frames, DialogueListColumn.Text, DialogueListColumn.OriginalText);
        Assert.Equal(new object?[] { "Hello\nworld", "Hej\nverden" }, DialogueListExporter.MakeCells(Rows(), options, 25)[0]);

        options.LineBreaksInCell = false;
        options.RemoveFormatting = false;
        Assert.Equal(new object?[] { "<i>Hello</i> | world", "Hej | verden" }, DialogueListExporter.MakeCells(Rows(), options, 25)[0]);
    }

    [Fact]
    public void Number_Actor_Forced_ReadingSpeed()
    {
        var cells = DialogueListExporter.MakeCells(Rows(), Options(DialogueListTimeCodeFormat.Frames,
            DialogueListColumn.Number, DialogueListColumn.Actor, DialogueListColumn.Forced, DialogueListColumn.Characters, DialogueListColumn.Cps), 25);

        Assert.Equal(1, cells[0][0]);
        Assert.Equal("Anna", cells[0][1]);
        Assert.Equal(string.Empty, cells[0][2]);
        Assert.Equal("Yes", cells[1][2]);
        Assert.Equal(11, cells[1][3]); // "Second line"
        Assert.IsType<double>(cells[1][4]);
    }

    [Fact]
    public void HasData()
    {
        var rows = Rows();
        Assert.True(DialogueListExporter.HasData(DialogueListColumn.Actor, rows));
        Assert.True(DialogueListExporter.HasData(DialogueListColumn.OriginalText, rows));
        Assert.False(DialogueListExporter.HasData(DialogueListColumn.Layer, rows));
        Assert.False(DialogueListExporter.HasData(DialogueListColumn.OriginalText, rows.Select(r => r with { OriginalText = string.Empty }).ToList()));
    }

    /// <summary>
    /// The header names are the ones SE's import recognizes, so an edited list opens again.
    /// </summary>
    [Theory]
    [InlineData(DialogueListTimeCodeFormat.Milliseconds)]
    [InlineData(DialogueListTimeCodeFormat.Frames)]
    public void RoundTrip_ThroughXlsxImport(DialogueListTimeCodeFormat format)
    {
        var options = Options(format, DialogueListColumn.Number, DialogueListColumn.Start, DialogueListColumn.End,
            DialogueListColumn.Text, DialogueListColumn.Actor);
        var fileName = Path.Combine(Path.GetTempPath(), "dialogue_list_" + Guid.NewGuid().ToString("N") + ".xlsx");
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            File.WriteAllBytes(fileName, DialogueListExporter.CreateXlsx(Rows(), options, 25));

            var subtitle = new UnknownFormatImporterXlsx().AutoGuessImport(fileName);

            Assert.Equal(2, subtitle.Paragraphs.Count);
            // hh:mm:ss:ff holds whole frames - 3.5 s is frame 12.5 at 25 fps
            var tolerance = format == DialogueListTimeCodeFormat.Frames ? 20 : 0.5;
            Assert.InRange(subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1000 - tolerance, 1000 + tolerance);
            Assert.InRange(subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3500 - tolerance, 3500 + tolerance);
            Assert.Equal("Second line", subtitle.Paragraphs[1].Text);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
            File.Delete(fileName);
        }
    }
}
