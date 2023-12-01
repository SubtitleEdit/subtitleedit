using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Forms.Translate;
using System;
using System.Linq;

namespace Test.Logic.AutoTranslate
{
    [TestClass]
    public class MergeAndSplitHelperTest
    {
        [TestMethod]
        public void Test1()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hallo", 0, 1000));
            subtitle.Paragraphs.Add(new Paragraph("world.", 1000, 2000));

            var mergeResult = MergeAndSplitHelper.MergeMultipleLines(subtitle, 0, 1500);

            Assert.IsNotNull(mergeResult);
            Assert.AreEqual("Hallo world.", mergeResult.Text);
            Assert.AreEqual(subtitle.Paragraphs.Count, mergeResult.ParagraphCount);
            Assert.AreEqual(1, mergeResult.MergeResultItems.Count);
            Assert.AreEqual(true, mergeResult.MergeResultItems[0].Continious);
            Assert.AreEqual(0, mergeResult.MergeResultItems[0].StartIndex);
            Assert.AreEqual(1, mergeResult.MergeResultItems[0].EndIndex);

            var splitResult = MergeAndSplitHelper.SplitMultipleLines(mergeResult, mergeResult.Text, "en");
            Assert.AreEqual(subtitle.Paragraphs.Count, splitResult.Count);
            Assert.AreEqual(string.Join(" ", subtitle.Paragraphs.Select(p => p.Text)), string.Join(" ", splitResult));
        }

