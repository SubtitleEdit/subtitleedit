using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Linq;
using System.Windows.Input;

namespace UITests.Features.Translate;

/// <summary>
/// The auto-translate window paints one footer button in the accent colour to mark it as the
/// default: Translate until something has been translated, then OK. Avalonia has no WinForms-style
/// AcceptButton though, so the accent was all it was - Enter did nothing until the button was
/// tabbed to or clicked. Enter now runs the accented button, and it starts out focused.
/// </summary>
public class AutoTranslateDefaultButtonTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static AutoTranslateViewModel MakeViewModel()
    {
        return new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there.", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("How are you?", 2000, 4000));
        return subtitle;
    }

    private static bool SendEnter(AutoTranslateViewModel vm, KeyModifiers modifiers = KeyModifiers.None)
    {
        var e = new KeyEventArgs
        {
            Key = Key.Enter,
            KeyModifiers = modifiers,
            RoutedEvent = InputElement.KeyDownEvent,
        };
        vm.KeyDown(e);
        return e.Handled;
    }

    private static (AutoTranslateViewModel Vm, Window Window) OpenWindow()
    {
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle());

        var window = new AutoTranslateWindow(vm);
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        return (vm, window);
    }

    // By command, not by caption: the captions are translated and can carry an access key.
    private static Button FooterButton(Window window, ICommand command)
    {
        return window.GetVisualDescendants()
            .OfType<Button>()
            .First(b => ReferenceEquals(b.Command, command));
    }

    [AvaloniaFact]
    public void TheDefaultButtonFollowsWhatTheUserCanDoNext()
    {
        var vm = MakeViewModel();

        // Nothing translated yet - Translate.
        vm.IsTranslateEnabled = true;
        vm.HasTranslatedSomething = false;
        Assert.Equal(AutoTranslateViewModel.DefaultButtonAction.Translate, vm.GetDefaultButtonAction());

        // Something translated - OK, so Enter keeps the result instead of translating again.
        vm.HasTranslatedSomething = true;
        Assert.Equal(AutoTranslateViewModel.DefaultButtonAction.Ok, vm.GetDefaultButtonAction());

        // Mid-translation both buttons are disabled, so Enter must not press either.
        vm.IsTranslateEnabled = false;
        Assert.Equal(AutoTranslateViewModel.DefaultButtonAction.None, vm.GetDefaultButtonAction());
    }

    [AvaloniaFact]
    public void EnterStartsTheTranslation()
    {
        var vm = MakeViewModel();
        vm.IsTranslateEnabled = true;

        // No window, so the command stops before it translates anything - handling the key is
        // what says Enter reached the Translate button instead of falling through.
        Assert.True(SendEnter(vm));
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void EnterPressesOkOnceSomethingIsTranslated()
    {
        var vm = MakeViewModel();
        vm.IsTranslateEnabled = true;
        vm.HasTranslatedSomething = true;

        Assert.True(SendEnter(vm));
        Assert.True(vm.OkPressed);
    }

    [AvaloniaFact]
    public void EnterDoesNothingWhileTranslating()
    {
        var vm = MakeViewModel();
        vm.IsTranslateEnabled = false;
        vm.HasTranslatedSomething = true;

        Assert.False(SendEnter(vm));
        Assert.False(vm.OkPressed);
    }

    // Alt+Enter and friends belong to whatever has focus, not to the default button.
    [AvaloniaFact]
    public void ModifiedEnterIsLeftAlone()
    {
        var vm = MakeViewModel();
        vm.IsTranslateEnabled = true;
        vm.HasTranslatedSomething = true;

        Assert.False(SendEnter(vm, KeyModifiers.Alt));
        Assert.False(SendEnter(vm, KeyModifiers.Control));
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void TheTranslateButtonIsFocusedWhenTheWindowOpens()
    {
        var (vm, window) = OpenWindow();

        Assert.True(FooterButton(window, vm.TranslateCommand).IsFocused);

        window.Close();
    }

    // Once a translation is done the accent moves to OK - the focus has to move with it, or the
    // still-focused Translate button would answer Enter itself and translate all over again.
    [AvaloniaFact]
    public void FocusMovesToOkWhenItBecomesTheDefaultButton()
    {
        var (vm, window) = OpenWindow();

        Assert.True(vm.IsTranslateEnabled);
        vm.HasTranslatedSomething = true;
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        Assert.True(FooterButton(window, vm.OkCommand).IsFocused);

        window.Close();
    }

    // The row grid is where the keyboard ends up after clicking a line to translate from - Enter
    // has to reach the window from there too.
    [AvaloniaFact]
    public void EnterReachesTheDefaultButtonFromTheRowGrid()
    {
        var (vm, window) = OpenWindow();

        var grid = window.GetVisualDescendants().OfType<TableView>().First();
        var container = (Visual?)grid.ContainerFromItem(vm.Rows[0]);
        Assert.NotNull(container);

        var bounds = container!.Bounds;
        var point = container.TranslatePoint(new Point(bounds.Width / 2, bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.True(grid.IsKeyboardFocusWithin, "the grid has to have the keyboard for this to test anything");

        vm.HasTranslatedSomething = true; // OK is the default button - pressing it has no side effects
        Dispatcher.UIThread.RunJobs();

        window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
    }
}
