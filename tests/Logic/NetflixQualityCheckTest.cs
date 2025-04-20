using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.NetflixQualityCheck;
using System.Globalization;

namespace Tests.Logic
{
    
    public class NetflixQualityCheckTest
    {
        [Fact]
        public void TestNetflixCheckGlyph()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit௓ amet, consectetur adi௟piscing elit.", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckGlyph();

            checker.Check(sub, controller);

            Assert.Equal(2, controller.Records.Count);
            Assert.Equal("or sit௓ amet", controller.Records[0].Context);
            Assert.Equal("ur adi௟pisci", controller.Records[1].Context);
        }

        [Fact]
        public void TestNetflixCheckWhiteSpace()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem  ipsum dolor   sit amet, consectetur\r\n\r\nadipiscing\n\r\nelit.", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckWhiteSpace();

            checker.Check(sub, controller);

            Assert.Equal(4, controller.Records.Count);
            Assert.Equal("Lorem  ipsu", controller.Records[0].Context);
            Assert.Equal(" dolor   sit", controller.Records[1].Context);
            Assert.Equal("ctetur\r\n\r\nad", controller.Records[2].Context);
            Assert.Equal("iscing\n\r\neli", controller.Records[3].Context);
        }

        [Fact]
        public void TestNetflixCheckDialogeHyphenNoSpace()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("- Lorem ipsum dolor sit." + Environment.NewLine + "- Nelit focasia venlit dokalalam dilars.", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem - ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckDialogHyphenSpace();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].FixedParagraph.Text, "-Lorem ipsum dolor sit." + Environment.NewLine + "-Nelit focasia venlit dokalalam dilars.");
        }

        [Fact]
        public void TestNetflixCheckDialogeHyphenNoSpaceItalic()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("<i>- Lorem ipsum dolor sit.</i>" + Environment.NewLine + "<i>- Nelit focasia venlit dokalalam dilars.</i>", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem - ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckDialogHyphenSpace();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].FixedParagraph.Text, "<i>-Lorem ipsum dolor sit.</i>" + Environment.NewLine + "<i>-Nelit focasia venlit dokalalam dilars.</i>");
        }

        [Fact]
        public void TestNetflixCheckDialogeHyphenAddSpace()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("-Lorem ipsum dolor sit." + Environment.NewLine + "-Nelit focasia venlit dokalalam dilars.", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem-ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController() { Language = "fr" };
            var checker = new NetflixCheckDialogHyphenSpace();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].FixedParagraph.Text, "- Lorem ipsum dolor sit." + Environment.NewLine + "- Nelit focasia venlit dokalalam dilars.");
        }

        [Fact]
        public void TestNetflixCheckDialogeHyphenAddSpaceItalic()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("<i>-Lorem ipsum dolor sit.</i>" + Environment.NewLine + "<i>-Nelit focasia venlit dokalalam dilars.</i>", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem-ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController() { Language = "fr" };
            var checker = new NetflixCheckDialogHyphenSpace();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].FixedParagraph.Text, "<i>- Lorem ipsum dolor sit.</i>" + Environment.NewLine + "<i>- Nelit focasia venlit dokalalam dilars.</i>");
        }

        [Fact]
        public void TestNetflixCheckMaxCps()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet consectetur adipiscing nelit focasia venlit dokalalam dilars.", 0, 1000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckMaxCps();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
        }


        [Fact]
        public void TestNetflixCheckMaxLineLength()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet lasdf lajsdf ljdsf asdf asf.", 0, 8000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 7000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckMaxLineLength();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
        }

        [Fact]
        public void TestNetflixCheckMaxDuration()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet.", 0, 8000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 7000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckMaxDuration();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
        }

        [Fact]
        public void TestNetflixCheckMinDuration()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum.", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 834);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckMinDuration();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
        }


        [Fact]
        public void TestNetflixCheckNumberOfLines()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2." + Environment.NewLine + "Line 3.", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);
            var p3 = new Paragraph("Lorem ipsum dolor sit amet," + Environment.NewLine + "consectetur adipiscing elit", 0, 832);
            sub.Paragraphs.Add(p3);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckNumberOfLines();

            checker.Check(sub, controller);

            Assert.Equal(2, controller.Records.Count);
            Assert.Equal("Lorem ipsum. Line 2. Line 3.", controller.Records[0].FixedParagraph.Text);
            Assert.Equal("Lorem ipsum. Line 2.", controller.Records[1].FixedParagraph.Text);
        }

        [Fact]
        public void TestNetflixCheckNumbersOneToTenSpellOut()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("This is 1 man", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckNumbersOneToTenSpellOut();

            checker.Check(sub, controller);

            Assert.Equal(2, controller.Records.Count);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
            Assert.Equal("This is one man", controller.Records[0].FixedParagraph.Text);
        }

        [Fact]
        public void TestNetflixCheckStartNumberSpellOut()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("12 is nice!", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckStartNumberSpellOut();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
            Assert.Equal("Twelve is nice!", controller.Records[0].FixedParagraph.Text);
        }

        [Fact]
        public void TestNetflixCheckNumbersOneToTenSpellOut_NoChange()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("This is 5,000 dollers!", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line two.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckNumbersOneToTenSpellOut();

            checker.Check(sub, controller);

            Assert.Empty(controller.Records);
        }

        [Fact]
        public void TestNetflixCheckNumbersOneToTenSpellOut_NoChange2()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("This is 5.0 dollers!", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line two.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckNumbersOneToTenSpellOut();

            checker.Check(sub, controller);

            Assert.Empty(controller.Records);
        }

        [Fact]
        public void TestNetflixCheckTextForHiUseBrackets()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("(Enginie starting)", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckTextForHiUseBrackets();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
            Assert.Equal("[Enginie starting]", controller.Records[0].FixedParagraph.Text);
        }

        [Fact]
        public void TestNetflixCheckTwoFramesGap()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("(Enginie starting)", 0, 1000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 1010, 2832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckTwoFramesGap();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
            Assert.True(controller.Records[0].FixedParagraph.EndTime.TotalMilliseconds < 1000);
        }

        [Fact]
        public void TestNetflixCheckItalicsRemove()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("<i>Enginie starting</i>", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController { Language = "zh" };
            var checker = new NetflixCheckItalics();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
            Assert.DoesNotContain("<i>", controller.Records[0].FixedParagraph.Text);
        }

        [Fact]
        public void TestNetflixCheckItalicsKeep()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("<i>Enginie starting</i>", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController { Language = "en" };
            var checker = new NetflixCheckItalics();

            checker.Check(sub, controller);

            Assert.Empty(controller.Records);
        }

        [Fact]
        public void TestNetflixCheckItalicsFix()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("</i>Enginie starting</i>", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckItalics();

            checker.Check(sub, controller);

            Assert.Single(controller.Records);
            Assert.Equal(controller.Records[0].OriginalParagraph, p1);
            Assert.Equal("<i>Enginie starting</i>", controller.Records[0].FixedParagraph.Text);
        }

        [Fact]
        public void TestNetflixValidFrameRatesDrop()
        {
            var sub = new Subtitle();
            var template = @"<?xml version='1.0' encoding='utf-8'?>
<tt xml:lang='en' xmlns='http://www.w3.org/ns/ttml' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' ttp:profile='http://www.netflix.com/ns/ttml/profile/nflx-tt' ttp:frameRate='[frameRate]' ttp:frameRateMultiplier='[frameRateMultiplier]' ttp:dropMode='nonDrop' ttp:timeBase='smpte'>
  <head>
    <metadata />
  </head>
  <body>
    <div xml:lang='en'>
      <p begin='00:00:55:01' xml:id='p0' end='00:00:58:07'>
        <span tts:fontStyle='italic'>Enginie starting</span>
      </p>
    </div>
  </body>
</tt>".Replace('\'', '"');

            var p1 = new Paragraph("<i>Enginie starting</i>", 0, 1000);
            sub.Paragraphs.Add(p1);
            for (int frameRate = 0; frameRate < 100; frameRate++)
            {
                var controller = new NetflixQualityController();
                sub.Header = template.Replace("[frameRate]", frameRate.ToString(CultureInfo.InvariantCulture)).Replace("[frameRateMultiplier]", "1000 1001"); //ttp:frameRate='25' ttp:frameRateMultiplier='1000 1001'
                var checker = new NetflixCheckTimedTextFrameRate();
                if (frameRate == 24 || frameRate == 30 || frameRate == 60)
                {
                    checker.Check(sub, controller);
                    Assert.Empty(controller.Records);
                }
                else
                {
                    checker.Check(sub, controller);
                    Assert.Single(controller.Records);
                    Assert.Equal("Frame rate is invalid", controller.Records[0].Comment);
                }
            }
        }

        [Fact]
        public void TestNetflixValidFrameRates()
        {
            var sub = new Subtitle();
            var template = @"<?xml version='1.0' encoding='utf-8'?>
<tt xml:lang='en' xmlns='http://www.w3.org/ns/ttml' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' ttp:profile='http://www.netflix.com/ns/ttml/profile/nflx-tt' ttp:frameRate='[frameRate]' ttp:frameRateMultiplier='[frameRateMultiplier]' ttp:dropMode='nonDrop' ttp:timeBase='smpte'>
  <head>
    <metadata />
  </head>
  <body>
    <div xml:lang='en'>
      <p begin='00:00:55:01' xml:id='p0' end='00:00:58:07'>
        <span tts:fontStyle='italic'>Enginie starting</span>
      </p>
    </div>
  </body>
</tt>".Replace('\'', '"');

            var p1 = new Paragraph("<i>Enginie starting</i>", 0, 1000);
            sub.Paragraphs.Add(p1);
            for (int frameRate = 0; frameRate < 200; frameRate++)
            {
                var controller = new NetflixQualityController();
                sub.Header = template.Replace("[frameRate]", frameRate.ToString(CultureInfo.InvariantCulture)).Replace("[frameRateMultiplier]", "1 1"); //ttp:frameRate='25' ttp:frameRateMultiplier='1000 1001'
                var checker = new NetflixCheckTimedTextFrameRate();
                if (frameRate == 24 || frameRate == 25 || frameRate == 30 || frameRate == 50 || frameRate == 60)
                {
                    checker.Check(sub, controller);
                    Assert.Empty(controller.Records);
                }
                else
                {
                    checker.Check(sub, controller);
                    Assert.Single(controller.Records);
                    Assert.Equal("Frame rate is invalid", controller.Records[0].Comment);
                }
            }
        }

    }
}
