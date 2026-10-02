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
    /// <summary>
    /// Roll-up captions (as upconverted from CEA-608): every line starts with CR, then re-defines a
    /// visible window and writes its text. No Hide/Clear/Delete command ever comes, so the CR must
    /// end the line on screen - it used to be decoded as a lone "\r" and every line of the stream
    /// ran into one cue.
    /// </summary>
    [Fact]
    public void DecodeRollUpCarriageReturnEndsTheLine()
    {
        byte[] Line(string text) => new byte[] { 0x0D, 0x98, 0x3B, 0x80, 0x0F, 0x01, 0x1F, 0x11, 0x92, 0x01, 0x00 }
            .Concat(System.Text.Encoding.ASCII.GetBytes(text)).ToArray();

        var state = new CommandState();
        Assert.Equal(string.Empty, Nikse.SubtitleEdit.Core.Cea708.Cea708.Decode(0, Line("Line one"), state, false));

        Assert.Equal("Line one", Nikse.SubtitleEdit.Core.Cea708.Cea708.Decode(1, Line("Line two"), state, false));
        Assert.Equal(0, state.StartLineIndex);

        Assert.Equal("Line two", Nikse.SubtitleEdit.Core.Cea708.Cea708.Decode(2, new byte[] { 0x0D }, state, false));
        Assert.Equal(1, state.StartLineIndex);
    }

    /// <summary>
    /// A pop-on caption is built in a hidden window - a CR there is a line break inside the caption.
    /// </summary>
    [Fact]
    public void DecodePopOnCarriageReturnIsLineBreak()
    {
        var bytes = new byte[] { 0x98, 0x1B, 0x80, 0x0F, 0x01, 0x1F, 0x11, 0x4F, 0x6E, 0x65, 0x0D, 0x54, 0x77, 0x6F, 0x8B, 0x01, 0x8A, 0x01 };

        var text = Nikse.SubtitleEdit.Core.Cea708.Cea708.Decode(0, bytes, new CommandState(), false);

        Assert.Equal("One" + Environment.NewLine + "Two", text);
    }
    // DefineWindow 0: hidden (pop-on being built) or visible (roll-up/paint-on)
    private static readonly byte[] HiddenWindow = { 0x98, 0x1B, 0x80, 0x0F, 0x01, 0x1F, 0x11 };
    private static readonly byte[] VisibleWindow = { 0x98, 0x3B, 0x80, 0x0F, 0x01, 0x1F, 0x11 };

    // ToggleWindows + HideWindows: the caption is displayed and removed in one packet, the way
    // SE's own MCC writer ends a caption - it keeps the time it was written
    private static string DecodeWithHideWindows(params byte[][] parts)
    {
        var bytes = parts.SelectMany(p => p).Concat(new byte[] { 0x8B, 0x01, 0x8A, 0x01 }).ToArray();
        return Nikse.SubtitleEdit.Core.Cea708.Cea708.Decode(0, bytes, new CommandState(), false);
    }

    private static byte[] Ascii(string text) => System.Text.Encoding.ASCII.GetBytes(text);

    /// <summary>
    /// SetPenLocation skipping columns on the same row leaves blank cells - a space in the text
    /// (a CEA-608 upconvert places words after mid-row codes this way).
    /// </summary>
    [Fact]
    public void DecodePenLocationSkippingColumnsIsSpace()
    {
        Assert.Equal("you were", DecodeWithHideWindows(HiddenWindow, Ascii("you"), new byte[] { 0x92, 0x00, 0x04 }, Ascii("were")));
    }

    [Fact]
    public void DecodePenLocationRightAfterTextAddsNoSpace()
    {
        Assert.Equal("youwere", DecodeWithHideWindows(HiddenWindow, Ascii("you"), new byte[] { 0x92, 0x00, 0x03 }, Ascii("were")));
    }

    [Fact]
    public void DecodeBackspaceErasesLastChar()
    {
        Assert.Equal("Cat", DecodeWithHideWindows(HiddenWindow, Ascii("Cax"), new byte[] { 0x08 }, Ascii("t")));
    }

    [Fact]
    public void DecodeHorizontalCarriageReturnErasesCurrentRowOnly()
    {
        Assert.Equal("One" + Environment.NewLine + "Two",
            DecodeWithHideWindows(HiddenWindow, Ascii("One"), new byte[] { 0x0D }, Ascii("Tw0"), new byte[] { 0x0E }, Ascii("Two")));
    }

    [Fact]
    public void DecodeFormFeedInHiddenWindowDiscardsCaption()
    {
        Assert.Equal("New", DecodeWithHideWindows(HiddenWindow, Ascii("Old"), new byte[] { 0x0C }, Ascii("New")));
    }

    /// <summary>
    /// FF erases a visible window - the caption on screen ends there.
    /// </summary>
    [Fact]
    public void DecodeFormFeedInVisibleWindowEndsCaption()
    {
        var state = new CommandState();
        var decode = new Func<int, byte[], string>((lineIndex, bytes) => Nikse.SubtitleEdit.Core.Cea708.Cea708.Decode(lineIndex, bytes, state, false));

        Assert.Equal(string.Empty, decode(0, VisibleWindow.Concat(Ascii("First")).ToArray()));
        Assert.Equal("First", decode(1, new byte[] { 0x0C }.Concat(Ascii("Second")).ToArray()));
        Assert.Equal(0, state.StartLineIndex);
    }

    /// <summary>
    /// Two roll-up lines each ended by CR in one packet are two captions - they used to run
    /// together as "ABCD".
    /// </summary>
    [Fact]
    public void DecodeTwoCarriageReturnsInOnePacketGiveTwoCaptions()
    {
        var state = new CommandState();
        var bytes = VisibleWindow.Concat(Ascii("AB")).Concat(new byte[] { 0x0D }).Concat(Ascii("CD")).Concat(new byte[] { 0x0D }).ToArray();

        var text = Nikse.SubtitleEdit.Core.Cea708.Cea708.Decode(0, bytes, state, false);

        Assert.Equal("AB" + Environment.NewLine + "CD", text);
        Assert.Equal(new[] { "AB", "CD" }, state.FlushedTexts.Select(p => p.Value));

        var decoder = new DtvccServiceDecoder();
        AddDtvccPacket(decoder, bytes, 1000);
        var paragraphs = Assert.Single(decoder.Finish(2000)).Value;
        Assert.Equal(new[] { "AB", "CD" }, paragraphs.Select(p => p.Text));
    }

    /// <summary>
    /// Text still on screen at the end of the stream ends at the given end time (the end of the
    /// last video frame, like CEA-608) - not at its own start.
    /// </summary>
    [Fact]
    public void DtvccFinishEndsLastCaptionAtEndTime()
    {
        var decoder = new DtvccServiceDecoder();
        AddDtvccPacket(decoder, VisibleWindow.Concat(Ascii("HI")).ToArray(), 1000);

        var paragraph = Assert.Single(Assert.Single(decoder.Finish(1033)).Value);

        Assert.Equal("HI", paragraph.Text);
        Assert.Equal(1000, paragraph.StartTime.TotalMilliseconds);
        Assert.Equal(1033, paragraph.EndTime.TotalMilliseconds);
    }

    /// <summary>
    /// Korean broadcasters send P16 characters as EUC-KR (KS X 1001) - "금방" is B1DD B9E6. Read
    /// as UTF-16 that was nonsense Hangul ("뇝맦").
    /// </summary>
    [Fact]
    public void DecodeP16EucKr()
    {
        Assert.Equal("금방", DecodeWithHideWindows(HiddenWindow, new byte[] { 0x18, 0xB1, 0xDD, 0x18, 0xB9, 0xE6 }));
    }

    /// <summary>
    /// P16 characters outside the EUC-KR range stay UTF-16.
    /// </summary>
    [Fact]
    public void DecodeP16Unicode()
    {
        Assert.Equal("é가", DecodeWithHideWindows(HiddenWindow, new byte[] { 0x18, 0x00, 0xE9, 0x18, 0xAC, 0x00 }));
    }

    /// <summary>
    /// A roll-up line stays on screen after its CR - it ends when the next line starts, or when
    /// the window is cleared. It used to end at its own CR, so every cue lasted about one frame.
    /// </summary>
    [Fact]
    public void DtvccRollUpLineStaysOnScreenUntilNextLineOrClear()
    {
        var decoder = new DtvccServiceDecoder();
        AddDtvccPacket(decoder, VisibleWindow.Concat(Ascii("One")).Concat(new byte[] { 0x0D }).ToArray(), 1000);
        AddDtvccPacket(decoder, Ascii("Two").Concat(new byte[] { 0x0D }).ToArray(), 3000);
        AddDtvccPacket(decoder, new byte[] { 0x88, 0x01 }, 5000); // ClearWindows: window 0

        var paragraphs = Assert.Single(decoder.Finish(9000)).Value;

        Assert.Equal(new[] { "One", "Two" }, paragraphs.Select(p => p.Text));
        Assert.Equal(new double[] { 1000, 3000 }, paragraphs.Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(new double[] { 3000, 5000 }, paragraphs.Select(p => p.EndTime.TotalMilliseconds));
    }

    /// <summary>
    /// A roll-up line still on screen at the end of the stream ends at the end time.
    /// </summary>
    [Fact]
    public void DtvccRollUpLineOnScreenAtEndOfStreamEndsAtEndTime()
    {
        var decoder = new DtvccServiceDecoder();
        AddDtvccPacket(decoder, VisibleWindow.Concat(Ascii("Last")).Concat(new byte[] { 0x0D }).ToArray(), 1000);

        var paragraph = Assert.Single(Assert.Single(decoder.Finish(4000)).Value);

        Assert.Equal("Last", paragraph.Text);
        Assert.Equal(4000, paragraph.EndTime.TotalMilliseconds);
    }

    // One DTVCC packet with one service 1 block
    /// <summary>
    /// A pop-on caption is built in a hidden window and shown by DisplayWindows - it starts there,
    /// not when its text was written (a whole caption early, as the next one is built while the
    /// current one shows). Hiding or deleting other windows must not end it.
    /// </summary>
    [Fact]
    public void DtvccPopOnCaptionStartsWhenDisplayed()
    {
        byte[] HiddenWindowN(int n) => new byte[] { (byte)(0x98 + n), 0x1B, 0x80, 0x0F, 0x01, 0x1F, 0x11 };
        var decoder = new DtvccServiceDecoder();
        AddDtvccPacket(decoder, HiddenWindowN(1).Concat(Ascii("First")).ToArray(), 1000);
        AddDtvccPacket(decoder, new byte[] { 0x8A, 0xFF, 0x89, 0x02 }, 3000); // hide all, display 1
        AddDtvccPacket(decoder, new byte[] { 0x8C, 0xFD }.Concat(HiddenWindowN(2)).Concat(Ascii("Second")).ToArray(), 3100); // delete all but 1
        AddDtvccPacket(decoder, new byte[] { 0x8A, 0xFF, 0x89, 0x04 }, 5000); // hide all, display 2

        var paragraphs = Assert.Single(decoder.Finish(7000)).Value;

        Assert.Equal(new[] { "First", "Second" }, paragraphs.Select(p => p.Text));
        Assert.Equal(new double[] { 3000, 5000 }, paragraphs.Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(new double[] { 5000, 7000 }, paragraphs.Select(p => p.EndTime.TotalMilliseconds));
    }

    /// <summary>
    /// ToggleWindows swaps the hidden caption in and the shown one out.
    /// </summary>
    [Fact]
    public void DtvccPopOnCaptionShownByToggleWindows()
    {
        byte[] HiddenWindowN(int n) => new byte[] { (byte)(0x98 + n), 0x1B, 0x80, 0x0F, 0x01, 0x1F, 0x11 };
        var decoder = new DtvccServiceDecoder();
        AddDtvccPacket(decoder, HiddenWindowN(0).Concat(Ascii("First")).ToArray(), 1000);
        AddDtvccPacket(decoder, new byte[] { 0x8B, 0x01 }, 2000); // toggle 0: shown
        AddDtvccPacket(decoder, HiddenWindowN(1).Concat(Ascii("Second")).ToArray(), 2500);
        AddDtvccPacket(decoder, new byte[] { 0x8B, 0x03 }, 4000); // toggle 0 + 1: 0 hidden, 1 shown
        AddDtvccPacket(decoder, new byte[] { 0x8C, 0xFF }, 6000); // delete all

        var paragraphs = Assert.Single(decoder.Finish(9000)).Value;

        Assert.Equal(new[] { "First", "Second" }, paragraphs.Select(p => p.Text));
        Assert.Equal(new double[] { 2000, 4000 }, paragraphs.Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(new double[] { 4000, 6000 }, paragraphs.Select(p => p.EndTime.TotalMilliseconds));
    }

    /// <summary>
    /// Redefining the displayed window as hidden hides it - the caption shown there ends, and is
    /// not lost when the next caption built in the same window is displayed.
    /// </summary>
    [Fact]
    public void DtvccRedefiningShownWindowAsHiddenEndsCaption()
    {
        var decoder = new DtvccServiceDecoder();
        AddDtvccPacket(decoder, HiddenWindow.Concat(Ascii("First")).ToArray(), 1000);
        AddDtvccPacket(decoder, new byte[] { 0x89, 0x01 }, 2000); // display 0
        AddDtvccPacket(decoder, HiddenWindow.Concat(Ascii("Second")).ToArray(), 4000); // redefine 0 hidden
        AddDtvccPacket(decoder, new byte[] { 0x89, 0x01 }, 5000); // display 0
        AddDtvccPacket(decoder, new byte[] { 0x8C, 0xFF }, 7000); // delete all

        var paragraphs = Assert.Single(decoder.Finish(9000)).Value;

        Assert.Equal(new[] { "First", "Second" }, paragraphs.Select(p => p.Text));
        Assert.Equal(new double[] { 2000, 5000 }, paragraphs.Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(new double[] { 4000, 7000 }, paragraphs.Select(p => p.EndTime.TotalMilliseconds));
    }

    private static void AddDtvccPacket(DtvccServiceDecoder decoder, byte[] serviceData, double timeMs)
    {
        var content = new List<byte> { (byte)((1 << 5) | serviceData.Length) };
        content.AddRange(serviceData);
        if ((content.Count + 1) % 2 != 0)
        {
            content.Add(0); // padding (null service block)
        }

        decoder.Add(3, (content.Count + 1) / 2, content[0], timeMs);
        for (var i = 1; i < content.Count; i += 2)
        {
            decoder.Add(2, content[i], content[i + 1], timeMs);
        }
    }
}
