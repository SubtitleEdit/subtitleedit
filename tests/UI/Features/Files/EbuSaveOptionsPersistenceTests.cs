using System.Text;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Files;

/// <summary>
/// The justification code and the layout values (margins, rows per line break, teletext box /
/// double height) are the EBU STL save options that do not fit in the 1024-character GSI header
/// stored on the subtitle. They used to be reset to hard-coded defaults every time the save
/// options dialog opened, so "Centered text" silently became "Left-justified text" on reopen
/// (reported by email against 5.2.0-beta24). They are persisted in Se.Settings now.
/// </summary>
public class EbuSaveOptionsPersistenceTests
{
    public EbuSaveOptionsPersistenceTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static string[] ScopePaths => new[]
    {
        "File.EbuSaveOptions.JustificationCode",
        "File.EbuSaveOptions.MarginTop",
        "File.EbuSaveOptions.MarginBottom",
        "File.EbuSaveOptions.NewLineRows",
        "File.EbuSaveOptions.TeletextUseBox",
        "File.EbuSaveOptions.TeletextUseDoubleHeight",
    };

    /// <summary>
    /// The dialog's OK also writes into libse's Configuration singleton (the values Ebu.Save
    /// reads), which SettingsScope does not cover - snapshot and restore that side too, or a
    /// margin left behind here changes what every later EBU test writes.
    /// </summary>
    private sealed class LibSeEbuScope : IDisposable
    {
        private readonly int _justification = Configuration.Settings.SubtitleSettings.EbuStlJustificationCode;
        private readonly int _marginTop = Configuration.Settings.SubtitleSettings.EbuStlMarginTop;
        private readonly int _marginBottom = Configuration.Settings.SubtitleSettings.EbuStlMarginBottom;
        private readonly int _newLineRows = Configuration.Settings.SubtitleSettings.EbuStlNewLineRows;
        private readonly bool _useBox = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox;
        private readonly bool _useDoubleHeight = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight;

