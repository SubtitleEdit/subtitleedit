using Nikse.SubtitleEdit.Core.AutoTranslate;
using System;

namespace Tests.Logic.AutoTranslate
{
    /// <summary>
    /// Each entry in data.translations has more string properties than "translatedText" - reading them all
    /// appended the source language code ("en", "ja") to every translated subtitle (#15085).
    /// </summary>
    [TestClass]
    public class GoogleTranslateV2ParseTest
    {
        [TestMethod]
        public void DetectedSourceLanguage_IsNotPartOfTheTranslation()
        {
            var json = "{ \"data\": { \"translations\": [ { \"translatedText\": \"Bedankt.\", \"detectedSourceLanguage\": \"en\" } ] } }";

            Assert.AreEqual("Bedankt.", GoogleTranslateV2.ParseTranslations(json, "nl"));
        }

        [TestMethod]
        public void Model_IsNotPartOfTheTranslation()
        {
            var json = "{ \"data\": { \"translations\": [ { \"detectedSourceLanguage\": \"ja\", \"model\": \"nmt\", \"translatedText\": \"Bedankt.\" } ] } }";

            Assert.AreEqual("Bedankt.", GoogleTranslateV2.ParseTranslations(json, "nl"));
        }

        [TestMethod]
        public void MultipleTranslations_AreJoinedWithNewLine()
        {
            var json = "{ \"data\": { \"translations\": [ { \"translatedText\": \"Hallo.\" }, { \"translatedText\": \"Bedankt.\" } ] } }";

            Assert.AreEqual("Hallo." + Environment.NewLine + "Bedankt.", GoogleTranslateV2.ParseTranslations(json, "nl"));
        }
    }
}
