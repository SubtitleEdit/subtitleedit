using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// 3D-Planes (OFS files): a 3D Blu-ray's subtitle depth for every frame, so each exported 3D
/// subtitle stands where the disc put it.
/// </summary>
public class Stereo3DPlaneTests
{
    private const byte Undefined = Stereo3DPlane.UndefinedOffset;

    public enum CountStyle
    {
        BigEndian,
        LittleEndian,

        // OFSExtractor 2026: rolls left at 0, little endian count plus an 8 byte length.
        OfsExtractor,
    }

    private static byte[] MakeOfs(byte[] offsets, int frameRateCode = 1, CountStyle countStyle = CountStyle.BigEndian)
    {
        var data = new List<byte>();
        data.AddRange([0x89, 0x4F, 0x46, 0x53, 0x0D, 0x0A, 0x1A, 0x0A]);
        data.AddRange("0100"u8.ToArray());
        data.AddRange(new byte[16]);
        data.Add((byte)(frameRateCode << 4));
        data.Add(countStyle == CountStyle.OfsExtractor ? (byte)0 : (byte)1);
        data.AddRange(new byte[2]);
        data.Add(0); // marker bits
        data.AddRange(new byte[4]); // start time code
        var count = (uint)offsets.Length;
        if (countStyle == CountStyle.BigEndian)
        {
            data.AddRange([(byte)(count >> 24), (byte)(count >> 16), (byte)(count >> 8), (byte)count]);
        }
        else
        {
            data.AddRange(BitConverter.GetBytes(count));
        }

        if (countStyle == CountStyle.OfsExtractor)
        {
            data.AddRange(BitConverter.GetBytes((ulong)count));
        }

        data.AddRange(offsets);
        return data.ToArray();
    }

    private static TimeSpan Frames(int frame, double fps = 24000.0 / 1001.0)
    {
        // Rounded to whole milliseconds, like subtitle times are.
        return TimeSpan.FromMilliseconds(Math.Round(frame * 1000.0 / fps));
    }

    [Theory]
    [InlineData(CountStyle.BigEndian)]
    [InlineData(CountStyle.LittleEndian)]
    [InlineData(CountStyle.OfsExtractor)]
    public void FromBytes_ReadsEveryFrame_WhateverTheCountsByteOrder(CountStyle countStyle)
    {
        var plane = Stereo3DPlane.FromBytes(MakeOfs([1, 2, 3, Undefined, 0x85], countStyle: countStyle));

        Assert.Equal(5, plane.FrameCount);
        Assert.Equal(4, plane.DefinedFrameCount);
        Assert.Equal((-5, 3), plane.GetDepthRange());
    }

    [Theory]
    [InlineData(1, 24000.0 / 1001.0)]
    [InlineData(2, 24.0)]
    [InlineData(3, 25.0)]
    [InlineData(4, 30000.0 / 1001.0)]
    [InlineData(6, 50.0)]
    [InlineData(7, 60000.0 / 1001.0)]
    public void FromBytes_ReadsTheFrameRate(int code, double expected)
    {
        var plane = Stereo3DPlane.FromBytes(MakeOfs([1], frameRateCode: code));

        Assert.Equal(expected, plane.FramesPerSecond, 6);
    }

    [Fact]
    public void FromBytes_RejectsAFileThatIsNotOfs()
    {
        Assert.Throws<InvalidDataException>(() => Stereo3DPlane.FromBytes(new byte[100]));
    }

    [Fact]
    public void FromBytes_RejectsAFileCutShort()
    {
        var data = MakeOfs(new byte[1000]);

        Assert.Throws<InvalidDataException>(() => Stereo3DPlane.FromBytes(data[..500]));
    }

    [Fact]
    public void GetOffset_DirectionBitMeansBehindTheScreen()
    {
        var plane = Stereo3DPlane.FromBytes(MakeOfs([0x8C]));

        Assert.Equal(-12, plane.GetOffset(TimeSpan.Zero, Frames(1)));
    }

