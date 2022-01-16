using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic
{
    public class SetRtbHtml
    {
        public static void SetText(RichTextBox rtb, string textOrHtml)
        {
            var alignLeft = textOrHtml.StartsWith("{\\a1}", StringComparison.Ordinal) || textOrHtml.StartsWith("{\\a5}", StringComparison.Ordinal) || textOrHtml.StartsWith("{\\a9}", StringComparison.Ordinal) || // sub station alpha
                            textOrHtml.StartsWith("{\\an1}", StringComparison.Ordinal) || textOrHtml.StartsWith("{\\an4}", StringComparison.Ordinal) || textOrHtml.StartsWith("{\\an7}", StringComparison.Ordinal); // advanced sub station alpha

            var alignRight = textOrHtml.StartsWith("{\\a3}", StringComparison.Ordinal) || textOrHtml.StartsWith("{\\a7}", StringComparison.Ordinal) || textOrHtml.StartsWith("{\\a11}", StringComparison.Ordinal) || // sub station alpha
                                  textOrHtml.StartsWith("{\\an3}", StringComparison.Ordinal) || textOrHtml.StartsWith("{\\an6}", StringComparison.Ordinal) || textOrHtml.StartsWith("{\\an9}", StringComparison.Ordinal); // advanced sub station alpha

            // remove styles for display text (except italic)
            var text = Utilities.RemoveSsaTags(textOrHtml);
            text = text.Replace("<b></b>", string.Empty);
            text = text.Replace("<i></i>", string.Empty);
            text = text.Replace("<u></u>", string.Empty);

            rtb.Text = string.Empty;
            var sb = new StringBuilder();
            var i = 0;
            var isItalic = false;
            var isBold = false;
            var isUnderline = false;
            var isFontColor = false;
            var fontColorBegin = 0;
            var letterCount = 0;
            var fontColorLookups = new Dictionary<Point, Color>();
            var styleLookups = new Dictionary<int, FontStyle>(text.Length);
            var defaultFontStyle = Configuration.Settings.General.VideoPlayerPreviewFontBold ? FontStyle.Bold : FontStyle.Regular;
            var inlineFontStyles = false;
            for (var j = 0; j < text.Length; j++)
            {
                styleLookups.Add(j, defaultFontStyle);
            }

            var fontColor = Color.White;
            var extraAdd = 0;
            while (i < text.Length)
            {
                if (text.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    rtb.AppendText(sb.ToString());
                    sb.Clear();
                    isItalic = true;
                    i += 2;
                }
                else if (isItalic && text.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                {
                    rtb.AppendText(sb.ToString());
                    sb.Clear();
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Configuration.Settings.General.VideoPlayerPreviewFontBold)
                    {
                        rtb.AppendText(sb.ToString());
                        sb.Clear();
                        isBold = true;
                    }
                    i += 2;
                }
                else if (isBold && text.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Configuration.Settings.General.VideoPlayerPreviewFontBold)
                    {
                        rtb.AppendText(sb.ToString());
                        sb.Clear();
                        isBold = false;
                    }
                    i += 3;
                }
                else if (Configuration.Settings.General.VideoPlayerPreviewFontBold && text.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                {
                    i += 3;
                }
                else if (text.Substring(i).StartsWith("<u>", StringComparison.OrdinalIgnoreCase))
                {
                    rtb.AppendText(sb.ToString());
                    sb.Clear();
                    isUnderline = true;
                    i += 2;
                }
                else if (isUnderline && text.Substring(i).StartsWith("</u>", StringComparison.OrdinalIgnoreCase))
                {
                    rtb.AppendText(sb.ToString());
                    sb.Clear();
                    isUnderline = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                {
                    var s = text.Substring(i);
                    var fontFound = false;
                    var end = s.IndexOf('>');
                    if (end > 0)
                    {
                        var f = s.Substring(0, end);
                        var colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);

                        if (colorStart > 0)
                        {
                            var colorEnd = colorStart + " color=".Length + 1;
                            if (colorEnd < f.Length)
                            {
                                colorEnd = f.IndexOf('"', colorEnd);
                                if (colorEnd > 0 || colorEnd == -1)
                                {
                                    if (colorEnd == -1)
                                    {
                                        s = f.Substring(colorStart);
                                    }
                                    else
                                    {
                                        s = f.Substring(colorStart, colorEnd - colorStart);
                                    }

                                    s = s.Remove(0, " color=".Length);
                                    s = s.Trim('"');
                                    s = s.Trim('\'');
                                    try
                                    {
                                        fontColor = ColorTranslator.FromHtml(s);
                                        fontFound = true;
                                    }
                                    catch
                                    {
                                        fontFound = false;
                                        if (s.Length > 0)
                                        {
                                            try
                                            {
                                                fontColor = ColorTranslator.FromHtml("#" + s);
                                                fontFound = true;
                                            }
                                            catch
                                            {
                                                fontFound = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        i += end;
                    }
                    if (fontFound)
                    {
                        rtb.AppendText(sb.ToString());
                        sb.Clear();
                        isFontColor = true;
                        fontColorBegin = letterCount + extraAdd;
                    }
                }
                else if (isFontColor && text.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                {
                    fontColorLookups.Add(new Point(fontColorBegin, rtb.Text.Length + sb.ToString().Length - fontColorBegin), fontColor);
                    rtb.AppendText(sb.ToString());
                    sb.Clear();
                    isFontColor = false;
                    i += 6;
                }
                else if (text.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                {
                    i += 6;
                }
                else if (text[i] == '\n') // RichTextBox only count NewLine as one character!
                {
                    sb.Append(text[i]);
                }
                else if (text[i] == '\r')
                {
                    extraAdd++;
                    // skip carriage return (0xd / 13)
                }
                else
                {
                    var idx = rtb.TextLength + sb.Length;
                    if (isBold)
                    {
                        styleLookups[idx] |= FontStyle.Bold;
                    }

                    if (isItalic)
                    {
                        styleLookups[idx] |= FontStyle.Italic;
                    }

                    if (isUnderline)
                    {
                        styleLookups[idx] |= FontStyle.Underline;
                    }

                    if (defaultFontStyle != styleLookups[idx])
                    {
                        inlineFontStyles = true;
                    }

                    sb.Append(text[i]);
                    letterCount++;
                }
                i++;
            }

            rtb.Text += sb.ToString();
            rtb.SelectAll();

            if (alignLeft)
            {
                rtb.SelectionAlignment = HorizontalAlignment.Left;
            }
            else if (alignRight)
            {
                rtb.SelectionAlignment = HorizontalAlignment.Right;
            }
            else
            {
                rtb.SelectionAlignment = HorizontalAlignment.Center;
            }

            rtb.DeselectAll();

            var currentFont = rtb.SelectionFont;
            if (inlineFontStyles && rtb.TextLength < 200)
            {
                for (var k = 0; k < rtb.TextLength; k++)
                {
                    rtb.SelectionStart = k;
                    rtb.SelectionLength = 1;
                    rtb.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, styleLookups[k]);
                    rtb.DeselectAll();
                }
            }
            else
            {
                rtb.SelectAll();
                rtb.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, defaultFontStyle);
                rtb.DeselectAll();
            }

            foreach (var entry in fontColorLookups)
            {
                rtb.SelectionStart = entry.Key.X;
                rtb.SelectionLength = entry.Key.Y;
                rtb.SelectionColor = entry.Value;
                rtb.DeselectAll();
            }
        }
    }
}