        [TestMethod]
        public void Test2()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("", 0, 1000));
            subtitle.Paragraphs.Add(new Paragraph("Hallo there.", 1, 2000));
            subtitle.Paragraphs.Add(new Paragraph("How are you?", 3000, 4000));

            var mergeResult = MergeAndSplitHelper.MergeMultipleLines(subtitle, 0, 1500);

            Assert.IsNotNull(mergeResult);
            Assert.AreEqual("Hallo there." + Environment.NewLine + "How are you?", mergeResult.Text);
            Assert.AreEqual(subtitle.Paragraphs.Count, mergeResult.ParagraphCount);
            Assert.AreEqual(3, mergeResult.MergeResultItems.Count);
            Assert.AreEqual(true, mergeResult.MergeResultItems[0].IsEmpty);
            Assert.AreEqual(false, mergeResult.MergeResultItems[1].IsEmpty);
            Assert.AreEqual(false, mergeResult.MergeResultItems[1].Continious);
            Assert.AreEqual(1, mergeResult.MergeResultItems[1].StartIndex);
            Assert.AreEqual(1, mergeResult.MergeResultItems[1].EndIndex);
            Assert.AreEqual('.', mergeResult.MergeResultItems[1].EndChar);
            Assert.AreEqual(1, mergeResult.MergeResultItems[1].EndCharOccurences);

            var splitResult = MergeAndSplitHelper.SplitMultipleLines(mergeResult, mergeResult.Text, "en");
            Assert.AreEqual(subtitle.Paragraphs.Count, splitResult.Count);
            Assert.AreEqual(string.Join(" ", subtitle.Paragraphs.Select(p => p.Text)), string.Join(" ", splitResult));
        }

        [TestMethod]
        public void Test3()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hallo there. In the garden.", 0, 1000));
            subtitle.Paragraphs.Add(new Paragraph("How are you?", 1000, 2000));

            var mergeResult = MergeAndSplitHelper.MergeMultipleLines(subtitle, 0, 1500);

            Assert.IsNotNull(mergeResult);
            Assert.AreEqual("Hallo there. In the garden." + Environment.NewLine + "How are you?", mergeResult.Text);
            Assert.AreEqual(subtitle.Paragraphs.Count, mergeResult.ParagraphCount);
            Assert.AreEqual(2, mergeResult.MergeResultItems.Count);
            Assert.AreEqual(false, mergeResult.MergeResultItems[0].Continious);
            Assert.AreEqual(0, mergeResult.MergeResultItems[0].StartIndex);
            Assert.AreEqual(0, mergeResult.MergeResultItems[0].EndIndex);
            Assert.AreEqual('.', mergeResult.MergeResultItems[0].EndChar);
            Assert.AreEqual(2, mergeResult.MergeResultItems[0].EndCharOccurences);
        }

        [TestMethod]
        public void Test4()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hallo there. In the garden", 0, 1000));
            subtitle.Paragraphs.Add(new Paragraph("today are we? So I will very soon", 1000, 2000));
            subtitle.Paragraphs.Add(new Paragraph("be going home to Sweden.", 1000, 2000));
            subtitle.Paragraphs.Add(new Paragraph("My name is Peter!", 8000, 10000));
            subtitle.Paragraphs.Add(new Paragraph("My name is Peter! And Jones.", 11000, 13000));
            subtitle.Paragraphs.Add(new Paragraph("My name is Peter. And Jones.", 15000, 18000));
            subtitle.Paragraphs.Add(new Paragraph("", 20000, 21000));
            subtitle.Paragraphs.Add(new Paragraph("Hallo there.", 21000, 22000));

            var mergeResult = MergeAndSplitHelper.MergeMultipleLines(subtitle, 0, 1500);

            Assert.IsNotNull(mergeResult);
            Assert.AreEqual("Hallo there. In the garden today are we? So I will very soon be going home to Sweden." + Environment.NewLine +
                             "My name is Peter!" + Environment.NewLine +
                             "My name is Peter! And Jones." + Environment.NewLine +
                             "My name is Peter. And Jones." + Environment.NewLine +
                             "Hallo there.", mergeResult.Text);

            Assert.AreEqual(6, mergeResult.MergeResultItems.Count);
            Assert.AreEqual(subtitle.Paragraphs.Count, mergeResult.ParagraphCount);

            Assert.AreEqual(true, mergeResult.MergeResultItems[0].Continious);
            Assert.AreEqual(0, mergeResult.MergeResultItems[0].StartIndex);
            Assert.AreEqual(2, mergeResult.MergeResultItems[0].EndIndex);
            Assert.AreEqual('.', mergeResult.MergeResultItems[0].EndChar);
            Assert.AreEqual(2, mergeResult.MergeResultItems[0].EndCharOccurences);

            Assert.AreEqual(false, mergeResult.MergeResultItems[1].Continious);
            Assert.AreEqual(3, mergeResult.MergeResultItems[1].StartIndex);
            Assert.AreEqual(3, mergeResult.MergeResultItems[1].EndIndex);
            Assert.AreEqual('!', mergeResult.MergeResultItems[1].EndChar);

            Assert.AreEqual(false, mergeResult.MergeResultItems[2].Continious);
            Assert.AreEqual(4, mergeResult.MergeResultItems[2].StartIndex);
            Assert.AreEqual(4, mergeResult.MergeResultItems[2].EndIndex);
            Assert.AreEqual('.', mergeResult.MergeResultItems[2].EndChar);
            Assert.AreEqual(1, mergeResult.MergeResultItems[2].EndCharOccurences);

            Assert.AreEqual(false, mergeResult.MergeResultItems[3].Continious);
            Assert.AreEqual(5, mergeResult.MergeResultItems[3].StartIndex);
            Assert.AreEqual(5, mergeResult.MergeResultItems[3].EndIndex);
            Assert.AreEqual('.', mergeResult.MergeResultItems[3].EndChar);
            Assert.AreEqual(2, mergeResult.MergeResultItems[3].EndCharOccurences);

            Assert.AreEqual(true, mergeResult.MergeResultItems[4].IsEmpty);

            Assert.AreEqual(false, mergeResult.MergeResultItems[5].Continious);
            Assert.AreEqual(7, mergeResult.MergeResultItems[5].StartIndex);
            Assert.AreEqual(7, mergeResult.MergeResultItems[5].EndIndex);
            Assert.AreEqual('.', mergeResult.MergeResultItems[5].EndChar);
            Assert.AreEqual(1, mergeResult.MergeResultItems[5].EndCharOccurences);

            var splitResult = MergeAndSplitHelper.SplitMultipleLines(mergeResult, mergeResult.Text, "en");
            Assert.AreEqual(subtitle.Paragraphs.Count, splitResult.Count);
            Assert.IsTrue(splitResult[0].Length > 5);
            Assert.IsTrue(splitResult[1].Length > 5);
            Assert.IsTrue(splitResult[2].Length > 5);
            Assert.AreEqual(string.Join(" ", subtitle.Paragraphs.Select(p => p.Text)), string.Join(" ", splitResult));
            Assert.AreEqual("My name is Peter!", splitResult[3]);
            Assert.AreEqual("My name is Peter! And Jones.", splitResult[4]);
            Assert.AreEqual("My name is Peter. And Jones.", splitResult[5]);
            Assert.AreEqual("", splitResult[6]);
            Assert.AreEqual("Hallo there.", splitResult[7]);
        }
    }
}
