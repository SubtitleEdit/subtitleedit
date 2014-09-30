using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Dictionaries;

namespace Test.Logic
{
    [TestClass]
    public class OcrFixReplaceListTest
    {
        [TestMethod]
        public void OcrFixReplaceListAddWord()
        {
            // Arrange
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".xml");
            var fixList = new OcrFixReplaceList(fileName);
            fixList.WordReplaceList.Clear();

            // Act
            fixList.AddWordOrPartial("from", "to");

            // Assert
            Assert.IsTrue(fixList.WordReplaceList["from"] == "to");

            // Clean up
            try
            {
                File.Delete(fileName);
            }
            catch
            {

            }
        }

        [TestMethod]
        public void OcrFixReplaceListAddWordReload()
        {
            // Arrange
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".xml");
            var fixList = new OcrFixReplaceList(fileName);
            fixList.WordReplaceList.Clear();
            fixList.AddWordOrPartial("from", "to");

            // Act
            fixList = new OcrFixReplaceList(fileName);

            // Assert
            Assert.IsTrue(fixList.WordReplaceList["from"] == "to");

            // Clean up
            try
            {
                File.Delete(fileName);
            }
            catch
            {

            }
        }

        [TestMethod]
        public void OcrFixReplaceListAddPartialLine()
        {
            // Arrange
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".xml");
            var fixList = new OcrFixReplaceList(fileName);
            fixList.PartialLineWordBoundaryReplaceList.Clear();

            // Act
            fixList.AddWordOrPartial("from me", "to you");

            // Assert
            Assert.IsTrue(fixList.PartialLineWordBoundaryReplaceList["from me"] == "to you");

            // Clean up
            try
            {
                File.Delete(fileName);
            }
            catch
            {

            }
        }

        [TestMethod]
        public void OcrFixReplaceListAddPartialLineReload()
        {
            // Arrange
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".xml");
            var fixList = new OcrFixReplaceList(fileName);
            fixList.PartialLineWordBoundaryReplaceList.Clear();
            fixList.AddWordOrPartial("from me", "to you");

            // Act
            fixList = new OcrFixReplaceList(fileName);

            // Assert
            Assert.IsTrue(fixList.PartialLineWordBoundaryReplaceList["from me"] == "to you");

            // Clean up
            try
            {
                File.Delete(fileName);
            }
            catch
            {

            }
        }

        [TestMethod]
        public void OcrFixReplaceListRemoveWord()
        {
            // Arrange
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".xml");
            var fixList = new OcrFixReplaceList(fileName);
            fixList.PartialLineWordBoundaryReplaceList.Clear();
            fixList.AddWordOrPartial("from", "to");

            // Act
            fixList.RemoveWordOrPartial("from");

            // Assert
            Assert.IsTrue(!fixList.WordReplaceList.ContainsKey("from"));

            // Clean up
            try
            {
                File.Delete(fileName);
            }
            catch
            {

            }
        }

        [TestMethod]
        public void OcrFixReplaceListRemoveWordReload()
        {
            // Arrange
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".xml");
            var fixList = new OcrFixReplaceList(fileName);
            fixList.PartialLineWordBoundaryReplaceList.Clear();
            fixList.AddWordOrPartial("from", "to");
            fixList.RemoveWordOrPartial("from");

            // Act
            fixList = new OcrFixReplaceList(fileName);

            // Assert
            Assert.IsTrue(!fixList.WordReplaceList.ContainsKey("from"));

            // Clean up
            try
            {
                File.Delete(fileName);
            }
            catch
            {

            }
        }

        [TestMethod]
        public void OcrFixReplaceListRemovePartialLine()
        {
            // Arrange
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".xml");
            var fixList = new OcrFixReplaceList(fileName);
            fixList.PartialLineWordBoundaryReplaceList.Clear();
            fixList.AddWordOrPartial("from me", "to you");

            // Act
            fixList.RemoveWordOrPartial("from me");

            // Assert
            Assert.IsTrue(!fixList.WordReplaceList.ContainsKey("from me"));

            // Clean up
            try
            {
                File.Delete(fileName);
            }
            catch
            {

            }
        }

        [TestMethod]
        public void OcrFixReplaceListRemovePartialLineReload()
        {
            // Arrange
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".xml");
            var fixList = new OcrFixReplaceList(fileName);
            fixList.PartialLineWordBoundaryReplaceList.Clear();
            fixList.AddWordOrPartial("from me", "to you");
            fixList = new OcrFixReplaceList(fileName);
            fixList.RemoveWordOrPartial("from me");

            // Act
            fixList = new OcrFixReplaceList(fileName);

            // Assert
            Assert.IsTrue(!fixList.WordReplaceList.ContainsKey("from me"));

            // Clean up
            try
            {
                File.Delete(fileName);
            }
            catch
            {

            }
        }

    }
}
