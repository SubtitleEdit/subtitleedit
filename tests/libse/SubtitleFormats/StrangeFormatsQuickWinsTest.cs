using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Detection fixes found by scanning a collection of odd subtitle files: formats that threw
/// from IsMine on unrelated files, and readers that rejected (or half-read) files they can load.
/// </summary>
public class StrangeFormatsQuickWinsTest
{
    private static string WriteTempFile(string extension, byte[] content)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        File.WriteAllBytes(path, content);
        return path;
    }

    /// <summary>A zero-length record read one byte back ("remove padding") and threw.</summary>
    [Fact]
    public void VideoCdDatIsMineDoesNotThrowOnUnrelatedDatFile()
    {
        var path = WriteTempFile(".dat", new byte[4096]);
        try
        {
            Assert.False(new VideoCdDat().IsMine(null, path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void VideoCdDatReadsPlausibleRecord()
    {
        var path = WriteTempFile(".dat", MakeVideoCdDat(22500, 45000, "Hello"));
        try
        {
            var format = new VideoCdDat();
            var subtitle = new Subtitle();
            Assert.True(format.IsMine(null, path));
            format.LoadSubtitle(subtitle, null, path);
            Assert.Equal("Hello", subtitle.Paragraphs[0].Text.TrimEnd('\0'));
            Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
            Assert.Equal(2000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>".dat" is a common extension - binary data with backwards times and control characters is not a VCD subtitle.</summary>
    [Fact]
    public void VideoCdDatRejectsImplausibleRecords()
    {
        var path = WriteTempFile(".dat", MakeVideoCdDat(45000, 22500, "\u0001\u0002\u0003\u0004\u0005"));
        try
        {
            Assert.False(new VideoCdDat().IsMine(null, path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ClqttJsonIsMineDoesNotThrowOnJsonWithNestedEvents()
    {
        var lines = new List<string> { "{\"data\":{\"events\":[{\"t\":1}]}}" };
        Assert.False(new ClqttJson().IsMine(lines, "a.json"));
    }

    [Theory]
    [InlineData(".elr")]
    [InlineData(".ela")]
    [InlineData(".elb")]
    public void ElrStudioAcceptsAllItsExtensions(string extension)
    {
        var path = WriteTempFile(extension, MakeElrStudioFile());
        try
        {
            Assert.True(new ELRStudioClosedCaption().IsMine(null, path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ElrStudioRejectsOtherExtensions()
    {
        var path = WriteTempFile(".txt", MakeElrStudioFile());
        try
        {
            Assert.False(new ELRStudioClosedCaption().IsMine(null, path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>ELR Studio's json export: spaces around ':' and end_time as the last value in each object.</summary>
    [Fact]
    public void JsonType8ReadsTrailingNumberBeforeClosingBrace()
    {
        var json = "{\n" +
                   "    \"subtitles\": [\n" +
                   "        {\n" +
                   "            \"sub_order\" : 1,\n" +
                   "            \"text\" : \"this presentation is delivered by\",\n" +
                   "            \"start_time\" : 13.128,\n" +
                   "            \"end_time\" : 16.399\n" +
                   "        },        {\n" +
                   "            \"sub_order\" : 2,\n" +
                   "            \"text\" : \"development\",\n" +
                   "            \"start_time\" : 16.399,\n" +
                   "            \"end_time\" : 23.399\n" +
                   "        }    ]\n" +
                   "}";
        var lines = json.SplitToLines();
        var format = new JsonType8();
        var subtitle = new Subtitle();

        Assert.True(format.IsMine(lines, "subtitles.json"));
        format.LoadSubtitle(subtitle, lines, "subtitles.json");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("this presentation is delivered by", subtitle.Paragraphs[0].Text);
        Assert.Equal(13128, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(16399, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal(23399, subtitle.Paragraphs[1].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void JsonType8CompactStillLoads()
    {
        var lines = new List<string> { "[{\"start_time\":1.5,\"end_time\":3.75,\"text\":\"Hello\"},{\"start_time\":4,\"end_time\":5,\"text\":\"World\"}]" };
        var subtitle = new Subtitle();
        new JsonType8().LoadSubtitle(subtitle, lines, "a.json");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal(3750, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal("World", subtitle.Paragraphs[1].Text);
    }

    /// <summary>One 2048-byte VCD segment with a single record (times in 1/22500 s, big-endian).</summary>
    private static byte[] MakeVideoCdDat(int start, int end, string text)
    {
        var buffer = new byte[2048];
        var i = 8; // first time in + last time out
        buffer[i++] = 0; // zero byte
        buffer[i++] = 0; // multiple line flag
        buffer[i++] = 0;
        buffer[i++] = 100; // x
        buffer[i++] = 1;
        buffer[i++] = 0; // y = 256 (bottom)
        foreach (var value in new[] { start, end })
        {
            buffer[i++] = (byte)(value >> 24);
            buffer[i++] = (byte)(value >> 16);
            buffer[i++] = (byte)(value >> 8);
            buffer[i++] = (byte)value;
        }

        buffer[i++] = 0;
        buffer[i++] = (byte)text.Length;
        foreach (var c in text)
        {
            buffer[i++] = (byte)c;
        }

        // the record ends with one "last" byte; a start/end of -1 ends the segment
        buffer[i++] = 1;
        for (var j = 0; j < 16; j++)
        {
            buffer[i + j] = 0xFF;
        }

        return buffer;
    }

    /// <summary>The ELR Studio signature plus one subtitle-number marker (00 00 FE FF FF FF + number).</summary>
    private static byte[] MakeElrStudioFile()
    {
        var buffer = new byte[1024];
        byte[] header = { 0x05, 0x01, 0x0D, 0x15, 0x11, 0x00, 0xA9, 0x00, 0x45, 0x00, 0x6C, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x20, 0x00, 0x53, 0x00, 0x74, 0x00, 0x75, 0x00, 0x64, 0x00, 0x69, 0x00, 0x6F, 0x00 };
        Array.Copy(header, buffer, header.Length);
        byte[] subNumber = { 0x00, 0x00, 0xFE, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x01 };
        Array.Copy(subNumber, 0, buffer, 200, subNumber.Length);
        return buffer;
    }
}
