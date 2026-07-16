using System;
using System.Collections.Generic;

using AvaloniaEdit.Document;
using TextMateSharp.Themes;
using AM = Avalonia.Media;

namespace AvaloniaEdit.TextMate
{
    public abstract class TextTransformation : TextSegment
    {
        public abstract void Transform(GenericLineTransformer transformer, DocumentLine line);
    }

    public class ForegroundTextTransformation : TextTransformation
    {
        public Dictionary<int, AM.IBrush> ColorMap { get; set; }
        public Action<Exception> ExceptionHandler { get; set; }
        public int ForegroundColor { get; set; }
        public int BackgroundColor { get; set; }
        public FontStyle FontStyle { get; set; }

        public override void Transform(GenericLineTransformer transformer, DocumentLine line)
        {
            try
            {
                if (Length == 0)
                {
                    return;
                }

                var formattedOffset = 0;
                var endOffset = line.EndOffset;

                if (StartOffset > line.Offset)
                {
                    formattedOffset = StartOffset - line.Offset;
                }

                if (EndOffset < line.EndOffset)
                {
                    endOffset = EndOffset;
                }

                transformer.SetTextStyle(line, formattedOffset, endOffset - line.Offset - formattedOffset,
                    GetBrush(ForegroundColor),
                    GetBrush(BackgroundColor),
                    GetFontStyle(),
                    GetFontWeight(),
                    IsUnderline());
            }
            catch (Exception ex)
            {
                ExceptionHandler?.Invoke(ex);
            }
        }

        AM.FontStyle GetFontStyle()
        {
            if (FontStyle != TextMateSharp.Themes.FontStyle.NotSet &&
                (FontStyle & TextMateSharp.Themes.FontStyle.Italic) != 0)
                return AM.FontStyle.Italic;

            return AM.FontStyle.Normal;
        }

        AM.FontWeight GetFontWeight()
        {
            if (FontStyle != TextMateSharp.Themes.FontStyle.NotSet &&
                (FontStyle & TextMateSharp.Themes.FontStyle.Bold) != 0)
                return AM.FontWeight.Bold;

            return AM.FontWeight.Regular;
        }

        bool IsUnderline()
        {
            if (FontStyle != TextMateSharp.Themes.FontStyle.NotSet &&
                (FontStyle & TextMateSharp.Themes.FontStyle.Underline) != 0)
                return true;

            return false;
        }

        AM.IBrush GetBrush(int colorId)
        {
            if (ColorMap == null)
                return null;

            ColorMap.TryGetValue(colorId, out AM.IBrush result);
            return result;
        }
    }
}