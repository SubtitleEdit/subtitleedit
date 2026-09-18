using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.ActorPicker;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Main;

/// <summary>
/// Actor assignment workflow (#15018): the "Set actor 1-10" shortcuts keep pointing at the same
/// actor while new actors are added, they work outside the subtitle grid, and the actor picker
/// lists the whole cast with number keys, a filter and "new actor" by typing.
/// </summary>
public class ActorPickerTests
{
    [Fact]
    public void Order_StartsAlphabetical_AndAppendsLaterActors()
    {
        var order = new ActorOrder();

        Assert.Equal(["Bob", "Zoe"], order.GetOrdered(["Zoe", "Bob", "Zoe", "", null]));

        // "Anna" used to sort first and push every shortcut down by one.
        Assert.Equal(["Bob", "Zoe", "Anna"], order.GetOrdered(["Anna", "Zoe", "Bob"]));
    }

    [Fact]
    public void Order_ActorThatDisappearsComesBackInItsOldPlace()
    {
        var order = new ActorOrder();
        order.GetOrdered(["Anna", "Bob", "Zoe"]);

        Assert.Equal(["Anna", "Zoe"], order.GetOrdered(["Anna", "Zoe"]));
        Assert.Equal(["Anna", "Bob", "Zoe"], order.GetOrdered(["Zoe", "Bob", "Anna"])); // e.g. after undo
    }

    [Fact]
    public void Order_ResetStartsOver()
    {
        var order = new ActorOrder();
        order.GetOrdered(["Bob"]);
        order.GetOrdered(["Bob", "Anna"]);

        order.Reset();

        Assert.Equal(["Anna", "Bob"], order.GetOrdered(["Bob", "Anna"]));
    }

    [Fact]
    public void Order_RenameKeepsThePosition()
    {
        var order = new ActorOrder();
        order.GetOrdered(["Anna", "Bob", "Zoe"]);

        order.Rename("Anna", "Xena");

        Assert.Equal(["Xena", "Bob", "Zoe"], order.GetOrdered(["Xena", "Bob", "Zoe"]));
    }

    [Fact]
    public void Order_RenameIntoExistingActorMergesWithoutDuplicate()
    {
        var order = new ActorOrder();
        order.GetOrdered(["Anna", "Bob", "Zoe"]);

        order.Rename("Anna", "Zoe");

        Assert.Equal(["Bob", "Zoe"], order.GetOrdered(["Bob", "Zoe"]));
    }

    [Fact]
    public void Order_SetOrderLeavesAbsentActorsInPlace()
    {
        var order = new ActorOrder();
        order.GetOrdered(["Anna", "Bob", "Cleo", "Zoe"]);

        // "Bob" is not in the file right now, so the picker only knew the other three.
        order.SetOrder(["Zoe", "Anna", "Cleo"]);

        Assert.Equal(["Zoe", "Bob", "Anna", "Cleo"], order.GetOrdered(["Anna", "Bob", "Cleo", "Zoe"]));
    }

    [Fact]
    public void Picker_NumberKeyPicksByPosition_ZeroIsTheTenth()
    {
        var actors = Enumerable.Range(1, 12).Select(i => $"Actor {i:00}").ToList();

        var vm = MakePicker(actors);
        vm.OnKeyDown(Press(Avalonia.Input.Key.D3));
        Assert.True(vm.OkPressed);
        Assert.Equal("Actor 03", vm.ResultActor);

        vm = MakePicker(actors);
        vm.OnKeyDown(Press(Avalonia.Input.Key.NumPad0));
        Assert.Equal("Actor 10", vm.ResultActor);

        Assert.Equal("1", vm.VisibleItems[0].NumberText);
        Assert.Equal("0", vm.VisibleItems[9].NumberText);
        Assert.Equal(string.Empty, vm.VisibleItems[10].NumberText);
    }

    [Fact]
    public void Picker_NumberKeyWithoutAnActorIsSwallowed()
    {
        var vm = MakePicker(["Anna", "Bob"]);
        var e = Press(Avalonia.Input.Key.D7);

        vm.OnKeyDown(e);

        Assert.True(e.Handled); // not typed into the filter box
        Assert.False(vm.OkPressed);
    }

