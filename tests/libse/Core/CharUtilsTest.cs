using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class CharUtilsTest
{
    [Fact]
    public void IsDigit()
    {
        Assert.True(CharUtils.IsDigit('0'));
        Assert.True(CharUtils.IsDigit('1'));
        Assert.True(CharUtils.IsDigit('2'));
        Assert.True(CharUtils.IsDigit('3'));
        Assert.True(CharUtils.IsDigit('4'));
        Assert.True(CharUtils.IsDigit('5'));
        Assert.True(CharUtils.IsDigit('6'));
        Assert.True(CharUtils.IsDigit('7'));
        Assert.True(CharUtils.IsDigit('8'));
        Assert.True(CharUtils.IsDigit('9'));

        Assert.False(CharUtils.IsDigit('.'));
        Assert.False(CharUtils.IsDigit('A'));
        Assert.False(CharUtils.IsDigit(' '));
        Assert.False(CharUtils.IsDigit('z'));
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
