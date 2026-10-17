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

        Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping = true;
        Type(textBox, "s");
        Assert.Equal("This is a line that is too" + Environment.NewLine + "longs", textBox.Text);
        Assert.Equal(textBox.Text.Length, textBox.CaretIndex);
    }

    private static void Type(TextBox textBox, string text)
    {
        textBox.RaiseEvent(new TextInputEventArgs { RoutedEvent = InputElement.TextInputEvent, Text = text });
    }
}
