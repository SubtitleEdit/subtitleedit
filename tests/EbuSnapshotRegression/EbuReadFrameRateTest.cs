using System.Reflection;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

[CollectionDefinition("EBU import global state", DisableParallelization = true)]
public class EbuImportGlobalStateCollection { }

[Collection("EBU import global state")]
public class EbuReadFrameRateTest
{
    private static byte[] Fixture(string diskCode, int count = 151, byte frame = 5)
    {
        var header = new Ebu.EbuGeneralSubtitleInformation { DiskFormatCode = diskCode, DisplayStandardCode = "0" };
        var bytes = new byte[1024 + 128 * count];
        Encoding.ASCII.GetBytes(header.ToString()).CopyTo(bytes, 0);
        for (var i = 0; i < count; i++)
        {
            var n = 1024 + i * 128;
            bytes[n + 1] = (byte)(i + 1); bytes[n + 2] = (byte)((i + 1) >> 8);
            bytes[n + 3] = 0xff; bytes[n + 5] = 10;
            bytes[n + 7] = (byte)(i % 50); bytes[n + 8] = frame;
            bytes[n + 9] = 10; bytes[n + 11] = (byte)(i % 50 + 2); bytes[n + 12] = 17;
            bytes[n + 13] = 20; bytes[n + 14] = 2;
            Array.Fill(bytes, (byte)0x8f, n + 16, 112); bytes[n + 16] = (byte)'A';
        }
        return bytes;
    }

    private static Subtitle Load(byte[] bytes, Action<int>? hook = null)
    {
        var reader = new Ebu();
        typeof(Ebu).GetProperty("BeforeReadTti", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(reader, hook);
        var subtitle = new Subtitle(); reader.LoadSubtitle(subtitle, bytes); return subtitle;
    }

    private static void WithState(Action body)
    {
        var fps = Configuration.Settings.General.CurrentFrameRate; var over = Ebu.OverrideReadFrameRate;
        try { Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); Ebu.OverrideReadFrameRate = 0; body(); }
        finally { Configuration.Settings.General.CurrentFrameRate = fps; Ebu.OverrideReadFrameRate = over; }
    }

    [Theory]
    [InlineData(0)][InlineData(100)][InlineData(-1)]
    public void GlobalSwitchCannotChangeTimingOrMetadata(int trigger) => WithState(() =>
    {
        var bytes = Fixture("STL25.01");
        Configuration.Settings.General.CurrentFrameRate = 25;
        var control = Load(bytes);
        var raced = Load(bytes, i => { if (i == trigger || trigger == -1 && i % 25 == 0)
            Configuration.Settings.General.CurrentFrameRate = trigger == -1 ? new[] { 23.976, 30.0, 24, 25 }[(i / 25) % 4] : 24000.0 / 1001; });
        Assert.Equal(control.Header, raced.Header); Assert.Equal(control.Paragraphs.Count, raced.Paragraphs.Count);
        for (var i = 0; i < control.Paragraphs.Count; i++)
        {
            var a = control.Paragraphs[i]; var b = raced.Paragraphs[i];
            Assert.Equal(a.StartTime.TotalMilliseconds, b.StartTime.TotalMilliseconds);
            Assert.Equal(a.EndTime.TotalMilliseconds, b.EndTime.TotalMilliseconds);
            Assert.Equal(a.Text, b.Text); Assert.Equal(a.MarginV, b.MarginV);
            Assert.Equal(0, b.StartTime.TotalMilliseconds % 40); Assert.Equal(0, b.EndTime.TotalMilliseconds % 40);
        }
    });

    [Theory]
    [InlineData("STL24.01", 0, 24)][InlineData("STL25.01", 0, 25)][InlineData("STL30.01", 0, 30)]
    [InlineData("STL23.01", 0, 23)][InlineData("STL29.01", 0, 29)]
    [InlineData("STL25.01", 23.976, 23.976)][InlineData("STL25.01", 29.97, 29.97)]
    [InlineData("STL25.01", 24, 24)][InlineData("STL25.01", 20, 25)]
    public void ExistingHeaderAndOverrideSemanticsStayStable(string code, double over, double expected) => WithState(() =>
    {
        Ebu.OverrideReadFrameRate = over;
        var bytes = Fixture(code);
        var control = Load(bytes); var raced = Load(bytes, _ => Configuration.Settings.General.CurrentFrameRate = 50);
        Assert.Equal(Math.Min(999, SubtitleFormat.FramesToMilliseconds(5, expected)), control.Paragraphs[0].StartTime.Milliseconds);
        Assert.Equal(control.Header, raced.Header);
        Assert.Equal(control.Paragraphs.Select(p => (p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds)),
            raced.Paragraphs.Select(p => (p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds)));
        Assert.Equal(over, Ebu.OverrideReadFrameRate);
    });

    [Fact] public void Control25AndMax999KeepExistingRounding() => WithState(() =>
    {
        var normal = Load(Fixture("STL25.01"));
        Assert.All(normal.Paragraphs, p => { Assert.Equal(0, p.StartTime.TotalMilliseconds % 40); Assert.Equal(0, p.EndTime.TotalMilliseconds % 40); });
        var capped = Load(Fixture("STL24.01", 1, 255)); Assert.Equal(999, capped.Paragraphs[0].StartTime.Milliseconds);
    });
}
