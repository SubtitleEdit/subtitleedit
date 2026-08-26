using System.Globalization;
using System.Text;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Files;

/// <summary>
/// The EBU STL save options dialog hands its result to the writer as a 1024-character GSI header
/// stored on the subtitle. Anything that makes that header come out wrong is invisible to the user:
/// <see cref="Ebu.Save"/> just falls back to a default header ("No Title", USA, no start of
/// programme) and still writes a perfectly good subtitle grid. Reported by email against 5.2.0,
/// where a programme title longer than the 32-character field did exactly that.
/// </summary>
public class ExportEbuStlHeaderTests
{
    public ExportEbuStlHeaderTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Second line", 4000, 6000));
        return subtitle;
    }

    private static byte[] SaveViaDialog(Action<ExportEbuStlViewModel> fillIn)
    {
        return SaveViaDialog(MakeSubtitle(), fillIn);
    }

    /// <summary>
    /// Runs the options dialog the way the main window does - fill in, OK, hand the leftovers to
    /// the save helper - and saves the subtitle it worked on.
    /// </summary>
    private static byte[] SaveViaDialog(Subtitle subtitle, Action<ExportEbuStlViewModel> fillIn)
    {
        var viewModel = new ExportEbuStlViewModel(new FileHelper());
        viewModel.Initialize(subtitle);
        Dispatcher.UIThread.RunJobs();

        fillIn(viewModel);
        viewModel.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var helper = new UiEbuSaveHelper { JustificationCode = viewModel.JustificationCode };
        helper.SetFrameRate(viewModel.StoredHeader, viewModel.FrameRateFromSaveDialog);
        Ebu.EbuUiHelper = helper;

        return Save(subtitle);
    }

    private static byte[] Save(Subtitle subtitle)
    {
        var fileName = Path.Combine(Path.GetTempPath(), "ebu-header-test-" + Guid.NewGuid() + ".stl");
        try
        {
            new Ebu().Save(fileName, subtitle);
            return File.ReadAllBytes(fileName);
        }
        finally
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }

    // TTI block 0 starts right after the 1024-byte header; the in-cue time code sits at 5..8.
    private static string InCueTimeCode(byte[] bytes)
    {
        return $"{bytes[1024 + 5]:00}:{bytes[1024 + 6]:00}:{bytes[1024 + 7]:00}:{bytes[1024 + 8]:00}";
    }

    private static string Field(byte[] bytes, int index, int length)
    {
        return Ebu.GetEncoding(Encoding.ASCII.GetString(bytes, 0, 3)).GetString(bytes, index, length);
    }

    [AvaloniaFact]
    public void HeaderFields_ReachTheSavedFile()
    {
        var bytes = SaveViaDialog(vm =>
        {
            vm.OriginalProgramTitle = "My film";
            vm.OriginalEpisodeTitle = "Episode 1";
            vm.CountryOfOrigin = "DEU";
            vm.StartOfProgramme = new TimeSpan(10, 0, 0);
        });

        Assert.Equal("My film".PadRight(32), Field(bytes, 16, 32));
        Assert.Equal("Episode 1".PadRight(32), Field(bytes, 48, 32));
        Assert.Equal("DEU", Field(bytes, 274, 3));
        Assert.Equal("10000000", Field(bytes, 256, 8));
    }

    // A title longer than the 32-character GSI field used to push the header past 1024 characters,
    // and every other field the user filled in went down with it.
    [AvaloniaFact]
    public void OverlongTextFields_AreTruncated_RestOfHeaderSurvives()
    {
        var bytes = SaveViaDialog(vm =>
        {
            vm.OriginalProgramTitle = "A very long programme title that is well past thirty-two characters";
            vm.SubtitleListReferenceCode = "reference code that is too long";
            vm.TranslatorsName = new string('x', 40);
            vm.CountryOfOrigin = "DEU";
            vm.StartOfProgramme = new TimeSpan(10, 0, 0);
        });

        Assert.Equal("A very long programme title that", Field(bytes, 16, 32));
        Assert.Equal("reference code t", Field(bytes, 208, 16));
        Assert.Equal(new string('x', 32), Field(bytes, 144, 32));
        Assert.Equal("DEU", Field(bytes, 274, 3));
        Assert.Equal("10000000", Field(bytes, 256, 8));
    }

    // STL23.01 is offered by the dialog's own disk format code list, but the "is this an STL
    // header" check did not know about it - so picking it dropped the whole header on save.
    [AvaloniaFact]
    public void NonStandardDiskFormatCode_KeepsTheHeader()
    {
        var bytes = SaveViaDialog(vm =>
        {
            vm.SelectedDiskFormatCode = vm.DiskFormatCodes.First(p => p.StartsWith("STL23"));
            vm.OriginalProgramTitle = "My film";
            vm.CountryOfOrigin = "DEU";
        });

        Assert.Equal("STL23.01", Field(bytes, 3, 8));
        Assert.Equal("My film".PadRight(32), Field(bytes, 16, 32));
        Assert.Equal("DEU", Field(bytes, 274, 3));
    }

    // A country code shorter than three characters was replaced by "USA" rather than padded.
    [AvaloniaFact]
    public void ShortCountryCode_IsKeptNotReplacedByUsa()
    {
        var bytes = SaveViaDialog(vm =>
        {
            vm.OriginalProgramTitle = "My film";
            vm.CountryOfOrigin = "D";
        });

        Assert.Equal("D  ", Field(bytes, 274, 3));
    }

    // An untouched country field still gets the long-standing default.
    [AvaloniaFact]
    public void EmptyCountryCode_FallsBackToUsa()
    {
        var bytes = SaveViaDialog(vm => { vm.OriginalProgramTitle = "My film"; });

        Assert.Equal("USA", Field(bytes, 274, 3));
    }

    // The frame rate is the one save option that is not part of the 1024-character header, so it
    // was lost when the writer re-read the header off the subtitle and every file came out with
    // the rate its disk format code implies.
    [AvaloniaTheory]
    [InlineData("23.976", "00:00:01:23")]
    [InlineData("25", "00:00:01:24")]
    public void PickedFrameRate_DecidesTheTimeCodeFrames(string frameRate, string expected)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1960, 3000));

        var bytes = SaveViaDialog(subtitle, vm =>
        {
            vm.SelectedFrameRate = frameRate;
            vm.OriginalProgramTitle = "My film";
        });

        Assert.Equal(expected, InCueTimeCode(bytes));
    }

    // ...but it may not leak to the next file: that one keeps the rate its own header implies.
    [AvaloniaFact]
    public void PickedFrameRate_DoesNotLeakToAnotherSubtitle()
    {
        var first = new Subtitle();
        first.Paragraphs.Add(new Paragraph("Hello world", 1960, 3000));
        SaveViaDialog(first, vm =>
        {
            vm.SelectedFrameRate = "23.976";
            vm.OriginalProgramTitle = "My film";
        });

        // A second, STL25 subtitle saved without opening the dialog again.
        var second = new Subtitle { Header = new Ebu.EbuGeneralSubtitleInformation().ToString() };
        second.Paragraphs.Add(new Paragraph("Hello world", 1960, 3000));

        Assert.Equal("00:00:01:24", InCueTimeCode(Save(second)));
    }

    // Reopening the dialog showed the 25 fps default for every file, because the rate was read from
    // a field that is never part of a header read back from bytes.
    [AvaloniaFact]
    public void ReopeningTheDialog_ShowsTheRateOfTheFile()
    {
        var subtitle = new Subtitle
        {
            Header = new Ebu.EbuGeneralSubtitleInformation { DiskFormatCode = "STL30.01" }.ToString(),
        };
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000));

        var viewModel = new ExportEbuStlViewModel(new FileHelper());
        viewModel.Initialize(subtitle);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("30", viewModel.SelectedFrameRate);

        // And a rate that no disk format code can express is remembered for this header.
        Ebu.EbuUiHelper = new UiEbuSaveHelper();
        SaveViaDialog(subtitle, vm => { vm.SelectedFrameRate = "23.976"; });

        var reopened = new ExportEbuStlViewModel(new FileHelper());
        reopened.Initialize(subtitle);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("23.976", reopened.SelectedFrameRate);
    }

    // An unquoted font color plus a quotation mark later in the line crashed the writer with
    // "length ('-13') must be a non-negative value", so File > Export > EBU STL silently produced
    // no file at all (reported by email against 5.2.0-beta24).
    [AvaloniaFact]
    public void UnquotedFontColor_TeletextExport_WritesTheFile()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<font color=#ffff00>Er sagte \"Hallo\" zu mir</font>", 1000, 3000));

        var bytes = SaveViaDialog(subtitle, vm =>
        {
            vm.SelectedDisplayStandardCode = vm.DisplayStandardCodes[1]; // Level-1 teletext
        });

        Assert.True(bytes.Length >= 1024 + 128);
    }

    // The frame rate list is written with invariant decimal points, so it must not be read back
    // with the UI culture: in a comma-decimal culture "23.976" parsed as 23976 and the pick was
    // dropped on the floor.
    [AvaloniaFact]
    public void PickedFrameRate_SurvivesACommaDecimalCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("da-DK");
        try
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hello world", 1960, 3000));

            var bytes = SaveViaDialog(subtitle, vm =>
            {
                vm.SelectedFrameRate = "23.976";
                vm.OriginalProgramTitle = "My film";
            });

            Assert.Equal("00:00:01:23", InCueTimeCode(bytes));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }
}
