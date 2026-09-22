using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

namespace UITests.Features.Video;

/// <summary>
/// "Edit second subtitle settings" (#15110) feeds the second subtitle already on the video player
/// back into the style dialog. The dialog expects a freshly parsed subtitle and mutates what it is
/// given while previewing, so it gets a copy with the styling stripped.
/// </summary>
public class SecondarySubtitleStylerUnstyleTests
{
    private static Subtitle MakeStyled()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("World", 3000, 4000));
        return SecondarySubtitleStyler.BuildFromSettings(subtitle, null);
    }

    [Fact]
    public void Unstyle_StripsHeaderStyleNameAndLayer()
    {
        var styled = MakeStyled();
        styled.Paragraphs[0].Layer = -1;
        Assert.All(styled.Paragraphs, p => Assert.False(string.IsNullOrEmpty(p.Extra)));

        var result = SecondarySubtitleStyler.Unstyle(styled);

        Assert.Equal(string.Empty, result.Header);
        Assert.All(result.Paragraphs, p => Assert.Null(p.Extra));
        Assert.All(result.Paragraphs, p => Assert.Equal(0, p.Layer));
    }

    [Fact]
    public void Unstyle_KeepsTextAndTimes()
    {
        var result = SecondarySubtitleStyler.Unstyle(MakeStyled());

        Assert.Equal(2, result.Paragraphs.Count);
        Assert.Equal("Hello", result.Paragraphs[0].Text);
        Assert.Equal(3000, result.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(4000, result.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void Unstyle_IsACopy_TheStyledSubtitleIsLeftAlone()
    {
        var styled = MakeStyled();
        var header = styled.Header;
        var styleName = styled.Paragraphs[0].Extra;

        var result = SecondarySubtitleStyler.Unstyle(styled);
        result.Paragraphs[0].Text = "Changed";
        result.Paragraphs[0].Extra = "Other";

        Assert.Equal(header, styled.Header);
        Assert.Equal(styleName, styled.Paragraphs[0].Extra);
        Assert.Equal("Hello", styled.Paragraphs[0].Text);
    }
}
