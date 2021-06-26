using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportCustomTextFormat : Form
    {
        public const string EnglishDoNotModify = "[Do not modify]";
        public string FormatOk { get; set; }

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
            if (!string.IsNullOrEmpty(format))
            {
                var arr = format.Split('Æ');
                if (arr.Length == 6)
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
            var p2 = new Paragraph("Line 2a." + Environment.NewLine + "Line 2b.", 1000, 3500) { Actor = "Smith" };
            var start2 = GetTimeCode(p2.StartTime, comboBoxTimeCode.Text);
            var end2 = GetTimeCode(p2.EndTime, comboBoxTimeCode.Text);
            subtitle.Paragraphs.Add(p1);
            subtitle.Paragraphs.Add(p2);
            try
            {
                var newLine = comboBoxNewLine.Text.Replace(LanguageSettings.Current.ExportCustomTextFormat.DoNotModify, EnglishDoNotModify);
                var template = GetParagraphTemplate(textBoxParagraph.Text);
                textBoxPreview.Text = GetHeaderOrFooter("Demo", subtitle, textBoxHeader.Text) +
                                      GetParagraph(template, start1, end1, GetText(p1.Text, newLine), GetText("Line 1a." + Environment.NewLine + "Line 1b.", newLine), 0, p1.Actor, p1.Duration, comboBoxTimeCode.Text, Utilities.GetCharactersPerSecond(p1)) +
                                      GetParagraph(template, start2, end2, GetText(p2.Text, newLine), GetText("Line 2a." + Environment.NewLine + "Line 2b.", newLine), 1, p2.Actor, p2.Duration, comboBoxTimeCode.Text, Utilities.GetCharactersPerSecond(p2)) +
                                      GetHeaderOrFooter("Demo", subtitle, textBoxFooter.Text);
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
            s = s.Replace("{translation}", "{3}");
            s = s.Replace("{number}", "{4}");
            s = s.Replace("{number:", "{4:");
            s = s.Replace("{number-1}", "{5}");
            s = s.Replace("{number-1:", "{5:");
            s = s.Replace("{duration}", "{6}");
            s = s.Replace("{actor}", "{7}");
            s = s.Replace("{text-line-1}", "{8}");
            s = s.Replace("{text-line-2}", "{9}");
            s = s.Replace("{cps-comma}", "{10}");
            s = s.Replace("{cps-period}", "{11}");
            s = s.Replace("{text-length}", "{12}");
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
                t = t.Replace("s", $"{timeCode.TotalSeconds}");
            }

            if (templateTrimmed == "zzz")
            {
                t = t.Replace("zzz", $"{timeCode.TotalMilliseconds:000}");
            }

            if (templateTrimmed == "z")
            {
                t = t.Replace("z", $"{timeCode.TotalMilliseconds}");
            }

            if (templateTrimmed == "ff")
            {
                t = t.Replace("ff", $"{SubtitleFormat.MillisecondsToFrames(timeCode.TotalMilliseconds)}");
            }

            var totalSeconds = (int)timeCode.TotalSeconds;
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

            var totalMilliseconds = (long)timeCode.TotalMilliseconds;
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
            FormatOk = textBoxName.Text + "Æ" + textBoxHeader.Text + "Æ" + textBoxParagraph.Text + "Æ" + comboBoxTimeCode.Text + "Æ" + comboBoxNewLine.Text.Replace(LanguageSettings.Current.ExportCustomTextFormat.DoNotModify, EnglishDoNotModify) + "Æ" + textBoxFooter.Text;
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

        public static string GetHeaderOrFooter(string title, Subtitle subtitle, string template)
        {
            template = template.Replace("{title}", title);
            template = template.Replace("{#lines}", subtitle.Paragraphs.Count.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("{tab}", "\t");
            return template;
        }

        internal static string GetParagraph(string template, string start, string end, string text, string translation, int number, string actor, TimeCode duration, string timeCodeTemplate, double cps)
        {
            string d = duration.ToString();
            if (timeCodeTemplate == "ff" || timeCodeTemplate == "f")
            {
                d = SubtitleFormat.MillisecondsToFrames(duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
            }

            if (timeCodeTemplate == "zzz" || timeCodeTemplate == "zz" || timeCodeTemplate == "z")
            {
                d = duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
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

            string s = template;
            s = s.Replace("{{", "@@@@_@@@{");
            s = s.Replace("}}", "}@@@_@@@@");
            s = string.Format(s, start, end, text, translation, number + 1, number, d, actor, line1, line2, cps.ToString(CultureInfo.InvariantCulture).Replace(".", ","), cps.ToString(CultureInfo.InvariantCulture), text.Length);
            s = s.Replace("@@@@_@@@", "{");
            s = s.Replace("@@@_@@@@", "}");
            return s;
        }
    }
}
