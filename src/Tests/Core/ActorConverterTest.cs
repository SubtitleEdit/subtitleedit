using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;

namespace Tests.Core
{
    [TestClass]
    public class ActorConverterTest
    {
        [TestMethod]
        public void SquareToSquare()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToSquare = true,
            };

            var p = new Paragraph() { Text = "[Joe] How are you?" };
            var result = c.FixActors(p, '[', ']', null, null);
            Assert.AreEqual("[Joe] How are you?", result.Paragraph.Text);
        }

        [TestMethod]
        public void SquareToSquareUppercase()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToSquare = true,
            };

            var p = new Paragraph() { Text = "[Joe] How are you?" };
            var result = c.FixActors(p, '[', ']', ActorConverter.UpperCase, null);
            Assert.AreEqual("[JOE] How are you?", result.Paragraph.Text);
        }

        [TestMethod]
        public void SquareToParentheses()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToParentheses = true,
            };

            var p = new Paragraph() { Text = "[Joe] How are you?" };
            var result = c.FixActors(p, '[', ']', null, null);
            Assert.AreEqual("(Joe) How are you?", result.Paragraph.Text);
        }

        [TestMethod]
        public void SquareToParenthesesWithSecondLineNoActor()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToParentheses = true,
            };

            var p = new Paragraph() { Text = "[Joe] How are you?" + Environment.NewLine + "Are you okay?" };
            var result = c.FixActors(p, '[', ']', null, null);
            Assert.AreEqual("(Joe) How are you?" + Environment.NewLine + "Are you okay?", result.Paragraph.Text);
        }

        [TestMethod]
        public void SquareToParenthesesWithSecondLine()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToParentheses = true,
            };

            var p = new Paragraph() { Text = "How are you?" + Environment.NewLine + "[Joe] Are you okay?" };
            var result = c.FixActors(p, '[', ']', null, null);
            Assert.AreEqual("How are you?" + Environment.NewLine + "(Joe) Are you okay?", result.Paragraph.Text);
        }

        [TestMethod]
        public void SquareToParenthesesUppercase()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToParentheses = true,
            };

            var p = new Paragraph() { Text = "[Joe] How are you?" };
            var result = c.FixActors(p, '[', ']', ActorConverter.UpperCase, null);
            Assert.AreEqual("(JOE) How are you?", result.Paragraph.Text);
        }

        [TestMethod]
        public void SquareToParenthesesLowercase()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToParentheses = true,
            };

            var p = new Paragraph() { Text = "[Joe] How are you?" };
            var result = c.FixActors(p, '[', ']', ActorConverter.LowerCase, null);
            Assert.AreEqual("(joe) How are you?", result.Paragraph.Text);
        }

        [TestMethod]
        public void ParenthesesToSquareLowercase()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToSquare = true,
            };

            var p = new Paragraph() { Text = "(JOE) How are you?" };
            var result = c.FixActors(p, '(', ')', ActorConverter.NormalCase, null);
            Assert.AreEqual("[Joe] How are you?", result.Paragraph.Text);
        }

        [TestMethod]
        public void ColorToParenthesesLowercase()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToParentheses = true,
            };

            var p = new Paragraph() { Text = "Joe: How are you?" };
            var text = c.FixActorsFromBeforeColon(p, ':', ActorConverter.LowerCase, null);
            Assert.AreEqual("(joe) How are you?", text);
        }

        [TestMethod]
        public void FromActorToSquare()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToSquare = true,
            };

            var p = new Paragraph() { Text = "How are you?", Actor = "Joe" };
            var text = c.FixActorsFromActor(p, null, null);
            Assert.AreEqual("[Joe] How are you?", text);
        }

        [TestMethod]
        public void SquareToActorUppercase()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToActor = true,
            };

            var p = new Paragraph() { Text = "[Joe] How are you?" };
            var result = c.FixActors(p, '[', ']', ActorConverter.UpperCase, null);
            Assert.AreEqual("How are you?", result.Paragraph.Text);
            Assert.AreEqual("JOE", result.Paragraph.Actor);
        }

        [TestMethod]
        public void ColonDialogToSquare1()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToSquare = true,
            };

            var p = new Paragraph() { Text = "Joe: How are you?" + Environment.NewLine + "Jane: I'm fine." };
            var text = c.FixActorsFromBeforeColon(p, ':', null, null);
            Assert.AreEqual("[Joe] How are you?" + Environment.NewLine + "[Jane] I'm fine.", text);
        }

        [TestMethod]
        public void ColonDialogToSquare2()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToSquare = true,
            };

            var p = new Paragraph() { Text = "- Joe: How are you?" + Environment.NewLine + "- Jane: I'm fine." };
            var text = c.FixActorsFromBeforeColon(p, ':', null, null);
            Assert.AreEqual("[Joe] How are you?" + Environment.NewLine + "[Jane] I'm fine.", text);
        }

        [TestMethod]
        public void SquareToParenthesesDialog()
        {
            var c = new ActorConverter(new SubRip(), "en")
            {
                ToParentheses = true,
            };

            var p = new Paragraph() { Text = "[Joe] How are you?" + Environment.NewLine + "[Jane] I am fine." };
            var result = c.FixActors(p, '[', ']', null, null);
            Assert.AreEqual("(Joe) How are you?" + Environment.NewLine + "(Jane) I am fine.", result.Paragraph.Text);
        }

        [TestMethod]
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
            Assert.AreEqual("How are you?", result.Paragraph.Text);
            Assert.AreEqual("Joe", result.Paragraph.Actor);
            Assert.AreEqual("I am fine.", result.NextParagraph.Text);
            Assert.AreEqual("Jane", result.NextParagraph.Actor);
            Assert.AreEqual(p.StartTime.TotalMilliseconds, result.NextParagraph.StartTime.TotalMilliseconds);
            Assert.AreEqual(p.EndTime.TotalMilliseconds, result.NextParagraph.EndTime.TotalMilliseconds);
            Assert.AreEqual(p.Style, result.NextParagraph.Style);
            Assert.AreNotEqual(p.Id, result.NextParagraph.Id);
        }
    }
}
