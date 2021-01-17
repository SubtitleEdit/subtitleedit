using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;

namespace Test.Logic
{
    [TestClass]
    public class ContinuationUtilitiesTest
    {
        [TestMethod]
        public void SanitizeString1()
        {
            string line1 = @"{\an8}<i>'This is a test.'</i>" + Environment.NewLine + " " + Environment.NewLine + " _";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "This is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString2()
        {
            string line1 = "<font color=\"#000000\"><i>Just testin'</i></font>";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "Just testin'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString2B()
        {
            string line1 = "<font color=\"#000000\"><i>But this is an ending quote.'</i></font>";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "But this is an ending quote.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString3()
        {
            string line1 = "'s Avonds gaat de zon onder.'";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "'s Avonds gaat de zon onder.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString4()
        {
            string line1 = "MAN IN BACKGROUND: this is a test: like this.";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "this is a test: like this.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString4B()
        {
            string line1 = "Unit tests: used to test code.";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "Unit tests: used to test code.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString5()
        {
            string line1 = ">> ...this is a test.";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "...this is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString6()
        {
            string line1 = "for example: ...this is a test.";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "for example: ...this is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString7()
        {
            string line1 = "- ...this is a test." + Environment.NewLine + "<i>- Another test...</i>";
            string line1Actual = ContinuationUtilities.SanitizeString(line1);
            string line1Expected = "...this is a test." + Environment.NewLine + "- Another test...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString8()
        {
            string line1 = "- this is a test." + Environment.NewLine + "<i>- Another test -</i>";
            string line1Actual = ContinuationUtilities.SanitizeString(line1, false);
            string line1Expected = "- this is a test." + Environment.NewLine + "- Another test -";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString9()
        {
            string line1 = "";
            string line1Actual = ContinuationUtilities.SanitizeString(line1, false);
            string line1Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString10()
        {
            string line1 = "<i></i>";
            string line1Actual = ContinuationUtilities.SanitizeString(line1, false);
            string line1Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString11()
        {
            string line1 = "<i>♪ la la la ♪</i>";
            Configuration.Settings.General.FixContinuationStyleIgnoreLyrics = false;
            string line1Actual = ContinuationUtilities.SanitizeString(line1, false);
            string line1Expected = "la la la";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString12()
        {
            string line1 = "<i>end of the song ♪</i>";
            string line1Actual = ContinuationUtilities.SanitizeString(line1, false);
            string line1Expected = "end of the song";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString13()
        {
            string line1 = "(TEST)";
            string line1Actual = ContinuationUtilities.SanitizeString(line1, false);
            string line1Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void SanitizeString14()
        {
            string line1 = "(TEST " + Environment.NewLine + " TEST)";
            string line1Actual = ContinuationUtilities.SanitizeString(line1, false);
            string line1Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void ExtractParagraphOnly1()
        {
            string line1 = @"{\an8}<i>'This is a test.'</i>" + Environment.NewLine + " " + Environment.NewLine + " _";
            string line1Actual = ContinuationUtilities.ExtractParagraphOnly(line1);
            string line1Expected = "<i>'This is a test.'</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void ReplaceFirstOccurrence1()
        {
            string line1 = "Mark and Fred. Mark is the strongest.";
            string line1Actual = ContinuationUtilities.ReplaceFirstOccurrence(line1, "Mark", "...Mark");
            string line1Expected = "...Mark and Fred. Mark is the strongest.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void ReplaceFirstOccurrence2()
        {
            string line1 = "Mark and Fred. Mark is the strongest.";
            string line1Actual = ContinuationUtilities.ReplaceFirstOccurrence(line1, "John", "...John");
            string line1Expected = "Mark and Fred. Mark is the strongest.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void ReplaceLastOccurrence1()
        {
            string line1 = "You ask who's the strongest. Mark is the strongest";
            string line1Actual = ContinuationUtilities.ReplaceLastOccurrence(line1, "strongest", "strongest...");
            string line1Expected = "You ask who's the strongest. Mark is the strongest...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void ReplaceLastOccurrence2()
        {
            string line1 = "You ask who's the strongest. Mark is the strongest";
            string line1Actual = ContinuationUtilities.ReplaceLastOccurrence(line1, "tallest", "tallest...");
            string line1Expected = "You ask who's the strongest. Mark is the strongest";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void ShouldAddSuffix1()
        {
            string line1 = "This is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.ShouldAddSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void ShouldAddSuffix2()
        {
            string line1 = "This is a test:";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.ShouldAddSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void ShouldAddSuffix3()
        {
            string line1 = "<i>This is a test,</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.ShouldAddSuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void ShouldAddSuffix4()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.ShouldAddSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void ShouldAddSuffix4B()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.ShouldAddSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void ShouldAddSuffix5()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.ShouldAddSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void ShouldAddSuffix6()
        {
            string line1 = "<i>...</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.ShouldAddSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded1()
        {
            string line1 = "This is a test,";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a test...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded2()
        {
            string line1 = "This is a test,";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.NoneLeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a test,";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded3()
        {
            string line1 = "This is a test,";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.NoneLeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, true);
            string line1Expected = "This is a test...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded4()
        {
            string line1 = @"{\an8}<i>This is a test,</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = @"{\an8}<i>This is a test...</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded5()
        {
            string line1 = "<i>- What is this?</i>" + Environment.NewLine + "<i>- This is a test,</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "<i>- What is this?</i>" + Environment.NewLine + "<i>- This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded6()
        {
            string line1 = "<i>- What is this?" + Environment.NewLine + "- This is a test,</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "<i>- What is this?" + Environment.NewLine + "- This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded7()
        {
            string line1 = "<i>- What are you doing?" + Environment.NewLine + "- I'm just testin'</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "<i>- What are you doing?" + Environment.NewLine + "- I'm just testin'...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded8()
        {
            string line1 = "- What is this?" + Environment.NewLine + "- This is a <b>test</b>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "- What is this?" + Environment.NewLine + "- This is a <b>test</b>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded8B()
        {
            string line1 = "This is a <i>test</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a <i>test</i>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded8C()
        {
            string line1 = "<i>This is a test</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "<i>This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded8F()
        {
            string line1 = "We just vibin' here you know";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "We just vibin' here you know...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded8G()
        {
            string line1 = "'We just vibin' here you know";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "'We just vibin' here you know...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded8H()
        {
            string line1 = "'We just vibin' here you know'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "'We just vibin' here you know...'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded8I()
        {
            string line1 = "test in'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "test in...'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded8J()
        {
            string line1 = "in'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "in...'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded9()
        {
            string line1 = "- Hello." + Environment.NewLine + "- This is a <i>test</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "- Hello." + Environment.NewLine + "- This is a <i>test</i>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded9B()
        {
            string line1 = "- Hello." + Environment.NewLine + "- <i>This is a test</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "- Hello." + Environment.NewLine + "- <i>This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded9C()
        {
            string line1 = "<i>- Hello." + Environment.NewLine + "- This is a test</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "<i>- Hello." + Environment.NewLine + "- This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded10()
        {
            string line1 = "This is a <i>test</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a <i>test</i>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded10B()
        {
            string line1 = "This is a <i>test</i>,";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a <i>test</i>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded10C()
        {
            string line1 = "This is a <i>test</i>...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a <i>test</i>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded10D()
        {
            string line1 = "<i>This is a test,</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "<i>This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded10E()
        {
            string line1 = "<i>This is a test...</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "<i>This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        /*[TestMethod]
        public void AddSuffixIfNeeded10F()
        {
            string line1 = "This is a <i>test,</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a <i>test</i>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded10G()
        {
            string line1 = "This is a <i>test...</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a <i>test</i>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }*/

        [TestMethod]
        public void AddSuffixIfNeeded11()
        {
            string line1 = "- Hello." + Environment.NewLine + "- This is a <i>test</i>,";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "- Hello." + Environment.NewLine + "- This is a <i>test</i>...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded11B()
        {
            string line1 = "- Hello." + Environment.NewLine + "- <i>This is a test,</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "- Hello." + Environment.NewLine + "- <i>This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded11C()
        {
            string line1 = "<i>- Hello." + Environment.NewLine + "- This is a test,</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "<i>- Hello." + Environment.NewLine + "- This is a test...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded12()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded12B()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = " ";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded13()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded14()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded15()
        {
            string line1 = "妈";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "妈...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded16()
        {
            string line1 = "...test";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "...test...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded17()
        {
            string line1 = "...test,";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "...test...";
            Assert.AreEqual(line1Expected, line1Actual);
        }
        
        [TestMethod]
        public void AddSuffixIfNeeded18()
        {
            string line1 = "This is a 'test'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a 'test'...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded18B()
        {
            string line1 = "This is a 'test'...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a 'test'...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded18C()
        {
            string line1 = "This is a 'test',";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a 'test'...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        /*[TestMethod]
        public void AddSuffixIfNeeded18D()
        {
            string line1 = "This is a 'test...'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a 'test'...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded18E()
        {
            string line1 = "This is a 'test,'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "This is a 'test'...";
            Assert.AreEqual(line1Expected, line1Actual);
        }*/

        [TestMethod]
        public void AddSuffixIfNeeded19()
        {
            string line1 = "'This is a test'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "'This is a test...'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded19B()
        {
            string line1 = "'This is a test...'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "'This is a test...'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddSuffixIfNeeded19C()
        {
            string line1 = "'This is a test,'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "'This is a test...'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        /*[TestMethod]
        public void AddSuffixIfNeeded13()
        {
            string line1 = "What are you <i>do</i>ing,";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddSuffixIfNeeded(line1, profile, false);
            string line1Expected = "What are you <i>do</i>ing...";
            Assert.AreEqual(line1Expected, line1Actual);
        }*/

        [TestMethod]
        public void AddPrefixIfNeeded1()
        {
            string line1 = "this is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, false);
            string line1Expected = "...this is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded2()
        {
            string line1 = "this is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.NoneLeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, false);
            string line1Expected = "this is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded3()
        {
            string line1 = "this is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.NoneLeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "...this is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded4()
        {
            string line1 = @"{\an8}<i>this is a test.</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = @"{\an8}<i>...this is a test.</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded5()
        {
            string line1 = "<i>- this is a test." + Environment.NewLine + "-A what do you say?";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "<i>- ...this is a test." + Environment.NewLine + "-A what do you say?";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded5B()
        {
            string line1 = "<i>- this is a test." + Environment.NewLine + "-A what do you say?";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "<i>- this is a test." + Environment.NewLine + "-A what do you say?";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded6()
        {
            string line1 = "- <b>this</b> is a test." + Environment.NewLine + "-A what do you say?";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "- ...<b>this</b> is a test." + Environment.NewLine + "-A what do you say?";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded7()
        {
            string line1 = "- <b>this</b> is a test." + Environment.NewLine + "-A what do you say?";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "- <b>this</b> is a test." + Environment.NewLine + "-A what do you say?";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded8()
        {
            string line1 = "<i>this is a test.</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "<i>...this is a test.</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded8B()
        {
            string line1 = "<i>this</i> is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "...<i>this</i> is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded9()
        {
            string line1 = "- <i>this is a test.</i>" + Environment.NewLine + "- Okay.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "- <i>...this is a test.</i>" + Environment.NewLine + "- Okay.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded9B()
        {
            string line1 = "- <i>this</i> is a test." + Environment.NewLine + "- Okay.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "- ...<i>this</i> is a test." + Environment.NewLine + "- Okay.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded10()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded10B()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = " ";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded10C()
        {
            string line1 = "象";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "...象";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded11()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded12()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded13()
        {
            string line1 = "<i>...</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "<i>...</i>";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded14()
        {
            string line1 = "test...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "...test...";
            Assert.AreEqual(line1Expected, line1Actual);
        }
        
        [TestMethod]
        public void AddPrefixIfNeeded15()
        {
            string line1 = "'this' is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, false);
            string line1Expected = "...'this' is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded15B()
        {
            string line1 = "...'this' is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, false);
            string line1Expected = "...'this' is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        /*[TestMethod]
        public void AddPrefixIfNeeded15D()
        {
            string line1 = "'...this' is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, false);
            string line1Expected = "...'this' is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }*/

        [TestMethod]
        public void AddPrefixIfNeeded16()
        {
            string line1 = "'this is a test'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, false);
            string line1Expected = "'...this is a test'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void AddPrefixIfNeeded16B()
        {
            string line1 = "'...this is a test'";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, false);
            string line1Expected = "'...this is a test'";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        /*[TestMethod]
        public void AddPrefixIfNeeded8()
        {
            string line1 = "<i>do</i>ing is what I meant.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "...<i>do</i>ing is what I meant.";
            Assert.AreEqual(line1Expected, line1Actual);
        }*/

        /*[TestMethod]
        public void AddPrefixIfNeeded9()
        {
            string line1 = "MAN: <i>do</i>ing is what I meant.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.AddPrefixIfNeeded(line1, profile, true);
            string line1Expected = "MAN: ...<i>do</i>ing is what I meant.";
            Assert.AreEqual(line1Expected, line1Actual);
        }*/

        [TestMethod]
        public void RemoveSuffix1()
        {
            string line1 = "<i>This is a test,</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile, new List<string> { "," }, false);
            string line1Expected = "<i>This is a test</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemoveSuffix2()
        {
            string line1 = "<i>This is a test...</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile);
            string line1Expected = "<i>This is a test</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemoveSuffix2B()
        {
            string line1 = "This is a test ...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile);
            string line1Expected = "This is a test";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemoveSuffix3()
        {
            string line1 = "<i>This is a <b>test...</b></i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile);
            string line1Expected = "<i>This is a <b>test</b></i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemoveSuffix4()
        {
            string line1 = "<i>This is a <b>test</b>...</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile);
            string line1Expected = "<i>This is a <b>test</b></i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemoveSuffix5()
        {
            string line1 = "<i>This is a test -</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile);
            string line1Expected = "<i>This is a test</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemoveSuffix6()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile);
            string line1Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemoveSuffix6B()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile);
            string line1Expected = " ";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemoveSuffix7()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemoveSuffix(line1, profile);
            string line1Expected = "...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix1()
        {
            string line1 = "<i>...this is a test.</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "<i>this is a test.</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix1B()
        {
            string line1 = "... this is a test.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "this is a test.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix2()
        {
            string line1 = "<i>...<b>this</b> is a test.</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "<i><b>this</b> is a test.</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix3()
        {
            string line1 = "<i><b>...this</b> is a test.</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "<i><b>this</b> is a test.</i>" + Environment.NewLine + " " + Environment.NewLine + "_";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix4()
        {
            string line1 = "- and this is the end.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "and this is the end.";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix5()
        {
            string line1 = "- and this is the end." + Environment.NewLine + "- Really?";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "- and this is the end." + Environment.NewLine + "- Really?";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix6()
        {
            string line1 = "- ...and this is the end." + Environment.NewLine + "- Really?";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "- and this is the end." + Environment.NewLine + "- Really?";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix7()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix7B()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = " ";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void RemovePrefix8()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            string line1Actual = ContinuationUtilities.RemovePrefix(line1, profile);
            string line1Expected = "...";
            Assert.AreEqual(line1Expected, line1Actual);
        }

        [TestMethod]
        public void IsNewSentence1()
        {
            string line1 = "This is a new sentence.";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsNewSentence2()
        {
            string line1 = "but this is not a new sentence.";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsNewSentence3()
        {
            string line1 = "iPhone is the market leader.";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsNewSentence4()
        {
            string line1 = "'s Avonds gaat de zon onder.";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsNewSentence5()
        {
            string line1 = "'s avonds gaat de zon onder.";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsNewSentence6()
        {
            string line1 = "¿Habla Español?";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsNewSentence7()
        {
            string line1 = "¿habla Español?";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsNewSentence8()
        {
            string line1 = "";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsNewSentence9()
        {
            string line1 = " ";
            bool line1Actual = ContinuationUtilities.IsNewSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence1()
        {
            string line1 = "This is the end of a sentence.";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence2()
        {
            string line1 = "this is the end of a sentence.";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence3()
        {
            string line1 = "is the end?";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence4()
        {
            string line1 = "This is the end!";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence5()
        {
            string line1 = "This is not the end:";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence6()
        {
            string line1 = "This is not the end...";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence7()
        {
            string line1 = "This is not the end,";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence8()
        {
            string line1 = "This is--";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence9()
        {
            string line1 = "This is not the end;";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence10()
        {
            string line1 = "Ψάξατε πραγματικά αυτό;";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence11()
        {
            string line1 = "";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence11B()
        {
            string line1 = " ";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence12()
        {
            string line1 = "Hey...";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsEndOfSentence13()
        {
            string line1 = "...";
            bool line1Actual = ContinuationUtilities.IsEndOfSentence(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing1()
        {
            string line1 = "This ends with nothing";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing2()
        {
            string line1 = "THIS ENDS WITH NOTHING";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing3()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing3B()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing4()
        {
            string line1 = "This ends with something,";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing5()
        {
            string line1 = "This ends with something..";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing6()
        {
            string line1 = "This ends with something--";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing7()
        {
            string line1 = "This ends with something --";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing8()
        {
            string line1 = "This ends with something:";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing9()
        {
            string line1 = "This ends with something;";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing9B()
        {
            string line1 = "Ψάξατε πραγματικά αυτό;";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing10()
        {
            string line1 = "This ends with something?";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing11()
        {
            string line1 = "This ends with something...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void EndsWithNothing12()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.EndsWithNothing(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsAllCaps1()
        {
            string line1 = "THIS IS ALL CAPS.";
            bool line1Actual = ContinuationUtilities.IsAllCaps(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsAllCaps2()
        {
            string line1 = "This is not. NO, it's NOT! SHUT UP!";
            bool line1Actual = ContinuationUtilities.IsAllCaps(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsAllCaps3()
        {
            string line1 = "THE NEW iPHONE";
            bool line1Actual = ContinuationUtilities.IsAllCaps(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsAllCaps4()
        {
            string line1 = "ИГРА В ПРЯТКИ";
            bool line1Actual = ContinuationUtilities.IsAllCaps(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsAllCaps5()
        {
            string line1 = "";
            bool line1Actual = ContinuationUtilities.IsAllCaps(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsAllCaps5B()
        {
            string line1 = " ";
            bool line1Actual = ContinuationUtilities.IsAllCaps(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsItalic1()
        {
            string line1 = "<i>This is italic.</i>";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsItalic2()
        {
            string line1 = "<i>This</i> is italic.";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsItalic3()
        {
            string line1 = "<i>This is italic." + Environment.NewLine + "- Really?</i> Just stop it.";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsItalic4()
        {
            string line1 = "<i>This is italic." + Environment.NewLine + "This is, too.</i>";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsItalic5()
        {
            string line1 = "<i>This is italic.</i>" + Environment.NewLine + "<i>This is, too.</i>";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsItalic6()
        {
            string line1 = "<i>- This is italic.</i>" + Environment.NewLine + "<i>- This is, too.</i>";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsItalic7()
        {
            string line1 = "<i>This is italic.</i> <i>This too.</i>";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsItalic8()
        {
            string line1 = "<i>This is italic." + Environment.NewLine + "This too.";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsItalic9()
        {
            string line1 = "<i>This is italic." + Environment.NewLine + "<i>This too.";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsItalic10()
        {
            string line1 = "";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsItalic10B()
        {
            string line1 = " ";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsItalic11()
        {
            string line1 = "<i>";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsItalic12()
        {
            string line1 = "<i></i>";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsItalic13()
        {
            string line1 = "<i> </i>";
            bool line1Actual = ContinuationUtilities.IsItalic(line1);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasPrefix1()
        {
            string line1 = "...this is a prefix.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasPrefix2()
        {
            string line1 = "Not here.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasPrefix3()
        {
            string line1 = "~~ this is my own prefix.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasPrefix4()
        {
            string line1 = "~~ this is my own prefix.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            profile.Prefix = "~~";
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasPrefix5()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasPrefix5B()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasPrefix6()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasPrefix7()
        {
            string line1 = "<i>...</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasPrefix8()
        {
            string line1 = "...a";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasPrefix8B()
        {
            string line1 = "... a";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasPrefix9()
        {
            string line1 = "-a";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasPrefix10()
        {
            string line1 = "- a";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            bool line1Actual = ContinuationUtilities.HasPrefix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasSuffix1()
        {
            string line1 = "This is a suffix...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasSuffix2()
        {
            string line1 = "Not here.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasSuffix3()
        {
            string line1 = "This is my own suffix ~~";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasSuffix4()
        {
            string line1 = "This is my own suffix ~~";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            profile.Suffix = "~~";
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasSuffix5()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasSuffix5B()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasSuffix6()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasSuffix6B()
        {
            string line1 = " ...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void HasSuffix6C()
        {
            string line1 = "象...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasSuffix7()
        {
            string line1 = "a...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasSuffix7B()
        {
            string line1 = "a ...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasSuffix7C()
        {
            string line1 = "a-";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasSuffix7D()
        {
            string line1 = "a -";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void HasSuffix8()
        {
            string line1 = "<i>...</i>";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.HasSuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix1()
        {
            string line1 = "...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix2()
        {
            string line1 = "-";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix3()
        {
            string line1 = "a...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix4()
        {
            string line1 = " ... ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix5()
        {
            string line1 = "!...";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix6()
        {
            string line1 = "?";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix7()
        {
            string line1 = "象";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix8()
        {
            string line1 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void IsOnlySuffix9()
        {
            string line1 = " ";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            bool line1Actual = ContinuationUtilities.IsOnlySuffix(line1, profile);
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void StartsWithConjunction1()
        {
            string line1 = "but is this reality?";
            bool line1Actual = ContinuationUtilities.StartsWithConjunction(line1, "en");
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void StartsWithConjunction2()
        {
            string line1 = "is a walk in the park.";
            bool line1Actual = ContinuationUtilities.StartsWithConjunction(line1, "en");
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void StartsWithConjunction3()
        {
            string line1 = "maar dat is ook de waarheid.";
            bool line1Actual = ContinuationUtilities.StartsWithConjunction(line1, "nl");
            Assert.IsTrue(line1Actual);
        }

        [TestMethod]
        public void StartsWithConjunction4()
        {
            string line1 = "is een fluitje van een cent.";
            bool line1Actual = ContinuationUtilities.StartsWithConjunction(line1, "nl");
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void StartsWithConjunction5()
        {
            string line1 = "";
            bool line1Actual = ContinuationUtilities.StartsWithConjunction(line1, "nl");
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void StartsWithConjunction6()
        {
            string line1 = " ";
            bool line1Actual = ContinuationUtilities.StartsWithConjunction(line1, "nl");
            Assert.IsFalse(line1Actual);
        }

        [TestMethod]
        public void MergeHelper1()
        {
            string line1 = "This needs to be merged...";
            string line2 = "as smoothly as possible.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "as smoothly as possible.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper2()
        {
            string line1 = "This needs to be merged...";
            string line2 = "...but keeping in mind the conjunctions.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged,";
            string line2Expected = "but keeping in mind the conjunctions.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper3()
        {
            string line1 = "This needs to be merged...";
            string line2 = "But this is a new sentence.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged...";
            string line2Expected = "But this is a new sentence.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper4()
        {
            string line1 = "The winner is...";
            string line2 = "...Mark.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "The winner is";
            string line2Expected = "Mark.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper5()
        {
            string line1 = "The winner is -";
            string line2 = "- Mark.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "The winner is";
            string line2Expected = "Mark.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper5B()
        {
            string line1 = "The winner is -";
            string line2 = "- anyone.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "The winner is";
            string line2Expected = "anyone.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper6()
        {
            string line1 = "This needs to be merged";
            string line2 = "- as smoothly as possible." + Environment.NewLine + "- Shut up.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "as smoothly as possible." + Environment.NewLine + "- Shut up.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper7()
        {
            string line1 = "This needs to be merged -";
            string line2 = "- as smoothly as possible." + Environment.NewLine + "- Shut up.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "as smoothly as possible." + Environment.NewLine + "- Shut up.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper8()
        {
            string line1 = "This needs to be merged...";
            string line2 = "- ...as smoothly as possible." + Environment.NewLine + "- Shut up.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "as smoothly as possible." + Environment.NewLine + "- Shut up.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper9()
        {
            string line1 = "This needs to be merged...";
            string line2 = "- as smoothly as possible." + Environment.NewLine + "- Shut up.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "as smoothly as possible." + Environment.NewLine + "- Shut up.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper10()
        {
            string line1 = "This needs to be merged...";
            string line2 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged...";
            string line2Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper11()
        {
            string line1 = "This needs to be merged";
            string line2 = "";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper12()
        {
            string line1 = "This needs to be merged...";
            string line2 = "-This is a response.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged...";
            string line2Expected = "-This is a response.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper12A()
        {
            string line1 = "This needs to be merged...";
            string line2 = "-This is a response.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged...";
            string line2Expected = "-This is a response.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper12B()
        {
            string line1 = "This needs to be merged-";
            string line2 = "-This is a response.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDash);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "This is a response.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper13()
        {
            string line1 = "This needs to be merged...";
            string line2 = "with this.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "with this.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper13B()
        {
            string line1 = "This needs to be merged...";
            string line2 = "-just something random.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged...";
            string line2Expected = "-just something random.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper13C()
        {
            string line1 = "This needs to be merged.";
            string line2 = "-just something random.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged.";
            string line2Expected = "-just something random.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper13D()
        {
            string line1 = "This needs to be merged";
            string line2 = "-just something random.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "-just something random.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper13E()
        {
            string line1 = "This needs to be merged...";
            string line2 = "-just something random." + Environment.NewLine + "-Good idea.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged";
            string line2Expected = "just something random." + Environment.NewLine + "-Good idea.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }

        [TestMethod]
        public void MergeHelper14()
        {
            string line1 = "This needs to be merged.";
            string line2 = "just something random.";
            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.LeadingTrailingDots);
            var result = ContinuationUtilities.MergeHelper(line1, line2, profile, "en");
            string line1Actual = result.Item1;
            string line2Actual = result.Item2;
            string line1Expected = "This needs to be merged.";
            string line2Expected = "just something random.";
            Assert.AreEqual(line1Expected, line1Actual);
            Assert.AreEqual(line2Expected, line2Actual);
        }
    }
}
