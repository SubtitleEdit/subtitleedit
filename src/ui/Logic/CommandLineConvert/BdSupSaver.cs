using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Windows.Forms;
using static Nikse.SubtitleEdit.Forms.ExportPngXml;

namespace Nikse.SubtitleEdit.Logic.CommandLineConvert
{
    public static class BdSupSaver
    {
        public static void SaveBdSup(string fileName, Subtitle sub, IList<IBinaryParagraph> binaryParagraphs, ExportPngXml form, int width, int height, bool isImageBased, FileStream binarySubtitleFile, SubtitleFormat format, CancellationToken cancellationToken)
        {
            var generalBitmapParameter = form.MakeMakeBitmapParameter(0, width, height);
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
            var pms = new MakeBitmapParameter[sub.Paragraphs.Count];
            var po = new ParallelOptions { CancellationToken = cancellationToken };
            Application.DoEvents();
            Parallel.For(0, sub.Paragraphs.Count, po, i =>
            {
                var mp = MakeMakeBitmapParameter(sub, i, generalBitmapParameter, language, format, width, height);
                pms[i] = GenerateImage(fileName, sub, binaryParagraphs, isImageBased, mp, i);
            });

            Application.DoEvents();
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            foreach (var x in pms)
            {
                binarySubtitleFile.Write(x.Buffer, 0, x.Buffer.Length);
            }

            Application.DoEvents();
        }

        private static MakeBitmapParameter MakeMakeBitmapParameter(Subtitle subtitle, int index, MakeBitmapParameter bp, string language, SubtitleFormat format, int width, int height)
        {
            var p = subtitle.GetParagraphOrDefault(index);
            if (p == null)
            {
                return bp;
            }

            var parameter = new MakeBitmapParameter
            {
                Type = bp.Type,
                SubtitleColor = bp.SubtitleColor,
                SubtitleFontName = bp.SubtitleFontName,
                SubtitleFontSize = bp.SubtitleFontSize,
                SubtitleFontBold = bp.SubtitleFontBold,
                BorderColor = bp.BorderColor,
                BorderWidth = bp.BorderWidth,
                SimpleRendering = bp.SimpleRendering,
                AlignLeft = bp.AlignLeft,
                AlignRight = bp.AlignRight,
                JustifyLeft = GetJustifyLeft(p.Text, language), // center, left justify
                JustifyTop = bp.JustifyTop,
                JustifyRight = bp.JustifyRight,
                ScreenWidth = bp.ScreenWidth,
                ScreenHeight = bp.ScreenHeight,
                VideoResolution = bp.VideoResolution,
                Bitmap = null,
                FramesPerSeconds = bp.FramesPerSeconds,
                BottomMargin = GetBottomMarginInPixels(p, format, subtitle, height),
                LeftMargin = GetLeftMarginInPixels(p, format, subtitle, width),
                RightMargin = GetRightMarginInPixels(p, format, subtitle, width),
                Saved = false,
                Alignment = ContentAlignment.BottomCenter,
                Type3D = bp.Type3D,
                Depth3D = bp.Depth3D,
                BackgroundColor = bp.BackgroundColor,
                ShadowColor = bp.ShadowColor,
                ShadowWidth = bp.ShadowWidth,
                ShadowAlpha = bp.ShadowAlpha,
                LineHeight = bp.LineHeight,
                FullFrame = bp.FullFrame,
                FullFrameBackgroundColor = bp.FullFrameBackgroundColor,
                BoxSingleLine = bp.BoxSingleLine,
                P = p,
            };

            parameter.Alignment = GetAlignmentFromParagraph(parameter, format, subtitle);
            parameter.OverridePosition = GetAssPoint(parameter.P.Text);

            if (format.HasStyleSupport && !string.IsNullOrEmpty(parameter.P.Extra))
            {
                if (format.GetType() == typeof(SubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(parameter.P.Extra, subtitle.Header);
                    parameter.SubtitleColor = style.Primary;
                    parameter.SubtitleFontBold = style.Bold;
                    parameter.SubtitleFontSize = (float)style.FontSize;
                    parameter.SubtitleFontName = style.FontName;
                    parameter.BottomMargin = style.MarginVertical;
                    if (style.BorderStyle == "3")
                    {
                        parameter.BackgroundColor = style.Background;
                    }
                    parameter.ShadowColor = style.Outline;
                }
                else if (format.GetType() == typeof(AdvancedSubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(parameter.P.Extra, subtitle.Header);
                    parameter.SubtitleColor = style.Primary;
                    parameter.SubtitleFontBold = style.Bold;
                    parameter.SubtitleFontSize = (float)style.FontSize;
                    parameter.SubtitleFontName = style.FontName;
                    parameter.BottomMargin = style.MarginVertical;
                    if (style.BorderStyle == "3")
                    {
                        parameter.BackgroundColor = style.Outline;
                    }
                    parameter.ShadowAlpha = style.Background.A;
                    parameter.ShadowColor = style.Background;
                }
            }

            return parameter;
        }

        private static int GetBottomMarginInPixels(Paragraph p, SubtitleFormat format, Subtitle subtitle, int height)
        {
            if (!string.IsNullOrEmpty(p?.Extra) && (format.Name == AdvancedSubStationAlpha.NameOfFormat || format.Name == SubStationAlpha.NameOfFormat))
            {
                var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, subtitle.Header);
                return style.MarginVertical;
            }

            if (Configuration.Settings.Tools.ExportBottomMarginUnit == "%")
            {
                return (int)Math.Round(Configuration.Settings.Tools.ExportBluRayBottomMarginPercent / 100.0 * height, MidpointRounding.AwayFromZero);
            }

            // pixels
            return Configuration.Settings.Tools.ExportBluRayBottomMarginPixels;
        }

        private static int GetLeftMarginInPixels(Paragraph p, SubtitleFormat format, Subtitle subtitle, int width)
        {
            if (!string.IsNullOrEmpty(p?.Extra) && (format.Name == AdvancedSubStationAlpha.NameOfFormat || format.Name == SubStationAlpha.NameOfFormat))
            {
                var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, subtitle.Header);
                return style.MarginLeft;
            }

            if (Configuration.Settings.Tools.ExportLeftRightMarginUnit == "%")
            {
                return (int)Math.Round(Configuration.Settings.Tools.ExportLeftRightMarginPercent / 100.0 * width, MidpointRounding.AwayFromZero);
            }

            // pixels
            return Configuration.Settings.Tools.ExportLeftRightMarginPixels;
        }

