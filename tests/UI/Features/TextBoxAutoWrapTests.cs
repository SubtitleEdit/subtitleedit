using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features;

/// <summary>
/// "Auto-wrap while typing" from SE4 (#16087). These tests change the global Se.Settings, so
/// restore what was there afterwards - other tests in this assembly read the same instance.
/// </summary>
public class TextBoxAutoWrapTests : IDisposable
{
    private readonly bool _originalAutoWrap = Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping;
    private readonly int _originalMaxLength = Se.Settings.General.SubtitleLineMaximumLength;

    public void Dispose()
    {
        Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping = _originalAutoWrap;
        Se.Settings.General.SubtitleLineMaximumLength = _originalMaxLength;
    }

    [Fact]
    public void TryWrap_BreaksAtLastSpace_WhenLineGetsTooLong()
    {
        const string text = "This is a line that is too long";

        var wrapped = TextBoxAutoWrap.TryWrap(text, text.Length, 30, () => "en", out var newText, out var caret);

        Assert.True(wrapped);
        Assert.Equal("This is a line that is too" + Environment.NewLine + "long", newText);
        Assert.Equal(newText.Length, caret);
    }

    [Fact]
    public void TryWrap_TypedSpaceAtEnd_StartsLineTwo()
    {
        const string text = "This is a line that is too long ";

        TextBoxAutoWrap.TryWrap(text, text.Length, 30, () => "en", out var newText, out var caret);

        Assert.Equal("This is a line that is too long" + Environment.NewLine, newText);
        Assert.Equal(newText.Length, caret);
    }

    [Fact]
    public void TryWrap_CaretBeforeBreak_StaysPut()
    {
        const string text = "This is a line that is too long";

        TextBoxAutoWrap.TryWrap(text, 4, 30, () => "en", out _, out var caret);

        Assert.Equal(4, caret);
    }

    [Theory]
    [InlineData("<font face=\"Arial\">", "</font>")]
    [InlineData("{\\fn Times New Roman}", "")]
    public void TryWrap_NoSpaceOutsideTags_LeavesTagsAlone(string startTag, string endTag)
    {
        // CJK text has no spaces - the only one is inside the tag, and breaking there corrupts it
        var text = startTag + new string('あ', 35) + endTag;

        var wrapped = TextBoxAutoWrap.TryWrap(text, text.Length, 30, () => "ja", out var newText, out _);

        Assert.False(wrapped);
        Assert.Equal(text, newText);
    }

    [Theory]
    [InlineData("Short line")]
    [InlineData("This is a line that is too long\r\nbut already has two lines")]
    [InlineData("Thisisalinewithoutanyspacesthatistoolong")]
    [InlineData("<i>This is a line that fits</i>")]
    public void TryWrap_LeavesTextAlone(string text)
    {
        var wrapped = TextBoxAutoWrap.TryWrap(text, text.Length, 30, () => "en", out var newText, out _);

        Assert.False(wrapped);
        Assert.Equal(text, newText);
    }

    [AvaloniaFact]
    public void Typing_WrapsOnlyWhenSettingIsOn()
    {
        Se.Settings.General.SubtitleLineMaximumLength = 30;
        var textBox = new TextBox { AcceptsReturn = true, Text = "This is a line that is too lon" };
        textBox.CaretIndex = textBox.Text.Length;
        TextBoxAutoWrap.Attach(textBox, () => "en");

        Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping = false;
        Type(textBox, "g");
        Assert.Equal("This is a line that is too long", textBox.Text);

        // Back at the limit: the line is now 31 chars, and the gate only wraps the keystroke that
        // crosses the limit (#16219), so put the 30-char text back before typing with it on.
        textBox.Text = "This is a line that is too lon";
        textBox.CaretIndex = textBox.Text.Length;
        Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping = true;
        Type(textBox, "g");
        Assert.Equal("This is a line that is too" + Environment.NewLine + "long", textBox.Text);
        Assert.Equal(textBox.Text.Length, textBox.CaretIndex);
    }

    [AvaloniaFact]
    public void Typing_IntoLineAlreadyTooLong_LeavesItAlone()
    {
        // A line that was too long when it landed in the box - a freshly opened file, or a
        // profile that lowered the limit - is not rewrapped by the first edit (#16219).
        Se.Settings.General.SubtitleLineMaximumLength = 30;
        Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping = true;
        var textBox = new TextBox { AcceptsReturn = true, Text = "This is a line that is already too long" };
        textBox.CaretIndex = 4;
        TextBoxAutoWrap.Attach(textBox, () => "en");

        Type(textBox, "x");

        Assert.Equal("Thisx is a line that is already too long", textBox.Text);
    }

    [AvaloniaFact]
    public void Typing_TheCharacterThatOverflows_Wraps()
    {
        Se.Settings.General.SubtitleLineMaximumLength = 30;
        Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping = true;
        var textBox = new TextBox { AcceptsReturn = true, Text = "This is a line that is too lon" };
        textBox.CaretIndex = textBox.Text.Length;
        TextBoxAutoWrap.Attach(textBox, () => "en");

        Type(textBox, "g");

        Assert.Equal("This is a line that is too" + Environment.NewLine + "long", textBox.Text);
    }

    [AvaloniaFact]
    public void EmptyTextInput_DoesNothing()
    {
        Se.Settings.General.SubtitleLineMaximumLength = 30;
        Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping = true;
        const string text = "This is a line that is already too long";
        var textBox = new TextBox { AcceptsReturn = true, Text = text };
        TextBoxAutoWrap.Attach(textBox, () => "en");

        Type(textBox, string.Empty);

        Assert.Equal(text, textBox.Text);
    }

    private static void Type(TextBox textBox, string text)
    {
        textBox.RaiseEvent(new TextInputEventArgs { RoutedEvent = InputElement.TextInputEvent, Text = text });
    }
}
