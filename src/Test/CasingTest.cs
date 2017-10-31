using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Casing;
using System.Collections.Generic;
using System.Globalization;

namespace Test
{
    [TestClass]
    public class CasingTest
    {
        private static readonly IList<string> _nameList;
        private static CasingContext _context;

        private enum ConverterType
        {
            Uppercase,
            Lowercase,
            Normal,
            Names
        }

        static CasingTest()
        {
            // TODO: add names for fixes that requires names.
            _nameList = new List<string>();
        }

        [TestMethod]
        public void StrippableTextChangeCasing()
        {
            CaseConverter converter = new NormalCaseConverter(_nameList, new CasingOptions() { Names = false, OnlyWhereUppercase = false });

            // each line represents a paragraph
            string[] pragraphsString =
            {
                "this is for www.nikse.dk. thank you.",
                "this is for www.nikse.dk! thank you.",
                "www.nikse.dk"
            };

            Subtitle subtitle = GetSubtitleFromText(pragraphsString);
            converter.Convert(subtitle, GetCoontext());

            Assert.AreEqual("This is for www.nikse.dk. Thank you.", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("This is for www.nikse.dk! Thank you.", subtitle.Paragraphs[1].Text);
            Assert.AreEqual("www.nikse.dk", subtitle.Paragraphs[2].Text);
        }

        public static Subtitle GetSubtitleFromText(params string[] lines)
        {
            var subtitle = new Subtitle();
            for (int i = 0; i < lines.Length; i++)
            {
                var p = new Paragraph();
                p.StartTime = new TimeCode(TimeCode.BaseUnit * i);
                p.EndTime = new TimeCode(p.StartTime.TotalMilliseconds + 2000);
                p.Text = lines[i];
                subtitle.Paragraphs.Add(p);
            }
            return subtitle;
        }

        private static CasingContext GetCoontext()
        {
            if (_context == null)
            {
                _context = new CasingContext()
                {
                    Culture = CultureInfo.CurrentCulture,
                    Language = "eng"
                };
            }
            return _context;
        }
    }
}
