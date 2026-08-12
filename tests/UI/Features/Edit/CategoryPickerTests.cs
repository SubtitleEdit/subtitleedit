using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Edit;

/// <summary>
/// The dialog that ticks off which Multiple replace categories an export writes out, and - new in
/// #13529 - which ones an import brings in. Picking them one checkbox at a time is what the issue
/// was about, so the select all / none / invert commands and their shortcuts are the point here.
/// </summary>
public class CategoryPickerTests
{
    private static List<RuleTreeNode> BuildCategories(int count)
    {
        var categories = new List<RuleTreeNode>();
        for (var i = 1; i <= count; i++)
        {
            categories.Add(new RuleTreeNode(true) { CategoryName = $"c{i}" });
        }

        return categories;
    }

    private static string Ticked(CategoryPickerViewModel vm) =>
        string.Join(",", vm.Rules.Where(r => r.IsSelected).Select(r => r.CategoryName));

    private static bool SendKey(CategoryPickerViewModel vm, Key key, KeyModifiers modifiers)
    {
        var e = new KeyEventArgs { Key = key, KeyModifiers = modifiers, RoutedEvent = InputElement.KeyDownEvent };
        vm.KeyDown(null, e);
        return e.Handled;
    }

    // Export starts from one category (the one right-clicked), import starts from the whole file
    // so that plain OK keeps doing what the import did before there was a dialog at all.
    [AvaloniaFact]
    public void InitializeForExport_TicksOnlyTheGivenCategory()
    {
        var vm = new CategoryPickerViewModel();
        var categories = BuildCategories(3);

        vm.InitializeForExport(categories, categories[1]);

        Assert.Equal("c2", Ticked(vm));
        Assert.Equal(categories[1], vm.SelectedRule);
        Assert.Equal(Se.Language.Edit.MultipleReplace.ExportReplaceRules, vm.Title);
    }

    [AvaloniaFact]
    public void InitializeForImport_TicksEverythingInTheFile()
    {
        var vm = new CategoryPickerViewModel();

        vm.InitializeForImport(BuildCategories(3));

        Assert.Equal("c1,c2,c3", Ticked(vm));
        Assert.Equal(Se.Language.Edit.MultipleReplace.ImportReplaceRules, vm.Title);
    }

    [AvaloniaFact]
    public void SelectAll_SelectNone_AndInvert_HitEveryRow()
    {
        var vm = new CategoryPickerViewModel();
        var categories = BuildCategories(4);
        vm.InitializeForExport(categories, categories[0]);

        vm.SelectAllCommand.Execute(null);
        Assert.Equal("c1,c2,c3,c4", Ticked(vm));

        vm.SelectNoneCommand.Execute(null);
        Assert.Equal(string.Empty, Ticked(vm));

        vm.Rules[2].IsSelected = true;
        vm.InvertSelectionCommand.Execute(null);
        Assert.Equal("c1,c2,c4", Ticked(vm));
    }

    // Ctrl/Cmd + A / D / Shift+I - the same three SE's other tick-a-column lists use, so the
    // gestures mean the same wherever the user learned them (RemoveTextForHearingImpaired).
    [AvaloniaTheory]
    [InlineData(Key.A, KeyModifiers.Control, "c1,c2,c3")]
    [InlineData(Key.A, KeyModifiers.Meta, "c1,c2,c3")]
    [InlineData(Key.D, KeyModifiers.Control, "")]
    [InlineData(Key.D, KeyModifiers.Meta, "")]
    [InlineData(Key.I, KeyModifiers.Control | KeyModifiers.Shift, "c1,c3")]
    [InlineData(Key.I, KeyModifiers.Meta | KeyModifiers.Shift, "c1,c3")]
    public void Shortcuts_ChangeTheSelection(Key key, KeyModifiers modifiers, string expected)
    {
        var vm = new CategoryPickerViewModel();
        var categories = BuildCategories(3);
        vm.InitializeForExport(categories, categories[1]); // only c2 ticked

        Assert.True(SendKey(vm, key, modifiers));
        Assert.Equal(expected, Ticked(vm));
    }

    // The window reads its title off the view model, which only works because the view model is
    // initialized before the window is built - build both to keep that ordering honest.
    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Window_TakesItsTitleFromTheViewModel(bool isImport)
    {
        var vm = new CategoryPickerViewModel();
        var categories = BuildCategories(2);
        if (isImport)
        {
            vm.InitializeForImport(categories);
        }
        else
        {
            vm.InitializeForExport(categories, categories[0]);
        }

        var window = new CategoryPickerWindow(vm);

        Assert.Equal(isImport
            ? Se.Language.Edit.MultipleReplace.ImportReplaceRules
            : Se.Language.Edit.MultipleReplace.ExportReplaceRules, window.Title);
    }

    // A bare "a" is a list type-ahead key, not select all - and a bare shift+I types a capital I.
    [AvaloniaFact]
    public void Shortcuts_IgnoreUnmodifiedKeys()
    {
        var vm = new CategoryPickerViewModel();
        var categories = BuildCategories(3);
        vm.InitializeForExport(categories, categories[1]);

        Assert.False(SendKey(vm, Key.A, KeyModifiers.None));
        Assert.False(SendKey(vm, Key.I, KeyModifiers.None));
        Assert.False(SendKey(vm, Key.I, KeyModifiers.Shift));
        Assert.False(SendKey(vm, Key.D, KeyModifiers.None));
        Assert.Equal("c2", Ticked(vm));
    }

    // Alt is somebody else's shortcut (menu access keys), so it is not a near-miss of these.
    [AvaloniaFact]
    public void Shortcuts_IgnoreAltCombinations()
    {
        var vm = new CategoryPickerViewModel();
        var categories = BuildCategories(3);
        vm.InitializeForExport(categories, categories[1]);

        Assert.False(SendKey(vm, Key.A, KeyModifiers.Control | KeyModifiers.Alt));
        Assert.Equal("c2", Ticked(vm));
    }
}
