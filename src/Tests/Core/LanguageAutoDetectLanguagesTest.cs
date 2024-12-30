using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Tests.Core
{
    [TestClass]
    public class LanguageAutoDetectLanguagesTest
    {
        [TestMethod]
        public void AutoDetectEnglish()
        {
            var res = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull2(new Subtitle(new List<Paragraph>
            {
                new Paragraph("In this tutorial, I'll show you how to add", 0,0)
            }));
            Assert.AreEqual("en", res);
        }

        [TestMethod]
        public void AutoDetectDanish()
        {
            var res = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull2(new Subtitle(new List<Paragraph>
            {
                new Paragraph("Jeg kan ikke finde på noget at lave i dag. Kun at være glad.", 0,0)
            }));
            Assert.AreEqual("da", res);
        }
    }
}
