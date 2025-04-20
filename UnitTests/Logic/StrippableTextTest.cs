using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
{
    
    public class StrippableTextTest
    {

        [Fact]
        public void StrippableTextItalic()
        {
            var st = new StrippableText("<i>Hi!</i>");
            Assert.Equal("<i>", st.Pre);
            Assert.Equal("!</i>", st.Post);
            Assert.Equal("Hi", st.StrippedText);
        }

        [Fact]
        public void StrippableTextAss()
        {
            var st = new StrippableText("{\\an9}Hi!");
            Assert.Equal("{\\an9}", st.Pre);
            Assert.Equal("!", st.Post);
            Assert.Equal("Hi", st.StrippedText);
        }

        [Fact]
        public void StrippableTextFont()
        {
            var st = new StrippableText("<font color=\"red\">Hi!</font>");
            Assert.Equal("<font color=\"red\">", st.Pre);
            Assert.Equal("!</font>", st.Post);
            Assert.Equal("Hi", st.StrippedText);
        }

        [Fact]
        public void StrippableTextItalic2()
        {
            var st = new StrippableText("<i>O</i>");
            Assert.Equal("<i>", st.Pre);
            Assert.Equal("</i>", st.Post);
            Assert.Equal("O", st.StrippedText);
        }

        [Fact]
        public void StrippableTextItalic3()
        {
            var st = new StrippableText("<i>Hi!");
            Assert.Equal("<i>", st.Pre);
            Assert.Equal("!", st.Post);
            Assert.Equal("Hi", st.StrippedText);
        }

        [Fact]
        public void StrippableTextFontDontTouch()
        {
            var st = new StrippableText("{MAN} Hi, how are you today!");
            Assert.Equal("", st.Pre);
            Assert.Equal("!", st.Post);
            Assert.Equal("{MAN} Hi, how are you today", st.StrippedText);
        }

        [Fact]
        public void StrippableOnlyPre()
        {
            var st = new StrippableText("(");
            Assert.Equal("(", st.Pre);
            Assert.Equal("", st.Post);
            Assert.Equal("", st.StrippedText);
        }

        [Fact]
        public void StrippableOnlyPre2()
        {
            var st = new StrippableText("<");
            Assert.Equal("", st.Pre);
            Assert.Equal("", st.Post);
            Assert.Equal("<", st.StrippedText);
        }

        [Fact]
        public void StrippableOnlyPre3()
        {
            var st = new StrippableText("<i>");
            Assert.Equal("<i>", st.Pre);
            Assert.Equal("", st.Post);
            Assert.Equal("", st.StrippedText);
        }

        [Fact]
        public void StrippableOnlyText()
        {
            var st = new StrippableText("H");
            Assert.Equal("", st.Pre);
            Assert.Equal("", st.Post);
            Assert.Equal("H", st.StrippedText);
        }

        [Fact]
        public void StrippableTextItalicAndFont()
        {
            var st = new StrippableText("<i><font color=\"red\">Hi!</font></i>");
            Assert.Equal("<i><font color=\"red\">", st.Pre);
            Assert.Equal("!</font></i>", st.Post);
            Assert.Equal("Hi", st.StrippedText);
        }

        [Fact]
        public void StrippableTextItalicAndMore()
        {
            var st = new StrippableText("<i>...<b>Hi!</b></i>");
            Assert.Equal("<i>...<b>", st.Pre);
            Assert.Equal("!</b></i>", st.Post);
            Assert.Equal("Hi", st.StrippedText);
        }

        [Fact]
        public void StrippableTextChangeCasing()
        {
            var st = new StrippableText("this is for www.nikse.dk. thank you.");
            st.FixCasing(new List<string>(), false, true, true, "Bye.");
            Assert.Equal("This is for www.nikse.dk. Thank you.", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeCasing2()
        {
            var st = new StrippableText("this is for www.nikse.dk! thank you.");
            st.FixCasing(new List<string>(), false, true, true, "Bye.");
            Assert.Equal("This is for www.nikse.dk! Thank you.", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeCasing3()
        {
            var st = new StrippableText("www.nikse.dk");
            st.FixCasing(new List<string>(), false, true, true, "Bye.");
            Assert.Equal("www.nikse.dk", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeCasing4()
        {
            var st = new StrippableText("- hi joe!" + Environment.NewLine + "- hi jane.");
            st.FixCasing(new List<string>(), false, true, true, "Bye.");
            Assert.Equal("- Hi joe!" + Environment.NewLine + "- Hi jane.", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeCasing6()
        {
            var st = new StrippableText("- hi joe!" + Environment.NewLine + "- hi jane.");
            st.FixCasing(new List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.Equal("- Hi Joe!" + Environment.NewLine + "- Hi Jane.", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeCasing7()
        {
            var st = new StrippableText("[ newsreel narrator ] ominous clouds of war.");
            st.FixCasing(new List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.Equal("[ Newsreel narrator ] Ominous clouds of war.", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeCasing8()
        {
            var st = new StrippableText("andy: dad!");
            st.FixCasing(new List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.Equal("Andy: Dad!", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeCasing9()
        {
            var st = new StrippableText("- quit! wait outside!" + Environment.NewLine + "- girl: miss, i've got a headache.");
            st.FixCasing(new List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.Equal("- Quit! Wait outside!" + Environment.NewLine + "- Girl: Miss, i've got a headache.", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeCasing10()
        {
            var st = new StrippableText("Uh, “thor and doctor jones”");
            st.FixCasing(new List<string> { "Thor", "Jones" }, true, true, true, "Bye.");
            Assert.Equal("Uh, “Thor and doctor Jones”", st.MergedString);
        }

        [Fact]
        public void StrippableTextChangeEllipsis()
        {
            var st = new StrippableText("…but never could.");
            st.FixCasing(new List<string>(), true, true, true, "Bye.");
            Assert.Equal("…but never could.", st.MergedString);
        }

    }
}
