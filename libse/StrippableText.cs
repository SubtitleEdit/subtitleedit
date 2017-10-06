using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class StrippableText
    {
        public string Pre { get; set; }
        public string Post { get; set; }
        public string StrippedText { get; set; }
        public string OriginalText { get; }

        public string MergedString => Pre + StrippedText + Post;

        public StrippableText(string text)
            : this(text, " >-\"„”“['‘`´¶(♪¿¡.…—", " -\"”“]'`´¶)♪.!?:…—؛،؟")
        {
        }

        public StrippableText(string text, string stripStartCharacters, string stripEndCharacters)
        {
            OriginalText = text;

            Pre = string.Empty;
            if (text.Length > 0 && ("<{" + stripStartCharacters).Contains(text[0]))
            {
                int beginLength;
                do
                {
                    beginLength = text.Length;

                    while (text.Length > 0 && stripStartCharacters.Contains(text[0]))
                    {
                        Pre += text[0];
                        text = text.Remove(0, 1);
                    }

                    // ASS/SSA codes like {\an9}
                    int endIndex = text.IndexOf('}');
                    if (endIndex > 0 && text.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        int nextStartIndex = text.IndexOf('{', 2);
                        if (nextStartIndex == -1 || nextStartIndex > endIndex)
                        {
                            endIndex++;
                            Pre += text.Substring(0, endIndex);
                            text = text.Remove(0, endIndex);
                        }
                    }

                    // tags like <i> or <font face="Segoe Print" color="#ff0000">
                    endIndex = text.IndexOf('>');
                    if (text.StartsWith('<') && endIndex >= 2)
                    {
                        endIndex++;
                        Pre += text.Substring(0, endIndex);
                        text = text.Remove(0, endIndex);
                    }
                }
                while (text.Length < beginLength);
            }

            Post = string.Empty;
            if (text.Length > 0 && (">" + stripEndCharacters).Contains(text[text.Length - 1]))
            {
                int beginLength;
                do
                {
                    beginLength = text.Length;

                    while (text.Length > 0 && stripEndCharacters.Contains(text[text.Length - 1]))
                    {
                        Post = text[text.Length - 1] + Post;
                        text = text.Substring(0, text.Length - 1);
                    }

                    if (text.EndsWith('>'))
                    {
                        // tags </i> </b> </u>
                        if (text.EndsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                            text.EndsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                            text.EndsWith("</u>", StringComparison.OrdinalIgnoreCase))
                        {
                            Post = text.Substring(text.Length - 4) + Post;
                            text = text.Substring(0, text.Length - 4);
                        }

                        // tag </font>
                        if (text.EndsWith("</font>", StringComparison.OrdinalIgnoreCase))
                        {
                            Post = text.Substring(text.Length - 7) + Post;
                            text = text.Substring(0, text.Length - 7);
                        }
                    }
                }
                while (text.Length < beginLength);
            }

            StrippedText = text;
        }

        public string CombineWithPrePost(string text)
        {
            return Pre + text + Post;
        }
    }
}
