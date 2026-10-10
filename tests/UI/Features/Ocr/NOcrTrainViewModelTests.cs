using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using System;
using System.Collections.Generic;
using System.IO;
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

    [Fact]
    public void StartOrAbortTraining_StaysEnabledWhileRunning()
    {
        var attribute = typeof(NOcrTrainViewModel)
            .GetMethod("StartOrAbortTraining", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetCustomAttributes(typeof(CommunityToolkit.Mvvm.Input.RelayCommandAttribute), false)
            .Cast<CommunityToolkit.Mvvm.Input.RelayCommandAttribute>()
            .Single();
        Assert.True(attribute.AllowConcurrentExecutions);
    }

    [AvaloniaFact]
    public void SaveTrainedDatabase_SavesAndSetsName()
    {
        var fileName = Path.Combine(Path.GetTempPath(), "nocr-train-" + Guid.NewGuid() + ".nocr");
        try
        {
            var vm = MakeViewModel();
            var db = new NOcrDb(fileName) { OcrCharacters = new(), OcrCharactersExpanded = new() };

            Assert.True(vm.SaveTrainedDatabase(db, "MyDb"));

            Assert.True(File.Exists(fileName));
            Assert.Equal("MyDb", vm.TrainedDatabaseName);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [AvaloniaFact]
    public void SaveTrainedDatabase_WhenClosing_DoesNotOverwriteExistingDatabase()
    {
        var fileName = Path.Combine(Path.GetTempPath(), "nocr-train-" + Guid.NewGuid() + ".nocr");
        try
        {
            File.WriteAllBytes(fileName, new byte[] { 1, 2, 3 });
            var vm = MakeViewModel();
            var db = new NOcrDb(Path.Combine(Path.GetTempPath(), "missing-" + Guid.NewGuid() + ".nocr"))
            {
                FileName = fileName,
                OcrCharacters = new(),
                OcrCharactersExpanded = new(),
            };

            vm.OnClosing();

            Assert.False(vm.SaveTrainedDatabase(db, "MyDb"));
            Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(fileName));
            Assert.Null(vm.TrainedDatabaseName);
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
