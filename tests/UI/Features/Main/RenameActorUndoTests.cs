using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// "Rename actor" rewrites the actor on every matching line in the file - a bulk edit that has
/// to be one undo step, and one that treats a name differing only by surrounding whitespace as
/// no rename at all.
/// </summary>
public class RenameActorUndoTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    [AvaloniaFact]
    public async Task RenameActor_IsOneUndoStep_AndUndoRestoresTheOldName()
    {
        var (window, vm) = CreateMainViewModel();
        AddLine(vm, "One", "Bob");
        AddLine(vm, "Two", "Alice");
        AddLine(vm, "Three", "Bob");
        SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
        var undoRedo = GetUndoRedoManager(vm);
        undoRedo.Do(vm.MakeUndoRedoObject("loaded"));

        vm.RenameActorInAllLines("Bob", "Robert");
        await SettleAsync(window);

        Assert.Equal(new[] { "Robert", "Alice", "Robert" }, vm.Subtitles.Select(p => p.Actor));
        Assert.Equal(2, undoRedo.UndoCount);
        Assert.True(undoRedo.CanUndo);

        vm.UndoCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(new[] { "Bob", "Alice", "Bob" }, vm.Subtitles.Select(p => p.Actor));
    }

    [AvaloniaFact]
    public async Task RenameActor_TrimsBeforeComparing_SoWhitespaceOnlyIsNoRename()
    {
        var (window, vm) = CreateMainViewModel();
        AddLine(vm, "One", "Bob");
        SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
        var undoRedo = GetUndoRedoManager(vm);
        undoRedo.Do(vm.MakeUndoRedoObject("loaded"));

        vm.RenameActorInAllLines("Bob", "  Bob ");
        await SettleAsync(window);

        Assert.Equal("Bob", vm.Subtitles[0].Actor);
        Assert.Equal(1, undoRedo.UndoCount);
    }

    private static void AddLine(MainViewModel vm, string text, string actor)
    {
        var startMs = vm.Subtitles.Count * 2000;
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, startMs + 1500) { Actor = actor }, null!)
        {
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static async Task SettleAsync(Window window)
    {
        for (var round = 0; round < 2; round++)
        {
            for (var pump = 0; pump < 8; pump++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            await Task.Delay(50);
        }
    }

    private static IUndoRedoManager GetUndoRedoManager(MainViewModel vm) =>
        (IUndoRedoManager)GetField("_undoRedoManager").GetValue(vm)!;

    private static void SetPrivateField(MainViewModel vm, string name, object value) =>
        GetField(name).SetValue(vm, value);

    private static FieldInfo GetField(string name) =>
        typeof(MainViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Field not found: " + name);

    private (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }
}
