using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;
using Nikse.SubtitleEdit.Features.Tools.MergeTwoSubtitles;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System.Reflection;
using System.Text.Json;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// The two styles of "Merge translation with original" are kept in the batch convert settings
/// (so presets carry them too), are edited in the function view, and become the styles the
/// converter writes.
/// </summary>
public class BatchConvertMergeTranslationWithOriginalViewModelTests
{
    private static BatchConvertViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<BatchConvertViewModel>();
    }

    private static object? Invoke(BatchConvertViewModel vm, string method, params object[] args) =>
        typeof(BatchConvertViewModel)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, args);

    /// <summary>Restores the batch convert settings, so the test leaves no trace.</summary>
    private sealed class BatchConvertSettingsScope : IDisposable
    {
        private readonly string _saved = JsonSerializer.Serialize(Se.Settings.Tools.BatchConvert, SeJsonContext.Default.SeBatchConvert);

        public void Dispose()
        {
            Se.Settings.Tools.BatchConvert = JsonSerializer.Deserialize(_saved, SeJsonContext.Default.SeBatchConvert)!;
        }
    }

    [AvaloniaFact]
    public void Styles_SurviveSaveAndLoad()
    {
        using var _ = new BatchConvertSettingsScope();

        var saver = MakeViewModel();
        var fontName = saver.MergeTranslationWithOriginalFontNames.LastOrDefault() ?? "Arial";
        saver.MergeTranslationWithOriginalFontName1 = fontName;
        saver.MergeTranslationWithOriginalFontSize1 = 33;
        saver.MergeTranslationWithOriginalBold1 = true;
        saver.MergeTranslationWithOriginalItalic1 = true;
        saver.MergeTranslationWithOriginalPrimaryColor1 = Colors.Yellow;
        saver.MergeTranslationWithOriginalOutlineColor1 = Colors.Blue;
        saver.MergeTranslationWithOriginalOutlineWidth1 = 3.5m;
        saver.MergeTranslationWithOriginalShadowWidth1 = 0;
        saver.MergeTranslationWithOriginalAlignTop1 = false;
        saver.MergeTranslationWithOriginalFontSize2 = 18;
        saver.MergeTranslationWithOriginalItalic2 = true;
        saver.MergeTranslationWithOriginalPrimaryColor2 = Colors.Cyan;
        saver.MergeTranslationWithOriginalAlignTop2 = true;
        Invoke(saver, "SaveSettings");

        // A freshly built view model starts from the stored settings, like reopening the window.
        var loader = MakeViewModel();
        loader.MergeTranslationWithOriginalFontSize1 = 20;
        loader.MergeTranslationWithOriginalBold1 = false;
        loader.MergeTranslationWithOriginalAlignTop2 = false;
        Invoke(loader, "LoadSettings");

        Assert.Equal(fontName, loader.MergeTranslationWithOriginalFontName1);
        Assert.Equal(33, loader.MergeTranslationWithOriginalFontSize1);
        Assert.True(loader.MergeTranslationWithOriginalBold1);
        Assert.True(loader.MergeTranslationWithOriginalItalic1);
        Assert.Equal(Colors.Yellow, loader.MergeTranslationWithOriginalPrimaryColor1);
        Assert.Equal(Colors.Blue, loader.MergeTranslationWithOriginalOutlineColor1);
        Assert.Equal(3.5m, loader.MergeTranslationWithOriginalOutlineWidth1);
        Assert.Equal(0, loader.MergeTranslationWithOriginalShadowWidth1);
        Assert.False(loader.MergeTranslationWithOriginalAlignTop1);
        Assert.Equal(18, loader.MergeTranslationWithOriginalFontSize2);
        Assert.True(loader.MergeTranslationWithOriginalItalic2);
        Assert.Equal(Colors.Cyan, loader.MergeTranslationWithOriginalPrimaryColor2);
        Assert.True(loader.MergeTranslationWithOriginalAlignTop2);
    }

    [AvaloniaFact]
    public void FontNotInstalled_FallsBackToFirstSystemFont()
    {
        using var _ = new BatchConvertSettingsScope();
        Se.Settings.Tools.BatchConvert.MergeTranslationWithOriginalFontName2 = "No Such Font 4711";

        var vm = MakeViewModel();

        Assert.Equal(vm.MergeTranslationWithOriginalFontNames.FirstOrDefault() ?? "Arial", vm.MergeTranslationWithOriginalFontName2);
    }

    [AvaloniaFact]
    public void Config_HasBothStyles_WhenTheFunctionIsSelected()
    {
        using var _ = new BatchConvertSettingsScope();

        var vm = MakeViewModel();
        vm.BatchFunctions.Single(p => p.Type == BatchConvertFunctionType.MergeTranslationWithOriginal).IsSelected = true;
        vm.MergeTranslationWithOriginalFontSize1 = 26;
        vm.MergeTranslationWithOriginalBold1 = true;
        vm.MergeTranslationWithOriginalPrimaryColor1 = Colors.Yellow;
        vm.MergeTranslationWithOriginalAlignTop1 = true;
        vm.MergeTranslationWithOriginalItalic2 = true;
        vm.MergeTranslationWithOriginalOutlineWidth2 = 4;
        vm.MergeTranslationWithOriginalAlignTop2 = false;

        var config = (BatchConvertConfig)Invoke(vm, "MakeBatchConvertConfig")!;

        var c = config.MergeTranslationWithOriginal;
        Assert.True(c.IsActive);
        Assert.Equal(MergeTwoSubtitlesHelper.StyleName1, c.StyleOriginal.Name);
        Assert.Equal(26, c.StyleOriginal.FontSize);
        Assert.True(c.StyleOriginal.Bold);
        Assert.Equal(SKColors.Yellow, c.StyleOriginal.Primary);
        Assert.Equal("8", c.StyleOriginal.Alignment);
        Assert.Equal(MergeTwoSubtitlesHelper.StyleName2, c.StyleTranslation.Name);
        Assert.True(c.StyleTranslation.Italic);
        Assert.Equal(4, c.StyleTranslation.OutlineWidth);
        Assert.Equal("2", c.StyleTranslation.Alignment);
    }

    [AvaloniaTheory]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    public async Task Convert_NeedsAutoTranslate(bool merge, bool autoTranslate, bool canConvert)
    {
        var vm = MakeViewModel();
        var config = new BatchConvertConfig();
        config.MergeTranslationWithOriginal.IsActive = merge;
        config.AutoTranslate.IsActive = autoTranslate;

        var result = await (Task<bool>)Invoke(vm, "EnsureMergeTranslationWithOriginalHasAutoTranslate", config)!;

        Assert.Equal(canConvert, result);
    }

    [AvaloniaFact]
    public void SelectingTheFunction_SwitchesTheTargetFormatToAssa()
    {
        using var _ = new BatchConvertSettingsScope();
        Se.Settings.Tools.BatchConvert.ActiveFunctions = [];
        var vm = MakeViewModel();
        vm.SelectedTargetFormat = "SubRip";
        var function = vm.BatchFunctions.Single(p => p.Type == BatchConvertFunctionType.MergeTranslationWithOriginal);

        function.IsSelected = true;

        Assert.Equal(AdvancedSubStationAlpha.NameOfFormat, vm.SelectedTargetFormat);

        // A format picked afterwards is respected
        vm.SelectedTargetFormat = "SubRip";
        Assert.Equal("SubRip", vm.SelectedTargetFormat);
    }

    [AvaloniaTheory]
    [InlineData(true, true, "Advanced Sub Station Alpha", false)]
    [InlineData(true, true, "SubRip", true)]
    [InlineData(true, false, "SubRip", false)]
    [InlineData(false, true, "SubRip", false)]
    public void TargetFormatQuestion_OnlyWhenMergeAndTranslateAreSelectedWithAnotherFormat(bool merge, bool autoTranslate, string target, bool asks)
    {
        Assert.Equal(asks, BatchConvertViewModel.NeedsTargetFormatQuestion(merge, autoTranslate, target));
    }

    [AvaloniaFact]
    public void View_TopAndBottom_FollowAndSetEachStyle()
    {
        using var _ = new BatchConvertSettingsScope();

        var vm = MakeViewModel();
        vm.MergeTranslationWithOriginalAlignTop1 = true;
        vm.MergeTranslationWithOriginalAlignTop2 = false;
        var window = new Window { Content = ViewMergeTranslationWithOriginal.Make(vm) };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var radios = window.GetLogicalDescendants().OfType<RadioButton>().ToList();
        RadioButton Radio(string style, string position) =>
            radios.Single(r => AutomationProperties.GetName(r) == $"{style} - {position}");
        var language = Se.Language.Tools.BatchConvert;
        var top1 = Radio(language.MergeTranslationWithOriginalStyleOriginal, Se.Language.General.Top);
        var bottom1 = Radio(language.MergeTranslationWithOriginalStyleOriginal, Se.Language.General.Bottom);
        var top2 = Radio(language.MergeTranslationWithOriginalStyleTranslation, Se.Language.General.Top);
        var bottom2 = Radio(language.MergeTranslationWithOriginalStyleTranslation, Se.Language.General.Bottom);

        Assert.True(top1.IsChecked);
        Assert.False(bottom1.IsChecked);
        Assert.False(top2.IsChecked);
        Assert.True(bottom2.IsChecked);

        // Like a user click - the other style keeps its position.
        bottom1.IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        Assert.False(vm.MergeTranslationWithOriginalAlignTop1);
        Assert.False(top1.IsChecked);
        Assert.False(vm.MergeTranslationWithOriginalAlignTop2);
        Assert.True(bottom2.IsChecked);

        top2.IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.MergeTranslationWithOriginalAlignTop2);
        Assert.False(bottom2.IsChecked);
        Assert.False(vm.MergeTranslationWithOriginalAlignTop1);
        Assert.True(bottom1.IsChecked);
        window.Close();
    }
}
