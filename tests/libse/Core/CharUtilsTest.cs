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
    public void IsEnglishAlphabet()
    {
        Assert.True(CharUtils.IsEnglishAlphabet('a'));
        Assert.True(CharUtils.IsEnglishAlphabet('b'));
        Assert.True(CharUtils.IsEnglishAlphabet('z'));
        Assert.True(CharUtils.IsEnglishAlphabet('A'));
        Assert.True(CharUtils.IsEnglishAlphabet('Y'));
        Assert.True(CharUtils.IsEnglishAlphabet('Z'));

        Assert.False(CharUtils.IsEnglishAlphabet('æ'));
        Assert.False(CharUtils.IsEnglishAlphabet('ü'));
        Assert.False(CharUtils.IsEnglishAlphabet('2'));
        Assert.False(CharUtils.IsEnglishAlphabet('!'));
    }
}