    [Fact]
    public void GetOffset_TakesTheNearestOfTheFramesTheSubtitleIsShownOn()
    {
        // Frames 10-19 go from 2 to 7 and back; frame 20 (the end frame, not shown) is 30.
        var offsets = Enumerable.Repeat(Undefined, 30).ToArray();
        byte[] depths = [2, 3, 4, 5, 6, 7, 6, 5, 4, 3];
        depths.CopyTo(offsets, 10);
        offsets[9] = 40;
        offsets[20] = 30;
        var plane = Stereo3DPlane.FromBytes(MakeOfs(offsets));

        Assert.Equal(7, plane.GetOffset(Frames(10), Frames(20)));
        Assert.Equal(2, plane.GetOffset(Frames(10), Frames(11)));
    }

    [Fact]
    public void GetOffset_SkipsFramesWithoutDepth_AndIsNullWhenNoneHasOne()
    {
        var plane = Stereo3DPlane.FromBytes(MakeOfs([Undefined, 0x83, Undefined, Undefined]));

        Assert.Equal(-3, plane.GetOffset(TimeSpan.Zero, Frames(3)));
        Assert.Null(plane.GetOffset(Frames(2), Frames(4)));
    }

    [Fact]
    public void GetOffset_IsNullAfterTheLastFrame()
    {
        var plane = Stereo3DPlane.FromBytes(MakeOfs([5, 5]));

        Assert.Null(plane.GetOffset(Frames(10), Frames(20)));
    }

    [Fact]
    public void GetOffset_ASubtitleShorterThanAFrameGetsTheFrameItStartsOn()
    {
        var plane = Stereo3DPlane.FromBytes(MakeOfs([1, 9, 1]));

        Assert.Equal(9, plane.GetOffset(Frames(1), Frames(1)));
    }

    [Theory]
    [InlineData(10, Export3DMode.HalfTopBottom, 1920, 10)]
    [InlineData(10, Export3DMode.HalfSideBySide, 1920, 5)]
    [InlineData(-7, Export3DMode.HalfSideBySide, 1920, -4)]
    [InlineData(12, Export3DMode.HalfTopBottom, 1280, 8)]
    [InlineData(12, Export3DMode.HalfSideBySide, 3840, 12)]
    public void DepthFromPlaneOffset_ScalesTheBluRayOffsetToTheExport(int offset, Export3DMode mode, int screenWidth, int expected)
    {
        Assert.Equal(expected, Stereo3DImage.DepthFromPlaneOffset(offset, mode, screenWidth));
    }

    [Fact]
    public void Apply_UsesThePlanesDepth_AndTheFixedDepthWhereItHasNone()
    {
        var offsets = Enumerable.Repeat(Undefined, 100).ToArray();
        for (var i = 0; i < 50; i++)
        {
            offsets[i] = 10;
        }

        var plane = Stereo3DPlane.FromBytes(MakeOfs(offsets));

        var withDepth = MakeParameter(plane, Frames(10), Frames(20));
        Stereo3DImage.Apply(withDepth);
        var withoutDepth = MakeParameter(plane, Frames(60), Frames(70));
        Stereo3DImage.Apply(withoutDepth);

        // Flat at 910; half side-by-side 455. A plane offset of 10 is a depth of 5, the fixed depth is 2.
        Assert.Equal(460, withDepth.OverridePosition!.Value.X);
        Assert.Equal(457, withoutDepth.OverridePosition!.Value.X);
    }

    private static ImageParameter MakeParameter(Stereo3DPlane plane, TimeSpan start, TimeSpan end)
    {
        var bitmap = new SKBitmap(100, 40);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
        }

        return new ImageParameter
        {
            Bitmap = bitmap,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            Alignment = ExportAlignment.BottomCenter,
            BottomTopMargin = 10,
            LeftRightMargin = 10,
            StartTime = start,
            EndTime = end,
            Mode3D = Export3DMode.HalfSideBySide,
            Depth3D = 2,
            Plane3D = plane,
        };
    }
}