        private static int GetRightMarginInPixels(Paragraph p, SubtitleFormat format, Subtitle subtitle, int width)
        {
            if (!string.IsNullOrEmpty(p?.Extra) && (format.Name == AdvancedSubStationAlpha.NameOfFormat || format.Name == SubStationAlpha.NameOfFormat))
            {
                var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, subtitle.Header);
                return style.MarginRight;
            }

            return GetLeftMarginInPixels(p, format, subtitle, width);
        }

        private static bool GetJustifyLeft(string text, string language)
        {
            if (Configuration.Settings.Tools.ExportHorizontalAlignment != 6 || string.IsNullOrEmpty(text))
            {
                return Configuration.Settings.Tools.ExportHorizontalAlignment == 3;
            }

            var s = Utilities.RemoveUnneededSpaces(text, language);
            var dialogHelper = new DialogSplitMerge { TwoLetterLanguageCode = language };
            var lines = s.SplitToLines();
            return dialogHelper.IsDialog(lines) || HasTwoSpeakers(lines);
        }

        private static bool HasTwoSpeakers(List<string> lines)
        {
            return lines.Count == 2 && lines[0].HasSentenceEnding() && lines[0].Contains(':') && lines[1].Contains(':');
        }

        private static ContentAlignment GetAlignmentFromParagraph(MakeBitmapParameter p, SubtitleFormat format, Subtitle subtitle)
        {
            var alignment = ContentAlignment.BottomCenter;
            if (p.AlignLeft)
            {
                alignment = ContentAlignment.BottomLeft;
            }
            else if (p.AlignRight)
            {
                alignment = ContentAlignment.BottomRight;
            }

            if (format.HasStyleSupport && !string.IsNullOrEmpty(p.P.Extra))
            {
                if (format.GetType() == typeof(SubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.P.Extra, subtitle.Header);
                    alignment = GetSsaAlignment("{\\a" + style.Alignment + "}", alignment);
                }
                else if (format.GetType() == typeof(AdvancedSubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.P.Extra, subtitle.Header);
                    alignment = GetAssAlignment("{\\an" + style.Alignment + "}", alignment);
                }
            }

            var text = p.P.Text;
            if (format.GetType() == typeof(SubStationAlpha) && text.Length > 5)
            {
                text = p.P.Text.Substring(0, 6);
                alignment = GetSsaAlignment(text, alignment);
            }
            else if (text.Length > 6)
            {
                text = p.P.Text.Substring(0, 6);
                alignment = GetAssAlignment(text, alignment);
            }
            return alignment;
        }

