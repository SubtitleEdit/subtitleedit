using System;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ExportCustomTextFormat : Form
    {

        public string FormatOk { get; set; }
        public const string EnglishDoNoModify = "[Do not modify]";

        public ExportCustomTextFormat(string format)
        {
            InitializeComponent();
            var l = Configuration.Settings.Language.ExportCustomTextFormat;
            comboBoxNewLine.Items.Clear();
            comboBoxNewLine.Items.Add(l.DoNotModify);
            comboBoxNewLine.Items.Add("||");
            comboBoxNewLine.Items.Add(" ");

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
                    comboBoxNewLine.Text = arr[4].Replace(EnglishDoNoModify, l.DoNotModify);
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
            var p1 = new Paragraph("Line 1a." + Environment.NewLine + "Line 1b.", 1000, 3500);
            string start1 = GetTimeCode(p1.StartTime, comboBoxTimeCode.Text);
            string end1 = GetTimeCode(p1.EndTime, comboBoxTimeCode.Text);
            var p2 = new Paragraph("Line 2a." + Environment.NewLine + "Line 2b.", 1000, 3500);
            string start2 = GetTimeCode(p2.StartTime, comboBoxTimeCode.Text);
            string end2 = GetTimeCode(p2.EndTime, comboBoxTimeCode.Text);
            subtitle.Paragraphs.Add(p1);
            subtitle.Paragraphs.Add(p2);
            try
            {
                string newLine = comboBoxNewLine.Text.Replace(Configuration.Settings.Language.ExportCustomTextFormat.DoNotModify, EnglishDoNoModify);
                string template = GetParagraphTemplate(textBoxParagraph.Text);
                textBoxPreview.Text = GetHeaderOrFooter("Demo", subtitle, textBoxHeader.Text) +
                                      GetParagraph(template, start1, end1, GetText(p1.Text, newLine), GetText("Linje 1a." + Environment.NewLine + "Line 1b.", newLine), 0, p1.Duration, comboBoxTimeCode.Text) +
                                      GetParagraph(template, start2, end2, GetText(p2.Text, newLine), GetText("Linje 2a." + Environment.NewLine + "Line 2b.", newLine), 1, p2.Duration, comboBoxTimeCode.Text) +
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
            template = template.Replace("{tab}", "\t");
            return template;
        }

        public static string GetText(string text, string newLine)
        {
            string template = newLine;
            if (string.IsNullOrEmpty(newLine) || template == "[Do not modify]" || template == Configuration.Settings.Language.ExportCustomTextFormat.DoNotModify)
                return text;
            if (template == "[Only newline (hex char 0xd)]")
                return text.Replace(Environment.NewLine, "\n");
            return text.Replace(Environment.NewLine, template);
        }

        public static string GetTimeCode(TimeCode timeCode, string template)
        {
            if (template.Trim() == "ss")
                template = template.Replace("ss", string.Format("{0:00}", timeCode.TotalSeconds));
            if (template.Trim() == "s")
                template = template.Replace("s", string.Format("{0}", timeCode.TotalSeconds));
            if (template.Trim() == "zzz")
                template = template.Replace("zzz", string.Format("{0:000}", timeCode.TotalMilliseconds));
            if (template.Trim() == "z")
                template = template.Replace("z", string.Format("{0}", timeCode.TotalMilliseconds));
            if (template.Trim() == "ff")
                template = template.Replace("ff", string.Format("{0}", SubtitleFormat.MillisecondsToFrames(timeCode.TotalMilliseconds)));

            if (template.StartsWith("ssssssss"))
                template = template.Replace("ssssssss", string.Format("{0:00000000}", timeCode.TotalSeconds));
            if (template.StartsWith("sssssss"))
                template = template.Replace("sssssss", string.Format("{0:0000000}", timeCode.TotalSeconds));
            if (template.StartsWith("ssssss"))
                template = template.Replace("ssssss", string.Format("{0:000000}", timeCode.TotalSeconds));
            if (template.StartsWith("sssss"))
                template = template.Replace("sssss", string.Format("{0:00000}", timeCode.TotalSeconds));
            if (template.StartsWith("ssss"))
                template = template.Replace("ssss", string.Format("{0:0000}", timeCode.TotalSeconds));
            if (template.StartsWith("sss"))
                template = template.Replace("sss", string.Format("{0:000}", timeCode.TotalSeconds));
            if (template.StartsWith("ss"))
                template = template.Replace("ss", string.Format("{0:00}", timeCode.TotalSeconds));

            if (template.StartsWith("zzzzzzzz"))
                template = template.Replace("zzzzzzzz", string.Format("{0:00000000}", timeCode.TotalMilliseconds));
            if (template.StartsWith("zzzzzzz"))
                template = template.Replace("zzzzzzz", string.Format("{0:0000000}", timeCode.TotalMilliseconds));
            if (template.StartsWith("zzzzzz"))
                template = template.Replace("zzzzzz", string.Format("{0:000000}", timeCode.TotalMilliseconds));
            if (template.StartsWith("zzzzz"))
                template = template.Replace("zzzzz", string.Format("{0:00000}", timeCode.TotalMilliseconds));
            if (template.StartsWith("zzzz"))
                template = template.Replace("zzzz", string.Format("{0:0000}", timeCode.TotalMilliseconds));
            if (template.StartsWith("zzz"))
                template = template.Replace("zzz", string.Format("{0:000}", timeCode.TotalMilliseconds));

            template = template.Replace("hh", string.Format("{0:00}", timeCode.Hours));
            template = template.Replace("h", string.Format("{0}", timeCode.Hours));
            template = template.Replace("mm", string.Format("{0:00}", timeCode.Minutes));
            template = template.Replace("m", string.Format("{0}", timeCode.Minutes));
            template = template.Replace("ss", string.Format("{0:00}", timeCode.Seconds));
            template = template.Replace("s", string.Format("{0}", timeCode.Seconds));
            template = template.Replace("zzz", string.Format("{0:000}", timeCode.Milliseconds));
            template = template.Replace("zz", string.Format("{0:00}", Math.Round(timeCode.Milliseconds / 10.0)));
            template = template.Replace("z", string.Format("{0:0}", Math.Round(timeCode.Milliseconds / 100.0)));
            template = template.Replace("ff", string.Format("{0:00}", SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds)));
            template = template.Replace("f", string.Format("{0}", SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds)));
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
            FormatOk = textBoxName.Text + "Æ" + textBoxHeader.Text + "Æ" + textBoxParagraph.Text + "Æ" + comboBoxTimeCode.Text + "Æ" + comboBoxNewLine.Text.Replace(Configuration.Settings.Language.ExportCustomTextFormat.DoNotModify, EnglishDoNoModify) + "Æ" + textBoxFooter.Text;
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

        internal static string GetParagraph(string template, string start, string end, string text, string translation, int number, TimeCode duration, string timeCodeTemplate)
        {
            string d = duration.ToString();
            if (timeCodeTemplate == "ff" || timeCodeTemplate == "f")
                d = SubtitleFormat.MillisecondsToFrames(duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
            if (timeCodeTemplate == "zzz" || timeCodeTemplate == "zz" || timeCodeTemplate == "z")
                d = duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            if (timeCodeTemplate == "sss" || timeCodeTemplate == "ss" || timeCodeTemplate == "s")
                d = duration.Seconds.ToString(CultureInfo.InvariantCulture);
            else if (timeCodeTemplate.EndsWith("ss.ff", StringComparison.Ordinal))
                d = string.Format("{0:00}.{1:00}", duration.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds));
            else if (timeCodeTemplate.EndsWith("ss:ff", StringComparison.Ordinal))
                d = string.Format("{0:00}:{1:00}", duration.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds));
            else if (timeCodeTemplate.EndsWith("ss,ff", StringComparison.Ordinal))
                d = string.Format("{0:00},{1:00}", duration.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds));
            else if (timeCodeTemplate.EndsWith("ss;ff", StringComparison.Ordinal))
                d = string.Format("{0:00};{1:00}", duration.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds));
            else if (timeCodeTemplate.EndsWith("ss;ff", StringComparison.Ordinal))
                d = string.Format("{0:00};{1:00}", duration.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds));
            else if (timeCodeTemplate.EndsWith("ss.zzz", StringComparison.Ordinal))
                d = string.Format("{0:00}.{1:000}", duration.Seconds, duration.Milliseconds);
            else if (timeCodeTemplate.EndsWith("ss:zzz", StringComparison.Ordinal))
                d = string.Format("{0:00}:{1:000}", duration.Seconds, duration.Milliseconds);
            else if (timeCodeTemplate.EndsWith("ss,zzz", StringComparison.Ordinal))
                d = string.Format("{0:00},{1:000}", duration.Seconds, duration.Milliseconds);
            else if (timeCodeTemplate.EndsWith("ss;zzz", StringComparison.Ordinal))
                d = string.Format("{0:00};{1:000}", duration.Seconds, duration.Milliseconds);
            else if (timeCodeTemplate.EndsWith("ss;zzz", StringComparison.Ordinal))
                d = string.Format("{0:00};{1:000}", duration.Seconds, duration.Milliseconds);
            else if (timeCodeTemplate.EndsWith("ss.zz", StringComparison.Ordinal))
                d = string.Format("{0:00}.{1:00}", duration.Seconds, Math.Round(duration.Milliseconds / 10.0));
            else if (timeCodeTemplate.EndsWith("ss:zz", StringComparison.Ordinal))
                d = string.Format("{0:00}:{1:00}", duration.Seconds, Math.Round(duration.Milliseconds / 10.0));
            else if (timeCodeTemplate.EndsWith("ss,zz", StringComparison.Ordinal))
                d = string.Format("{0:00},{1:00}", duration.Seconds, Math.Round(duration.Milliseconds / 10.0));
            else if (timeCodeTemplate.EndsWith("ss;zz", StringComparison.Ordinal))
                d = string.Format("{0:00};{1:00}", duration.Seconds, Math.Round(duration.Milliseconds / 10.0));

            string s = template;
            s = s.Replace("{{", "@@@@_@@@{");
            s = s.Replace("}}", "}@@@_@@@@");
            s = string.Format(s, start, end, text, translation, number + 1, number, d);
            s = s.Replace("@@@@_@@@", "{");
            s = s.Replace("@@@_@@@@", "}");
            return s;
        }
    }
}