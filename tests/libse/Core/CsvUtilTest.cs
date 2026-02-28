using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class CsvUtilTest
{
    [Fact]
    public void Simple_No_Quotes()
    {
        // Arrange
        var csv = "How are you?,I'm fine!,Thank you.";

        // Act
        var result = CsvUtil.CsvSplit(csv, false, out var con);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("How are you?", result[0]);
        Assert.Equal("I'm fine!", result[1]);
        Assert.Equal("Thank you.", result[2]);
        Assert.False(con);
    }

    [Fact]
    public void Simple_With_Quotes()
    {
        // Arrange
        var csv = "\"How are you?\",\"I'm fine!\",\"Thank you.\"";

        // Act
        var result = CsvUtil.CsvSplit(csv, false, out var con);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("How are you?", result[0]);
        Assert.Equal("I'm fine!", result[1]);
        Assert.Equal("Thank you.", result[2]);
        Assert.False(con);
    }

    [Fact]
    public void Simple_With_Quotes_And_Comma()
    {
        // Arrange
        var csv = "\"How are you?\",\"I'm fine!\",\"Thank you, my friend.\"";

        // Act
        var result = CsvUtil.CsvSplit(csv, false, out var con);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("How are you?", result[0]);
        Assert.Equal("I'm fine!", result[1]);
        Assert.Equal("Thank you, my friend.", result[2]);
        Assert.False(con);
    }

    [Fact]
    public void Simple_With_Quotes_And_Escaped_Quote()
    {
        // Arrange
        var csv = "\"How are you?\",\"I'm fine!\",\"Thank \"\"you\"\".\"";

        // Act
        var result = CsvUtil.CsvSplit(csv, false, out var con);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("How are you?", result[0]);
        Assert.Equal("I'm fine!", result[1]);
        Assert.Equal("Thank \"you\".", result[2]);
        Assert.False(con);
    }

    [Fact]
    public void CsvSplitLines1()
    {
        // Arrange
        var lines = new List<string>
        {
            "1,How are you?",
            "2,\"How are you?\"",
            "3,\"Thanks, I'm fine.\"",
            "4,\"Well,",
            "that is good to hear.\"",
            "5,Bye!",
        };

        // Act
        var result = CsvUtil.CsvSplitLines(lines, ',');

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal("How are you?", result[0][1]);
        Assert.Equal("How are you?", result[1][1]);
        Assert.Equal("Thanks, I'm fine.", result[2][1]);
        Assert.Equal("Well," + Environment.NewLine + "that is good to hear.", result[3][1]);
        Assert.Equal("Bye!", result[4][1]);
    }

    [Fact]
    public void CsvSplitLines2()
    {
        // Arrange
        var lines = new List<string>
        {
            "1;How are you?",
            "2;\"How are you?\"",
            "3;\"Thanks, I'm fine.\"",
            "4;\"Well,",
            "that is good to hear.\"",
            "5;Bye!",
        };

        // Act
        var result = CsvUtil.CsvSplitLines(lines, ';');

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal("How are you?", result[0][1]);
        Assert.Equal("How are you?", result[1][1]);
        Assert.Equal("Thanks, I'm fine.", result[2][1]);
        Assert.Equal("Well," + Environment.NewLine + "that is good to hear.", result[3][1]);
        Assert.Equal("Bye!", result[4][1]);
    }
}
