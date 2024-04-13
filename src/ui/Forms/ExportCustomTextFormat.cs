using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportCustomTextFormat : Form
    {
        public const string EnglishDoNotModify = "[Do not modify]";
        public string FormatOk { get; set; }
        private static readonly Regex CurlyCodePattern = new Regex("{\\d+}", RegexOptions.Compiled);

        public ExportCustomTextFormat(string format)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            var l = LanguageSettings.Current.ExportCustomTextFormat;
            comboBoxNewLine.Items.Clear();
            comboBoxNewLine.Items.Add(l.DoNotModify);
            comboBoxNewLine.Items.Add("||");
            comboBoxNewLine.Items.Add(" ");
            comboBoxNewLine.Items.Add("{newline}");
            comboBoxNewLine.Items.Add("{tab}");
            comboBoxNewLine.Items.Add("{lf}");
            comboBoxNewLine.Items.Add("{cr}");

            comboBoxTimeCode.Text = "hh:mm:ss,zzz";
            textBoxFileExtension.Text = "txt";
            if (!string.IsNullOrEmpty(format))
            {
                var arr = format.Split('Æ');
                if (arr.Length >= 6)
                {
                    textBoxName.Text = arr[0];
                    textBoxHeader.Text = arr[1];
                    if (!comboBoxTimeCode.Items.Contains(arr[3]))
                    {
                        comboBoxTimeCode.Items.Add(arr[3]);
                    }

                    comboBoxTimeCode.Text = arr[3];
                    textBoxParagraph.Text = arr[2];
                    comboBoxNewLine.Text = arr[4].Replace(EnglishDoNotModify, l.DoNotModify);
                    textBoxFooter.Text = arr[5];

                    if (arr.Length >= 7)
                    {
                        textBoxFileExtension.Text = arr[6];
                    }
                }
            }
            GeneratePreview();

            Text = l.Title;
            labelName.Text = LanguageSettings.Current.General.Name;
            groupBoxTemplate.Text = l.Template;
            labelTimeCode.Text = l.TimeCode;
            labelNewLine.Text = l.NewLine;
            labelHeader.Text = l.Header;
            labelTextLine.Text = l.TextLine;
            labelFooter.Text = l.Footer;
            labelFileExt.Text = l.FileExtension;
            textBoxFileExtension.Left = labelFileExt.Right + 5;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;
        }

        private void ExportCustomTextFormatKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void TextBoxParagraphTextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void GeneratePreview()
        {
            var subtitle = new Subtitle();
            var p1 = new Paragraph("Line 1a." + Environment.NewLine + "Line 1b.", 1000, 3500) { Actor = "Joe" };
            var start1 = GetTimeCode(p1.StartTime, comboBoxTimeCode.Text);
            var end1 = GetTimeCode(p1.EndTime, comboBoxTimeCode.Text);
            var p2 = new Paragraph("Line 2a." + Environment.NewLine + "Line 2b.", 4000, 5500) { Actor = "Smith" };
            var start2 = GetTimeCode(p2.StartTime, comboBoxTimeCode.Text);
            var end2 = GetTimeCode(p2.EndTime, comboBoxTimeCode.Text);
            subtitle.Paragraphs.Add(p1);
            subtitle.Paragraphs.Add(p2);
            var gap1 = GetTimeCode(new TimeCode(p2.StartTime.TotalMilliseconds - p1.EndTime.TotalMilliseconds), comboBoxTimeCode.Text);
            var gap2 = string.Empty;
            try
            {
                var newLine = comboBoxNewLine.Text.Replace(LanguageSettings.Current.ExportCustomTextFormat.DoNotModify, EnglishDoNotModify);
                var template = GetParagraphTemplate(textBoxParagraph.Text);
                var videoFileName = "C:\\Video\\MyVideo.mp4";
                textBoxPreview.Text = GetHeaderOrFooter("Demo", videoFileName, subtitle, textBoxHeader.Text) +
                                      GetParagraph(template, start1, end1, GetText(p1.Text, newLine), GetText("Line 1a." + Environment.NewLine + "Line 1b.", newLine), 0, p1.Actor, p1.Duration, gap1, comboBoxTimeCode.Text, p1, videoFileName) +
                                      GetParagraph(template, start2, end2, GetText(p2.Text, newLine), GetText("Line 2a." + Environment.NewLine + "Line 2b.", newLine), 1, p2.Actor, p2.Duration, gap2, comboBoxTimeCode.Text, p2, videoFileName) +
                                      GetHeaderOrFooter("Demo", videoFileName, subtitle, textBoxFooter.Text);
            }
            catch (Exception ex)
            {
                textBoxPreview.Text = ex.Message;
            }
        }

        public static string GetParagraphTemplate(string template)
        {
            var s = template.Replace("{start}", "{0}");
            s = s.Replace("{end}", "{1}");
            s = s.Replace("{text}", "{2}");
            s = s.Replace("{original-text}", "{3}");
            s = s.Replace("{number}", "{4}");
            s = s.Replace("{number:", "{4:");
            s = s.Replace("{number-1}", "{5}");
            s = s.Replace("{number-1:", "{5:");
            s = s.Replace("{duration}", "{6}");
            s = s.Replace("{actor}", "{7}");
            s = s.Replace("{actor-colon-space}", "{21}");
            s = s.Replace("{actor-upper-brackets-space}", "{22}");
            s = s.Replace("{text-line-1}", "{8}");
            s = s.Replace("{text-line-2}", "{9}");
            s = s.Replace("{cps-comma}", "{10}");
            s = s.Replace("{cps-period}", "{11}");
            s = s.Replace("{text-length}", "{12}");
            s = s.Replace("{text-length-br0}", "{13}");
            s = s.Replace("{text-length-br1}", "{14}");
            s = s.Replace("{text-length-br2}", "{15}");
            s = s.Replace("{gap}", "{16}");
            s = s.Replace("{bookmark}", "{17}");
            s = s.Replace("{media-file-name}", "{18}");
            s = s.Replace("{media-file-name-full}", "{19}");
            s = s.Replace("{media-file-name-with-ext}", "{20}");
            s = s.Replace("{tab}", "\t");
            return s;
        }

        public static string GetText(string text, string newLine)
        {
            if (!string.IsNullOrEmpty(newLine) && newLine != EnglishDoNotModify)
            {
                newLine = newLine.Replace("{newline}", Environment.NewLine);
                newLine = newLine.Replace("{tab}", "\t");
                newLine = newLine.Replace("{lf}", "\n");
                newLine = newLine.Replace("{cr}", "\r");
                return text.Replace(Environment.NewLine, newLine);
            }
            return text;
        }

        public static string GetTimeCode(TimeCode timeCode, string template)
        {
            var t = template;
            var templateTrimmed = t.Trim();
            if (templateTrimmed == "ss")
            {
                t = t.Replace("ss", $"{timeCode.TotalSeconds:00}");
            }

            if (templateTrimmed == "s")
            {
                t = t.Replace("s", $"{(long)Math.Round(timeCode.TotalSeconds, MidpointRounding.AwayFromZero)}");
            }

            if (templateTrimmed == "zzz")
            {
                t = t.Replace("zzz", $"{timeCode.TotalMilliseconds:000}");
            }

            if (templateTrimmed == "z")
            {
                t = t.Replace("z", $"{(long)Math.Round(timeCode.TotalMilliseconds, MidpointRounding.AwayFromZero)}");
            }

            if (templateTrimmed == "ff")
            {
                t = t.Replace("ff", $"{SubtitleFormat.MillisecondsToFrames(timeCode.TotalMilliseconds)}");
            }

            var totalSeconds = (int)Math.Round(timeCode.TotalSeconds, MidpointRounding.AwayFromZero);
            if (t.StartsWith("ssssssss", StringComparison.Ordinal))
            {
                t = t.Replace("ssssssss", $"{totalSeconds:00000000}");
            }

            if (t.StartsWith("sssssss", StringComparison.Ordinal))
            {
                t = t.Replace("sssssss", $"{totalSeconds:0000000}");
            }

            if (t.StartsWith("ssssss", StringComparison.Ordinal))
            {
                t = t.Replace("ssssss", $"{totalSeconds:000000}");
            }

            if (t.StartsWith("sssss", StringComparison.Ordinal))
            {
                t = t.Replace("sssss", $"{totalSeconds:00000}");
            }

            if (t.StartsWith("ssss", StringComparison.Ordinal))
            {
                t = t.Replace("ssss", $"{totalSeconds:0000}");
            }

            if (t.StartsWith("sss", StringComparison.Ordinal))
            {
                t = t.Replace("sss", $"{totalSeconds:000}");
            }

            if (t.StartsWith("ss", StringComparison.Ordinal))
            {
                t = t.Replace("ss", $"{totalSeconds:00}");
            }

            var totalMilliseconds = (long)Math.Round(timeCode.TotalMilliseconds, MidpointRounding.AwayFromZero);
            if (t.StartsWith("zzzzzzzz", StringComparison.Ordinal))
            {
                t = t.Replace("zzzzzzzz", $"{totalMilliseconds:00000000}");
            }

            if (t.StartsWith("zzzzzzz", StringComparison.Ordinal))
            {
                t = t.Replace("zzzzzzz", $"{totalMilliseconds:0000000}");
            }

            if (t.StartsWith("zzzzzz", StringComparison.Ordinal))
            {
                t = t.Replace("zzzzzz", $"{totalMilliseconds:000000}");
            }

            if (t.StartsWith("zzzzz", StringComparison.Ordinal))
            {
                t = t.Replace("zzzzz", $"{totalMilliseconds:00000}");
            }

            if (t.StartsWith("zzzz", StringComparison.Ordinal))
            {
                t = t.Replace("zzzz", $"{totalMilliseconds:0000}");
            }

            if (t.StartsWith("zzz", StringComparison.Ordinal))
            {
                t = t.Replace("zzz", $"{totalMilliseconds:000}");
            }

            t = t.Replace("hh", $"{timeCode.Hours:00}");
            t = t.Replace("h", $"{timeCode.Hours}");
            t = t.Replace("mm", $"{timeCode.Minutes:00}");
            t = t.Replace("m", $"{timeCode.Minutes}");
            t = t.Replace("ss", $"{timeCode.Seconds:00}");
            t = t.Replace("s", $"{timeCode.Seconds}");
            t = t.Replace("zzz", $"{timeCode.Milliseconds:000}");
            t = t.Replace("zz", $"{Math.Round(timeCode.Milliseconds / 10.0):00}");
            t = t.Replace("z", $"{Math.Round(timeCode.Milliseconds / 100.0):0}");
            t = t.Replace("ff", $"{SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}");
            t = t.Replace("f", $"{SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds)}");

            if (timeCode.TotalMilliseconds < 0)
            {
                return "-" + t.RemoveChar('-');
            }

            return t;
        }

        private void InsertTag(object sender, EventArgs e)
        {
            if (sender is ToolStripItem item)
            {
                textBoxParagraph.Text = textBoxParagraph.Text.Insert(textBoxParagraph.SelectionStart, item.Text);
            }
        }

        private void comboBoxTimeCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void comboBoxNewLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void comboBoxTimeCode_TextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void comboBoxNewLine_TextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            FormatOk = textBoxName.Text + "Æ" + textBoxHeader.Text + "Æ" + textBoxParagraph.Text + "Æ" + comboBoxTimeCode.Text + "Æ" + comboBoxNewLine.Text.Replace(LanguageSettings.Current.ExportCustomTextFormat.DoNotModify, EnglishDoNotModify) + "Æ" + textBoxFooter.Text + "Æ" + textBoxFileExtension.Text.Trim('.', ' ');
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void InsertTagHeader(object sender, EventArgs e)
        {
            if (sender is ToolStripItem item)
            {
                string s = item.Text;
                textBoxHeader.Text = textBoxHeader.Text.Insert(textBoxHeader.SelectionStart, s);
            }
        }

        private void InsertTagFooter(object sender, EventArgs e)
        {
            if (sender is ToolStripItem item)
            {
                string s = item.Text;
                textBoxFooter.Text = textBoxFooter.Text.Insert(textBoxFooter.SelectionStart, s);
            }
        }

        public static string GetHeaderOrFooter(string title, string videoFileName, Subtitle subtitle, string template)
        {
            template = template.Replace("{title}", title);
            template = template.Replace("{media-file-name-full}", videoFileName);
            template = template.Replace("{media-file-name}", string.IsNullOrEmpty(videoFileName) ? videoFileName : Path.GetFileNameWithoutExtension(videoFileName));
            template = template.Replace("{media-file-name-with-ext}", string.IsNullOrEmpty(videoFileName) ? videoFileName : Path.GetFileName(videoFileName));
            template = template.Replace("{#lines}", subtitle.Paragraphs.Count.ToString(CultureInfo.InvariantCulture));

            template = template.Replace("{tab}", "\t");
            return template;
        }

        internal static string GetParagraph(string template, string start, string end, string text, string originalText, int number, string actor, TimeCode duration, string gap, string timeCodeTemplate, Paragraph p, string videoFileName)
        {
            var cps = Utilities.GetCharactersPerSecond(p);
            var d = duration.ToString();
            if (timeCodeTemplate == "ff" || timeCodeTemplate == "f")
            {
                d = SubtitleFormat.MillisecondsToFrames(duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
            }

            if (timeCodeTemplate == "zzz" || timeCodeTemplate == "zz" || timeCodeTemplate == "z")
            {
                d = ((long)Math.Round(duration.TotalMilliseconds, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
            }

            if (timeCodeTemplate == "sss" || timeCodeTemplate == "ss" || timeCodeTemplate == "s")
            {
                d = duration.Seconds.ToString(CultureInfo.InvariantCulture);
            }
            else if (timeCodeTemplate.EndsWith("ss.ff", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00}.{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            }
            else if (timeCodeTemplate.EndsWith("ss:ff", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00}:{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            }
            else if (timeCodeTemplate.EndsWith("ss,ff", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00},{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            }
            else if (timeCodeTemplate.EndsWith("ss;ff", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00};{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            }
            else if (timeCodeTemplate.EndsWith("ss.zzz", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00}.{duration.Milliseconds:000}";
            }
            else if (timeCodeTemplate.EndsWith("ss:zzz", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00}:{duration.Milliseconds:000}";
            }
            else if (timeCodeTemplate.EndsWith("ss,zzz", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00},{duration.Milliseconds:000}";
            }
            else if (timeCodeTemplate.EndsWith("ss;zzz", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00};{duration.Milliseconds:000}";
            }
            else if (timeCodeTemplate.EndsWith("ss.zz", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00}.{Math.Round(duration.Milliseconds / 10.0):00}";
            }
            else if (timeCodeTemplate.EndsWith("ss:zz", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00}:{Math.Round(duration.Milliseconds / 10.0):00}";
            }
            else if (timeCodeTemplate.EndsWith("ss,zz", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00},{Math.Round(duration.Milliseconds / 10.0):00}";
            }
            else if (timeCodeTemplate.EndsWith("ss;zz", StringComparison.Ordinal))
            {
                d = $"{duration.Seconds:00};{Math.Round(duration.Milliseconds / 10.0):00}";
            }

            var lines = text.SplitToLines();
            var line1 = string.Empty;
            var line2 = string.Empty;
            if (lines.Count > 0)
            {
                line1 = lines[0];
            }

            if (lines.Count > 1)
            {
                line2 = lines[1];
            }

            var s = template;
            var replaceStart = GetReplaceChar(s);
            var replaceEnd = GetReplaceChar(s + replaceStart);
            var actorColonSpace = string.IsNullOrEmpty(actor) ? string.Empty : $"{actor}: ";
            var actorUppercaseBracketsSpace = string.IsNullOrEmpty(actor) ? string.Empty : $"[{actor.ToUpperInvariant()}] ";
            s = PreBeginCurly(s, replaceStart);
            s = PreEndCurly(s, replaceEnd);
            s = string.Format(s, start, end, text, originalText, number + 1, number, d, actor, line1, line2,
                              cps.ToString(CultureInfo.InvariantCulture).Replace(".", ","),
                              cps.ToString(CultureInfo.InvariantCulture),
                              text.Length,
                              p.Text.RemoveChar('\r', '\n').Length,
                              p.Text.RemoveChar('\r', '\n').Length + lines.Count - 1,
                              p.Text.RemoveChar('\r', '\n').Length + (lines.Count - 1) * 2,
                              gap,
                              p.Bookmark == string.Empty ? "*" : p.Bookmark,
                              string.IsNullOrEmpty(videoFileName) ? string.Empty : Path.GetFileNameWithoutExtension(videoFileName),
                              videoFileName,
                              string.IsNullOrEmpty(videoFileName) ? string.Empty : Path.GetFileName(videoFileName),
                              actorColonSpace,
                              actorUppercaseBracketsSpace
                              );
            s = PostCurly(s, replaceStart, replaceEnd);
            return s;
        }

        private static string GetReplaceChar(string s)
        {
            var chars = new List<char> { '@', '¤', '%', '=', '+', 'æ', 'Æ', '`', '*', ';' };

            foreach (var c in chars)
            {
                if (!s.Contains(c))
                {
                    return c.ToString();
                }
            }

            return string.Empty;
        }

        private static string PreBeginCurly(string s, string replaceStart)
        {
            if (string.IsNullOrEmpty(replaceStart))
            {
                return s;
            }

            var indices = GetCurlyBeginIndexesReversed(s);
            for (var i = 0; i < indices.Count; i++)
            {
                var idx = indices[i];
                s = s.Remove(idx, 1);
                s = s.Insert(idx, replaceStart);
            }

            return s;
        }

        private static string PreEndCurly(string s, string replaceEnd)
        {
            if (string.IsNullOrEmpty(replaceEnd))
            {
                return s;
            }

            var indices = GetCurlyEndIndexesReversed(s);
            for (var i = 0; i < indices.Count; i++)
            {
                var idx = indices[i];
                s = s.Remove(idx, 1);
                s = s.Insert(idx, replaceEnd);
            }

            return s;
        }

        private static string PostCurly(string s, string replaceStart, string replaceEnd)
        {
            if (!string.IsNullOrEmpty(replaceStart))
            {
                s = s.Replace(replaceStart, "{");
            }

            if (!string.IsNullOrEmpty(replaceEnd))
            {
                s = s.Replace(replaceEnd, "}");
            }

            return s;
        }

        private static List<int> GetCurlyBeginIndexesReversed(string s)
        {
            var matchIndices = CurlyCodePattern.Matches(s)
                .Cast<Match>()
                .Select(m => m.Index)
                .ToList();
            var list = new List<int>();
            for (var i = s.Length - 1; i >= 0; i--)
            {
                var c = s[i];
                if (c == '{' && !matchIndices.Contains(i))
                {
                    list.Add(i);
                }
            }

            return list;
        }

        private static List<int> GetCurlyEndIndexesReversed(string s)
        {
            var matchIndices = CurlyCodePattern.Matches(s)
                .Cast<Match>()
                .Select(m => m.Index + m.Length - 1)
                .ToList();
            var list = new List<int>();
            for (var i = s.Length - 1; i >= 0; i--)
            {
                var c = s[i];
                if (c == '}' && !matchIndices.Contains(i))
                {
                    list.Add(i);
                }
            }

            return list;
        }
    }
}
