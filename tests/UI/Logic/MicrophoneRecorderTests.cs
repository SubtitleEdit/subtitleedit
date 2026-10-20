using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic;

/// <summary>
/// The "record speech to text box" button: ffmpeg capture arguments, the dshow device listing
/// parser, and how the recognized text lands in the text box.
/// </summary>
public class MicrophoneRecorderTests
{
    [Fact]
    public void ParseDshowAudioDevices_ReturnsAudioDevicesOnly_InOrder()
    {
        const string output = """
            [dshow @ 000001] DirectShow video devices (some may be both video and audio devices)
            [dshow @ 000001]  "Integrated Camera" (video)
            [dshow @ 000001]     Alternative name "@device_pnp_\\?\usb#vid_04f2"
            [dshow @ 000001] DirectShow audio devices
            [dshow @ 000001]  "Microphone (Realtek High Definition Audio)" (audio)
            [dshow @ 000001]     Alternative name "@device_cm_{33D9A762-90C8-11D0-BD43-00A0C911CE86}\wave_{1234}"
            [dshow @ 000001]  "Stereo Mix (Realtek High Definition Audio)" (audio)
            dummy: Immediate exit requested
            """;

        var devices = MicrophoneRecorder.ParseDshowAudioDevices(output);

        Assert.Equal(["Microphone (Realtek High Definition Audio)", "Stereo Mix (Realtek High Definition Audio)"], devices);
    }

    [Fact]
    public void ParseDshowAudioDevices_NoAudioDevices_ReturnsEmpty()
    {
        Assert.Empty(MicrophoneRecorder.ParseDshowAudioDevices("[dshow @ 1] DirectShow audio devices\r\ndummy: Immediate exit requested"));
    }

    [Fact]
    public void BuildWindowsCaptureArguments_QuotesDeviceAndWrites16kMonoWav()
    {
        var arguments = MicrophoneRecorder.BuildWindowsCaptureArguments("Microphone (USB)", @"C:\tmp\se_microphone.wav");

        Assert.Equal("-hide_banner -y -f dshow -i audio=\"Microphone (USB)\" -ac 1 -ar 16000 -c:a pcm_s16le \"C:\\tmp\\se_microphone.wav\"", arguments);
        // stdin is how the recording is stopped ("q"), so it must stay open
        Assert.DoesNotContain("-nostdin", arguments);
    }

    private static TextBoxWrapper MakeWrapper(string text, int selectionStart, int selectionEnd, out TextBox textBox)
    {
        textBox = new TextBox { Text = text };
        textBox.SelectionStart = selectionStart;
        textBox.SelectionEnd = selectionEnd;
        return new TextBoxWrapper(textBox);
    }

    [AvaloniaFact]
    public void InsertTextAtCaret_EmptyTextBox_InsertsText()
    {
        var wrapper = MakeWrapper(string.Empty, 0, 0, out var textBox);

        MainViewModel.InsertTextAtCaret(wrapper, "Hello there");

        Assert.Equal("Hello there", textBox.Text);
        Assert.Equal(11, textBox.CaretIndex);
    }

    [AvaloniaFact]
    public void InsertTextAtCaret_CaretAtZeroWithText_AppendsWithSpace()
    {
        var wrapper = MakeWrapper("Hello", 0, 0, out var textBox);

        MainViewModel.InsertTextAtCaret(wrapper, "there");

        Assert.Equal("Hello there", textBox.Text);
        Assert.Equal(11, textBox.CaretIndex);
    }

    [AvaloniaFact]
    public void InsertTextAtCaret_MidWord_PadsBothSides()
    {
        var wrapper = MakeWrapper("Hello world", 5, 5, out var textBox);

        MainViewModel.InsertTextAtCaret(wrapper, "big");

        Assert.Equal("Hello big world", textBox.Text);
    }

    [AvaloniaFact]
    public void InsertTextAtCaret_AfterSpace_NoDoubleSpace()
    {
        var wrapper = MakeWrapper("Hello ", 6, 6, out var textBox);

        MainViewModel.InsertTextAtCaret(wrapper, "there");

        Assert.Equal("Hello there", textBox.Text);
    }

    [AvaloniaFact]
    public void InsertTextAtCaret_ReplacesSelection()
    {
        var wrapper = MakeWrapper("Hello cruel world", 6, 11, out var textBox);

        MainViewModel.InsertTextAtCaret(wrapper, "kind");

        Assert.Equal("Hello kind world", textBox.Text);
        Assert.Equal(10, textBox.CaretIndex);
    }
}
