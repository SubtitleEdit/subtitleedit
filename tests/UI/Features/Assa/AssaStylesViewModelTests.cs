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
        Assert.Contains(Se.Language.Assa.DefaultCategory, viewModel.StorageCategories);
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
}