        private static ContentAlignment GetSsaAlignment(string text, ContentAlignment defaultAlignment)
        {
            //1: Bottom left
            //2: Bottom center
            //3: Bottom right
            //9: Middle left
            //10: Middle center
            //11: Middle right
            //5: Top left
            //6: Top center
            //7: Top right
            switch (text)
            {
                case "{\\a1}":
                    return ContentAlignment.BottomLeft;
                case "{\\a2}":
                    return ContentAlignment.BottomCenter;
                case "{\\a3}":
                    return ContentAlignment.BottomRight;
                case "{\\a9}":
                    return ContentAlignment.MiddleLeft;
                case "{\\a10}":
                    return ContentAlignment.MiddleCenter;
                case "{\\a11}":
                    return ContentAlignment.MiddleRight;
                case "{\\a5}":
                    return ContentAlignment.TopLeft;
                case "{\\a6}":
                    return ContentAlignment.TopCenter;
                case "{\\a7}":
                    return ContentAlignment.TopRight;
            }
            return defaultAlignment;
        }

        private static ContentAlignment GetAssAlignment(string text, ContentAlignment defaultAlignment)
        {
            //1: Bottom left
            //2: Bottom center
            //3: Bottom right
            //4: Middle left
            //5: Middle center
            //6: Middle right
            //7: Top left
            //8: Top center
            //9: Top right
            switch (text)
            {
                case "{\\an1}":
                    return ContentAlignment.BottomLeft;
                case "{\\an2}":
                    return ContentAlignment.BottomCenter;
                case "{\\an3}":
                    return ContentAlignment.BottomRight;
                case "{\\an4}":
                    return ContentAlignment.MiddleLeft;
                case "{\\an5}":
                    return ContentAlignment.MiddleCenter;
                case "{\\an6}":
                    return ContentAlignment.MiddleRight;
                case "{\\an7}":
                    return ContentAlignment.TopLeft;
                case "{\\an8}":
                    return ContentAlignment.TopCenter;
                case "{\\an9}":
                    return ContentAlignment.TopRight;
            }
            return defaultAlignment;
        }

        private static Point? GetAssPoint(string s)
        {
            var k = s.IndexOf("{\\", StringComparison.Ordinal);
            while (k >= 0)
            {
                var l = s.IndexOf('}', k + 1);
                if (l < k)
                {
                    break;
                }

                var assTags = s.Substring(k + 1, l - k - 1).Split('\\');
                foreach (var assTag in assTags)
                {
                    if (assTag.StartsWith("pos(", StringComparison.Ordinal))
                    {
                        var numbers = assTag.Remove(0, 4).TrimEnd(')').Trim().Split(',');
                        if (numbers.Length == 2 && Utilities.IsInteger(numbers[0]) && Utilities.IsInteger(numbers[1]))
                        {
                            return new Point(int.Parse(numbers[0]), int.Parse(numbers[1]));
                        }
                    }
                }

                k = s.IndexOf("{\\", k + 1, StringComparison.Ordinal);
            }

            return null;
        }

        private static MakeBitmapParameter GenerateImage(string fileName, Subtitle sub, IList<IBinaryParagraph> binaryParagraphs, bool isImageBased, MakeBitmapParameter mp, int index)
        {
            mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
            if (binaryParagraphs != null && binaryParagraphs.Count > 0)
            {
                if (index < binaryParagraphs.Count)
                {
                    mp.Bitmap = binaryParagraphs[index].GetBitmap();
                    mp.Forced = binaryParagraphs[index].IsForced;
                }
            }
            else if (isImageBased)
            {
                using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(fileName) ?? throw new InvalidOperationException(), sub.Paragraphs[index].Text))))
                {
                    mp.Bitmap = (Bitmap)Image.FromStream(ms);
                }
            }
            else
            {
                mp.Bitmap = GenerateImageFromTextWithStyle(mp);
            }

            MakeBluRaySupImage(mp);
            mp.Bitmap?.Dispose();
            return mp;
        }
    }
}