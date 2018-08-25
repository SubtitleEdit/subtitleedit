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
            sub.LoadSubtitle(fileName, out _, null);
            return LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
        }

        [TestMethod]
        public void AutoDetectRussian()
        {
            var languageCode = GetLanguageCode("auto_detect_Russian.srt");
            Assert.AreEqual("ru", languageCode);
        }

        [TestMethod]
        public void AutoDetectDanish()
        {
            var languageCode = GetLanguageCode("auto_detect_Danish.srt");
            Assert.AreEqual("da", languageCode);
        }

        private static Encoding DetectAnsiEncoding(string fileName)
        {
            fileName = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            return LanguageAutoDetect.DetectAnsiEncoding(FileUtil.ReadAllBytesShared(fileName));
        }

        [TestMethod]
        public void AutoDetectCodePage1250()
        {
            var encoding = DetectAnsiEncoding("auto_detect_windows-1250.srt");
            Assert.AreEqual(1250, encoding.CodePage);
        }

        [TestMethod]
        public void AutoDetectCodePage1251()
        {
            var encoding = DetectAnsiEncoding("auto_detect_windows-1251.srt");
            Assert.AreEqual(1251, encoding.CodePage);
        }
    }
}
