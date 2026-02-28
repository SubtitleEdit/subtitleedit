using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

public class ActorConverterTest
{
    [Fact]
    public void SquareToSquare()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" };
        var result = c.FixActors(p, '[', ']', null, null);
        Assert.Equal("[Joe] How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void SquareToSquareUppercase()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" };
        var result = c.FixActors(p, '[', ']', ActorConverter.UpperCase, null);
        Assert.Equal("[JOE] How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void SquareToParentheses()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToParentheses = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" };
        var result = c.FixActors(p, '[', ']', null, null);
        Assert.Equal("(Joe) How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void SquareToParenthesesWithSecondLineNoActor()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToParentheses = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" + Environment.NewLine + "Are you okay?" };
        var result = c.FixActors(p, '[', ']', null, null);
        Assert.Equal("(Joe) How are you?" + Environment.NewLine + "Are you okay?", result.Paragraph.Text);
    }

    [Fact]
    public void SquareToParenthesesWithSecondLine()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToParentheses = true,
        };

        var p = new Paragraph() { Text = "How are you?" + Environment.NewLine + "[Joe] Are you okay?" };
        var result = c.FixActors(p, '[', ']', null, null);
        Assert.Equal("How are you?" + Environment.NewLine + "(Joe) Are you okay?", result.Paragraph.Text);
    }

    [Fact]
    public void SquareToParenthesesUppercase()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToParentheses = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" };
        var result = c.FixActors(p, '[', ']', ActorConverter.UpperCase, null);
        Assert.Equal("(JOE) How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void SquareToParenthesesLowercase()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToParentheses = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" };
        var result = c.FixActors(p, '[', ']', ActorConverter.LowerCase, null);
        Assert.Equal("(joe) How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void ParenthesesToSquareLowercase()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "(JOE) How are you?" };
        var result = c.FixActors(p, '(', ')', ActorConverter.NormalCase, null);
        Assert.Equal("[Joe] How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void ColorToParenthesesLowercase()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToParentheses = true,
        };

        var p = new Paragraph() { Text = "Joe: How are you?" };
        var text = c.FixActorsFromBeforeColon(p, ':', ActorConverter.LowerCase, null);
        Assert.Equal("(joe) How are you?", text);
    }

    [Fact]
    public void FromActorToSquare()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "How are you?", Actor = "Joe" };
        var text = c.FixActorsFromActor(p, null, null);
        Assert.Equal("[Joe] How are you?", text);
    }

    [Fact]
    public void SquareToActorUppercase()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" };
        var result = c.FixActors(p, '[', ']', ActorConverter.UpperCase, null);
        Assert.Equal("How are you?", result.Paragraph.Text);
        Assert.Equal("JOE", result.Paragraph.Actor);
    }

    [Fact]
    public void ColonDialogToSquare1()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "Joe: How are you?" + Environment.NewLine + "Jane: I'm fine." };
        var text = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("[Joe] How are you?" + Environment.NewLine + "[Jane] I'm fine.", text);
    }

    [Fact]
    public void ColonDialogToSquare2()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "- Joe: How are you?" + Environment.NewLine + "- Jane: I'm fine." };
        var text = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("[Joe] How are you?" + Environment.NewLine + "[Jane] I'm fine.", text);
    }

    [Fact]
    public void SquareToParenthesesDialog()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToParentheses = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" + Environment.NewLine + "[Jane] I am fine." };
        var result = c.FixActors(p, '[', ']', null, null);
        Assert.Equal("(Joe) How are you?" + Environment.NewLine + "(Jane) I am fine.", result.Paragraph.Text);
    }

    [Fact]
    public void SquareToActor()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" + Environment.NewLine + "[Jane] I am fine." };
        p.StartTime.TotalMilliseconds = 1000;
        p.EndTime.TotalMilliseconds = 2000;
        p.Style = "style";
        var result = c.FixActors(p, '[', ']', null, null);
        Assert.Equal("How are you?", result.Paragraph.Text);
        Assert.Equal("Joe", result.Paragraph.Actor);
        Assert.Equal("I am fine.", result.NextParagraph.Text);
        Assert.Equal("Jane", result.NextParagraph.Actor);
        Assert.Equal(p.StartTime.TotalMilliseconds, result.NextParagraph.StartTime.TotalMilliseconds);
        Assert.Equal(p.EndTime.TotalMilliseconds, result.NextParagraph.EndTime.TotalMilliseconds);
        Assert.Equal(p.Style, result.NextParagraph.Style);
        Assert.NotEqual(p.Id, result.NextParagraph.Id);
    }
}
