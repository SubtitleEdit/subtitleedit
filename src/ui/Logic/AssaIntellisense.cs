using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public static class AssaIntellisense
    {
        public class IntellisenseItem
        {
            public string Value { get; set; }
            public string Hint { get; set; }
            public string TypedWord { get; set; }
            public Font Font { get; set; }

            public IntellisenseItem(string value, string hint)
            {
                Value = value;
                Hint = hint;
            }

            public override string ToString()
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var textSize = g.MeasureString(Value, Font);
                    var spaceSize = g.MeasureString(" ", Font);
                    var v = Value;
                    if (textSize.Width < 200)
                    {
                        var missing = 200 - textSize.Width;
                        var rest = (int)(missing / spaceSize.Width);
                        v += "".PadRight(rest, ' ');
                    }

                    return v + "\t " + Hint;
                }
            }
        }

        private static readonly List<IntellisenseItem> Keywords = new List<IntellisenseItem>
        {
            new IntellisenseItem("{\\b}",  "Bold"),
            new IntellisenseItem("{\\b0}",  "Bold off"),
            new IntellisenseItem("{\\b1}",  "Bold on"),
            new IntellisenseItem("{\\i}",  "Italic"),
            new IntellisenseItem("{\\i0}",  "Italic off"),
            new IntellisenseItem("{\\i1}",  "Italic on"),
            new IntellisenseItem("{\\u}",  "Underline"),
            new IntellisenseItem("{\\u0}",  "Underline off"),
            new IntellisenseItem("{\\u1}",  "Underline on"),
            new IntellisenseItem("{\\s}",  "Strikeout"),
            new IntellisenseItem("{\\s0}",  "Strikeout off"),
            new IntellisenseItem("{\\s1}",  "Strikeout on"),
            new IntellisenseItem("{\\bord<width>}",  "Border"),
            new IntellisenseItem("{\\xbord<x>}",  "Border width"),
            new IntellisenseItem("{\\ybord<y>}",  "Border height"),
            new IntellisenseItem("{\\shad<depth>}",  "Shadow"),
            new IntellisenseItem("{\\xshad<x>}",  "Shadow width"),
            new IntellisenseItem("{\\yshad<y>}",  "Shadow height"),
            new IntellisenseItem("{\\be}",  "Blur edges"),
            new IntellisenseItem("{\\be0}",  "Blur edges off"),
            new IntellisenseItem("{\\be1}",  "Blur edges on"),
            new IntellisenseItem("{\\fn<font_name>}",  "Font name"),
            new IntellisenseItem("{\\fs<font_size>}",  "Font size"),
            new IntellisenseItem("{\\fscx<percent>}",  "Scale X percentage"),
            new IntellisenseItem("{\\fscy<percent>}",  "Scale Y percentage"),
            new IntellisenseItem("{\\fsp<pixels>}",  "Spacing between letters in pixels"),
            new IntellisenseItem("{\\fr<degree>}",  "Angle (z axis for text rotation)"),
            new IntellisenseItem("{\\frx<degree>}",  "Angle (x axis for text rotation)"),
            new IntellisenseItem("{\\fry<degree>}",  "Angle (y axis for text rotation)"),
            new IntellisenseItem("{\\frz<degree>}",  "Angle (z axis for text rotation)"),
            new IntellisenseItem("{\\org<x,y>}",  "Set origin point for rotation"),
            new IntellisenseItem("{\\fax<degree>}",  "Shearing transformation (x axis)"),
            new IntellisenseItem("{\\fay<degree>}",  "Shearing transformation (y axis)"),
            new IntellisenseItem("{\\fe<charset>}",  "Encoding"),
            new IntellisenseItem("{\\c&H<bbggrr>}",  "Color"),
            new IntellisenseItem("{\\2c&H<bbggrr>}",  "Color for outline"),
            new IntellisenseItem("{\\3c&H<bbggrr>}",  "Color for opaque box"),
            new IntellisenseItem("{\\4c&H<bbggrr>}",  "Color for shadow"),
            new IntellisenseItem("{\\alpha&H<aa>&}",  "Alpha (00=fully visible, ff=transparent)"),
            new IntellisenseItem("{\\a2&H<aa>&}",  "Alpha for outline (00=fully visible, ff=transparent)"),
            new IntellisenseItem("{\\a3&H<aa>&}",  "Alpha for opaque box (00=fully visible, ff=transparent)"),
            new IntellisenseItem("{\\a4&H<aa>&}",  "Alpha for shadow (00=fully visible, ff=transparent)"),
            new IntellisenseItem("{\\an7}",  "Align top left"),
            new IntellisenseItem("{\\an8}",  "Align top center"),
            new IntellisenseItem("{\\an9}",  "Align top right"),
            new IntellisenseItem("{\\an4}",  "Align center left"),
            new IntellisenseItem("{\\an5}",  "Align center middle"),
            new IntellisenseItem("{\\an6}",  "Align center right"),
            new IntellisenseItem("{\\an1}",  "Align bottom left"),
            new IntellisenseItem("{\\an2}",  "Align bottom middle"),
            new IntellisenseItem("{\\an3}",  "Align bottom right"),
            new IntellisenseItem("{\\k<duration>}",  "Karaoke, delay in 100th of a second (10ms)"),
            new IntellisenseItem("{\\K<duration>}",  "Karaoke right to left, delay in 100th of a second (10ms)"),
            new IntellisenseItem("{\\mov(x1,y1,x2,y2,start,end)}",  "Move"),
            new IntellisenseItem("{\\pos(x,y)}",  "Set position"),
            new IntellisenseItem("{\\fade(a1,a2,a3,t1,t2,t3,t4)}",  "Fade advanced"),
            new IntellisenseItem("{\\fad(fadein time,fadeout time>}",  "Fade"),
            new IntellisenseItem("{\\clip(x1,y1,x2,y2)}",  "Clips (hides) any drawing outside the rectangle defined by the parameters."),
            new IntellisenseItem("{\\iclip(x1,y1,x2,y2)}",  "Clips (hides) any drawing inside the rectangle defined by the parameters."),
            new IntellisenseItem("{\\r}",  "Reset inline styles"),
            new IntellisenseItem("{\\t(<style modifiers>)}",  "Animated transform"),
            new IntellisenseItem("{\\t(<accel,style modifiers>)}",  "Animated transform"),
            new IntellisenseItem("{\\t(<t1,t2,style modifiers>)}",  "Animated transform"),
            new IntellisenseItem("{\\t(<t1,t2,accel,style modifiers>)}",  "Animated transform"),
        };

        private static readonly List<IntellisenseItem> TransformKeywords = new List<IntellisenseItem>
        {
            new IntellisenseItem("\\fs<font_size>",  "Font size"),
            new IntellisenseItem("\\fscx<percent>",  "Scale X percentage"),
            new IntellisenseItem("\\fscy<percent>",  "Scale Y percentage"),
            new IntellisenseItem("\\fsp<pixels>",  "Spacing between letters in pixels"),
            new IntellisenseItem("\\fr<degree>",  "Angle (z axis for text rotation)"),
            new IntellisenseItem("\\frx<degree>",  "Angle (x axis for text rotation)"),
            new IntellisenseItem("\\fry<degree>",  "Angle (y axis for text rotation)"),
            new IntellisenseItem("\\frz<degree>",  "Angle (z axis for text rotation)"),
            new IntellisenseItem("\\bord<width>",  "Border"),
            new IntellisenseItem("\\xbord<x>",  "Border width"),
            new IntellisenseItem("\\ybord<y>",  "Border height"),
            new IntellisenseItem("\\shad<depth>",  "Shadow"),
            new IntellisenseItem("\\xshad<x>",  "Shadow width"),
            new IntellisenseItem("\\yshad<y>",  "Shadow height"),
            new IntellisenseItem("\\c&H<bbggrr>",  "Color"),
            new IntellisenseItem("\\2c&H<bbggrr>",  "Color for outline"),
            new IntellisenseItem("\\3c&H<bbggrr>",  "Color for opaque box"),
            new IntellisenseItem("\\4c&H<bbggrr>",  "Color for shadow"),
            new IntellisenseItem("\\alpha&H<aa>&",  "Alpha (00=fully visible, ff=transparent)"),
            new IntellisenseItem("\\a2&H<aa>&",  "Alpha for outline (00=fully visible, ff=transparent)"),
            new IntellisenseItem("\\a3&H<aa>&",  "Alpha for opaque box (00=fully visible, ff=transparent)"),
            new IntellisenseItem("\\a4&H<aa>&",  "Alpha for shadow (00=fully visible, ff=transparent)"),
            new IntellisenseItem("\\be",  "Blur edges"),
            new IntellisenseItem("\\be0",  "Blur edges off"),
            new IntellisenseItem("\\be1",  "Blur edges on"),
            new IntellisenseItem("\\fax<degree>",  "Shearing transformation (x axis)"),
            new IntellisenseItem("\\fay<degree>",  "Shearing transformation (y axis)"),
            new IntellisenseItem("\\clip(x1,y1,x2,y2)",  "Clips (hides) any drawing outside the rectangle defined by the parameters."),
            new IntellisenseItem("\\iclip(x1,y1,x2,y2)",  "Clips (hides) any drawing inside the rectangle defined by the parameters."),
        };

        private static readonly List<string> LastAddedTags = new List<string>();

        public static void AddUsedTag(string tag)
        {
            LastAddedTags.Add(tag);
        }

        public static bool AutoCompleteTextBox(SETextBox textBox, ListBox listBox)
        {
            var text = string.IsNullOrEmpty(textBox.Text) ? string.Empty : textBox.Text.Substring(0, textBox.SelectionStart);
            var lastWord = GetLastString(text) ?? string.Empty;
            var keywords = Keywords;

            if (text.EndsWith("\\t(", StringComparison.Ordinal))
            {
                // use smaller list inside transformation
                keywords = TransformKeywords;
                lastWord = string.Empty;
            }

            var filteredList = keywords
                .Where(n => string.IsNullOrEmpty(lastWord) || n.Value.StartsWith(GetLastString(lastWord), StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Value)
                .ToList();

            if (filteredList.Count == 0 && lastWord.EndsWith("\\"))
            {
                // continuing ass tag, remove "{\" + "}"
                lastWord = string.Empty;
                filteredList = keywords
                    .OrderBy(p => p.Value)
                    .Select(p => new IntellisenseItem(p.Value.Replace("{\\", string.Empty).RemoveChar('}'), p.Hint))
                    .ToList();
            }

            listBox.Items.Clear();

            foreach (var item in filteredList)
            {
                item.TypedWord = lastWord;
                item.Font = listBox.Font;
                listBox.Items.Add(item);
            }

            if (listBox.Items.Count == 0)
            {
                return false;
            }

            listBox.SelectedIndex = 0;
            var endTag = GetEndTagFromLastTagInText(text);
            if (!string.IsNullOrEmpty(endTag))
            {
                var item = filteredList.Find(p => p.Value == endTag);
                if (item != null)
                {
                    listBox.SelectedIndex = filteredList.IndexOf(item);
                }
            }
            else if (IsLastTwoTagsEqual())
            {
                var item = filteredList.Find(p => p.Value == LastAddedTags.Last());
                if (item != null)
                {
                    listBox.SelectedIndex = filteredList.IndexOf(item);
                }
            }

            if (Configuration.Settings.General.UseDarkTheme)
            {
                DarkTheme.SetDarkTheme(listBox);
            }

            listBox.Width = 480;
            listBox.Height = 200;
            var height = listBox.Items.Count * listBox.ItemHeight + listBox.Items.Count + listBox.ItemHeight;
            if (height < listBox.Height)
            {
                listBox.Height = height;
            }

            listBox.Visible = true;
            return true;
        }

        private static bool IsLastTwoTagsEqual()
        {
            return LastAddedTags.Count >= 2 &&
                   LastAddedTags[LastAddedTags.Count - 1] == LastAddedTags[LastAddedTags.Count - 2];
        }
        private static string GetEndTagFromLastTagInText(string text)
        {
            var lastIdx = text.LastIndexOf("{\\", StringComparison.Ordinal);
            if (lastIdx >= 0)
            {
                var s = text.Substring(lastIdx);
                if (s.StartsWith("{\\i1}"))
                {
                    return "{\\i0}";
                }

                if (s.StartsWith("{\\u1}"))
                {
                    return "{\\u0}";
                }

                if (s.StartsWith("{\\b1}"))
                {
                    return "{\\b0}";
                }

                if (s.StartsWith("{\\s1}"))
                {
                    return "{\\s0}";
                }

                if (s.StartsWith("{\\be1}"))
                {
                    return "{\\be0}";
                }
            }

            return string.Empty;
        }

        private static string GetLastString(string s)
        {
            if (s.TrimEnd().EndsWith("}"))
            {
                return string.Empty;
            }

            var lastIndexOfStartBracket = s.LastIndexOf('{');
            var lastSeparatorIndex = s.LastIndexOfAny(new[] { ' ', ',', '.', '"', '\'', '}' });
            if (lastIndexOfStartBracket > lastSeparatorIndex)
            {
                return s.Remove(0, lastIndexOfStartBracket);
            }

            if (lastSeparatorIndex >= 0)
            {
                s = s.Remove(0, lastSeparatorIndex - 1);
                if (Keywords.Any(p => p.Value.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
                {
                    return s;
                }
            }
            else
            {
                if (Keywords.Any(p => p.Value.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
                {
                    return s;
                }
            }

            return string.Empty;
        }
    }
}
