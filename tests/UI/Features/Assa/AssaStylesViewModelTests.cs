using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Assa;

public class AssaStylesViewModelTests
{
    [Fact]
    public void RepointParagraphsToStyle_ReassignsOnlyMatchingStyles()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("a", 0, 1000) { Extra = "Old1" });
        subtitle.Paragraphs.Add(new Paragraph("b", 1000, 2000) { Extra = "*Old2" }); // '*' prefix
        subtitle.Paragraphs.Add(new Paragraph("c", 2000, 3000) { Extra = "Keep" });

        var oldNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "Old1", "Old2" };
        AssaStylesViewModel.RepointParagraphsToStyle(subtitle, oldNames, "Target");

        Assert.Equal("Target", subtitle.Paragraphs[0].Extra);
        Assert.Equal("Target", subtitle.Paragraphs[1].Extra);
        Assert.Equal("Keep", subtitle.Paragraphs[2].Extra);
    }

    /// <summary>
    /// The AssaStylesViewModel constructor builds the storage-style category list and a filtered
    /// DataGridCollectionView over the stored styles (#11921). Resolving it from the real DI
    /// container must not throw, and the category list must always offer "All" + "Default".
    /// </summary>
    [AvaloniaFact]
    public void AssaStylesViewModel_ResolvesFromDiContainer_WithCategories()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();

        var viewModel = provider.GetRequiredService<AssaStylesViewModel>();

        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.StorageStylesView);
        Assert.Contains(Se.Language.Assa.AllCategories, viewModel.StorageCategories);
        Assert.Contains(Se.Language.General.Default, viewModel.StorageCategories);
    }

    private static AssaStylesViewModel MakeInitializedVm(out Subtitle subtitle)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<AssaStylesViewModel>();

        var styles = new List<SsaStyle>
        {
            new() { Name = "Default" },
            new() { Name = "Top", Alignment = "8" },
        };
        subtitle = new Subtitle
        {
            Header = Nikse.SubtitleEdit.Core.SubtitleFormats.AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
                Nikse.SubtitleEdit.Core.SubtitleFormats.AdvancedSubStationAlpha.DefaultHeader, styles),
        };
        subtitle.Paragraphs.Add(new Paragraph("one", 0, 1000) { Extra = "Default" });
        subtitle.Paragraphs.Add(new Paragraph("two", 1000, 2000) { Extra = "Default" });
        subtitle.Paragraphs.Add(new Paragraph("three", 2000, 3000) { Extra = "Top" });

        vm.Initialize(subtitle, new Nikse.SubtitleEdit.Core.SubtitleFormats.AdvancedSubStationAlpha(), "test.ass", "Default", null);
        return vm;
    }

    /// <summary>
    /// Renaming a style must re-point the lines that use it, keeping usage counts and the
    /// styles applied on OK correct (#13101). The name text box binds per keystroke, so the
    /// re-pointing must also survive intermediate states while typing.
    /// </summary>
    [AvaloniaFact]
    public void RenamingFileStyle_RepointsSubtitleLines()
    {
        var vm = MakeInitializedVm(out _);
        try
        {
            var defaultStyle = vm.FileStyles.First(s => s.Name == "Default");
            Assert.Equal(2, defaultStyle.UsageCount);

            defaultStyle.Name = "Narrator";

            Assert.Equal("Narrator", vm.ResultSubtitle.Paragraphs[0].Extra);
            Assert.Equal("Narrator", vm.ResultSubtitle.Paragraphs[1].Extra);
            Assert.Equal("Top", vm.ResultSubtitle.Paragraphs[2].Extra);
            Assert.Equal(2, defaultStyle.UsageCount);
        }
        finally
        {
            vm.OnClosingCleanup();
        }
    }

    [AvaloniaFact]
    public void RenamingFileStyle_ThroughBlankName_StillRepointsSubtitleLines()
    {
        var vm = MakeInitializedVm(out _);
        try
        {
            var defaultStyle = vm.FileStyles.First(s => s.Name == "Default");

            // Select-all + delete, then type a new name: the blank state must not break the chain.
            defaultStyle.Name = string.Empty;
            defaultStyle.Name = "N";
            defaultStyle.Name = "Na";
            defaultStyle.Name = "Narrator";

            Assert.Equal("Narrator", vm.ResultSubtitle.Paragraphs[0].Extra);
            Assert.Equal("Narrator", vm.ResultSubtitle.Paragraphs[1].Extra);
            Assert.Equal("Top", vm.ResultSubtitle.Paragraphs[2].Extra);
        }
        finally
        {
            vm.OnClosingCleanup();
        }
    }

    [AvaloniaFact]
    public void RenamingFileStyle_ToAnotherStylesName_DoesNotStealItsLines()
    {
        var vm = MakeInitializedVm(out _);
        try
        {
            var topStyle = vm.FileStyles.First(s => s.Name == "Top");

            // "Top" -> "Default" collides with the other style; nothing may be re-pointed.
            topStyle.Name = "Default";
            Assert.Equal("Default", vm.ResultSubtitle.Paragraphs[0].Extra);
            Assert.Equal("Top", vm.ResultSubtitle.Paragraphs[2].Extra);

            // Typing on to a unique name must still move only the lines that used "Top".
            topStyle.Name = "Default2";
            Assert.Equal("Default", vm.ResultSubtitle.Paragraphs[0].Extra);
            Assert.Equal("Default", vm.ResultSubtitle.Paragraphs[1].Extra);
            Assert.Equal("Default2", vm.ResultSubtitle.Paragraphs[2].Extra);
        }
        finally
        {
            vm.OnClosingCleanup();
        }
    }

    /// <summary>
    /// A stored style with a category must surface that category in the combo list and the style
    /// must be filtered into its category when selected.
    /// </summary>
    [AvaloniaFact]
    public void AssaStylesViewModel_SurfacesStoredCategories()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();

        Se.Settings.Assa.StoredStyles.Add(new SeAssaStyle
        {
            Name = "ProjectA-Title",
            Category = "Project A",
            ColorPrimary = "#FFFFFFFF",
            ColorSecondary = "#FFFFFFFF",
            ColorOutline = "#FF000000",
            ColorShadow = "#FF000000",
        });

        try
        {
            var viewModel = provider.GetRequiredService<AssaStylesViewModel>();

            Assert.Contains("Project A", viewModel.StorageCategories);

            viewModel.SelectedStorageCategory = "Project A";
            var visible = viewModel.StorageStylesView.Cast<StyleDisplay>().ToList();
            Assert.Contains(visible, s => s.Name == "ProjectA-Title");
        }
        finally
        {
            Se.Settings.Assa.StoredStyles.RemoveAll(s => s.Name == "ProjectA-Title");
        }
    }

    /// <summary>
    /// The storage styles table shows the category column via CategoryDisplay - a style with no
    /// category belongs to the built-in "Default" category and must say so instead of being blank.
    /// </summary>
    [AvaloniaFact]
    public void StyleDisplay_ShowsDefaultCategoryName()
    {
        var style = new StyleDisplay { Category = string.Empty };
        Assert.Equal(Se.Language.General.Default, style.CategoryDisplay);

        var changed = new List<string?>();
        style.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        style.Category = "Project A";
        Assert.Equal("Project A", style.CategoryDisplay);
        Assert.Contains(nameof(StyleDisplay.CategoryDisplay), changed);
    }

    /// <summary>
    /// Storage styles can be reordered (#15312). The grid shows a category-filtered view, so a
    /// move within the view must reorder only that category's styles in the saved list and
    /// leave the other categories' styles where they are.
    /// </summary>
    [AvaloniaFact]
    public void StorageMoveUp_InFilteredCategory_ReordersOnlyThatCategory()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<AssaStylesViewModel>();

        vm.StorageStyles.Clear();
        var a = new StyleDisplay { Name = "A", Category = "X" };
        var b = new StyleDisplay { Name = "B", Category = "Y" };
        var c = new StyleDisplay { Name = "C", Category = "X" };
        vm.StorageStyles.Add(a);
        vm.StorageStyles.Add(b);
        vm.StorageStyles.Add(c);
        vm.StorageCategories.Add("X");
        vm.SelectedStorageCategory = "X";

        var grid = vm.StorageStyleGrid;
        grid.Columns.Add(new Avalonia.Controls.TableViewColumn { Header = "Name", Binding = new Avalonia.Data.Binding(nameof(StyleDisplay.Name)) });
        grid.ItemsSource = vm.StorageStylesView;
        var window = new Avalonia.Controls.Window { Width = 400, Height = 300, Content = grid };
        window.Show();
        try
        {
            Assert.Equal(new[] { a, c }, vm.StorageStylesView);
            grid.SelectedItem = c;

            vm.StorageMoveUpCommand.Execute(null);

            Assert.Equal(new[] { c, a }, vm.StorageStylesView);
            Assert.Equal(new[] { c, b, a }, vm.StorageStyles);
            Assert.Same(c, grid.SelectedItem);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// Storage categories are independent style sets (#15332): copying a file style into a
    /// category must not clash with a same-named style in another category - it used to offer
    /// only "Overwrite" (which changed the other category's style) or "Keep both" (a "_2" name).
    /// </summary>
    [AvaloniaFact]
    public void FileCopyToStorage_SameNameInOtherCategory_AddsToSelectedCategory()
    {
        var vm = MakeInitializedVm(out _);
        vm.StorageStyles.Clear();
        var defaultCategoryStyle = new StyleDisplay { Name = "Default", Category = string.Empty, FontSize = 20 };
        vm.StorageStyles.Add(defaultCategoryStyle);
        vm.StorageCategories.Add("Anime");
        vm.SelectedStorageCategory = "Anime";

        var fileStyle = vm.FileStyles.Single(p => p.Name == "Default");
        fileStyle.FontSize = 42;

        var grid = vm.FileStyleGrid;
        grid.Columns.Add(new Avalonia.Controls.TableViewColumn { Header = "Name", Binding = new Avalonia.Data.Binding(nameof(StyleDisplay.Name)) });
        grid.ItemsSource = vm.FileStyles;
        var window = new Avalonia.Controls.Window { Width = 400, Height = 300, Content = grid };
        window.Show();
        vm.Window = window;
        try
        {
            grid.SelectedItem = fileStyle;

            vm.FileCopyToStorageCommand.Execute(null);

            Assert.Equal(2, vm.StorageStyles.Count);
            Assert.Equal(20, defaultCategoryStyle.FontSize);
            Assert.Equal(string.Empty, defaultCategoryStyle.Category);
            var copy = vm.StorageStyles.Single(p => p.Category == "Anime");
            Assert.Equal("Default", copy.Name);
            Assert.Equal(42, copy.FontSize);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// Importing an SE 4 category export (#15332) keeps each style's category - SE 4's "Default"
    /// category is the built-in Default category - and only renames a style whose name is
    /// already taken in its own category.
    /// </summary>
    [Fact]
    public void MakeSe4TemplateImportStyles_KeepsCategoriesAndRenamesOnlyWithinCategory()
    {
        var categories = new List<Se4StyleCategory>
        {
            new("Default", true, new List<SsaStyle> { new() { Name = "Default" }, new() { Name = "Top" } }),
            new("Anime", false, new List<SsaStyle> { new() { Name = "Default" }, new() { Name = "Top" } }),
        };
        var storage = new List<StyleDisplay>
        {
            new() { Name = "Default", Category = string.Empty },
            new() { Name = "Signs", Category = "Anime" },
        };

        var styles = AssaStylesViewModel.MakeSe4TemplateImportStyles(categories, storage);

        Assert.Equal(new[] { "Default_2", "Top", "Default", "Top" }, styles.Select(p => p.Name));
        Assert.Equal(new[] { "", "", "Anime", "Anime" }, styles.Select(p => p.Category));
        Assert.All(styles, p => Assert.True(p.IsSelected));
    }

    /// <summary>
    /// Storage "Remove all" with a category selected must only remove that category's styles -
    /// it used to clear the whole storage, all categories.
    /// </summary>
    [AvaloniaFact]
    public async Task StorageRemoveAll_InFilteredCategory_RemovesOnlyThatCategory()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<AssaStylesViewModel>();

        vm.StorageStyles.Clear();
        var a = new StyleDisplay { Name = "A", Category = "X" };
        var b = new StyleDisplay { Name = "B", Category = "Y" };
        var c = new StyleDisplay { Name = "C", Category = string.Empty };
        vm.StorageStyles.Add(a);
        vm.StorageStyles.Add(b);
        vm.StorageStyles.Add(c);
        vm.StorageCategories.Add("X");
        vm.SelectedStorageCategory = "X";

        await vm.StorageRemoveAllCommand.ExecuteAsync(null);

        Assert.Equal(new[] { b, c }, vm.StorageStyles);
        Assert.Empty(vm.StorageStylesView);
    }

    /// <summary>
    /// Deleting a storage style selects the next one among the shown (category-filtered) styles.
    /// The index used to come from the full storage list, so the new current style could belong
    /// to another category - not in the grid, but edited by the style editor.
    /// </summary>
    [AvaloniaFact]
    public void StorageRemove_InFilteredCategory_SelectsNextShownStyle()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<AssaStylesViewModel>();

        vm.StorageStyles.Clear();
        var a = new StyleDisplay { Name = "A", Category = "X" };
        var b = new StyleDisplay { Name = "B", Category = "Y" };
        var c = new StyleDisplay { Name = "C", Category = "Y" };
        var d = new StyleDisplay { Name = "D", Category = "X" };
        vm.StorageStyles.Add(a);
        vm.StorageStyles.Add(b);
        vm.StorageStyles.Add(c);
        vm.StorageStyles.Add(d);
        vm.StorageCategories.Add("X");
        vm.StorageCategories.Add("Y");
        vm.SelectedStorageCategory = "Y";

        var grid = vm.StorageStyleGrid;
        grid.Columns.Add(new Avalonia.Controls.TableViewColumn { Header = "Name", Binding = new Avalonia.Data.Binding(nameof(StyleDisplay.Name)) });
        grid.ItemsSource = vm.StorageStylesView;
        var window = new Avalonia.Controls.Window { Width = 400, Height = 300, Content = grid };
        window.Show();
        vm.Window = window;
        var promptBeforeDelete = Se.Settings.General.PromptBeforeDelete;
        Se.Settings.General.PromptBeforeDelete = false;
        try
        {
            grid.SelectedItem = c;

            vm.StorageRemoveCommand.Execute(null);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(new[] { a, b, d }, vm.StorageStyles);
            Assert.Same(b, vm.SelectedStorageStyle);
            Assert.Same(b, vm.CurrentStyle);
        }
        finally
        {
            Se.Settings.General.PromptBeforeDelete = promptBeforeDelete;
            window.Close();
        }
    }

    /// <summary>
    /// Category names are case-insensitive: the category list shows "Anime" and "anime" as one
    /// category, so the filter must show the styles of both spellings. A style moved to "anime"
    /// used to be shown under no category but [All].
    /// </summary>
    [AvaloniaFact]
    public void StorageCategoryFilter_IsCaseInsensitive()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<AssaStylesViewModel>();

        vm.StorageStyles.Clear();
        var a = new StyleDisplay { Name = "A", Category = "Anime" };
        var b = new StyleDisplay { Name = "B", Category = "anime" };
        var c = new StyleDisplay { Name = "C", Category = string.Empty };
        vm.StorageStyles.Add(a);
        vm.StorageStyles.Add(b);
        vm.StorageStyles.Add(c);
        vm.StorageCategories.Add("Anime");
        vm.SelectedStorageCategory = "Anime";

        Assert.Equal(new[] { a, b }, vm.StorageStylesView);
        Assert.True(vm.IsCategoryActionVisible);
    }

    /// <summary>
    /// OK must not write an empty or duplicate file style name to the header - the rename
    /// tracker leaves the lines alone in those states, so they fell back to the first style.
    /// </summary>
    [AvaloniaTheory]
    [InlineData("default")]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Ok_WithInvalidFileStyleName_DoesNotClose(string newName)
    {
        var vm = MakeInitializedVm(out var subtitle);
        var headerBefore = subtitle.Header;
        var top = vm.FileStyles.Single(p => p.Name == "Top");

        top.Name = newName;
        await vm.OkCommand.ExecuteAsync(null);

        Assert.False(vm.OkPressed);
        Assert.Same(top, vm.SelectedFileStyle);
        Assert.Equal(headerBefore, vm.Header);
    }

    [Fact]
    public void FileStyleNameValidator_FindsEmptyAndDuplicateNames()
    {
        var a = new StyleDisplay { Name = "Default" };
        var b = new StyleDisplay { Name = "Top" };
        var c = new StyleDisplay { Name = "DEFAULT" };
        var d = new StyleDisplay { Name = " " };

        Assert.Null(FileStyleNameValidator.FindInvalidName(new[] { a, b }));
        Assert.Same(c, FileStyleNameValidator.FindInvalidName(new[] { a, b, c })!.Value.Style);
        Assert.Same(d, FileStyleNameValidator.FindInvalidName(new[] { a, d })!.Value.Style);
    }

    /// <summary>
    /// Storage Export writes the shown styles - with a category selected, only that category.
    /// </summary>
    [AvaloniaFact]
    public void StorageExport_InFilteredCategory_ExportsOnlyThatCategory()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<AssaStylesViewModel>();

        vm.StorageStyles.Clear();
        vm.StorageStyles.Add(new StyleDisplay { Name = "Default", FontName = "Arial", Category = string.Empty });
        vm.StorageStyles.Add(new StyleDisplay { Name = "Default", FontName = "Verdana", Category = "Anime" });
        vm.StorageStyles.Add(new StyleDisplay { Name = "Signs", FontName = "Verdana", Category = "Anime" });
        vm.StorageCategories.Add("Anime");
        vm.SelectedStorageCategory = "Anime";

        var text = vm.MakeStorageExportText();
        var styles = Nikse.SubtitleEdit.Core.SubtitleFormats.AdvancedSubStationAlpha.GetSsaStylesFromHeader(text);

        Assert.Equal(new[] { "Default", "Signs" }, styles.Select(p => p.Name));
        Assert.All(styles, p => Assert.Equal("Verdana", p.FontName));
    }

    /// <summary>
    /// Overwriting an existing style on copy between file and storage (#15312) takes the
    /// formatting but keeps the target's identity: name, category and default flag.
    /// </summary>
    [Fact]
    public void StyleDisplay_CopyFormattingFrom_KeepsIdentity()
    {
        var target = new StyleDisplay { Name = "Title", Category = "Project A", IsDefault = true, FontSize = 20 };
        var source = new StyleDisplay { Name = "title", FontSize = 42, Bold = true, OutlineWidth = 3, FontName = "Arial" };
        source.SetAlignment("8");

        target.CopyFormattingFrom(source);

        Assert.Equal("Title", target.Name);
        Assert.Equal("Project A", target.Category);
        Assert.True(target.IsDefault);
        Assert.Equal(42, target.FontSize);
        Assert.True(target.Bold);
        Assert.Equal(3, target.OutlineWidth);
        Assert.Equal("Arial", target.FontName);
        Assert.Equal("8", target.GetAlignment());
    }
}
