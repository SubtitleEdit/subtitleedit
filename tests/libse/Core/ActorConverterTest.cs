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
        var result = c.FixActorsFromBeforeColon(p, ':', ActorConverter.LowerCase, null);
        Assert.Equal("(joe) How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void FromActorToSquare()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "How are you?", Actor = "Joe" };
        var result = c.FixActorsFromActor(p, null, null);
        Assert.Equal("[Joe] How are you?", result.Paragraph.Text);

        // The name is in the text now - leaving it in the actor column too would write it twice.
        Assert.Equal(string.Empty, result.Paragraph.Actor);

        // The input paragraph is never touched, the result carries the conversion.
        Assert.Equal("How are you?", p.Text);
        Assert.Equal("Joe", p.Actor);
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
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("[Joe] How are you?" + Environment.NewLine + "[Jane] I'm fine.", result.Paragraph.Text);
    }

    [Fact]
    public void ColonDialogToSquare2()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "- Joe: How are you?" + Environment.NewLine + "- Jane: I'm fine." };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("[Joe] How are you?" + Environment.NewLine + "[Jane] I'm fine.", result.Paragraph.Text);
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

    /// <summary>
    /// The colon conversions used to hand back nothing but a string, so the actor the converter had
    /// written on the paragraph was thrown away with it - "Inline actor via :" to "Actor" left the
    /// actor column empty (#14077).
    /// </summary>
    [Fact]
    public void ColonToActor()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "Joe: How are you?" };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("How are you?", result.Paragraph.Text);
        Assert.Equal("Joe", result.Paragraph.Actor);
        Assert.Null(result.NextParagraph);
    }

    [Fact]
    public void ColonToActorUppercase()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "- Joe: How are you?" };
        var result = c.FixActorsFromBeforeColon(p, ':', ActorConverter.UpperCase, null);
        Assert.Equal("How are you?", result.Paragraph.Text);
        Assert.Equal("JOE", result.Paragraph.Actor);
    }

    /// <summary>Two speakers in one paragraph become two paragraphs, like the bracket formats.</summary>
    [Fact]
    public void ColonDialogToActor()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "Joe: How are you?" + Environment.NewLine + "Jane: I am fine." };
        p.StartTime.TotalMilliseconds = 1000;
        p.EndTime.TotalMilliseconds = 2000;
        p.Style = "style";
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("How are you?", result.Paragraph.Text);
        Assert.Equal("Joe", result.Paragraph.Actor);
        Assert.Equal("I am fine.", result.NextParagraph.Text);
        Assert.Equal("Jane", result.NextParagraph.Actor);
        Assert.Equal(p.StartTime.TotalMilliseconds, result.NextParagraph.StartTime.TotalMilliseconds);
        Assert.Equal(p.EndTime.TotalMilliseconds, result.NextParagraph.EndTime.TotalMilliseconds);
        Assert.Equal(p.Style, result.NextParagraph.Style);
        Assert.NotEqual(p.Id, result.NextParagraph.Id);
    }

    /// <summary>
    /// One speaker named on the second line: the name goes in the actor column of the paragraph it
    /// is in - splitting off a paragraph with no actor at all would just move the problem.
    /// </summary>
    [Fact]
    public void ColonOnSecondLineOnlyToActor()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "How are you?" + Environment.NewLine + "Jane: I am fine." };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("How are you?" + Environment.NewLine + "I am fine.", result.Paragraph.Text);
        Assert.Equal("Jane", result.Paragraph.Actor);
        Assert.Null(result.NextParagraph);
    }

    [Fact]
    public void SquareOnSecondLineOnlyToActor()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "How are you?" + Environment.NewLine + "[Jane] I am fine." };
        var result = c.FixActors(p, '[', ']', null, null);
        Assert.Equal("How are you?" + Environment.NewLine + "I am fine.", result.Paragraph.Text);
        Assert.Equal("Jane", result.Paragraph.Actor);
        Assert.Null(result.NextParagraph);
    }

    /// <summary>A three-line paragraph has room for two speakers - the unnamed line stays with the second.</summary>
    [Fact]
    public void ColonThreeLinesTwoActorsToActor()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "Joe: How are you?" + Environment.NewLine + "Jane: I am fine." + Environment.NewLine + "Thank you." };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("How are you?", result.Paragraph.Text);
        Assert.Equal("Joe", result.Paragraph.Actor);
        Assert.Equal("I am fine." + Environment.NewLine + "Thank you.", result.NextParagraph.Text);
        Assert.Equal("Jane", result.NextParagraph.Actor);
    }

    /// <summary>Three speakers do not fit in two paragraphs, so the line is left alone.</summary>
    [Fact]
    public void ColonThreeActorsToActorIsSkipped()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var text = "Joe: How are you?" + Environment.NewLine + "Jane: I am fine." + Environment.NewLine + "Bill: Me too.";
        var p = new Paragraph() { Text = text };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.True(result.Skip);
        Assert.Equal(text, result.Paragraph.Text);
    }

    /// <summary>Three lines are fine when the target is an inline format - nothing has to be split.</summary>
    [Fact]
    public void ColonThreeActorsToSquare()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToSquare = true,
        };

        var p = new Paragraph() { Text = "Joe: How are you?" + Environment.NewLine + "Jane: I am fine." + Environment.NewLine + "Bill: Me too." };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.False(result.Skip);
        Assert.Equal("[Joe] How are you?" + Environment.NewLine + "[Jane] I am fine." + Environment.NewLine + "[Bill] Me too.", result.Paragraph.Text);
    }

    /// <summary>
    /// A colon is not an actor by itself - "Only names" filters the preview by this flag, and every
    /// colon line used to come back pre-checked.
    /// </summary>
    [Fact]
    public void ColonInPlainTextIsNotSelected()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "Meet me at 3:30." };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.False(result.Selected);
    }

    /// <summary>The input paragraph is never touched - the result carries the conversion.</summary>
    [Fact]
    public void ColonToActorDoesNotChangeTheInputParagraph()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "Joe: How are you?" };
        c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("Joe: How are you?", p.Text);
        Assert.Null(p.Actor);
    }

    /// <summary>
    /// A closing bracket before an opening one is not an actor: the line - and every line after it -
    /// used to be dropped from the text.
    /// </summary>
    [Fact]
    public void ReversedBracketsKeepTheText()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "[Joe] How are you?" + Environment.NewLine + "5] > [3" };
        var result = c.FixActors(p, '[', ']', null, null);
        Assert.Equal("How are you?" + Environment.NewLine + "5] > [3", result.Paragraph.Text);
        Assert.Equal("Joe", result.Paragraph.Actor);
    }

    [Fact]
    public void FromActorToActorIsANoOp()
    {
        var c = new ActorConverter(new SubRip(), "en")
        {
            ToActor = true,
        };

        var p = new Paragraph() { Text = "How are you?", Actor = "Joe" };
        var result = c.FixActorsFromActor(p, null, null);
        Assert.Equal("How are you?", result.Paragraph.Text);
        Assert.Equal("Joe", result.Paragraph.Actor);
    }
}
