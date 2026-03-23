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
