using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert's "Remove formatting" check boxes (issue #13996). SE 4 saved and restored all
/// seven of them; the SE 5 port declared the settings but never wired load or save, so every time
/// the window opened they were back to unticked and the function silently did nothing.
///
/// Which function tabs are ticked is a separate setting (ActiveFunctions) and was never affected.
/// </summary>
public class BatchConvertRemoveFormattingPersistenceTests
{
    private static BatchConvertViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<BatchConvertViewModel>();
    }

    private static void Invoke(BatchConvertViewModel vm, string method) =>
        typeof(BatchConvertViewModel)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, null);

    /// <summary>Restores whatever the seven settings held, so the test leaves no trace.</summary>
    private sealed class FormattingSettingsScope : IDisposable
    {
        private readonly SeBatchConvert _saved = new()
        {
            FormattingRemoveAll = Se.Settings.Tools.BatchConvert.FormattingRemoveAll,
            FormattingRemoveItalic = Se.Settings.Tools.BatchConvert.FormattingRemoveItalic,
            FormattingRemoveBold = Se.Settings.Tools.BatchConvert.FormattingRemoveBold,
            FormattingRemoveUnderline = Se.Settings.Tools.BatchConvert.FormattingRemoveUnderline,
            FormattingRemoveFontTags = Se.Settings.Tools.BatchConvert.FormattingRemoveFontTags,
            FormattingRemoveAlignmentTags = Se.Settings.Tools.BatchConvert.FormattingRemoveAlignmentTags,
            FormattingRemoveColorTags = Se.Settings.Tools.BatchConvert.FormattingRemoveColorTags,
        };

        public void Dispose()
        {
            var s = Se.Settings.Tools.BatchConvert;
            s.FormattingRemoveAll = _saved.FormattingRemoveAll;
            s.FormattingRemoveItalic = _saved.FormattingRemoveItalic;
            s.FormattingRemoveBold = _saved.FormattingRemoveBold;
            s.FormattingRemoveUnderline = _saved.FormattingRemoveUnderline;
            s.FormattingRemoveFontTags = _saved.FormattingRemoveFontTags;
            s.FormattingRemoveAlignmentTags = _saved.FormattingRemoveAlignmentTags;
            s.FormattingRemoveColorTags = _saved.FormattingRemoveColorTags;
        }
    }

    [AvaloniaFact]
    public void TickedBoxes_SurviveSaveAndLoad()
    {
        using var _ = new FormattingSettingsScope();

        var saver = MakeViewModel();
        saver.FormattingRemoveAll = true;
        saver.FormattingRemoveItalic = true;
        saver.FormattingRemoveBold = false;
        saver.FormattingRemoveUnderline = true;
        saver.FormattingRemoveFontTags = false;
        saver.FormattingRemoveAlignmentTags = true;
        saver.FormattingRemoveColors = true;
        Invoke(saver, "SaveSettings");

        // A freshly built view model starts from the stored settings, like reopening the window.
        var loader = MakeViewModel();
        loader.FormattingRemoveAll = false;
        loader.FormattingRemoveItalic = false;
        loader.FormattingRemoveUnderline = false;
        loader.FormattingRemoveAlignmentTags = false;
        loader.FormattingRemoveColors = false;
        Invoke(loader, "LoadSettings");

        Assert.True(loader.FormattingRemoveAll);
        Assert.True(loader.FormattingRemoveItalic);
        Assert.False(loader.FormattingRemoveBold);
        Assert.True(loader.FormattingRemoveUnderline);
        Assert.False(loader.FormattingRemoveFontTags);
        Assert.True(loader.FormattingRemoveAlignmentTags);
        Assert.True(loader.FormattingRemoveColors);
    }

    // The view model calls them "colors" and the settings file calls them "color tags"; a mapping
    // that crossed wires here would be invisible in the round trip above if both ends were wrong.
    [AvaloniaFact]
    public void RemoveColors_MapsToTheColorTagsSetting()
    {
        using var _ = new FormattingSettingsScope();

        var vm = MakeViewModel();
        vm.FormattingRemoveColors = true;
        vm.FormattingRemoveFontTags = false;
        Invoke(vm, "SaveSettings");

        Assert.True(Se.Settings.Tools.BatchConvert.FormattingRemoveColorTags);
        Assert.False(Se.Settings.Tools.BatchConvert.FormattingRemoveFontTags);
    }

    [AvaloniaFact]
    public void UntickedBoxes_StayUnticked()
    {
        using var _ = new FormattingSettingsScope();

        var saver = MakeViewModel();
        saver.FormattingRemoveAll = false;
        saver.FormattingRemoveItalic = false;
        saver.FormattingRemoveBold = false;
        saver.FormattingRemoveUnderline = false;
        saver.FormattingRemoveFontTags = false;
        saver.FormattingRemoveAlignmentTags = false;
        saver.FormattingRemoveColors = false;
        Invoke(saver, "SaveSettings");

        var loader = MakeViewModel();
        loader.FormattingRemoveAll = true;
        Invoke(loader, "LoadSettings");

        Assert.False(loader.FormattingRemoveAll);
        Assert.False(loader.FormattingRemoveColors);
    }
}
