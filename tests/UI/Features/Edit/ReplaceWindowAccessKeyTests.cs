using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Logic;
using System.Linq;

namespace UITests.Features.Edit;

/// <summary>
/// The Replace dialog keeps SE4's Alt+F / Alt+R / Alt+A mnemonics (#14716). The chord must fire the
/// command once even while typing in the find box, and the letter must be underlined while Alt is
/// held, or nobody ever learns the shortcut exists.
/// </summary>
public class ReplaceWindowAccessKeyTests
{
    [AvaloniaTheory]
    [InlineData(PhysicalKey.F, nameof(ReplaceViewModel.FindNextPressed))]
    [InlineData(PhysicalKey.R, nameof(ReplaceViewModel.ReplacePressed))]
    [InlineData(PhysicalKey.A, nameof(ReplaceViewModel.ReplaceAllPressed))]
    public void AltAccessKey_FiresCommand_WhileFindBoxHasFocus(PhysicalKey key, string flag)
    {
        var vm = new ReplaceViewModel();
        vm.RefreshSubtitles(["Hello world"]);
        var window = new ReplaceWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var textBox = window.GetLogicalDescendants().OfType<TextBox>().First();
            textBox.Focus();
            Dispatcher.UIThread.RunJobs();

            window.KeyPressQwerty(key, RawInputModifiers.Alt);
            Dispatcher.UIThread.RunJobs();

            Assert.True((bool)typeof(ReplaceViewModel).GetProperty(flag)!.GetValue(vm)!);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AltAccessKey_FiresCommandExactlyOnce()
    {
        var count = 0;
        var button = UiUtil.MakeButton("_Find next", new RelayCommand(() => count++));
        var window = new Window { Content = new StackPanel { Children = { new TextBox(), button } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.GetLogicalDescendants().OfType<TextBox>().First().Focus();
            Dispatcher.UIThread.RunJobs();

            window.KeyPressQwerty(PhysicalKey.F, RawInputModifiers.Alt);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, count);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AccessLetter_IsUnderlined_WhileAltIsShown()
    {
        var button = UiUtil.MakeButton("_Find next", new RelayCommand(() => { }));
        var window = new Window { Content = button };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var accessText = Assert.IsType<AccessText>(button.Content);
            Assert.Equal("_Find next", accessText.Text);
            Assert.Equal("F", accessText.AccessKey.ToString()?.ToUpperInvariant());
            Assert.False(accessText.GetValue(AccessText.ShowAccessKeyProperty));

            // Avalonia's AccessKeyHandler flips this inherited property on the window while Alt is
            // held; AccessText draws the underline from it.
            window.SetValue(AccessText.ShowAccessKeyProperty, true);
            Dispatcher.UIThread.RunJobs();
            Assert.True(accessText.GetValue(AccessText.ShowAccessKeyProperty));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LabelWithoutMarker_StaysPlainText()
    {
        var button = UiUtil.MakeButton("Count", new RelayCommand(() => { }));
        Assert.Equal("Count", button.Content);
    }
}

/// <summary>
/// SE4 fired the mnemonics on the bare letter once focus rested on a button (WinForms processed
/// mnemonics for any control that did not consume text). Discussion #14716 asked for that back:
/// click Find once, then tap F / R / A with two fingers. Typing in the find box must stay typing.
/// </summary>
public class ReplaceWindowBareAccessKeyTests
{
    private static (ReplaceViewModel Vm, ReplaceWindow Window) Open()
    {
        var vm = new ReplaceViewModel();
        vm.RefreshSubtitles(["Hello world"]);
        var window = new ReplaceWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (vm, window);
    }

    [AvaloniaTheory]
    [InlineData(PhysicalKey.F, nameof(ReplaceViewModel.FindNextPressed))]
    [InlineData(PhysicalKey.R, nameof(ReplaceViewModel.ReplacePressed))]
    [InlineData(PhysicalKey.A, nameof(ReplaceViewModel.ReplaceAllPressed))]
    public void BareLetter_FiresCommand_WhenFocusIsOnAButton(PhysicalKey key, string flag)
    {
        var (vm, window) = Open();
        try
        {
            var findButton = window.GetLogicalDescendants().OfType<Button>()
                .First(b => b.HotKey?.Key == Key.F);
            findButton.Focus();
            Dispatcher.UIThread.RunJobs();
            Assert.True(findButton.IsFocused);

            window.KeyPressQwerty(key, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True((bool)typeof(ReplaceViewModel).GetProperty(flag)!.GetValue(vm)!);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BareLetter_MovesFocusToTheInvokedButton()
    {
        var (_, window) = Open();
        try
        {
            var buttons = window.GetLogicalDescendants().OfType<Button>().ToList();
            buttons.First(b => b.HotKey?.Key == Key.F).Focus();
            Dispatcher.UIThread.RunJobs();

            window.KeyPressQwerty(PhysicalKey.R, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True(buttons.First(b => b.HotKey?.Key == Key.R).IsFocused);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(0)] // find box
    [InlineData(1)] // replace box
    public void BareLetter_StaysTyping_WhileATextBoxHasFocus(int textBoxIndex)
    {
        var (vm, window) = Open();
        try
        {
            // The find box is an AutoCompleteBox whose TextBox lives in the visual tree only.
            var textBox = window.GetVisualDescendants().OfType<TextBox>().ElementAt(textBoxIndex);
            textBox.Focus();
            Dispatcher.UIThread.RunJobs();

            window.KeyPressQwerty(PhysicalKey.R, RawInputModifiers.None);
            window.KeyTextInput("r");
            Dispatcher.UIThread.RunJobs();

            Assert.False(vm.ReplacePressed);
            Assert.Equal("r", textBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BareLetter_IsIgnored_WhenFocusIsOnARadioButton_ButtonStillRuns()
    {
        // Any non-text control counts as "not typing" - a radio button, like in WinForms.
        var (vm, window) = Open();
        try
        {
            window.GetLogicalDescendants().OfType<RadioButton>().First().Focus();
            Dispatcher.UIThread.RunJobs();

            window.KeyPressQwerty(PhysicalKey.F, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.FindNextPressed);
        }
        finally
        {
            window.Close();
        }
    }
}
