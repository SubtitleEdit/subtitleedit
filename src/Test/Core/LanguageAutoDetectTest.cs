using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using System.IO;
using System.Text;

namespace Test.Core
{

    [DeploymentItem("Files")]
    [TestClass]
    public class LanguageAutoDetectTest
    {

        private static string GetLanguageCode(string fileName)
        {
            fileName = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var sub = new Subtitle();
            Encoding encoding;
            sub.LoadSubtitle(fileName, out encoding, null);
            return LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
        }

        [TestMethod]
        public void AutoDetectRussian()
        {
            var languageCode = GetLanguageCode("auto_detect_Russian.srt");
            Assert.AreEqual(languageCode, "ru");
        }

        [TestMethod]
        public void AutoDetectDanish()
        {
            var languageCode = GetLanguageCode("auto_detect_Danish.srt");
            Assert.AreEqual(languageCode, "da");
        }

    }
}
