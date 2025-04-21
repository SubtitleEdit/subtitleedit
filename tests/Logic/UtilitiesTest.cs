using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace Tests.Logic
{
    [Collection("NonParallelTests")]
    public class UtilitiesTest
    {
        [Fact]
        
        public void AutoBreakLine1()
        {
            const int maxLength = 43;
            var s = Utilities.AutoBreakLine("You have a private health insurance and life insurance." + Environment.NewLine + "A digital clone included.", 5, 33, string.Empty);
            var arr = s.Replace(Environment.NewLine, "\n").Split('\n');
            Assert.Equal(2, arr.Length);
            Assert.False(arr[0].Length > maxLength);
            Assert.False(arr[1].Length > maxLength);
        }

        [Fact]
        
        public void AutoBreakLine2()
        {
            var s = Utilities.AutoBreakLine("We're gonna lose him." + Environment.NewLine + "He's left him four signals in the last week.", 5, 33, string.Empty);
            Assert.False(s == "We're gonna lose him." + Environment.NewLine + "He's left him four signals in the last week.");
        }

        [Fact]
        
        public void AutoBreakLine3()
        {
            string s1 = "- We're gonna lose him." + Environment.NewLine + "- He's left him four signals in the last week.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal(s1, s2);
        }

        [Fact]
        
        public void AutoBreakLine4()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            const string s1 = "- Seriously, though. Are you being bullied? - Nope.";
            string s2 = Utilities.AutoBreakLine(s1);
            string target = "- Seriously, though. Are you being bullied?" + Environment.NewLine + "- Nope.";
            Assert.Equal(target, s2);
        }

        [Fact]
        
        public void AutoBreakLine5()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            const string s1 = "<i>30 years ago I'd found</i> The Book of the Dead.";
            var s2 = Utilities.AutoBreakLine(s1);
            var expected = "<i>30 years ago I'd found</i>" + Environment.NewLine + "The Book of the Dead.";
            Assert.Equal(expected, s2);
        }

        [Fact]
        
        public void AutoBreakLine5DoNoBreakAtArabicDialogue()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            string s1 = "!دعه -" + Environment.NewLine + "!دعه أنت -";
            string s2 = Utilities.AutoBreakLine(s1, "ar");
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void AutoBreakFrenchSpaceBeforePunctuation()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            string s1 = "Et elle te le dis maintenant ? Pour quoi donc donc ?";
            string s2 = Utilities.AutoBreakLine(s1, "fr");
            Assert.Equal("Et elle te le dis maintenant ?" + Environment.NewLine + "Pour quoi donc donc ?", s2);
        }

        [Fact]
        public void AutoBreakLine5DoNoBreakAtTwoMusicTaggedLines()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            string s1 = "♪ Yo ♪" + Environment.NewLine + "♪ Yo yo ♪";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void AutoBreakLine5DoNoBreakAtOneMusicTaggedLine()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            string s1 = "♪ Yo ♪" + Environment.NewLine + "Hallo.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void AutoBreakLine5DoNoBreakAtPeriod()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            const string s1 = "Oh, snap, we're still saying the same thing. This is amazing!";
            string s2 = Utilities.AutoBreakLine(s1);
            string target = "Oh, snap, we're still saying" + Environment.NewLine + "the same thing. This is amazing!";
            Assert.Equal(target, s2);
        }

        [Fact]
        public void AutoBreakLineDoNotBreakAfterDashDash()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            string s1 = "- That's hilarious, I don't--" + Environment.NewLine + "- Are the cheeks turning nice and pink?";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void AutoBreakLineDialog1()
        {
            const string s1 = "- Qu'est ce qui se passe ? - Je veux voir ce qu'ils veulent être.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("- Qu'est ce qui se passe ?" + Environment.NewLine + "- Je veux voir ce qu'ils veulent être.", s2);
        }

        [Fact]
        public void AutoBreakLineDialog2()
        {
            const string s1 = "- Je veux voir ce qu'ils veulent être. - Qu'est ce qui se passe ?";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("- Je veux voir ce qu'ils veulent être." + Environment.NewLine + "- Qu'est ce qui se passe ?", s2);
        }

        [Fact]
        public void AutoBreakLineDialog3()
        {
            const string s1 = "- Come on, I… - Here, let's try this one.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("- Come on, I…" + Environment.NewLine + "- Here, let's try this one.", s2);
        }

        [Fact]
        public void AutoBreakLineDialog4()
        {
            const string s1 = "JENNI: When I lose Nicole, I feel like I lose my child.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("JENNI: When I lose Nicole," + Environment.NewLine + "I feel like I lose my child.", s2);
        }

        [Fact]
        public void AutoBreakOneWordOnEachLine()
        {
            string s1 = "How" + Environment.NewLine + "are" + Environment.NewLine + "you?";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("How are you?", s2);
        }

        [Fact]
        public void AutoBreakLineHtmlTags1()
        {
            const string s1 = "JENNI: <i>When<i> I lose Nicole, I feel like I lose <i>my</i> child.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("JENNI: <i>When<i> I lose Nicole," + Environment.NewLine + "I feel like I lose <i>my</i> child.", s2);
        }

        [Fact]
        public void AutoBreakPreferPeriod()
        {
            var old = Configuration.Settings.General.MaxNumberOfLines;
            Configuration.Settings.General.MaxNumberOfLines = 3;
            Configuration.Settings.Tools.AutoBreakLineEndingEarly = true;
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = false;
            const string s1 = "Sorry. Sorry, I was miles away. Got to get everything ready for today.";
            string s2 = Utilities.AutoBreakLine(s1);
            Configuration.Settings.General.MaxNumberOfLines = old;
            Assert.Equal("Sorry. Sorry, I was miles away." + Environment.NewLine + "Got to get everything ready for today.", s2);
        }

        [Fact]
        public void AutoBreakPreferPeriod2()
        {
            const string s1 = "That's alright. I get it all the time.";
            Configuration.Settings.Tools.AutoBreakLineEndingEarly = true;
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("That's alright." + Environment.NewLine + "I get it all the time.", s2);
        }

        [Fact]
        public void AutoBreakPreferPeriod3()
        {
            const string s1 = "That's alright... I get it all the time.";
            Configuration.Settings.Tools.AutoBreakLineEndingEarly = true;
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("That's alright..." + Environment.NewLine + "I get it all the time.", s2);
        }

        [Fact]
        public void AutoBreakPreferExclamation()
        {
            const string s1 = "That's alright!!! I get it all the time.";
            Configuration.Settings.Tools.AutoBreakLineEndingEarly = true;
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("That's alright!!!" + Environment.NewLine + "I get it all the time.", s2);
        }

        [Fact]
        public void AutoBreakPreferPeriodAndItalic()
        {
            var old = Configuration.Settings.General.MaxNumberOfLines;
            Configuration.Settings.General.MaxNumberOfLines = 3;
            Configuration.Settings.Tools.AutoBreakLineEndingEarly = true;
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = false;
            const string s1 = "Sorry. Sorry, I was miles away. Got to get everything ready for <i>today</i>.";
            string s2 = Utilities.AutoBreakLine(s1);
            Configuration.Settings.General.MaxNumberOfLines = old;
            Assert.Equal("Sorry. Sorry, I was miles away." + Environment.NewLine + "Got to get everything ready for <i>today</i>.", s2);
        }

        [Fact]
        public void AutoBreakPreferComma()
        {
            Configuration.Settings.Tools.AutoBreakCommaBreakEarly = true;
            const string s1 = "Ha Ha Ha Ha Ha Ha Ha Ha Ha, Ha Ha Ha Ha Ha Ha Ha Ha Ha Ha Ha.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.Equal("Ha Ha Ha Ha Ha Ha Ha Ha Ha," + Environment.NewLine + "Ha Ha Ha Ha Ha Ha Ha Ha Ha Ha Ha.", s2);
        }

        [Fact]
        public void AutoBreakLine3Lines1ButOnlyTwo()
        {
            var old = Configuration.Settings.General.MaxNumberOfLines;
            Configuration.Settings.General.MaxNumberOfLines = 3;
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = false;
            const string s1 = "Follow him. Day and night wherever he goes and goes and goes and goes and goes <b>again<b>!";
            string s2 = Utilities.AutoBreakLine(s1);
            Configuration.Settings.General.MaxNumberOfLines = old;
            Assert.Equal("Follow him. Day and night wherever he goes" + Environment.NewLine + "and goes and goes and goes and goes <b>again<b>!", s2);
        }

        [Fact]
        public void AutoBreakLine3Lines1ButOnlyTwoWithSpaces()
        {
            var old = Configuration.Settings.General.MaxNumberOfLines;
            Configuration.Settings.General.MaxNumberOfLines = 3;
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = false;
            const string s1 = "Follow him. Day and night wherever he goes and goes and goes and goes and goes <b>again<b>!";
            string s2 = Utilities.AutoBreakLine(s1);
            Configuration.Settings.General.MaxNumberOfLines = old;
            Assert.Equal("Follow him. Day and night wherever he goes" + Environment.NewLine + "and goes and goes and goes and goes <b>again<b>!", s2);
        }

        [Fact]
        public void AutoBreakLine3Lines1()
        {
            var old = Configuration.Settings.General.MaxNumberOfLines;
            Configuration.Settings.General.MaxNumberOfLines = 3;
            const string s1 = "Follow him. Day and night wherever he goes and goes and goes and goes and goes and he goes and goes and goes and he goes <b>again<b>!!";
            string s2 = Utilities.AutoBreakLine(s1);
            Configuration.Settings.General.MaxNumberOfLines = old;
            Assert.Equal("Follow him. Day and night wherever he goes" + Environment.NewLine + "and goes and goes and goes and goes and he" + Environment.NewLine + "goes and goes and goes and he goes <b>again<b>!!", s2);
        }

        [Fact]
        public void AutoBreakLine3Lines2()
        {
            var old = Configuration.Settings.General.MaxNumberOfLines;
            Configuration.Settings.General.MaxNumberOfLines = 3;
            const string s1 = "la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la";
            string s2 = Utilities.AutoBreakLine(s1);
            Configuration.Settings.General.MaxNumberOfLines = old;
            Assert.Equal("la la la la la la la la la la la la la la" + Environment.NewLine + "la la la la la la la la la la la la la la" + Environment.NewLine + "la la la la la la la la la la la la la la", s2);
        }

        [Fact]
        
        public void AutoBreakLine3Lines3()
        {
            var old = Configuration.Settings.General.MaxNumberOfLines;
            Configuration.Settings.General.MaxNumberOfLines = 3;
            const string s1 = "<i>la</i> la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la la <i>la</i>";
            string s2 = Utilities.AutoBreakLine(s1);
            Configuration.Settings.General.MaxNumberOfLines = old;
            Assert.Equal("<i>la</i> la la la la la la la la la la la la la" + Environment.NewLine + "la la la la la la la la la la la la la la" + Environment.NewLine + "la la la la la la la la la la la la la <i>la</i>", s2);
        }

        [Fact]
        
        public void AutoBreakPreferPixelWidth()
        {
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;
            string res = Utilities.AutoBreakLine("Iiiiii iiiiii iiiiii iiii WWWWWW WWWWWW WWWWWW.");
            Assert.True(res.SplitToLines()[0].Contains('W'));
        }

        [Fact]
        
        public void AutoBreakPreferPixelWidth2()
        {
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;
            Configuration.Settings.Tools.AutoBreakPreferBottomHeavy = false;
            string res = Utilities.AutoBreakLine("Samo želim životnog partnera koji će mi biti prijatelj do kraja života,");
            Assert.Equal("Samo želim životnog partnera koji" + Environment.NewLine +
                            "će mi biti prijatelj do kraja života,", res);
        }

        [Fact]
        public void AutoBreakPreferPixelWidth3HeavyBottom()
        {
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;
            Configuration.Settings.Tools.AutoBreakPreferBottomHeavy = true;
            Configuration.Settings.Tools.AutoBreakLineEndingEarly = false;
            Configuration.Settings.Tools.AutoBreakPreferBottomPercent = 6;
            string res = Utilities.AutoBreakLine("Izvini. Trebalo bi da ima hrane za sve.");
            Assert.Equal("Izvini. Trebalo bi" + Environment.NewLine +
                            "da ima hrane za sve.", res);
        }

        [Fact]
        public void UnBreakLine1()
        {
            string s = Utilities.UnbreakLine("Hallo!" + Environment.NewLine + "Hallo!");
            Assert.Equal("Hallo! Hallo!", s);
        }

        [Fact]
        public void UnBreakLine2()
        {
            string s = Utilities.UnbreakLine("Hallo!\nHallo!");
            Assert.Equal("Hallo! Hallo!", s);
        }

        [Fact]
        public void UnBreakLine3()
        {
            string s = Utilities.UnbreakLine("Hallo!\r\nHallo!");
            Assert.Equal("Hallo! Hallo!", s);
        }

        [Fact]
        public void UnBreakLine4()
        {
            string s = Utilities.UnbreakLine("Hallo! \nHallo!");
            Assert.Equal("Hallo! Hallo!", s);
        }

        [Fact]
        public void FixInvalidItalicTags2()
        {
            const string s1 = "Gledaj prema kameri i rici <i>zdravo!";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("Gledaj prema kameri i rici <i>zdravo!</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicTags3()
        {
            string s1 = "<i>Line 1.</i>" + Environment.NewLine + "<i>Line 2.";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, "<i>Line 1." + Environment.NewLine + "Line 2.</i>");
        }

        [Fact]
        public void FixInvalidItalicTags4()
        {
            string s1 = "It <i>is</i> a telegram," + Environment.NewLine + "it <i>is</i> ordering an advance,";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, s1);
        }

        [Fact]
        public void FixInvalidItalicTags5()
        {
            string s1 = "- <i>It is a telegram?</i>" + Environment.NewLine + "<i>- It is.</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, "<i>- It is a telegram?" + Environment.NewLine + "- It is.</i>");
        }

        [Fact]
        public void FixInvalidItalicTags6()
        {
            string s1 = "- <i>Text1!</i>" + Environment.NewLine + "- <i>Text2.</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, "<i>- Text1!" + Environment.NewLine + "- Text2.</i>");
        }

        [Fact]
        public void FixInvalidItalicTags7()
        {
            string s1 = "<i>- You think they're they gone?<i>" + Environment.NewLine + "<i>- That can't be.</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, "<i>- You think they're they gone?" + Environment.NewLine + "- That can't be.</i>");
        }

        [Fact]
        public void FixInvalidItalicTags8()
        {
            string s1 = "<i>- You think they're they gone?</i>" + Environment.NewLine + "<i>- That can't be.<i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, "<i>- You think they're they gone?" + Environment.NewLine + "- That can't be.</i>");
        }

        [Fact]
        public void FixInvalidItalicTags9()
        {
            const string s1 = "FALCONE:<i> I didn't think</i>\r\n<i>it was going to be you,</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("FALCONE: <i>I didn't think\r\nit was going to be you,</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicTags10()
        {
            const string s1 = "< I>Hallo!</I>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("<i>Hallo!</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicTags11()
        {
            const string s1 = "< I >Hallo!< /I>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("<i>Hallo!</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicTags12()
        {
            const string s1 = "< I >Hallo!<I/>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("<i>Hallo!</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicTags13()
        {
            var s1 = "<i>Hallo!</i>" + Environment.NewLine + "<i>Hallo!</i>" + Environment.NewLine + "<i>Hallo!</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, s1);
        }

        [Fact]
        public void FixInvalidItalicTags14()
        {
            var s1 = "<i>Hallo!<i/>" + Environment.NewLine + "<i>Hallo!<i/>" + Environment.NewLine + "<i>Hallo!";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, "<i>Hallo!" + Environment.NewLine + "Hallo!" + Environment.NewLine + "Hallo!</i>");
        }

        [Fact]
        public void FixInvalidItalicTags15()
        {
            var s1 = "Foo<b><i>bar</b>";
            Assert.Equal("Foo<b><i>bar</i></b>", HtmlUtil.FixInvalidItalicTags(s1));
        }

        [Fact]
        public void FixInvalidItalicTags16()
        {
            var s1 = "Foo <i>bar";
            Assert.Equal("Foo <i>bar</i>", HtmlUtil.FixInvalidItalicTags(s1));
        }

        [Fact]
        public void FixInvalidItalicTags17()
        {
            var s1 = "Foobar<i>";
            Assert.Equal("Foobar", HtmlUtil.FixInvalidItalicTags(s1));
        }

        [Fact]
        public void FixInvalidItalicTags18()
        {
            var s1 = "<u><b><i>Foobar</b></u>";
            Assert.Equal("<u><b><i>Foobar</i></b></u>", HtmlUtil.FixInvalidItalicTags(s1));
        }

        [Fact]
        public void FixInvalidItalicTags19()
        {
            var s1 = "<i>Foobar";
            Assert.Equal("<i>Foobar</i>", HtmlUtil.FixInvalidItalicTags(s1));
        }

        [Fact]
        public void FixInvalidItalicTags20()
        {
            var s1 = "<i>Foobar: </i>" + Environment.NewLine + "<i>Line 2.</i>";
            var result = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("<i>Foobar:" + Environment.NewLine + "Line 2.</i>", result);
        }

        [Fact]
        public void FixInvalidItalicTagsWithAssTag()
        {
            var s1 = "{\\an8}<i>Hallo!<i/>" + Environment.NewLine + "<i>Hallo!<i/>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal(s2, "{\\an8}<i>Hallo!" + Environment.NewLine + "Hallo!</i>");
        }

        [Fact]
        public void FixInvalidItalicTagsBadStartTag()
        {
            var s1 = "<i<Hallo!<i/>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("<i>Hallo!</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicTagsMissingEndStartWithColon()
        {
            var s1 = "ADULT MARK: <i>New friends";
            var s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("ADULT MARK: <i>New friends</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicColonBracketItalic()
        {
            var s1 = "[König:]<i> Ich weiß, dass du dagegen</i>" + Environment.NewLine + "<i>bist.</i>";
            var s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("[König:] <i>Ich weiß, dass du dagegen</i>" + Environment.NewLine + "<i>bist.</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicAllEndTags()
        {
            var s1 = "</i>Line1.</i>" + Environment.NewLine + "</i>Line2.</i>";
            var s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("<i>Line1.</i>" + Environment.NewLine + "<i>Line2.</i>", s2);
        }

        [Fact]
        public void FixInvalidItalicAllStartTags()
        {
            var s1 = "<i>Line1.<i>" + Environment.NewLine + "<i>Line2.<i>";
            var s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.Equal("<i>Line1.</i>" + Environment.NewLine + "<i>Line2.</i>", s2);
        }

        [Fact]
        public void FixUnneededSpacesDoubleSpace1()
        {
            const string s1 = "This is  a test";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal("This is a test", s2);
        }

        [Fact]
        public void FixUnneededSpacesDoubleSpace2()
        {
            const string s1 = "This is a test  ";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal("This is a test", s2);
        }

        [Fact]
        public void FixUnneededSpacesItalics1()
        {
            const string s1 = "<i> This is a test</i>";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal("<i>This is a test</i>", s2);
        }

        [Fact]
        public void FixUnneededSpacesItalics2()
        {
            const string s1 = "<i>This is a test </i>";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal("<i>This is a test</i>", s2);
        }

        [Fact]
        public void FixUnneededSpacesHyphen1()
        {
            const string s1 = "This is a low- budget job";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal("This is a low-budget job", s2);
        }

        [Fact]
        public void FixUnneededSpacesHyphen2()
        {
            const string s1 = "This is a low- budget job";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal("This is a low-budget job", s2);
        }

        [Fact]
        public void FixUnneededSpacesHyphenDoNotChange1()
        {
            const string s1 = "This is it - and he likes it!";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void FixUnneededSpacesHyphenDoNotChange2()
        {
            const string s1 = "What are your long- and altitude stats?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void FixUnneededSpacesHyphenDoNotChange3()
        {
            const string s1 = "Did you buy that first- or second-handed?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void FixUnneededSpacesHyphenDoNotChangeDutch1()
        {
            const string s1 = "Wat zijn je voor- en familienaam?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "nl");
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void FixUnneededSpacesHyphenDoNotChangeDutch2()
        {
            const string s1 = "Was het in het voor- of najaar?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "nl");
            Assert.Equal(s1, s2);
        }

        [Fact]
        
        public void FixUnneededSpacesDialogDotDotDotLine1()
        {
            string s = Utilities.RemoveUnneededSpaces("- ... Careful", "en");
            Assert.Equal("- ...Careful", s);
        }

        [Fact]
        
        public void FixUnneededSpacesDialogDotDotDotLine2()
        {
            string s = Utilities.RemoveUnneededSpaces("- Hi!" + Environment.NewLine + "- ... Careful", "en");
            Assert.Equal(s, "- Hi!" + Environment.NewLine + "- ...Careful");
        }

        [Fact]
        public void FixUnneededSpacesFontTag1()
        {
            string s = Utilities.RemoveUnneededSpaces("<font color=\"#808080\"> (PEOPLE SPEAKING INDISTINCTLY) </font>", "en");
            Assert.Equal("<font color=\"#808080\">(PEOPLE SPEAKING INDISTINCTLY)</font>", s);
        }

        [Fact]
        public void FixUnneededSpacesFontTag2()
        {
            string s = Utilities.RemoveUnneededSpaces("Foobar\r\n<font color=\"#808080\"> (PEOPLE SPEAKING INDISTINCTLY) </font>", "en");
            Assert.Equal("Foobar\r\n<font color=\"#808080\">(PEOPLE SPEAKING INDISTINCTLY)</font>", s);
        }

        [Fact]
        public void FixUnneededSpacesFontTag3()
        {
            string s = Utilities.RemoveUnneededSpaces("<FONT COLOR=\"#808080\">- Foobar! </FONT>\r\n<font color=\"#808080\"> (PEOPLE SPEAKING INDISTINCTLY) </font>", "en");
            Assert.Equal("<font color=\"#808080\">- Foobar!</font>\r\n<font color=\"#808080\">(PEOPLE SPEAKING INDISTINCTLY)</font>", s);
        }

        [Fact]
        public void RemoveUnneededSpacesAfterQuote()
        {
            const string lang = "en";

            // variant 1
            string s = Utilities.RemoveUnneededSpaces("\" In five years the Corleone family\r\nwill be completely legitimate.\"", lang);
            Assert.Equal("\"In five years the Corleone family\r\nwill be completely legitimate.\"", s);

            // variant 2
            s = Utilities.RemoveUnneededSpaces("Foobar? \" Foobar\".", lang);
            Assert.Equal("Foobar? \"Foobar\".", s);
        }

        [Fact]
        public void RemoveUnneededSpacesBetweenNumbers1()
        {
            string s = Utilities.RemoveUnneededSpaces("Yes, it is at 8: 40 today.", "en");
            Assert.Equal("Yes, it is at 8:40 today.", s);
        }

        [Fact]
        public void RemoveUnneededSpacesBetweenNumbers1NoChange()
        {
            string s = Utilities.RemoveUnneededSpaces("Yes, it is at: 40 today.", "en");
            Assert.Equal("Yes, it is at: 40 today.", s);
        }

        [Fact]
        public void RemoveUnneededSpacesBetweenNumbers2()
        {
            string s = Utilities.RemoveUnneededSpaces("The time is 8. 40.", "en");
            Assert.Equal("The time is 8.40.", s);
        }

        [Fact]
        public void RemoveUnneededSpacesBetweenNumbersDates()
        {
            string s = Utilities.RemoveUnneededSpaces("The 4 th and 1 st 3 rd and 2 nd.", "en");
            Assert.Equal("The 4th and 1st 3rd and 2nd.", s);
        }

        [Fact]
        public void RemoveUnneededSpacesDutchAposS()
        {
            var s = Utilities.RemoveUnneededSpaces("Nou 's avonds?", "en");
            Assert.Equal("Nou's avonds?", s);

            s = Utilities.RemoveUnneededSpaces("Nou 's avonds?", "nl");
            Assert.Equal("Nou 's avonds?", s);
        }

        [Fact]
        public void CountTagInTextStringOneLetterString()
        {
            int count = Utilities.CountTagInText("HHH", "H");
            Assert.Equal(3, count);
        }

        [Fact]
        public void CountTagInTextStringNotThere()
        {
            int count = Utilities.CountTagInText("HHH", "B");
            Assert.Equal(0, count);
        }

        [Fact]
        public void CountTagInTextCharNormal()
        {
            int count = Utilities.CountTagInText("HHH", 'H');
            Assert.Equal(3, count);
        }

        [Fact]
        public void CountTagInTextCharNotThere()
        {
            int count = Utilities.CountTagInText("HHH", 'B');
            Assert.Equal(0, count);
        }

        [Fact]
        public void FixHyphensAddTest()
        {
            string test1 = "<font color=\"#008080\">- Foobar." + Environment.NewLine + "Foobar.</font>";
            string expected1 = "<font color=\"#008080\">- Foobar." + Environment.NewLine + "- Foobar.</font>";

            string test2 = "<i>Foobar.</i>" + Environment.NewLine + "- Foobar.";
            var expected2 = "<i>- Foobar.</i>" + Environment.NewLine + "- Foobar.";

            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph(test1, 0000, 11111));
            sub.Paragraphs.Add(new Paragraph(test2, 0000, 11111));

            string output1 = Helper.FixHyphensAdd(sub, 0, "en");
            string output2 = Helper.FixHyphensAdd(sub, 1, "en");

            Assert.Equal(output1, expected1);
            Assert.Equal(output2, expected2);
        }

        [Fact]
        public void FixHyphensAddTestAssTag()
        {
            string test1 = "{\\an5}- At least I was going back to Hawaii." + Environment.NewLine + "Woody.";
            string expected1 = "{\\an5}- At least I was going back to Hawaii." + Environment.NewLine + "- Woody.";
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph(test1, 0000, 11111));

            string output1 = Helper.FixHyphensAdd(sub, 0, "en");

            Assert.Equal(output1, expected1);
        }

        [Fact]
        public void FixHyphensAddTestItalic()
        {
            string test1 = "<i>- At least I was going back to Hawaii.</i>" + Environment.NewLine + "<i>Woody.</i>";
            string expected1 = "<i>- At least I was going back to Hawaii.</i>" + Environment.NewLine + "<i>- Woody.</i>";
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph(test1, 0000, 11111));

            string output1 = Helper.FixHyphensAdd(sub, 0, "en");

            Assert.Equal(output1, expected1);
        }

        [Fact]
        public void RemoveLineBreaks1()
        {
            string result = Utilities.RemoveLineBreaks("Hey" + Environment.NewLine + "you!");
            Assert.Equal("Hey you!", result);
        }

        [Fact]
        public void RemoveLineBreaks2()
        {
            string result = Utilities.RemoveLineBreaks("<i>Foobar " + Environment.NewLine + "</i> foobar.");
            Assert.Equal("<i>Foobar</i> foobar.", result);
        }

        [Fact]
        public void RemoveLineBreaks3()
        {
            string result = Utilities.RemoveLineBreaks("<i>Foobar " + Environment.NewLine + "</i>foobar.");
            Assert.Equal("<i>Foobar</i> foobar.", result);
        }

        [Fact]
        public void RemoveLineBreaks4()
        {
            string result = Utilities.RemoveLineBreaks("<i>Hey</i>" + Environment.NewLine + "<i>you!</i>");
            Assert.Equal("<i>Hey you!</i>", result);
        }

        [Fact]
        public void RemoveLineBreaks5()
        {
            string result = Utilities.RemoveLineBreaks("<i>Foobar" + Environment.NewLine + "</i>");
            Assert.Equal("<i>Foobar</i>", result);
        }

        [Fact]
        public void IsValidRegexOk1()
        {
            Assert.True(RegexUtils.IsValidRegex(@"^(?![\s\S])"));
        }

        [Fact]
        public void IsValidRegexOk2()
        {
            Assert.True(RegexUtils.IsValidRegex(@"\d+"));
        }

        [Fact]
        public void IsValidRegexBad1()
        {
            Assert.False(RegexUtils.IsValidRegex(@"[\s\S(\()()(()\)"));
        }

        [Fact]
        public void ReverseNumbers1()
        {
            Assert.Equal("Hallo 900", Utilities.ReverseNumbers("Hallo 009"));
        }

        [Fact]
        public void ReverseNumbers2()
        {
            Assert.Equal("Hallo 900 100 Bye", Utilities.ReverseNumbers("Hallo 009 001 Bye"));
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft1()
        {
            Assert.Equal(".I have a big head-", Utilities.ReverseStartAndEndingForRightToLeft("-I have a big head."));
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft2()
        {
            Assert.Equal("?So do I~", Utilities.ReverseStartAndEndingForRightToLeft("~So do I?"));
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft3()
        {
            Assert.Equal("!I do too+", Utilities.ReverseStartAndEndingForRightToLeft("+I do too!"));
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft4()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("(Mom)" + Environment.NewLine + "What are you doing here?");
            Assert.Equal("(Mom)" + Environment.NewLine + "?What are you doing here", result);
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft5()
        {
            Assert.Equal("{\\an8}!I do too+", Utilities.ReverseStartAndEndingForRightToLeft("{\\an8}+I do too!"));
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft6()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("-I have a big head." + Environment.NewLine + "~So do I?" + Environment.NewLine + "+I do too!");
            Assert.Equal(".I have a big head-" + Environment.NewLine + "?So do I~" + Environment.NewLine + "!I do too+", result);
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft7HtmlTags()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("<i>-I have a big head.</i>" + Environment.NewLine + "<font color='red'>~So do I?</font>" + Environment.NewLine + "+I do too!");
            Assert.Equal("<i>.I have a big head-</i>" + Environment.NewLine + "<font color='red'>?So do I~</font>" + Environment.NewLine + "!I do too+", result);
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft8BoldTag()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("<b>-I have a big head.</b>" + Environment.NewLine + "<font color='red'>~So do I?</font>" + Environment.NewLine + "+I do too!");
            Assert.Equal("<b>.I have a big head-</b>" + Environment.NewLine + "<font color='red'>?So do I~</font>" + Environment.NewLine + "!I do too+", result);
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeft9Alignment()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("{\an8}Hello" + Environment.NewLine + "Hi.");
            Assert.Equal("{\an8}Hello" + Environment.NewLine + ".Hi", result);
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeftWithQuotes()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("\"<font color=\"#000000\">مرحباً</font>\"");
            Assert.Equal("\"<font color=\"#000000\">مرحباً</font>\"", result);
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeftQuotes2()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("\"Hey." + Environment.NewLine + "Hey.\"");
            Assert.Equal(".Hey\"" + Environment.NewLine + "\".Hey", result);
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeftMusicSymbols()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("♪ Hey... ♪");
            Assert.Equal("♪ ...Hey ♪", result);
        }

        [Fact]
        public void ReverseStartAndEndingForRightToLeftMusicSymbols2()
        {
            var result = Utilities.ReverseStartAndEndingForRightToLeft("♫Hey...♫");
            Assert.Equal("♫...Hey♫", result);
        }

        [Fact]
        public void GetPathAndFileNameWithoutExtension1()
        {
            var result = Utilities.GetPathAndFileNameWithoutExtension("file.srt");
            Assert.Equal("file", result);
        }

        [Fact]
        public void GetPathAndFileNameWithoutExtension2()
        {
            var result = Utilities.GetPathAndFileNameWithoutExtension("how.are.you[eng].srt");
            Assert.Equal("how.are.you[eng]", result);
        }

        [Fact]
        public void GetPathAndFileNameWithoutExtension3()
        {
            var result = Utilities.GetPathAndFileNameWithoutExtension("C:" + Path.DirectorySeparatorChar + "my.files" + Path.DirectorySeparatorChar + "file1");
            Assert.Equal("C:" + Path.DirectorySeparatorChar + "my.files" + Path.DirectorySeparatorChar + "file1", result);
        }

        [Fact]
        public void GetPathAndFileNameWithoutExtension4()
        {
            var result = Utilities.GetPathAndFileNameWithoutExtension("C:" + Path.DirectorySeparatorChar + "my.files" + Path.DirectorySeparatorChar + "file1.srt");
            Assert.Equal("C:" + Path.DirectorySeparatorChar + "my.files" + Path.DirectorySeparatorChar + "file1", result);
        }

        [Fact]
        public void RemoveSsaTags1()
        {
            var result = Utilities.RemoveSsaTags("{\\i1}Hallo world!{\\i0}");
            Assert.Equal("Hallo world!", result);
        }

        [Fact]
        public void RemoveSsaTags2()
        {
            var result = Utilities.RemoveSsaTags("{\\i1}Hallo world!");
            Assert.Equal("Hallo world!", result);
        }

        [Fact]
        public void RemoveSsaTags3()
        {
            var result = Utilities.RemoveSsaTags("Hallo {\\i1}my{\\i0} world!");
            Assert.Equal("Hallo my world!", result);
        }

        [Fact]
        public void RemoveSsaTags4()
        {
            var result = Utilities.RemoveSsaTags("{\\p2}m 0 0 l 1 1{\\p0}Hallo world!", true);
            Assert.Equal("Hallo world!", result);
        }

        [Fact]
        public void UrlEncode()
        {
            var result = Utilities.UrlEncode("{\\fs50}Yo{\\Reset}Yo");
            Assert.Equal("%7B%5Cfs50%7DYo%7B%5CReset%7DYo", result);
        }

        [Fact]
        public void UrlEncodeLength()
        {
            var text = @"{\rSubtitle-_2}Blaf,{\RESET}.!? ";
            var result = Utilities.UrlEncode(text);
            var resultLength = Utilities.UrlEncodeLength(text);
            Assert.Equal(resultLength, result.Length);
        }

        [Fact]
        public void GetColorFromString1()
        {
            var c = HtmlUtil.GetColorFromString("#010203ff");

            Assert.Equal(byte.MaxValue, c.Alpha);
            Assert.Equal(1, c.Red);
            Assert.Equal(2, c.Green);
            Assert.Equal(3, c.Blue);
        }

        [Fact]
        public void GetColorFromString2()
        {
            var c = HtmlUtil.GetColorFromString("rgb(1,2,3)");

            Assert.Equal(byte.MaxValue, c.Alpha);
            Assert.Equal(1, c.Red);
            Assert.Equal(2, c.Green);
            Assert.Equal(3, c.Blue);
        }

        [Fact]
        public void GetColorFromString3()
        {
            var c = HtmlUtil.GetColorFromString("rgba(1,2,3, 1)");

            Assert.Equal(byte.MaxValue, c.Alpha);
            Assert.Equal(1, c.Red);
            Assert.Equal(2, c.Green);
            Assert.Equal(3, c.Blue);
        }

        [Fact]
        public void UrlDecode1()
        {
            var s = Utilities.UrlDecode("В о\u0442е\u043bе");

            Assert.Equal("В отеле", s);
        }
    }
}