        public void Dispose()
        {
            Configuration.Settings.SubtitleSettings.EbuStlJustificationCode = _justification;
            Configuration.Settings.SubtitleSettings.EbuStlMarginTop = _marginTop;
            Configuration.Settings.SubtitleSettings.EbuStlMarginBottom = _marginBottom;
            Configuration.Settings.SubtitleSettings.EbuStlNewLineRows = _newLineRows;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = _useBox;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = _useDoubleHeight;
        }
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000));
        return subtitle;
    }

    private static ExportEbuStlViewModel OpenDialog(Subtitle subtitle)
    {
        var viewModel = new ExportEbuStlViewModel(new FileHelper());
        viewModel.Initialize(subtitle);
        Dispatcher.UIThread.RunJobs();
        return viewModel;
    }

    [AvaloniaFact]
    public void Justification_DefaultsToCentered()
    {
        using var scope = new SettingsScope(ScopePaths);
        using var libSeScope = new LibSeEbuScope();
        Se.Settings.File.EbuSaveOptions.JustificationCode = new SeEbuSaveOptions().JustificationCode;

        var viewModel = OpenDialog(MakeSubtitle());

        Assert.Equal(viewModel.Justifications[2], viewModel.SelectedJustification); // centered
    }

    [AvaloniaFact]
    public void Justification_SurvivesReopeningTheDialog()
    {
        using var scope = new SettingsScope(ScopePaths);
        using var libSeScope = new LibSeEbuScope();
        var subtitle = MakeSubtitle();

        var viewModel = OpenDialog(subtitle);
        viewModel.SelectedJustification = viewModel.Justifications[3]; // right-justified
        viewModel.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var reopened = OpenDialog(subtitle);

        Assert.Equal(reopened.Justifications[3], reopened.SelectedJustification);
    }

    [AvaloniaFact]
    public void LayoutOptions_SurviveReopeningTheDialog()
    {
        using var scope = new SettingsScope(ScopePaths);
        using var libSeScope = new LibSeEbuScope();
        var subtitle = MakeSubtitle();

        var viewModel = OpenDialog(subtitle);
        viewModel.SelectedTopAlignment = 1;
        viewModel.SelectedBottomAlignment = 4;
        viewModel.SelectedRowsAddByNewLine = 1;
        viewModel.UseBox = false;
        viewModel.UseDoubleHeight = false;
        viewModel.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var reopened = OpenDialog(subtitle);

        Assert.Equal(1, reopened.SelectedTopAlignment);
        Assert.Equal(4, reopened.SelectedBottomAlignment);
        Assert.Equal(1, reopened.SelectedRowsAddByNewLine);
        Assert.False(reopened.UseBox);
        Assert.False(reopened.UseDoubleHeight);
    }

    // The justification travels to the writer on the UI helper, not in the stored header - a save
    // that never opens the dialog (Ctrl+S on a loaded STL file) must still use the persisted pick.
    [AvaloniaFact]
    public void FreshSaveHelper_UsesThePersistedJustification()
    {
        using var scope = new SettingsScope(ScopePaths);
        using var libSeScope = new LibSeEbuScope();
        Se.Settings.File.EbuSaveOptions.JustificationCode = 3;

        Assert.Equal(3, new UiEbuSaveHelper().JustificationCode);
    }

    [AvaloniaFact]
    public void ChosenJustification_ReachesTheSavedTtiBlock()
    {
        using var scope = new SettingsScope(ScopePaths);
        using var libSeScope = new LibSeEbuScope();
        var subtitle = MakeSubtitle();

        var viewModel = OpenDialog(subtitle);
        viewModel.SelectedJustification = viewModel.Justifications[2]; // centered
        viewModel.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Ebu.EbuUiHelper = new UiEbuSaveHelper { JustificationCode = viewModel.JustificationCode };

        var bytes = SaveToBytes(subtitle);

        Assert.Equal(2, bytes[1024 + 14]); // TTI justification code
    }

    // Cross-restart persistence rides on the source-generated JSON context picking the new
    // settings type up transitively from the root.
    [Fact]
    public void EbuSaveOptions_RoundTripThroughSettingsJson()
    {
        var settings = new Se();
        settings.File.EbuSaveOptions.JustificationCode = 3;
        settings.File.EbuSaveOptions.MarginBottom = 4;
        settings.File.EbuSaveOptions.TeletextUseBox = false;

        var json = System.Text.Json.JsonSerializer.Serialize(settings, SeJsonContext.Default.Se);
        var back = System.Text.Json.JsonSerializer.Deserialize(json, SeJsonContext.Default.Se)!;

        Assert.Equal(3, back.File.EbuSaveOptions.JustificationCode);
        Assert.Equal(4, back.File.EbuSaveOptions.MarginBottom);
        Assert.False(back.File.EbuSaveOptions.TeletextUseBox);
    }

    internal static byte[] SaveToBytes(Subtitle subtitle)
    {
        var fileName = Path.Combine(Path.GetTempPath(), "ebu-options-test-" + Guid.NewGuid() + ".stl");
        try
        {
            Assert.True(new Ebu().Save(fileName, subtitle));
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

    // The box and the double height are teletext control codes - Ebu.Save writes neither when the
    // display standard is open subtitling, and the video preview shows neither. The two check
    // boxes have to say so instead of promising a box nothing ever draws (user report on PR #14228).
    [AvaloniaFact]
    public void TeletextOptions_AreOnlyEnabledForTeletext()
    {
        using var scope = new SettingsScope(ScopePaths);
        using var libSeScope = new LibSeEbuScope();

        var viewModel = OpenDialog(MakeSubtitle());

        Assert.Equal(viewModel.DisplayStandardCodes[0], viewModel.SelectedDisplayStandardCode); // 0 = open subtitling
        Assert.False(viewModel.IsTeletext);

        viewModel.SelectedDisplayStandardCode = viewModel.DisplayStandardCodes[1]; // 1 = level-1 teletext
        Assert.True(viewModel.IsTeletext);

        viewModel.SelectedDisplayStandardCode = viewModel.DisplayStandardCodes[2]; // 2 = level-2 teletext
        Assert.True(viewModel.IsTeletext);

        viewModel.SelectedDisplayStandardCode = viewModel.DisplayStandardCodes[3]; // undefined
        Assert.False(viewModel.IsTeletext);
    }

    // The video preview reads the justification off the libse settings, next to the margins and the
    // teletext flags. It used to read Ebu.EbuUiHelper, which is the carrier that takes the code to
    // the writer and does not exist until a save or this dialog creates one - so the preview showed
    // everything centered until then, and followed batch convert's job code after (PR #14229).
    [AvaloniaFact]
    public void Justification_ReachesTheLibSeSettingsForThePreview()
    {
        using var scope = new SettingsScope(ScopePaths);
        using var libSeScope = new LibSeEbuScope();

        var viewModel = OpenDialog(MakeSubtitle());
        viewModel.SelectedJustification = viewModel.Justifications[1]; // left-justified
        viewModel.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, Configuration.Settings.SubtitleSettings.EbuStlJustificationCode);
    }

    // A save that never opens the dialog still has to preview and write the persisted pick, so the
    // settings sync has to carry it over like every other EBU STL option.
    [AvaloniaFact]
    public void Justification_IsMirroredIntoLibSeBySettingsSync()
    {
        using var scope = new SettingsScope(ScopePaths);
        using var libSeScope = new LibSeEbuScope();
        Se.Settings.File.EbuSaveOptions.JustificationCode = 3;
        Configuration.Settings.SubtitleSettings.EbuStlJustificationCode = 0;

        Se.UpdateLibSeSettings();

        Assert.Equal(3, Configuration.Settings.SubtitleSettings.EbuStlJustificationCode);
    }
}