    [Fact]
    public void Picker_NumberKeysTypeOnceTheFilterHasText()
    {
        var vm = MakePicker(["Anna", "Guard 1", "Guard 2"]);
        vm.FilterText = "Guard ";
        var e = Press(Avalonia.Input.Key.D2);

        vm.OnKeyDown(e);

        Assert.False(e.Handled);
        Assert.False(vm.OkPressed);
    }

    [Fact]
    public void Picker_FilterThenEnterPicksTheMatch_NotANewActor()
    {
        var vm = MakePicker(["Anna", "Bob", "Zoe"]);

        vm.FilterText = "zo";
        Assert.Equal(["Zoe", "zo"], vm.VisibleItems.Select(p => p.Name));
        Assert.True(vm.VisibleItems[1].IsNew);

        vm.OnKeyDown(Press(Avalonia.Input.Key.Enter));

        Assert.Equal("Zoe", vm.ResultActor);
    }

    [Fact]
    public void Picker_UnknownNameThenEnterCreatesIt()
    {
        var vm = MakePicker(["Anna", "Bob"]);

        vm.FilterText = " Innkeeper ";
        vm.OnKeyDown(Press(Avalonia.Input.Key.Enter));

        Assert.True(vm.OkPressed);
        Assert.Equal("Innkeeper", vm.ResultActor);
    }

    [Fact]
    public void Picker_ExactNameOffersNoNewRow()
    {
        var vm = MakePicker(["Anna", "Annabel"]);

        vm.FilterText = "Anna";

        Assert.Equal(["Anna", "Annabel"], vm.VisibleItems.Select(p => p.Name));
        Assert.DoesNotContain(vm.VisibleItems, p => p.IsNew);
    }

    [Fact]
    public void Picker_StartsOnTheCurrentActor_ArrowsMoveTheSelection()
    {
        var vm = MakePicker(["Anna", "Bob", "Zoe"], currentActor: "Bob");
        Assert.Equal("Bob", vm.SelectedItem?.Name);

        vm.OnKeyDown(Press(Avalonia.Input.Key.Down));
        vm.OnKeyDown(Press(Avalonia.Input.Key.Down)); // clamps at the end
        vm.OnKeyDown(Press(Avalonia.Input.Key.Enter));

        Assert.Equal("Zoe", vm.ResultActor);
    }

    [Fact]
    public void Picker_AltArrowReorders_AndNumbersFollowThePosition()
    {
        var vm = MakePicker(["Anna", "Bob", "Zoe"], currentActor: "Zoe", shortcutTexts: ["Ctrl+1", "Ctrl+2", "Ctrl+3"]);

        vm.OnKeyDown(Press(Avalonia.Input.Key.Up, KeyModifiers.Alt));
        vm.OnKeyDown(Press(Avalonia.Input.Key.Up, KeyModifiers.Alt));
        vm.OnKeyDown(Press(Avalonia.Input.Key.Up, KeyModifiers.Alt)); // already first

        Assert.Equal(["Zoe", "Anna", "Bob"], vm.GetActorsInOrder());
        Assert.Equal(["Zoe", "Anna", "Bob"], vm.VisibleItems.Select(p => p.Name));
        Assert.Equal(["1", "2", "3"], vm.VisibleItems.Select(p => p.NumberText));
        Assert.Equal(["Ctrl+1", "Ctrl+2", "Ctrl+3"], vm.VisibleItems.Select(p => p.ShortcutText));
        Assert.False(vm.OkPressed); // reordering alone does not pick
    }

