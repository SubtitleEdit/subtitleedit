using Nikse.SubtitleEdit.Core.Cea708;
using Nikse.SubtitleEdit.Core.Cea708.Commands;

namespace LibSETests.SubtitleFormats;

public class Cea708Test
{
    [Fact]
    public void CommandClearWindows()
    {
        var command = new ClearWindows(0, new[] { (byte)0xff }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(2, bytes.Length);
        Assert.Equal(ClearWindows.Id, bytes[0]);
        Assert.Equal(0xff, bytes[1]);
    }

    [Fact]
    public void CommandDefineWindow()
    {
        var command = new DefineWindow(0, new byte[] { 140, 63, 153, 0, 65, 11, 1 }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(7, bytes.Length);
        Assert.Equal((byte)command.Id, bytes[0]);
        Assert.Equal(63, bytes[1]);
        Assert.Equal(153, bytes[2]);
        Assert.Equal(0, bytes[3]);
        Assert.Equal(65, bytes[4]);
        Assert.Equal(11, bytes[5]);
        Assert.Equal(1, bytes[6]);
    }

    [Fact]
    public void CommandDelay()
    {
        var command = new Delay(0, new[] { (byte)0xff }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(2, bytes.Length);
        Assert.Equal(Delay.Id, bytes[0]);
        Assert.Equal(0xff, bytes[1]);
        Assert.Equal(25500, command.Milliseconds);
    }

    [Fact]
    public void CommandDelayCancel()
    {
        var command = new DelayCancel(0);
        var bytes = command.GetBytes();
        Assert.Single(bytes);
        Assert.Equal(DelayCancel.Id, bytes[0]);
    }

    [Fact]
    public void CommandDeleteWindows()
    {
        var command = new DeleteWindows(0, new[] { (byte)0xff }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(2, bytes.Length);
        Assert.Equal(DeleteWindows.Id, bytes[0]);
        Assert.Equal(0xff, bytes[1]);
    }

    [Fact]
    public void CommandDisplayWindows()
    {
        var command = new DisplayWindows(0, new[] { (byte)0xff }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(2, bytes.Length);
        Assert.Equal(DisplayWindows.Id, bytes[0]);
        Assert.Equal(0xff, bytes[1]);
    }

    [Fact]
    public void CommandEndOfText()
    {
        var command = new EndOfText(0);
        var bytes = command.GetBytes();
        Assert.Single(bytes);
        Assert.Equal(EndOfText.Id, bytes[0]);
    }

    [Fact]
    public void CommandHideWindows()
    {
        var command = new HideWindows(0, new[] { (byte)0xff }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(2, bytes.Length);
        Assert.Equal(HideWindows.Id, bytes[0]);
        Assert.Equal(0xff, bytes[1]);
    }

    [Fact]
    public void CommandReset()
    {
        var command = new Reset(0);
        var bytes = command.GetBytes();
        Assert.Single(bytes);
        Assert.Equal(Reset.Id, bytes[0]);
    }

    [Fact]
    public void CommandSetCurrentWindow()
    {
        var command = new SetCurrentWindow(0, 1);
        var bytes = command.GetBytes();
        Assert.Single(bytes);
        Assert.Equal(SetCurrentWindow.IdStart + 1, bytes[0]);
        Assert.Equal(1, command.WindowIndex);
    }

    [Fact]
    public void CommandSetPenAttributes()
    {
        var command = new SetPenAttributes(0, new byte[] { 140, 0xff }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(3, bytes.Length);
        Assert.Equal(SetPenAttributes.Id, bytes[0]);
        Assert.Equal(140, bytes[1]);
        Assert.Equal(0xff, bytes[2]);
    }

    [Fact]
    public void CommandSetPenColor()
    {
        var command = new SetPenColor(0, new byte[] { 145, 42, 0 }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(4, bytes.Length);
        Assert.Equal(SetPenColor.Id, bytes[0]);
        Assert.Equal(145, bytes[1]);
        Assert.Equal(42, bytes[2]);
        Assert.Equal(0, bytes[3]);
    }

    [Fact]
    public void CommandSetPenLocation()
    {
        var command = new SetPenLocation(0, new byte[] { 2, 4 }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(3, bytes.Length);
        Assert.Equal(SetPenLocation.Id, bytes[0]);
        Assert.Equal(2, bytes[1]);
        Assert.Equal(4, bytes[2]);
    }

    [Fact]
    public void CommandSetWindowAttributes()
    {
        var command = new SetWindowAttributes(0, new byte[] { 140, 255, 153, 0 }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(5, bytes.Length);
        Assert.Equal(SetWindowAttributes.Id, bytes[0]);
        Assert.Equal(140, bytes[1]);
        Assert.Equal(255, bytes[2]);
        Assert.Equal(153, bytes[3]);
        Assert.Equal(0, bytes[4]);
    }

    [Fact]
    public void CommandTextCommand()
    {
        var command = new SetText(0, "Hallo!");
        var bytes = command.GetBytes();
        Assert.Equal(6, bytes.Length);
        Assert.Equal(72, bytes[0]);
        Assert.Equal(97, bytes[1]);
        Assert.Equal(108, bytes[2]);
        Assert.Equal(108, bytes[3]);
        Assert.Equal(111, bytes[4]);
        Assert.Equal(33, bytes[5]);
    }

    [Fact]
    public void CommandToggleWindows()
    {
        var command = new ToggleWindows(0, new byte[] { 145 }, 0);
        var bytes = command.GetBytes();
        Assert.Equal(2, bytes.Length);
        Assert.Equal(ToggleWindows.Id, bytes[0]);
        Assert.Equal(145, bytes[1]);
    }

    [Fact]
    public void CcDataSectionTest()
    {
        var input = new byte[] { 0x72, 0xF4, 0xFC, 0x94, 0x2F, 0xFD, 0x80, 0x80, 0xFF, 0x0C, 0x34, 0xFE, 0x8C, 0xFF, 0xFE, 0x98, 0x00, 0xFE, 0x3C, 0x41, 0xFE, 0x02, 0x29, 0xFE, 0x11, 0x97, 0xFE, 0xD5, 0x15, 0xFE, 0x0C, 0x20, 0xFE, 0x92, 0x00, 0xFE, 0x02, 0x90, 0xFE, 0x05, 0x00, 0xFE, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00 };
        var ccDataSection = new CcDataSection(input, 0);
        var bytes = ccDataSection.GetBytes();
        Assert.Equal(20, ccDataSection.CcData.Length);
        Assert.Equal(input.Length, bytes.Length);
        for (var index = 0; index < input.Length; index++)
        {
            Assert.Equal(input[index], bytes[index]);
        }
    }

    [Fact]
    public void CcServiceInfoSectionTest()
    {
        var input = new byte[] { 0x73, 0xF2, 0xE0, 0x20, 0x20, 0x20, 0x7E, 0x7F, 0xFF, 0xE1, 0x65, 0x6E, 0x67, 0xC1, 0x7F, 0xFF };
        var serviceInfoSection = new CcServiceInfoSection(input, 0);
        var bytes = serviceInfoSection.GetBytes();
        Assert.Equal(2, serviceInfoSection.CcServiceInfoSectionElements.Length);
        Assert.Equal(input.Length, bytes.Length);
        for (var index = 0; index < input.Length; index++)
        {
            Assert.Equal(input[index], bytes[index]);
        }
    }

    [Fact]
    public void Smpte291MTest()
    {
        var input = new byte[] { 0x61, 0x01, 0x59, 0x96, 0x69, 0x59, 0x4F, 0x7F, 0x00, 0x00, 0x72, 0xF4, 0xFC, 0x94, 0x2F, 0xFD, 0x80, 0x80, 0xFF, 0x03, 0x22, 0xFE, 0x8A, 0xFF, 0xFE, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0x73, 0xF2, 0xE0, 0x20, 0x20, 0x20, 0x7E, 0x7F, 0xFF, 0xE1, 0x65, 0x6E, 0x67, 0xC1, 0x7F, 0xFF, 0x74, 0x00, 0x00, 0xFA, 0xBB };
        var smpte291M = new Smpte291M(input);
        var bytes = smpte291M.GetBytes();
        Assert.Equal(input.Length, bytes.Length);
        for (var index = 0; index < input.Length; index++)
        {
            Assert.Equal(input[index], bytes[index]);
        }
    }

    [Fact]
    public void VancTest()
    {
        var s = VancDataWriter.GenerateLinesFromText("Hi!", 0)[0];
        var smpte291M = new Smpte291M(HexStringToByteArray(s));
        var result = smpte291M.GetText(0, true, new CommandState());
        Assert.Equal("Hi!", result);
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        var numberChars = hex.Length;
        var bytes = new byte[numberChars / 2];
        for (var i = 0; i < numberChars - 1; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }
}
