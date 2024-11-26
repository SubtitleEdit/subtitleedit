using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixQualityController
    {
        /// <summary>
        /// Two letter language code (e.g. "en" is English)
        /// </summary>
        public string Language { get; set; } = "en";

        public double FrameRate { get; set; } = Configuration.Settings.General.CurrentFrameRate;
        public string VideoFileName { get; set; }
        public bool VideoExists => !string.IsNullOrEmpty(VideoFileName);

        public bool IsChildrenProgram { get; set; }
        public bool IsSDH { get; set; }

        public int CharactersPerSecond
        {
            get
            {
                if (IsChildrenProgram && IsSDH)
                {
                    switch (Language)
                    {
                        case "ar": // Arabic
                        case "hi": // Hindi
                            return 20;
                        case "ja": // Japanese
                            return 7;
                        case "ko": // Korean
                            return 11;
                        case "zh": // Chinese
                            return 9;
                        default:
                            return 17;
                    }
                }
                else if (IsChildrenProgram)
                {
                    switch (Language)
                    {
                        case "ar": // Arabic
                        case "en": // English
                            return 17;
                        case "hi": // Hindi
                            return 18;
                        case "ja": // Japanese
                            return 4;
                        case "ko": // Korean
                            return 9;
                        case "zh": // Chinese
                            return 7;
                        default:
                            return 13;
                    }
                }
                else if (IsSDH)
                {
                    switch (Language)
                    {
                        case "ar": // Arabic
                            return 23;
                        case "hi": // Hindi
                            return 25;
                        case "ja": // Japanese
                            return 7;
                        case "ko": // Korean
                            return 14;
                        case "zh": // Chinese
                            return 11;
                        default:
                            return 20;
                    }
                }
                else
                {
                    switch (Language)
                    {
                        case "ar": // Arabic
                        case "en": // English
                            return 20;
                        case "hi": // Hindi
                            return 22;
                        case "ja": // Japanese
                            return 4;
                        case "ko": // Korean
                            return 12;
                        case "zh": // Chinese
                            return 9;
                        default:
                            return 17;
                    }
                }
            }
        }

        public int SingleLineMaxLength
        {
            get
            {
                switch (Language)
                {
                    case "ja": // Japanese
                        return 23;
                    case "th": // Thai
                        return 35;
                    case "ko": // Korean
                    case "zh": // Chinese
                        return 16;
                    case "ru": // Russian
                        return 39;
                    default:
                        return 42;
                }
            }
        }

        public DialogType SpeakerStyle
        {
            get
            {
                switch (Language)
                {
                    case "ar": // Arabic
                    case "pt": // Brazilian Portuguese
                        return DialogType.DashBothLinesWithSpace;
                    case "cs": // Czech
                        return DialogType.DashBothLinesWithSpace;
                    case "fr": // French
                        return DialogType.DashBothLinesWithSpace;
                    case "hu": // Hungarian
                        return DialogType.DashBothLinesWithSpace;
                    case "in": // Indonesian
                        return DialogType.DashBothLinesWithSpace;
                    case "it": // Italian
                        return DialogType.DashBothLinesWithSpace;
                    case "ko": // Korean
                        return DialogType.DashBothLinesWithSpace;
                    case "ms": // Malay
                        return DialogType.DashBothLinesWithSpace;
                    case "pl": // Polish
                        return DialogType.DashBothLinesWithSpace;
                    case "ro": // Romanian
                        return DialogType.DashBothLinesWithSpace;
                    case "ru": // Russian
                        return DialogType.DashBothLinesWithSpace;
                    case "sk": // Slovak
                        return DialogType.DashBothLinesWithSpace;
                    case "es": // Spanish
                        return DialogType.DashBothLinesWithSpace;
                    case "th": // Thai
                        return DialogType.DashBothLinesWithSpace;
                    case "vi": // Vietnamese
                        return DialogType.DashBothLinesWithSpace;
                    case "nl": // Dutch 
                        return DialogType.DashSecondLineWithoutSpace;
                    case "fi": // Finnish
                        return DialogType.DashSecondLineWithoutSpace;
                    case "he": // Hebrew
                        return DialogType.DashSecondLineWithoutSpace;
                    case "sr": // Serbian
                        return DialogType.DashSecondLineWithoutSpace;
                    case "bg": // Bulgarian 
                        return DialogType.DashSecondLineWithSpace;
                    default:
                        return DialogType.DashBothLinesWithoutSpace;
                }
            }
        }

        public bool AllowItalics
        {
            get
            {
                if (!string.IsNullOrEmpty(Language))
                {
                    if (Language == "ar") // Arabic
                    {
                        return false;
                    }
                    else if (Language == "ko") // Korean
                    {
                        return false;
                    }
                    else if (Language == "zh") // Chinese
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public class Record
        {
            public string LineNumber { get; set; }
            public string TimeCode { get; set; }
            public string Context { get; set; }
            public string Comment { get; set; }
            public Paragraph OriginalParagraph { get; set; }
            public Paragraph FixedParagraph { get; set; }

            public Record()
            {
                LineNumber = string.Empty;
                TimeCode = string.Empty;
                Context = string.Empty;
                Comment = string.Empty;
            }

            public Record(string lineNumber, string timeCode, string context, string comment)
            {
                LineNumber = lineNumber;
                TimeCode = timeCode;
                Context = context;
                Comment = comment;
            }

            public string ToCsvRow()
            {
                return $"{LineNumber},{TimeCode},{CsvTextEncode(Context)},{CsvTextEncode(Comment)}";
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

        public void AddRecord(Paragraph originalParagraph, string timeCode, string context, string comment)
        {
            Records.Add(new Record(originalParagraph?.Number.ToString(CultureInfo.InvariantCulture), timeCode, context, comment) { OriginalParagraph = originalParagraph });
        }

        public void AddRecord(Paragraph originalParagraph, string comment)
        {
            Records.Add(new Record
            {
                LineNumber = originalParagraph?.Number.ToString(CultureInfo.InvariantCulture),
                Comment = comment,
                OriginalParagraph = originalParagraph,
                TimeCode = originalParagraph.StartTime.ToDisplayString()
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
                LineNumber = originalParagraph?.Number.ToString(CultureInfo.InvariantCulture),
                TimeCode = originalParagraph?.StartTime.ToDisplayString()
            });
        }

        public string ExportCsv()
        {
            var csvBuilder = new StringBuilder();

            // Header
            csvBuilder.AppendLine("LineNumber,TimeCode,Context,Comment");

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

        private static List<INetflixQualityChecker> GetAllCheckers()
        {
            return new List<INetflixQualityChecker>
            {
                new NetflixCheckTimedTextFrameRate(),
                new NetflixCheckDialogHyphenSpace(),
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
                new NetflixCheckBridgeGaps(),
                new NetflixCheckWhiteSpace(),
                new NetflixCheckItalics(),
                new NetflixCheckShotChange()
            };
        }

        public void RunChecks(Subtitle subtitle)
        {
            RunChecks(subtitle, GetAllCheckers());
        }

        public void RunChecks(Subtitle subtitle, List<INetflixQualityChecker> checks)
        {
            Records = new List<Record>();
            foreach (var checker in checks)
            {
                checker.Check(subtitle, this);
            }

            Records = Records
                .Where(p => p != null)
                .OrderBy(p => p.OriginalParagraph?.Number ?? 0)
                .ToList();
        }
    }
}
