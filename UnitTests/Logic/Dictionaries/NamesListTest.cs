using Nikse.SubtitleEdit.Core.Dictionaries;

namespace Tests.Logic.Dictionaries
{
    
    public class NamesListTest
    {
        [Fact]
        public void NamesListAddWord()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "sv", false, null);

            // Act
            namesList.Add("Jones123");
            var exists = namesList.GetNames().Contains("Jones123");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void NamesListAddMultiWord()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "da", false, null);

            // Act
            namesList.Add("Charlie Parker123");
            var exists = namesList.GetMultiNames().Contains("Charlie Parker123");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void NamesListIsInNameMultiWordList()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "ru", false, null);

            // Act
            namesList.Add("Charlie Parker123");
            var exists = namesList.IsInNamesMultiWordList("This is Charlie Parker123!", "Charlie Parker123");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void NamesListNotInList()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "fr", false, null);

            // Act
            var exists = namesList.GetNames().Contains("JonesASDFLKJCKJXFLKJSLDKFJASDF");

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void NamesListAddWordReload()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "it", false, null);
            namesList.Add("Jones123");

            // Act
            namesList = new NameList(Directory.GetCurrentDirectory(), "it", false, null);

            // Assert
            Assert.Contains("Jones123", namesList.GetNames());
        }

        [Fact]
        public void NamesListRemove()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "de", false, null);
            namesList.Add("Jones123");

            // Act
            namesList.Remove("Jones123");

            // Assert
            Assert.DoesNotContain("Jones123", namesList.GetNames());
        }

        [Fact]
        
        public void NamesListRemoveReload()
        {
            // Arrange
            var namesList = new NameList(Directory.GetCurrentDirectory(), "da", false, null);
            namesList.Add("Jones123");

            // Act
            namesList.Remove("Jones123");
            namesList = new NameList(Directory.GetCurrentDirectory(), "da", false, null);

            // Assert
            Assert.DoesNotContain("Jones123", namesList.GetNames());
        }

    }
}
