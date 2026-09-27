using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.StylePicker;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;

namespace UITests.Features.Main;

/// <summary>
/// "Set style..." picker (discussion #10742): one shortcut opens a list of the file's styles,
/// arrows/Enter or a number key set the style on all selected lines.
/// </summary>
public class StylePickerTests
{
    [AvaloniaFact]
    public void Picker_NumberKeyPicksByHeaderPosition_ZeroIsTheTenth()
    {
        var names = Enumerable.Range(1, 12).Select(i => $"Style {i:00}").ToList();

        var vm = MakePicker(names);
        vm.OnKeyDown(Press(Key.D3));
        Assert.True(vm.OkPressed);
        Assert.Equal("Style 03", vm.ResultStyle);
        Assert.False(vm.ResultIsNewStyle);

        vm = MakePicker(names);
        vm.OnKeyDown(Press(Key.NumPad0));
        Assert.Equal("Style 10", vm.ResultStyle);

        Assert.Equal("1", vm.VisibleItems[0].NumberText);
        Assert.Equal("0", vm.VisibleItems[9].NumberText);
        Assert.Equal(string.Empty, vm.VisibleItems[10].NumberText);
    }

    [AvaloniaFact]
    public void Picker_KeepsHeaderOrder_NotAlphabetical()
    {
        var vm = MakePicker(["Default", "voice", "Off-screen", "Phone"]);

        Assert.Equal(["Default", "voice", "Off-screen", "Phone"], vm.VisibleItems.Select(p => p.Name));
    }

