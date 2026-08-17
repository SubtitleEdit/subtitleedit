using System.Text;
using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class StringBuilderExtensionsTest
{
    [Fact]
    public void TrimLeadingAndTrailing()
    {
        var sb = new StringBuilder(" \t\r\n Hello world \r\n ");
        sb.Trim();
        Assert.Equal("Hello world", sb.ToString());
    }

    [Fact]
    public void TrimLeadingOnly()
    {
        var sb = new StringBuilder("  Hello");
        sb.Trim();
        Assert.Equal("Hello", sb.ToString());
    }

    [Fact]
    public void TrimTrailingOnly()
    {
        var sb = new StringBuilder("Hello  ");
        sb.Trim();
        Assert.Equal("Hello", sb.ToString());
    }

    [Fact]
    public void TrimWhiteSpaceOnly()
    {
        var sb = new StringBuilder(" \t\r\n ");
        sb.Trim();
        Assert.Equal(0, sb.Length);
    }

    [Fact]
    public void TrimEmpty()
    {
        var sb = new StringBuilder();
        sb.Trim();
        Assert.Equal(0, sb.Length);
    }

    [Fact]
    public void TrimNoWhiteSpace()
    {
        var sb = new StringBuilder("Hello");
        sb.Trim();
        Assert.Equal("Hello", sb.ToString());
    }

    [Fact]
    public void StartsWithEmpty()
    {
        var sb = new StringBuilder();
        Assert.False(sb.StartsWith('a'));
    }

    [Fact]
    public void StartsWithMatch()
    {
        var sb = new StringBuilder("abc");
        Assert.True(sb.StartsWith('a'));
    }

    [Fact]
    public void StartsWithNoMatch()
    {
        var sb = new StringBuilder("abc");
        Assert.False(sb.StartsWith('b'));
    }

    [Fact]
    public void EndsWithEmpty()
    {
        var sb = new StringBuilder();
        Assert.False(sb.EndsWith('c'));
    }

    [Fact]
    public void EndsWithMatch()
    {
        var sb = new StringBuilder("abc");
        Assert.True(sb.EndsWith('c'));
    }

    [Fact]
    public void EndsWithNoMatch()
    {
        var sb = new StringBuilder("abc");
        Assert.False(sb.EndsWith('b'));
    }

    [Fact]
    public void CountCharEmpty()
    {
        var sb = new StringBuilder();
        Assert.Equal(0, sb.CountChar('a'));
    }

    [Fact]
    public void CountCharNoMatch()
    {
        var sb = new StringBuilder("Hello world");
        Assert.Equal(0, sb.CountChar('z'));
    }

    [Fact]
    public void CountCharMultiple()
    {
        var sb = new StringBuilder("Hello world");
        Assert.Equal(3, sb.CountChar('l'));
    }

    [Fact]
    public void CountCharAcrossChunks()
    {
        var sb = new StringBuilder("a", 1);
        for (var i = 0; i < 100; i++)
        {
            sb.Append("bab");
        }

        Assert.Equal(101, sb.CountChar('a'));
    }

    [Fact]
    public void AppendNumberNoPadding()
    {
        var sb = new StringBuilder();
        sb.AppendNumber(5, 1);
        Assert.Equal("5", sb.ToString());
    }

    [Fact]
    public void AppendNumberPadsSingleDigit()
    {
        var sb = new StringBuilder();
        sb.AppendNumber(5, 2);
        Assert.Equal("05", sb.ToString());
    }

    [Fact]
    public void AppendNumberDoesNotPadTwoDigits()
    {
        var sb = new StringBuilder();
        sb.AppendNumber(42, 2);
        Assert.Equal("42", sb.ToString());
    }

    [Fact]
    public void AppendNumberZeroPadded()
    {
        var sb = new StringBuilder();
        sb.AppendNumber(0, 2);
        Assert.Equal("00", sb.ToString());
    }

    [Fact]
    public void AppendNumberNegative()
    {
        var sb = new StringBuilder();
        sb.AppendNumber(-5, 2);
        Assert.Equal("-05", sb.ToString());
    }

    [Fact]
    public void AppendNumberPadsToThreeDigits()
    {
        var sb = new StringBuilder();
        sb.AppendNumber(5, 3);
        Assert.Equal("005", sb.ToString());
    }

    [Fact]
    public void AppendNumberIntMinValue()
    {
        var sb = new StringBuilder();
        sb.AppendNumber(int.MinValue, 2);
        Assert.Equal("-2147483648", sb.ToString());
    }
}
