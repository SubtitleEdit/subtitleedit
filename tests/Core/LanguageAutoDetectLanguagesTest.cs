using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Core
{
    
    public class LanguageAutoDetectLanguagesTest
    {
        [Fact]
        public void AutoDetectEnglish()
        {
            var res = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull2(new Subtitle(new List<Paragraph>
            {
                new Paragraph("In this tutorial, I'll show you how to add", 0,0)
            }));
            Assert.Equal("en", res);
        }

        [Fact]
        public void AutoDetectDanish()
        {
            var res = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull2(new Subtitle(new List<Paragraph>
            {
                new Paragraph("Jeg kan ikke finde på noget at lave i dag. Kun at være glad.", 0,0)
            }));
            Assert.Equal("da", res);
        }
    }
}
