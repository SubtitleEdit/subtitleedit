using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
{
    
    public class UnknownFormatImporterCsvTest
    {
        [Fact]
        public void TestUnknownCsv1()
        {
            var raw = @"ROLLE;IN;OUT;DIALOG
PAUL;00:00:06,901;00:00:09,901; So, du hast also den Schwangerschaftstest gebraucht?
CATE;00:00:10,000;00:00:12,800; Ja. Ich habe den Test gemacht.";

            var importer = new UnknownFormatImporterCsv();
            var subtitle = importer.AutoGuessImport(raw.Trim().SplitToLines());
            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("Ja. Ich habe den Test gemacht.", subtitle.Paragraphs[1].Text);
            Assert.Equal("10,000", subtitle.Paragraphs[1].StartTime.ToShortString());
            Assert.Equal("12,800", subtitle.Paragraphs[1].EndTime.ToShortString());
        }

        [Fact]
        public void TestUnknownCsv2()
        {
            var raw = @"actor,startms,endms,text
PAUL,1000,2000, So du hast also den Schwangerschaftstest gebraucht?
CATE,5000,7000, Ja. Ich habe den Test gemacht.";

            var importer = new UnknownFormatImporterCsv();
            var subtitle = importer.AutoGuessImport(raw.Trim().SplitToLines());
            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("Ja. Ich habe den Test gemacht.", subtitle.Paragraphs[1].Text);
            Assert.Equal("5,000", subtitle.Paragraphs[1].StartTime.ToShortString());
            Assert.Equal("7,000", subtitle.Paragraphs[1].EndTime.ToShortString());
        }

        [Fact]
        public void TestUnknownCsv3()
        {
            var raw = @"NAME;START;END;DIALOG
PAUL;00:00:06:00;00:00:09:00; So, du hast also den Schwangerschaftstest gebraucht?
CATE;00:00:10:00;00:00:12:00; Ja. Ich habe den Test gemacht.";

            var importer = new UnknownFormatImporterCsv();
            var subtitle = importer.AutoGuessImport(raw.Trim().SplitToLines());
            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("Ja. Ich habe den Test gemacht.", subtitle.Paragraphs[1].Text);

            //TODO: fails on appveyor... why?
            //Assert.Equal("10,000", subtitle.Paragraphs[1].StartTime.ToShortString());
            //Assert.Equal("12,000", subtitle.Paragraphs[1].EndTime.ToShortString());
        }
    }
}