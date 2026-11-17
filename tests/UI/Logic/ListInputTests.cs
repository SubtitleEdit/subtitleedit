using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Delete on an editable dialog list runs the list's Remove/Delete command, like the button
/// next to it - as Join and Batch convert always did.
/// </summary>
public class ListInputTests
{
    private static KeyEventArgs Key(Key key, KeyModifiers modifiers = KeyModifiers.None)
        => new() { Key = key, KeyModifiers = modifiers, RoutedEvent = InputElement.KeyDownEvent };

    [AvaloniaFact]
    public void Delete_RunsTheCommand()
    {
        var runs = 0;
        var list = new ListBox();
        ListInput.RunOnDeleteKey(list, new RelayCommand(() => runs++));

        var e = Key(Avalonia.Input.Key.Delete);
        list.RaiseEvent(e);

        Assert.Equal(1, runs);
        Assert.True(e.Handled);
    }

    [AvaloniaFact]
    public void OtherKeysAndModifiers_DoNothing()
    {
        var runs = 0;
        var list = new ListBox();
        ListInput.RunOnDeleteKey(list, new RelayCommand(() => runs++));

        list.RaiseEvent(Key(Avalonia.Input.Key.Back));
        list.RaiseEvent(Key(Avalonia.Input.Key.Delete, KeyModifiers.Shift));

        Assert.Equal(0, runs);
    }

    [AvaloniaFact]
    public void Delete_InATextBox_IsLeftToTheTextBox()
    {
        // RaiseEvent sets Source to the raiser, so the key has to come from a real child.
        var runs = 0;
        var textBox = new TextBox();
        var list = new Border { Child = textBox };
        ListInput.RunOnDeleteKey(list, new RelayCommand(() => runs++));

        textBox.RaiseEvent(Key(Avalonia.Input.Key.Delete));

        Assert.Equal(0, runs);
    }

    [AvaloniaFact]
    public void Delete_WhileDisabled_DoesNothing()
    {
        var runs = 0;
        var enabled = false;
        var list = new ListBox();
        ListInput.RunOnDeleteKey(list, new RelayCommand(() => runs++), () => enabled);

        list.RaiseEvent(Key(Avalonia.Input.Key.Delete));
        Assert.Equal(0, runs);

        enabled = true;
        list.RaiseEvent(Key(Avalonia.Input.Key.Delete));
        Assert.Equal(1, runs);
    }
}
