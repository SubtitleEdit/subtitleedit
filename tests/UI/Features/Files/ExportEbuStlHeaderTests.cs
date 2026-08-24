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
        var viewModel = new ExportEbuStlViewModel(new FileHelper());
        var subtitle = MakeSubtitle();
        viewModel.Initialize(subtitle);
        Dispatcher.UIThread.RunJobs();

        fillIn(viewModel);
        viewModel.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Ebu.EbuUiHelper ??= new UiEbuSaveHelper();
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
}
