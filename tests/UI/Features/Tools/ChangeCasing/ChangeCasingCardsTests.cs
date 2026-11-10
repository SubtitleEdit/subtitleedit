using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Tools.ChangeCasing;

/// <summary>
/// Change casing shows its choices as cards, each with what it makes of the user's own first line.
/// </summary>
public class ChangeCasingCardsTests
{
    private static ChangeCasingViewModel MakeViewModel(params string[] lines)
    {
        var subtitle = new Subtitle();
        foreach (var line in lines)
        {
            subtitle.Paragraphs.Add(new Paragraph(line, 0, 1000));
        }

        var vm = new ChangeCasingViewModel(new StubWindowService());
        vm.Initialize(subtitle);
        return vm;
    }

    [AvaloniaFact]
    public void Samples_AreTheFirstLineOfSeveralWords()
    {
        var vm = MakeViewModel("...", "Yes", "<i>Hello there,\nmy friend.</i>");

        Assert.Equal("HELLO THERE, MY FRIEND.", vm.AllUppercaseSample);
        Assert.Equal("hello there, my friend.", vm.AllLowercaseSample);
        Assert.Equal("Hello there, my friend.", vm.NormalCasingSample);
    }

    [AvaloniaFact]
    public void OnlyFixUppercaseLines_LeavesAMixedCaseSampleAlone()
    {
        var vm = MakeViewModel("hello THERE my friend");
        vm.NormalCasingOnlyUpper = false;
        var fixedSample = vm.NormalCasingSample;

        vm.NormalCasingOnlyUpper = true;

        Assert.NotEqual("hello THERE my friend", fixedSample);
        Assert.Equal("hello THERE my friend", vm.NormalCasingSample);
    }

    [AvaloniaFact]
    public void Samples_AreEmptyWithoutText()
    {
        var vm = MakeViewModel();

        Assert.Empty(vm.NormalCasingSample);
        Assert.Empty(vm.AllUppercaseSample);
    }

    [AvaloniaFact]
    public void RadioButtons_InSeparateCards_StayOneChoice()
    {
        var vm = MakeViewModel("hello there");
        vm.NormalCasing = true;
        vm.FixNamesOnly = false;
        vm.AllUppercase = false;
        vm.AllLowercase = false;
        var window = new ChangeCasingWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var radios = window.GetLogicalDescendants().OfType<RadioButton>().ToList();
        radios[3].IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Single(radios, r => r.IsChecked == true);
        Assert.True(vm.AllLowercase);
        Assert.False(vm.NormalCasing);
        window.Close();
    }
}