    [AvaloniaFact]
    public void Picker_NumberKeyWithoutAStyleIsSwallowed()
    {
        var vm = MakePicker(["Default", "voice"]);
        var e = Press(Key.D7);

        vm.OnKeyDown(e);

        Assert.True(e.Handled); // not typed into the filter box
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void Picker_NumberKeysTypeOnceTheFilterHasText()
    {
        var vm = MakePicker(["Default", "Sign 1", "Sign 2"]);
        vm.FilterText = "Sign ";
        var e = Press(Key.D2);

        vm.OnKeyDown(e);

        Assert.False(e.Handled);
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void Picker_StartsOnTheSelectionsStyle_ArrowsAndEnterSetIt()
    {
        var vm = MakePicker(["Default", "voice", "Phone"], selectedStyles: ["voice", "voice"]);
        Assert.Equal("voice", vm.SelectedItem?.Name);
        Assert.True(vm.SelectedItem?.IsCurrent);

        vm.OnKeyDown(Press(Key.Down));
        vm.OnKeyDown(Press(Key.Down)); // clamps at the end
        vm.OnKeyDown(Press(Key.Enter));

        Assert.Equal("Phone", vm.ResultStyle);
    }

    [AvaloniaFact]
    public void Picker_MixedSelection_MarksEveryUsedStyleAndListsThem()
    {
        var vm = MakePicker(["Default", "voice", "Phone"], selectedStyles: ["Phone", "", "Phone"]);

        Assert.Equal(["Default", "Phone"], vm.VisibleItems.Where(p => p.IsCurrent).Select(p => p.Name));
        Assert.Contains("Phone, Default", vm.CurrentStyleInfo);
        Assert.Equal("Default", vm.SelectedItem?.Name); // no single current style - start at the top
    }

    [AvaloniaFact]
    public void Picker_FilterThenEnterPicksTheMatch_NotANewStyle()
    {
        var vm = MakePicker(["Default", "voice", "Phone"]);

        vm.FilterText = "vo";
        Assert.Equal(["voice", "vo"], vm.VisibleItems.Select(p => p.Name));
        Assert.True(vm.VisibleItems[1].IsNew);

        vm.OnKeyDown(Press(Key.Enter));

        Assert.Equal("voice", vm.ResultStyle);
        Assert.False(vm.ResultIsNewStyle);
    }

    [AvaloniaFact]
    public void Picker_UnknownNameThenEnterCreatesIt()
    {
        var vm = MakePicker(["Default"]);

        vm.FilterText = " Off-screen ";
        vm.OnKeyDown(Press(Key.Enter));

        Assert.True(vm.OkPressed);
        Assert.True(vm.ResultIsNewStyle);
        Assert.Equal("Off-screen", vm.ResultStyle);
    }

    [AvaloniaFact]
    public void Picker_NameThatDiffersOnlyInCaseOffersNoNewRow()
    {
        // ASS style names are unique ignoring case - "default" would become "default_2".
        var vm = MakePicker(["Default", "Defaults"]);

        vm.FilterText = "default";

        Assert.DoesNotContain(vm.VisibleItems, p => p.IsNew);
    }

    [AvaloniaFact]
    public void Picker_EscapeCancels_ManageStylesIsNotAPick()
    {
        var vm = MakePicker(["Default"]);
        vm.OnKeyDown(Press(Key.Escape));
        Assert.False(vm.OkPressed);
        Assert.Null(vm.ResultStyle);

        vm = MakePicker(["Default"]);
        vm.ShowStylesManager();
        Assert.True(vm.OpenStylesManager);
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void Item_ShowsFontColorsPositionAndUsage()
    {
        var style = new SsaStyle
        {
            Name = "Phone",
            FontName = "Verdana",
            FontSize = 24,
            Bold = true,
            Italic = true,
            Alignment = "8",
            Primary = new SKColor(255, 255, 0),
            OutlineWidth = 2,
            ShadowWidth = 0,
            BorderStyle = "3",
            MarginLeft = 20,
            MarginRight = 30,
            MarginVertical = 40,
        };

        var item = new StylePickerItem(style, 7, false, false, false);

        Assert.Equal("(7)", item.LineCountText);
        Assert.Contains("Verdana 24", item.Summary);
        Assert.Contains(Se.Language.General.Bold, item.Summary);
        Assert.Contains(Se.Language.General.TopCenter, item.Summary);
        Assert.Contains(Se.Language.General.BoxPerLine, item.BorderText);
        Assert.Contains("20", item.MarginsText);
        Assert.Contains("40", item.MarginsText);
        Assert.Equal("#FFFF00", item.PrimaryHex);
        Assert.Contains("7", item.UsageText);
    }

    [AvaloniaFact]
    public void Item_SsaLegacyAlignmentIsTranslated()
    {
        Assert.Equal(Se.Language.General.TopLeft, StylePickerItem.GetAlignmentName("5", isSsa: true));
        Assert.Equal(Se.Language.General.MiddleCenter, StylePickerItem.GetAlignmentName("10", isSsa: true));
        Assert.Equal(Se.Language.General.MiddleCenter, StylePickerItem.GetAlignmentName("5", isSsa: false));
        Assert.Equal(Se.Language.General.BottomCenter, StylePickerItem.GetAlignmentName("2", isSsa: true));
    }

    [AvaloniaFact]
    public void Window_BuildsAndLaysOut()
    {
        var vm = MakePicker(["Default", "voice", "Phone"], selectedStyles: ["voice"]);
        var window = new StylePickerWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.True(window.Bounds.Height > 0);

        window.Close();
    }

    [AvaloniaFact]
    public void Picker_ShowsTheSetStyleShortcutByPosition()
    {
        var styles = new List<SsaStyle> { new() { Name = "Default" }, new() { Name = "voice" }, new() { Name = "Phone" } };
        var vm = new StylePickerViewModel();
        vm.Initialize(styles, new Dictionary<string, int>(), [], 1, false, ["Ctrl+1", "", "Ctrl+3"]);

        Assert.Equal(["Ctrl+1", "", "Ctrl+3"], vm.VisibleItems.Select(p => p.ShortcutText));
    }

    [AvaloniaFact]
    public void StyleShortcuts_AreEverywhereShortcuts()
    {
        var (window, vm) = ShowMainWindowWithLines();
        try
        {
            var all = ShortcutsMain.GetAllShortcuts(vm);
            string[] names =
            [
                nameof(MainViewModel.ShowStylePickerCommand), nameof(MainViewModel.SetStyle1Command),
                nameof(MainViewModel.SetStyle5Command), nameof(MainViewModel.SetStyle10Command),
            ];
            foreach (var name in names)
            {
                Assert.Equal(ShortcutCategory.General, all.Single(s => s.Name == name).Category);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SetStyleN_SetsTheNthHeaderStyleOnAllSelectedLines()
    {
        var (window, vm) = ShowMainWindowWithLines();
        try
        {
            vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f.Name == "Advanced Sub Station Alpha");
            SetHeader(vm, MakeHeader("Default", "voice", "Off-screen"));
            Settle(window);

            // Header order, not alphabetical: 3 is "Off-screen".
            SelectFirstAndLast(vm);
            vm.SetStyle3Command.Execute(null);
            Settle(window);
            Assert.Equal(["Off-screen", "Default", "Off-screen"], vm.Subtitles.Select(p => p.Style));

            SelectFirstAndLast(vm);
            vm.SetStyle2Command.Execute(null);
            Settle(window);
            Assert.Equal(["voice", "Default", "voice"], vm.Subtitles.Select(p => p.Style));

            // No 10th style - nothing changes.
            SelectFirstAndLast(vm);
            vm.SetStyle10Command.Execute(null);
            Settle(window);
            Assert.Equal(["voice", "Default", "voice"], vm.Subtitles.Select(p => p.Style));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SetStyleN_DoesNothingForFormatsWithoutStyles()
    {
        var (window, vm) = ShowMainWindowWithLines();
        try
        {
            vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f.Name == "SubRip");
            SetHeader(vm, MakeHeader("Default", "voice"));
            Settle(window);

            vm.SetStyle2Command.Execute(null);
            Settle(window);

            Assert.DoesNotContain(vm.Subtitles, p => p.Style == "voice");
        }
        finally
        {
            window.Close();
        }
    }

    private static void SelectFirstAndLast(MainViewModel vm)
    {
        vm.SubtitleGrid.SelectedItems!.Clear();
        vm.SubtitleGrid.SelectedItems.Add(vm.Subtitles[0]);
        vm.SubtitleGrid.SelectedItems.Add(vm.Subtitles[2]);
        Assert.Equal(2, vm.SubtitleGridSelectedItems.Count);
    }

    private static string MakeHeader(params string[] styleNames)
    {
        var styles = string.Join(Environment.NewLine, styleNames.Select(p =>
            $"Style: {p},Arial,20,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1"));
        return "[Script Info]" + Environment.NewLine +
               "ScriptType: v4.00+" + Environment.NewLine + Environment.NewLine +
               "[V4+ Styles]" + Environment.NewLine +
               SsaStyle.DefaultAssStyleFormat + Environment.NewLine +
               styles + Environment.NewLine + Environment.NewLine +
               "[Events]";
    }

    private static void SetHeader(MainViewModel vm, string header)
    {
        var field = typeof(MainViewModel).GetField("_subtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("_subtitle not found");
        ((Subtitle)field.GetValue(vm)!).Header = header;
    }

    private static (Window Window, MainViewModel Vm) ShowMainWindowWithLines()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        for (var i = 0; i < 3; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", i * 5000 + 1000, i * 5000 + 3000), null!)
            {
                Number = i + 1,
                Style = "Default",
            });
        }

        Settle(window);

        vm.SelectedSubtitleIndex = 0;
        vm.SubtitleGrid.SelectedItem = vm.Subtitles[0];
        Settle(window);

        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static StylePickerViewModel MakePicker(IReadOnlyList<string> styleNames, IReadOnlyList<string>? selectedStyles = null)
    {
        var styles = styleNames.Select(p => new SsaStyle { Name = p }).ToList();
        var vm = new StylePickerViewModel();
        vm.Initialize(styles, styleNames.ToDictionary(p => p, _ => 1), selectedStyles ?? [], selectedStyles?.Count ?? 1, false);
        return vm;
    }

    private static KeyEventArgs Press(Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        return new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = key, KeyModifiers = modifiers };
    }
}
