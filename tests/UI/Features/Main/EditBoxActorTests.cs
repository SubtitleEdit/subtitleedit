using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// The optional Actor box in the edit panel (#16797): one box that both picks an existing actor
/// (inline completion, Down for the full list) and creates a new one, written on Enter or when
/// the selection moves on - never to a line that was selected only after typing started.
/// </summary>
public class EditBoxActorTests : IDisposable
{
    private readonly List<Window> _windows = [];

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Typing_a_prefix_and_Enter_picks_the_existing_actor()
    {
        var (window, vm, box) = ShowMainWindow();
        Select(window, vm, 2);
        Assert.Equal(string.Empty, box.Text ?? string.Empty);

        FocusBox(window, box);
        window.KeyTextInput("J");
        Settle(window);
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        Settle(window);

        Assert.Equal("John", vm.Subtitles[2].Actor);
        Assert.Equal("John", vm.Subtitles[0].Actor);
        Assert.Equal("Mary", vm.Subtitles[1].Actor);
    }

    [AvaloniaFact]
    public void A_new_name_is_written_to_the_line_it_was_typed_for_when_the_selection_moves()
    {
        var (window, vm, box) = ShowMainWindow();
        Select(window, vm, 1);
        Assert.Equal("Mary", box.Text);

        FocusBox(window, box);
        box.Text = string.Empty;
        window.KeyTextInput("Peter");
        Settle(window);
        Assert.Equal("Mary", vm.Subtitles[1].Actor); // not per keystroke

        Select(window, vm, 0);

        Assert.Equal("Peter", vm.Subtitles[1].Actor);
        Assert.Equal("John", vm.Subtitles[0].Actor);
        Assert.Equal("John", box.Text);
    }

    [AvaloniaFact]
    public void Escape_reverts_to_the_lines_actor()
    {
        var (window, vm, box) = ShowMainWindow();
        Select(window, vm, 0);

        FocusBox(window, box);
        box.Text = string.Empty;
        window.KeyTextInput("Zed");
        Settle(window);
        if (box.IsDropDownOpen)
        {
            window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Settle(window);
        }

        window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
        Settle(window);

        Assert.Equal("John", box.Text);
        Assert.Equal("John", vm.Subtitles[0].Actor);
    }

    [AvaloniaFact]
    public void Down_lists_the_whole_cast()
    {
        var (window, vm, box) = ShowMainWindow();
        Select(window, vm, 0);

        FocusBox(window, box);
        window.KeyPressQwerty(PhysicalKey.ArrowDown, RawInputModifiers.None);
        Settle(window);

        Assert.True(box.IsDropDownOpen);
        var popup = box.GetVisualDescendants().OfType<Popup>().First();
        var listBox = popup.Child!.GetVisualDescendants().Prepend(popup.Child).OfType<ListBox>().First();
        Assert.Equal(new[] { "John", "Mary" }, listBox.Items.Cast<string>());
    }

    [AvaloniaFact]
    public void Enter_sets_the_actor_on_every_selected_line()
    {
        var (window, vm, box) = ShowMainWindow();
        vm.SubtitleGrid.SelectedItems.Clear();
        vm.SubtitleGrid.SelectedItems.Add(vm.Subtitles[1]);
        vm.SubtitleGrid.SelectedItems.Add(vm.Subtitles[2]);
        Settle(window);

        FocusBox(window, box);
        box.Text = string.Empty;
        window.KeyTextInput("Kim");
        Settle(window);
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        Settle(window);

        Assert.Equal("John", vm.Subtitles[0].Actor);
        Assert.Equal("Kim", vm.Subtitles[1].Actor);
        Assert.Equal("Kim", vm.Subtitles[2].Actor);
    }

    private (Window Window, MainViewModel Vm, AutoCompleteBox Box) ShowMainWindow()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Settle(window);

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("one", 0, 1000), null!) { Number = 1, Actor = "John" });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("two", 2000, 3000), null!) { Number = 2, Actor = "Mary" });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("three", 4000, 5000), null!) { Number = 3 });
        vm.ShowActorBox = true;
        Settle(window);

        var box = view.GetVisualDescendants().OfType<AutoCompleteBox>()
            .Single(b => AutomationProperties.GetName(b) == Se.Language.General.Actor);
        Assert.True(box.IsEffectivelyVisible);
        return (window, vm, box);
    }

    private static void Select(Window window, MainViewModel vm, int index)
    {
        vm.SubtitleGrid.SelectedItem = vm.Subtitles[index];
        Settle(window);
        Assert.Same(vm.Subtitles[index], vm.SelectedSubtitle);
    }

    private static void FocusBox(Window window, AutoCompleteBox box)
    {
        var textBox = box.GetVisualDescendants().OfType<TextBox>().First();
        textBox.Focus();
        Settle(window);
        Assert.True(box.IsKeyboardFocusWithin);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
