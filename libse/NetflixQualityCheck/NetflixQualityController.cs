using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixQualityController
    {

        /// <summary>
        /// Two letter language code
        /// </summary>
        public string Language { get; set; } = "en";

        public double FrameRate { get; set; } = 24;

        public int CharactersPerSecond
        {
            get
            {
                if (!string.IsNullOrEmpty(Language))
                {
                    if (Language == "ar") // Arabic
                    {
                        return 20;
                    }
                    if (Language == "ko") // Korean
                    {
                        return 12;
                    }
                    if (Language == "zh") // Chinese
                    {
                        return 9;
                    }
                }
                return 17;
            }
        }

        public int SingleLineMaxLength
        {
            get
            {
                if (!string.IsNullOrEmpty(Language))
                {
                    if (Language == "ja") // Japanese
                    {
                        return 13;
                    }
                    if (Language == "ko") // Korean
                    {
                        return 16;
                    }
                    if (Language == "zh") // Chinese
                    {
                        return 16;
                    }
                }
                return 42;
            }
        }

        public bool AllowItalics
        {
            get
            {
                if (!string.IsNullOrEmpty(Language))
                {
                    if (Language == "ko") // Korean
                    {
                        return false;
                    }
                    if (Language == "zh") // Chinese
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool DualSpeakersHasHypenAndNoSplace
        {
            get
            {
                if (!string.IsNullOrEmpty(Language))
                {
                    if (Language == "cs") // Czech
                    {
                        return false;
                    }
                    if (Language == "fr") // French
                    {
                        return false;
                    }
                    if (Language == "ko") // Korean
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public class Record
        {
            public string Timecode { get; set; }
            public string Context { get; set; }
            public string Comment { get; set; }
            public Paragraph OriginalParagraph { get; set; }
            public Paragraph FixedParagraph { get; set; }

            public Record()
            {
                Timecode = string.Empty;
                Context = string.Empty;
                Comment = string.Empty;
            }

            public Record(string timecode, string context, string comment)
            {
                Timecode = timecode;
                Context = context;
                Comment = comment;
            }

            public string ToCsvRow()
            {
                return $"{Timecode},{CsvTextEncode(Context)},{CsvTextEncode(Comment)}";
            }

            private static string CsvTextEncode(string s)
            {
                s = s.Replace("\"", "\"\"");
                s = s.Replace("\r", "\\r");
                s = s.Replace("\n", "\\n");
                return $"\"{s}\"";
            }
        }

        public List<Record> Records { get; private set; }

        public NetflixQualityController()
        {
            Records = new List<Record>();
        }

        public void AddRecord(Paragraph originalParagraph, string timecod, string context, string comment)
        {
            Records.Add(new Record(timecod, context, comment) { OriginalParagraph = originalParagraph });
        }

        public void AddRecord(Paragraph originalParagraph, string comment)
        {
            Records.Add(new Record
            {
                Comment = comment,
                OriginalParagraph = originalParagraph,
                Timecode = originalParagraph.StartTime.ToDisplayString()
            });
        }
        public void AddRecord(Paragraph originalParagraph, Paragraph fixedParagraph, string comment, string context = "")
        {
            Records.Add(new Record
            {
                Comment = comment,
                Context = context,
                OriginalParagraph = originalParagraph,
                FixedParagraph = fixedParagraph,
                Timecode = originalParagraph.StartTime.ToDisplayString()
            });
        }

        public string ExportCsv()
        {
            var csvBuilder = new StringBuilder();

            // Header
            csvBuilder.AppendLine("Timecode,Context,Comment");

            // Rows
            Records.ForEach(r => csvBuilder.AppendLine(r.ToCsvRow()));

            return csvBuilder.ToString();
        }

        public void SaveCsv(string reportPath)
        {
            File.WriteAllText(reportPath, ExportCsv(), Encoding.UTF8);
        }

        public bool IsEmpty => Records.Count == 0;

        public static string StringContext(string str, int pos, int radius)
        {
            int beginPos = Math.Max(0, pos - radius);
            int endPos = Math.Min(str.Length, pos + radius);
            int length = endPos - beginPos;
            return str.Substring(beginPos, length);
        }

        private List<INetflixQualityChecker> GetAllCheckers()
        {
            return new List<INetflixQualityChecker>
            {
                new NetflixCheckDialogeHyphenNoSpace(),
                new NetflixCheckGlyph(),
                new NetflixCheckMaxCps(),
                new NetflixCheckMaxLineLength(),
                new NetflixCheckMaxDuration(),
                new NetflixCheckMinDuration(),
                new NetflixCheckNumberOfLines(),
                new NetflixCheckNumbersOneToTenSpellOut(),
                new NetflixCheckStartNumberSpellOut(),
                new NetflixCheckTextForHiUseBrackets(),
                new NetflixCheckTwoFramesGap(),
                new NetflixCheckWhiteSpace(),
                new NetflixCheckItalics()
            };
        }

        public void CheckAll(Subtitle subtitle)
        {
            Records = new List<Record>();
            foreach (var checker in GetAllCheckers())
            {
                checker.Check(subtitle, this);
            }
            Records = Records.OrderBy(p => p.OriginalParagraph.Number).ToList();
        }

    }
}
