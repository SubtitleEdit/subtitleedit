using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class CharUtilsTest
{
    [Fact]
    public void IsAsciiDigit()
    {
        Assert.True(CharUtils.IsAsciiDigit('0'));
        Assert.True(CharUtils.IsAsciiDigit('1'));
        Assert.True(CharUtils.IsAsciiDigit('2'));
        Assert.True(CharUtils.IsAsciiDigit('3'));
        Assert.True(CharUtils.IsAsciiDigit('4'));
        Assert.True(CharUtils.IsAsciiDigit('5'));
        Assert.True(CharUtils.IsAsciiDigit('6'));
        Assert.True(CharUtils.IsAsciiDigit('7'));
        Assert.True(CharUtils.IsAsciiDigit('8'));
        Assert.True(CharUtils.IsAsciiDigit('9'));

        Assert.False(CharUtils.IsAsciiDigit('.'));
        Assert.False(CharUtils.IsAsciiDigit('A'));
        Assert.False(CharUtils.IsAsciiDigit(' '));
        Assert.False(CharUtils.IsAsciiDigit('z'));
    }

    [Fact]
    public void IsAsciiLetter()
    {
        Assert.True(CharUtils.IsAsciiLetter('a'));
        Assert.True(CharUtils.IsAsciiLetter('b'));
        Assert.True(CharUtils.IsAsciiLetter('z'));
        Assert.True(CharUtils.IsAsciiLetter('A'));
        Assert.True(CharUtils.IsAsciiLetter('Y'));
        Assert.True(CharUtils.IsAsciiLetter('Z'));

        Assert.False(CharUtils.IsAsciiLetter('æ'));
        Assert.False(CharUtils.IsAsciiLetter('ü'));
        Assert.False(CharUtils.IsAsciiLetter('2'));
        Assert.False(CharUtils.IsAsciiLetter('!'));
    }
}
