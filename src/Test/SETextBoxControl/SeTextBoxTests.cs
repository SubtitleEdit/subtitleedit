using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Test.Core
{
    [TestClass]
    public class SeTextBoxTests
    {
        [TestMethod]
        public void SelectionStartFirstLine()
        {
            Configuration.Settings.General.SubtitleTextBoxSyntaxColor = false;
            var tbNormal = new SETextBox();
            Configuration.Settings.General.SubtitleTextBoxSyntaxColor = true;
            var tbRichText = new SETextBox();

            tbNormal.Text = "How" + Environment.NewLine + "are you" + Environment.NewLine + "today?";
            tbRichText.Text = "How" + Environment.NewLine + "are you" + Environment.NewLine + "today?";

            tbNormal.SelectionStart = 0;
            tbNormal.SelectionLength = 3;
            tbRichText.SelectionStart = 0;
            tbRichText.SelectionLength = 3;

            Assert.AreEqual(tbNormal.Text, tbRichText.Text);
            Assert.AreEqual(tbNormal.SelectedText, tbRichText.SelectedText);
            Assert.AreEqual(tbNormal.SelectionStart, tbRichText.SelectionStart);
            Assert.AreEqual(tbNormal.SelectionLength, tbRichText.SelectionLength);
        }

        [TestMethod]
        public void SelectionStartSecondLine()
        {
            Configuration.Settings.General.SubtitleTextBoxSyntaxColor = false;
            var tbNormal = new SETextBox();
            Configuration.Settings.General.SubtitleTextBoxSyntaxColor = true;
            var tbRichText = new SETextBox();

            tbNormal.Text = "How" + Environment.NewLine + "are you" + Environment.NewLine + "today?";
            tbRichText.Text = "How" + Environment.NewLine + "are you" + Environment.NewLine + "today?";

            tbNormal.SelectionStart = 5;
            tbNormal.SelectionLength = 3;
            tbRichText.SelectionStart = 5;
            tbRichText.SelectionLength = 3;

            Assert.AreEqual(tbNormal.Text, tbRichText.Text);
            Assert.AreEqual(tbNormal.SelectedText, tbRichText.SelectedText);
            Assert.AreEqual(tbNormal.SelectionStart, tbRichText.SelectionStart);
            Assert.AreEqual(tbNormal.SelectionLength, tbRichText.SelectionLength);
        }
    }
}
