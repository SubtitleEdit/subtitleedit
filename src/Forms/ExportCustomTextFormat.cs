using Nikse.SubtitleEdit.Core;
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
            var l = Configuration.Settings.Language.ExportCustomTextFormat;
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
                    textBoxParagraph.Text = arr[2];
                    comboBoxTimeCode.Text = arr[3];
                    comboBoxNewLine.Text = arr[4].Replace(EnglishDoNotModify, l.DoNotModify);
                    textBoxFooter.Text = arr[5];
                }
            }
            GeneratePreview();

            Text = l.Title;
            labelName.Text = Configuration.Settings.Language.General.Name;
            groupBoxTemplate.Text = l.Template;
            labelTimeCode.Text = l.TimeCode;
            labelNewLine.Text = l.NewLine;
            labelHeader.Text = l.Header;
            labelTextLine.Text = l.TextLine;
            labelFooter.Text = l.Footer;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            groupBoxPreview.Text = Configuration.Settings.Language.General.Preview;
        }

        private void ExportCustomTextFormatKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
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
                var newLine = comboBoxNewLine.Text.Replace(Configuration.Settings.Language.ExportCustomTextFormat.DoNotModify, EnglishDoNotModify);
                var template = GetParagraphTemplate(textBoxParagraph.Text);
                textBoxPreview.Text = GetHeaderOrFooter("Demo", subtitle, textBoxHeader.Text) +
                                      GetParagraph(template, start1, end1, GetText(p1.Text, newLine), GetText("Line 1a." + Environment.NewLine + "Line 1b.", newLine), 0, p1.Actor, p1.Duration, comboBoxTimeCode.Text) +
                                      GetParagraph(template, start2, end2, GetText(p2.Text, newLine), GetText("Line 2a." + Environment.NewLine + "Line 2b.", newLine), 1, p2.Actor, p2.Duration, comboBoxTimeCode.Text) +
                                      GetHeaderOrFooter("Demo", subtitle, textBoxFooter.Text);
            }
            catch (Exception ex)
            {
                textBoxPreview.Text = ex.Message;
            }
        }

        public static string GetParagraphTemplate(string template)
        {
            template = template.Replace("{start}", "{0}");
            template = template.Replace("{end}", "{1}");
            template = template.Replace("{text}", "{2}");
            template = template.Replace("{translation}", "{3}");
            template = template.Replace("{number}", "{4}");
            template = template.Replace("{number:", "{4:");
            template = template.Replace("{number-1}", "{5}");
            template = template.Replace("{number-1:", "{5:");
            template = template.Replace("{duration}", "{6}");
            template = template.Replace("{actor}", "{7}");
            template = template.Replace("{text-line-1}", "{8}");
            template = template.Replace("{text-line-2}", "{9}");
            template = template.Replace("{actor}", "{7}");
            template = template.Replace("{tab}", "\t");
            return template;
        }

        public static string GetText(string text, string newLine)
        {
            if (!string.IsNullOrEmpty(newLine) && newLine != EnglishDoNotModify)
            {
                newLine = newLine.Replace("{newline}", Environment.NewLine);
                newLine = newLine.Replace("{tab}", "\t");
                newLine = newLine.Replace("{lf}", "\n");
                newLine = newLine.Replace("{cr}", "\r");
                text = text.Replace(Environment.NewLine, newLine);
            }
            return text;
        }

        public static string GetTimeCode(TimeCode timeCode, string template)
        {
            var templateTrimmed = template.Trim();
            if (templateTrimmed == "ss")
                template = template.Replace("ss", $"{timeCode.TotalSeconds:00}");
            if (templateTrimmed == "s")
                template = template.Replace("s", $"{timeCode.TotalSeconds}");
            if (templateTrimmed == "zzz")
                template = template.Replace("zzz", $"{timeCode.TotalMilliseconds:000}");
            if (templateTrimmed == "z")
                template = template.Replace("z", $"{timeCode.TotalMilliseconds}");
            if (templateTrimmed == "ff")
                template = template.Replace("ff", $"{SubtitleFormat.MillisecondsToFrames(timeCode.TotalMilliseconds)}");

            if (template.StartsWith("ssssssss", StringComparison.Ordinal))
                template = template.Replace("ssssssss", $"{timeCode.TotalSeconds:00000000}");
            if (template.StartsWith("sssssss", StringComparison.Ordinal))
                template = template.Replace("sssssss", $"{timeCode.TotalSeconds:0000000}");
            if (template.StartsWith("ssssss", StringComparison.Ordinal))
                template = template.Replace("ssssss", $"{timeCode.TotalSeconds:000000}");
            if (template.StartsWith("sssss", StringComparison.Ordinal))
                template = template.Replace("sssss", $"{timeCode.TotalSeconds:00000}");
            if (template.StartsWith("ssss", StringComparison.Ordinal))
                template = template.Replace("ssss", $"{timeCode.TotalSeconds:0000}");
            if (template.StartsWith("sss", StringComparison.Ordinal))
                template = template.Replace("sss", $"{timeCode.TotalSeconds:000}");
            if (template.StartsWith("ss", StringComparison.Ordinal))
                template = template.Replace("ss", $"{timeCode.TotalSeconds:00}");

            if (template.StartsWith("zzzzzzzz", StringComparison.Ordinal))
                template = template.Replace("zzzzzzzz", $"{timeCode.TotalMilliseconds:00000000}");
            if (template.StartsWith("zzzzzzz", StringComparison.Ordinal))
                template = template.Replace("zzzzzzz", $"{timeCode.TotalMilliseconds:0000000}");
            if (template.StartsWith("zzzzzz", StringComparison.Ordinal))
                template = template.Replace("zzzzzz", $"{timeCode.TotalMilliseconds:000000}");
            if (template.StartsWith("zzzzz", StringComparison.Ordinal))
                template = template.Replace("zzzzz", $"{timeCode.TotalMilliseconds:00000}");
            if (template.StartsWith("zzzz", StringComparison.Ordinal))
                template = template.Replace("zzzz", $"{timeCode.TotalMilliseconds:0000}");
            if (template.StartsWith("zzz", StringComparison.Ordinal))
                template = template.Replace("zzz", $"{timeCode.TotalMilliseconds:000}");

            template = template.Replace("hh", $"{timeCode.Hours:00}");
            template = template.Replace("h", $"{timeCode.Hours}");
            template = template.Replace("mm", $"{timeCode.Minutes:00}");
            template = template.Replace("m", $"{timeCode.Minutes}");
            template = template.Replace("ss", $"{timeCode.Seconds:00}");
            template = template.Replace("s", $"{timeCode.Seconds}");
            template = template.Replace("zzz", $"{timeCode.Milliseconds:000}");
            template = template.Replace("zz", $"{Math.Round(timeCode.Milliseconds / 10.0):00}");
            template = template.Replace("z", $"{Math.Round(timeCode.Milliseconds / 100.0):0}");
            template = template.Replace("ff", $"{SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}");
            template = template.Replace("f", $"{SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds)}");
            return template;
        }

        private void InsertTag(object sender, EventArgs e)
        {
            var item = sender as ToolStripItem;
            if (item != null)
            {
                string s = item.Text;
                textBoxParagraph.Text = textBoxParagraph.Text.Insert(textBoxParagraph.SelectionStart, s);
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
            FormatOk = textBoxName.Text + "Æ" + textBoxHeader.Text + "Æ" + textBoxParagraph.Text + "Æ" + comboBoxTimeCode.Text + "Æ" + comboBoxNewLine.Text.Replace(Configuration.Settings.Language.ExportCustomTextFormat.DoNotModify, EnglishDoNotModify) + "Æ" + textBoxFooter.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void InsertTagHeader(object sender, EventArgs e)
        {
            var item = sender as ToolStripItem;
            if (item != null)
            {
                string s = item.Text;
                textBoxHeader.Text = textBoxHeader.Text.Insert(textBoxHeader.SelectionStart, s);
            }
        }

        private void InsertTagFooter(object sender, EventArgs e)
        {
            var item = sender as ToolStripItem;
            if (item != null)
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

        internal static string GetParagraph(string template, string start, string end, string text, string translation, int number, string actor, TimeCode duration, string timeCodeTemplate)
        {
            string d = duration.ToString();
            if (timeCodeTemplate == "ff" || timeCodeTemplate == "f")
                d = SubtitleFormat.MillisecondsToFrames(duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
            if (timeCodeTemplate == "zzz" || timeCodeTemplate == "zz" || timeCodeTemplate == "z")
                d = duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            if (timeCodeTemplate == "sss" || timeCodeTemplate == "ss" || timeCodeTemplate == "s")
                d = duration.Seconds.ToString(CultureInfo.InvariantCulture);
            else if (timeCodeTemplate.EndsWith("ss.ff", StringComparison.Ordinal))
                d = $"{duration.Seconds:00}.{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            else if (timeCodeTemplate.EndsWith("ss:ff", StringComparison.Ordinal))
                d = $"{duration.Seconds:00}:{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            else if (timeCodeTemplate.EndsWith("ss,ff", StringComparison.Ordinal))
                d = $"{duration.Seconds:00},{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            else if (timeCodeTemplate.EndsWith("ss;ff", StringComparison.Ordinal))
                d = $"{duration.Seconds:00};{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            else if (timeCodeTemplate.EndsWith("ss.zzz", StringComparison.Ordinal))
                d = $"{duration.Seconds:00}.{duration.Milliseconds:000}";
            else if (timeCodeTemplate.EndsWith("ss:zzz", StringComparison.Ordinal))
                d = $"{duration.Seconds:00}:{duration.Milliseconds:000}";
            else if (timeCodeTemplate.EndsWith("ss,zzz", StringComparison.Ordinal))
                d = $"{duration.Seconds:00},{duration.Milliseconds:000}";
            else if (timeCodeTemplate.EndsWith("ss;zzz", StringComparison.Ordinal))
                d = $"{duration.Seconds:00};{duration.Milliseconds:000}";
            else if (timeCodeTemplate.EndsWith("ss.zz", StringComparison.Ordinal))
                d = $"{duration.Seconds:00}.{Math.Round(duration.Milliseconds / 10.0):00}";
            else if (timeCodeTemplate.EndsWith("ss:zz", StringComparison.Ordinal))
                d = $"{duration.Seconds:00}:{Math.Round(duration.Milliseconds / 10.0):00}";
            else if (timeCodeTemplate.EndsWith("ss,zz", StringComparison.Ordinal))
                d = $"{duration.Seconds:00},{Math.Round(duration.Milliseconds / 10.0):00}";
            else if (timeCodeTemplate.EndsWith("ss;zz", StringComparison.Ordinal))
                d = $"{duration.Seconds:00};{Math.Round(duration.Milliseconds / 10.0):00}";

            var lines = text.SplitToLines();
            var line1 = string.Empty;
            var line2 = string.Empty;
            if (lines.Count > 0)
                line1 = lines[0];
            if (lines.Count > 1)
                line2 = lines[1];

            string s = template;
            s = s.Replace("{{", "@@@@_@@@{");
            s = s.Replace("}}", "}@@@_@@@@");
            s = string.Format(s, start, end, text, translation, number + 1, number, d, actor, line1, line2);
            s = s.Replace("@@@@_@@@", "{");
            s = s.Replace("@@@_@@@@", "}");
            return s;
        }
    }
}
