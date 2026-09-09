using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
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
