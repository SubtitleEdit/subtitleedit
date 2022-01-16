using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class EffectAnimationPart
    {
        public EffectAnimationPart()
        {
            Pre = string.Empty;
            Text = string.Empty;
            Post = string.Empty;
        }
        public string Pre { get; set; }
        public string Text { get; set; }
        public string Post { get; set; }
        public Color Color { get; set; }
        public string Face { get; set; }
        public string Size { get; set; }

        public static Color NullColor => Color.FromArgb(0, 254, 1, 198);

        public static string ToString(List<EffectAnimationPart> parts)
        {
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                sb.Append(part.Pre);
                sb.Append(part.Text);
                sb.Append(part.Post);
            }

            return sb.ToString();
        }

        public static Color GetColorFromFontString(string text, Color defaultColor)
        {
            var s = text.TrimEnd();
            var start = s.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
            if (start >= 0 && s.EndsWith("</font>", StringComparison.OrdinalIgnoreCase))
            {
                int end = s.IndexOf('>', start);
                if (end > 0)
                {
                    string f = s.Substring(start, end - start);
                    if (f.Contains(" color="))
                    {
                        int colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                        if (s.IndexOf('"', colorStart + " color=".Length + 1) > 0)
                        {
                            end = s.IndexOf('"', colorStart + " color=".Length + 1);
                        }

                        s = s.Substring(colorStart, end - colorStart);
                        s = s.Replace(" color=", string.Empty);
                        s = s.Trim('\'').Trim('"').Trim('\'');
                        try
                        {
                            if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                            {
                                var arr = s.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                return Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                            }
                            return ColorTranslator.FromHtml(s);
                        }
                        catch
                        {
                            return defaultColor;
                        }
                    }
                }
            }
            return defaultColor;
        }

        public static string GetAttributeFromFontString(string text, string attributeName)
        {
            var s = text.TrimEnd();
            var start = s.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
            if (start >= 0)
            {
                int end = s.IndexOf('>', start);
                if (end > 0)
                {
                    var f = s.Substring(start, end - start);
                    var tag = $" {attributeName}=";
                    if (f.Contains(tag))
                    {
                        int colorStart = f.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
                        if (s.IndexOf('"', colorStart + tag.Length + 1) > 0)
                        {
                            end = s.IndexOf('"', colorStart + tag.Length + 1);
                        }

                        s = s.Substring(colorStart, end - colorStart);
                        s = s.Replace(tag, string.Empty);
                        s = s.Trim('\'').Trim('"').Trim('\'');
                        return s;
                    }
                }
            }

            return string.Empty;
        }

        public static List<EffectAnimationPart> MakeBase(string text)
        {
            var i = 0;
            var allowedStartTags = new List<string> { "<i>", "<u>", "<b>", "<font ", "<span " };
            var allowedEndTags = new List<string> { "</i>", "</u>", "</b>", "</font>", "</span>" };
            var list = new List<EffectAnimationPart>();
            EffectAnimationPart p = null;
            var color = NullColor;
            var face = string.Empty;
            var size = string.Empty;
            var fontStack = new Stack<EffectAnimationPart>();
            while (i < text.Length)
            {
                var c = text[i];
                if (c == '{' && text.Substring(i).StartsWith("{\\") && text.IndexOf('}', i) > 0)
                {
                    var endIndex = text.IndexOf('}', i);
                    var tag = text.Substring(i, endIndex - i + 1);
                    i += tag.Length;

                    if (p == null)
                    {
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }
                    else if (!string.IsNullOrEmpty(p.Text))
                    {
                        list.Add(p);
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }

                    p.Pre += tag;
                }
                else if (c == '<' && allowedStartTags.Any(a => text.Substring(i).StartsWith(a, StringComparison.OrdinalIgnoreCase)) && text.IndexOf('>', i) > 0)
                {
                    var endIndex = text.IndexOf('>', i);
                    var tag = text.Substring(i, endIndex - i + 1);
                    i += tag.Length;

                    if (p == null)
                    {
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }
                    else if (!string.IsNullOrEmpty(p.Text))
                    {
                        list.Add(p);
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }

                    if (tag.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
                    {
                        fontStack.Push(new EffectAnimationPart { Color = color, Face = face, Size = size });

                        // get color
                        var tagColor = GetColorFromFontString(tag + "a</font>", NullColor);
                        if (tagColor != NullColor)
                        {
                            p.Color = tagColor;
                            color = tagColor;
                        }

                        // get font name (face)
                        var fontName = GetAttributeFromFontString(tag, "face");
                        if (!string.IsNullOrEmpty(fontName))
                        {
                            p.Face = fontName;
                            face = fontName;
                        }

                        tag = string.Empty;
                    }

                    p.Pre += tag;
                }
                else if (c == '<' && allowedEndTags.Any(a => text.Substring(i).StartsWith(a, StringComparison.OrdinalIgnoreCase)) && text.IndexOf('>', i) > 0)
                {
                    var endIndex = text.IndexOf('>', i);
                    var tag = text.Substring(i, endIndex - i + 1);
                    i += tag.Length;

                    if (p == null)
                    {
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }

                    // set color
                    if (tag == "</font>")
                    {
                        if (fontStack.Count > 0)
                        {
                            var f = fontStack.Pop();
                            color = f.Color;
                            face = f.Face;
                            size = f.Size;
                        }
                        else
                        {
                            color = NullColor;
                            face = string.Empty;
                            size = string.Empty;
                        }
                    }

                    tag = string.Empty;
                    p.Post += tag;

                }
                else if (c == '\r')
                {
                    if (p == null)
                    {
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }
                    else if (!string.IsNullOrEmpty(p.Text))
                    {
                        list.Add(p);
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }

                    p.Text = c.ToString();
                    i++;

                    if (i < text.Length && text[i] == '\n') // only take one part for \r\n
                    {
                        p.Text += '\n';
                        i++;
                    }
                }
                else
                {
                    if (p == null)
                    {
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }
                    else if (!string.IsNullOrEmpty(p.Text))
                    {
                        list.Add(p);
                        p = new EffectAnimationPart { Color = color, Face = face, Size = size };
                    }

                    p.Text = c.ToString();
                    i++;
                }
            }

            if (p != null && !string.IsNullOrEmpty(p.Pre + p.Text + p.Post))
            {
                list.Add(p);
            }

            return list;
        }

        public static List<EffectAnimationPart> MakeEffectKaraoke(List<EffectAnimationPart> parts, Color animationColor, int inputLast)
        {
            if (parts == null || parts.Count == 0)
            {
                return new List<EffectAnimationPart>();
            }

            var last = inputLast;
            if (last > parts.Count)
            {
                last = parts.Count - 1;
            }

            // add karaoke color
            var list = new List<EffectAnimationPart>();
            for (var index = 0; index < parts.Count; index++)
            {
                var part = parts[index];
                list.Add(new EffectAnimationPart
                {
                    Pre = part.Pre,
                    Text = part.Text,
                    Post = part.Post,
                    Color = index <= last ? animationColor : part.Color,
                    Face = part.Face,
                    Size = part.Size,
                });
            }

            for (var index = 0; index < list.Count - 1; index++)
            {
                var part = list[index];
                var next = list[index + 1];

                if (index == 0)
                {
                    part.Pre += MakeFontTag(part);
                    if (next.Color != part.Color || next.Face != part.Face || next.Size != part.Size)
                    {
                        part.Post = "</font>" + part.Post;
                        next.Pre += MakeFontTag(next);
                    }
                }
                else if (next.Color != part.Color || next.Face != part.Face || next.Size != part.Size)
                {
                    part.Post = "</font>" + part.Post;
                    next.Pre += MakeFontTag(next);
                }
            }

            return list;
        }

        private static string MakeFontTag(EffectAnimationPart part)
        {
            var attributes = string.Empty;
            if (part.Color != NullColor)
            {
                attributes = $" color=\"{ToHtml(part.Color)}\"";
            }

            if (!string.IsNullOrEmpty(part.Face))
            {
                attributes += $" face=\"{part.Face}\"";
            }

            if (!string.IsNullOrEmpty(part.Size))
            {
                attributes += $" size=\"{part.Size}\"";
            }

            var result = $"<font{attributes.TrimEnd()}>".Replace("<font>", string.Empty);
            return result;
        }

        public static string ToHtml(Color c)
        {
            return $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
        }
    }
}