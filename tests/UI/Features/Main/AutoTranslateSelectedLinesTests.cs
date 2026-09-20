using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Main;

/// <summary>
/// "Selected lines > Auto translate" with a single subtitle open (#14926): the selected lines are
/// translated from their own text, the grid shows the translation, and the subtitle becomes the
/// original - every row, not just the selected ones.
/// </summary>
public class AutoTranslateSelectedLinesTests
{
    [AvaloniaFact]
    public async Task NoOriginal_TranslatesSelectedLines_AndCapturesWholeOriginal()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "One", 0, 1000);
            AddLine(vm, "Two", 1000, 2000);
            AddLine(vm, "Three", 2000, 3000);
            var rows = vm.Subtitles.ToList();
            vm.SubtitleGrid.SelectedItems?.Clear();
            vm.SubtitleGrid.SelectedItems?.Add(rows[0]);
            vm.SubtitleGrid.SelectedItems?.Add(rows[1]);
            await SettleAsync(window);
            Assert.False(vm.ShowColumnOriginalText);

            FakeTranslateWindowService.Install(vm, translateInPlace: false);
            await vm.AutoTranslateSelectedLinesCommand.ExecuteAsync(null);
            await SettleAsync(window);

            Assert.True(FakeTranslateWindowService.LastInPlaceOffered);
            Assert.Equal(new[] { "T:One", "T:Two", "Three" }, vm.Subtitles.Select(p => p.Text));
            Assert.Equal(new[] { "One", "Two", "Three" }, vm.Subtitles.Select(p => p.OriginalText));
            Assert.True(vm.ShowColumnOriginalText);

            var gridText = GridCellTexts(vm);
            Assert.Contains("T:One", gridText);
            Assert.Contains("T:Two", gridText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>"Translate in place" in the dialog: the selected lines change, no original column appears.</summary>
    [AvaloniaFact]
    public async Task NoOriginal_TranslateInPlaceChecked_LeavesNoOriginal()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "One", 0, 1000);
            AddLine(vm, "Two", 1000, 2000);
            AddLine(vm, "Three", 2000, 3000);
            var rows = vm.Subtitles.ToList();
            vm.SubtitleGrid.SelectedItems?.Clear();
            vm.SubtitleGrid.SelectedItems?.Add(rows[1]);
            await SettleAsync(window);

            FakeTranslateWindowService.Install(vm, translateInPlace: true);
            await vm.AutoTranslateSelectedLinesCommand.ExecuteAsync(null);
            await SettleAsync(window);

            Assert.Equal(new[] { "One", "T:Two", "Three" }, vm.Subtitles.Select(p => p.Text));
            Assert.All(vm.Subtitles, p => Assert.True(string.IsNullOrEmpty(p.OriginalText)));
            Assert.False(vm.ShowColumnOriginalText);
            Assert.Contains("T:Two", GridCellTexts(vm));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static List<string> GridCellTexts(MainViewModel vm)
    {
        return vm.SubtitleGrid.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => Equals(tb.Tag, "Text"))
            .Select(tb => tb.Inlines == null || tb.Inlines.Count == 0 ? tb.Text ?? string.Empty : string.Concat(tb.Inlines.OfType<Avalonia.Controls.Documents.Run>().Select(r => r.Text)))
            .ToList();
    }

    /// <summary>Stands in for the auto-translate dialog: prefixes each row with "T:" and presses OK.</summary>
    public class FakeTranslateWindowService : DispatchProxy
    {
        private static bool _translateInPlace;

        public static bool LastInPlaceOffered { get; private set; }

        public static void Install(MainViewModel vm, bool translateInPlace)
        {
            _translateInPlace = translateInPlace;
            var field = typeof(MainViewModel).GetField("_windowService", BindingFlags.Instance | BindingFlags.NonPublic)!;
            field.SetValue(vm, Create<IWindowService, FakeTranslateWindowService>());
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name != nameof(IWindowService.ShowDialogAsync) ||
                targetMethod.GetGenericArguments() is not [_, var vmType] ||
                vmType != typeof(AutoTranslateViewModel))
            {
                throw new NotSupportedException(targetMethod?.Name);
            }

            var translateVm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
            ((Action<AutoTranslateViewModel>?)args![1])?.Invoke(translateVm);
            translateVm.OnLoaded();
            LastInPlaceOffered = translateVm.TranslateInPlaceIsVisible;
            translateVm.TranslateInPlace = _translateInPlace;
            foreach (var row in translateVm.Rows)
            {
                row.TranslatedText = "T:" + row.Text;
            }

            translateVm.OkPressed = true;
            return Task.FromResult(translateVm);
        }
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static async Task SettleAsync(Window window)
    {
        for (var i = 0; i < 2; i++)
        {
            for (var pump = 0; pump < 8; pump++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            await Task.Delay(50);
        }
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
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

    private static void AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
