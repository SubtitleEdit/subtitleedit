using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Dictionaries;
using System.IO;

namespace Test.Logic.Dictionaries
{
    [TestClass]
    public class NamesListTest
    {
        [TestMethod]
        public void NamesListAddWord()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "sv", false, null);

            // Act
            namesList.Add("Jones123");
            var exists = namesList.GetNames().Contains("Jones123");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void NamesListAddMultiWord()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "da", false, null);

            // Act
            namesList.Add("Charlie Parker123");
            var exists = namesList.GetMultiNames().Contains("Charlie Parker123");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void NamesListIsInNameMultiWordList()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "ru", false, null);

            // Act
            namesList.Add("Charlie Parker123");
            var exists = namesList.IsInNamesMultiWordList("This is Charlie Parker123!", "Charlie Parker123");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void NamesListNotInList()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "fr", false, null);

            // Act
            var exists = namesList.GetNames().Contains("JonesASDFLKJCKJXFLKJSLDKFJASDF");

            // Assert
            Assert.IsFalse(exists);
        }

        public void NamesListAddWordReload()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "it", false, null);
            namesList.Add("Jones123");

            // Act
            namesList = new NameList(Directory.GetCurrentDirectory(), "it", false, null);

            // Assert
            Assert.IsTrue(namesList.GetNames().Contains("Jones123"));
        }

        [TestMethod]
        public void NamesListRemove()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "de", false, null);
            namesList.Add("Jones123");

            // Act
            namesList.Remove("Jones123");

            // Assert
            Assert.IsFalse(namesList.GetNames().Contains("Jones123"));
        }

        [TestMethod]
        public void NamesListRemoveReload()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "da", false, null);
            namesList.Add("Jones123");

            // Act
            namesList.Remove("Jones123");
            namesList = new NameList(Directory.GetCurrentDirectory(), "da", false, null);

            // Assert
            Assert.IsFalse(namesList.GetNames().Contains("Jones123"));
        }

    }
}
