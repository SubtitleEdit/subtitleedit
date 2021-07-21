using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Controls;

namespace Test.Assa
{
    [TestClass]
    public class TagHelperRemoveTagTest
    {
        [TestMethod]
        public void RemoveTag1()
        {
            var tb = new SETextBox { Text = "{\\an1}Hallo!", SelectionStart = 1 };
            Nikse.SubtitleEdit.Logic.AssaTagHelper.RemoveTagAtCursor(tb);
            Assert.AreEqual("Hallo!", tb.Text);
        }

        [TestMethod]
        public void RemoveTag2()
        {
            var tb = new SETextBox { Text = "{\\an1\\i1}Hallo!", SelectionStart = 1 };
            Nikse.SubtitleEdit.Logic.AssaTagHelper.RemoveTagAtCursor(tb);
            Assert.AreEqual("{\\i1}Hallo!", tb.Text);
        }

        [TestMethod]
        public void RemoveTag3()
        {
            var tb = new SETextBox { Text = "{\\an1\\i1}Hallo!", SelectionStart = 6 };
            Nikse.SubtitleEdit.Logic.AssaTagHelper.RemoveTagAtCursor(tb);
            Assert.AreEqual("{\\an1}Hallo!", tb.Text);
        }

        [TestMethod]
        public void RemoveTag4()
        {
            var tb = new SETextBox { Text = "{\\an1\\i1\\b1}Hallo!", SelectionStart = 6 };
            Nikse.SubtitleEdit.Logic.AssaTagHelper.RemoveTagAtCursor(tb);
            Assert.AreEqual("{\\an1\\b1}Hallo!", tb.Text);
        }

        [TestMethod]
        public void RemoveTag5()
        {
            var tb = new SETextBox { Text = "{\\an1\\i1\\b1}Hallo!", SelectionStart = 2 };
            Nikse.SubtitleEdit.Logic.AssaTagHelper.RemoveTagAtCursor(tb);
            Assert.AreEqual("{\\i1\\b1}Hallo!", tb.Text);
        }
    }
}
