using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Ocr;

public class NOcrTrainViewModelTests
{
    private static NOcrTrainViewModel MakeViewModel(params string[] fonts)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var vm = services.BuildServiceProvider().GetRequiredService<NOcrTrainViewModel>();
        vm.Fonts.Clear();
        foreach (var font in fonts)
        {
            vm.Fonts.Add(new NOcrTrainFontItem(font, false));
        }

        return vm;
    }

    [Theory]
    [InlineData("30, 40", new[] { 30, 40 })]
    [InlineData("40;30 40", new[] { 40, 30 })]
    [InlineData("abc, 5, 300, 24", new[] { 24 })]
    [InlineData("", new int[0])]
    public void ParseFontSizes_KeepsDistinctSizesInRange(string text, int[] expected)
    {
        Assert.Equal(expected, NOcrTrainViewModel.ParseFontSizes(text));
    }

    [AvaloniaFact]
    public void FontSearch_FiltersTheShownFonts()
    {
        var vm = MakeViewModel("Arial", "Arial Narrow", "Verdana");

        vm.FontSearchText = "arial";

        Assert.Equal(new List<string> { "Arial", "Arial Narrow" }, vm.FilteredFonts.Select(f => f.Name).ToList());
    }

    [AvaloniaFact]
    public void SubtitleFonts_SelectsOnlyThePreset_AndCountsThem()
    {
        var vm = MakeViewModel("Arial", "Comic Sans MS", "Verdana", "Zapfino");
        vm.Fonts[1].IsSelected = true;

        vm.SelectSubtitleFontsCommand.Execute(null);

        Assert.Equal(new List<string> { "Arial", "Verdana" }, vm.Fonts.Where(f => f.IsSelected).Select(f => f.Name).ToList());
        Assert.StartsWith("2", vm.SelectedFontsText);
    }

    [AvaloniaFact]
    public void ClearFonts_UnselectsAll()
    {
        var vm = MakeViewModel("Arial", "Verdana");
        vm.Fonts[0].IsSelected = true;

        vm.ClearFontsCommand.Execute(null);

        Assert.DoesNotContain(vm.Fonts, f => f.IsSelected);
        Assert.StartsWith("0", vm.SelectedFontsText);
    }
}