    [Fact]
    public void Picker_DeleteRemovesTheActor_EscapeCancels()
    {
        var vm = MakePicker(["Anna"]);
        vm.OnKeyDown(Press(Avalonia.Input.Key.Delete));
        Assert.True(vm.OkPressed);
        Assert.Equal(string.Empty, vm.ResultActor);

        vm = MakePicker(["Anna"]);
        vm.OnKeyDown(Press(Avalonia.Input.Key.Escape));
        Assert.False(vm.OkPressed);
        Assert.Null(vm.ResultActor);

        // With text in the filter, Delete belongs to the text box.
        vm = MakePicker(["Anna"]);
        vm.FilterText = "An";
        var e = Press(Avalonia.Input.Key.Delete);
        vm.OnKeyDown(e);
        Assert.False(e.Handled);
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void ActorShortcuts_AreEverywhereShortcuts()
    {
        var (window, vm) = ShowMainWindowWithLines();
        try
        {
            var all = ShortcutsMain.GetAllShortcuts(vm);
            string[] names =
            [
                nameof(MainViewModel.SetActor1Command), nameof(MainViewModel.SetActor10Command),
                nameof(MainViewModel.ShowActorPickerCommand), nameof(MainViewModel.SetNewActorCommand),
                nameof(MainViewModel.RemoveActorCommand),
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
    public void SetActorShortcut_WaveformFocused_KeepsItsActorWhenANewOneIsAdded()
    {
        WithShortcut(nameof(MainViewModel.SetActor1Command), ["Control", "Shift", nameof(Avalonia.Input.Key.F9)], () =>
        {
            var (window, vm) = ShowMainWindowWithLines();
            try
            {
                vm.Subtitles[1].Actor = "Bob";
                vm.Subtitles[2].Actor = "Zoe";
                FocusWaveform(window, vm);

                window.KeyPressQwerty(PhysicalKey.F9, RawInputModifiers.Control | RawInputModifiers.Shift);
                Settle(window);
                Assert.Equal("Bob", vm.Subtitles[0].Actor);

                // "Anna" sorts before "Bob", but shortcut 1 stays on "Bob".
                vm.Subtitles[2].Actor = "Anna";
                vm.Subtitles[0].Actor = string.Empty;
                window.KeyPressQwerty(PhysicalKey.F9, RawInputModifiers.Control | RawInputModifiers.Shift);
                Settle(window);

                Assert.Equal("Bob", vm.Subtitles[0].Actor);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact]
    public void BareDigitShortcut_TextBoxFocused_StillTypes()
    {
        WithShortcut(nameof(MainViewModel.SetActor1Command), [nameof(Avalonia.Input.Key.D1)], () =>
        {
            var (window, vm) = ShowMainWindowWithLines();
            try
            {
                vm.Subtitles[1].Actor = "Bob";
                vm.EditTextBox.Focus();
                Settle(window);

                window.KeyPressQwerty(PhysicalKey.Digit1, RawInputModifiers.None);
                Settle(window);

                Assert.True(string.IsNullOrEmpty(vm.Subtitles[0].Actor));
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static ActorPickerViewModel MakePicker(IReadOnlyList<string> actors, string currentActor = "", IReadOnlyList<string>? shortcutTexts = null)
    {
        var vm = new ActorPickerViewModel();
        vm.Initialize(actors, actors.ToDictionary(p => p, _ => 1), shortcutTexts ?? [], currentActor, 1);
        return vm;
    }

    private static KeyEventArgs Press(Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        return new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = key, KeyModifiers = modifiers };
    }

    private static void FocusWaveform(Window window, MainViewModel vm)
    {
        var av = vm.AudioVisualizer!;
        var peaks = new WavePeak2[126 * 60];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        av.WavePeaks = new WavePeakData2(126, peaks);
        av.SetPosition(0, vm.Subtitles, 0, 0, new List<SubtitleLineViewModel> { vm.Subtitles[0] });
        Settle(window);

        av.Focus();
        Settle(window);
        Assert.True(av.IsFocused);
    }

    /// <summary>
    /// The settings singleton is shared across the whole test run, so give the action exactly one
    /// binding - the one under test - and put the original bindings back afterwards.
    /// </summary>
    private static void WithShortcut(string actionName, string[] keys, Action test)
    {
        var original = Se.Settings.Shortcuts.Where(s => s.ActionName == actionName).ToList();
        Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == actionName);
        Se.Settings.Shortcuts.Add(new SeShortCut(actionName, [.. keys]));
        try
        {
            test();
        }
        finally
        {
            Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == actionName);
            Se.Settings.Shortcuts.AddRange(original);
        }
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
}
