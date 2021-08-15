using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class AssaResampler
    {
        public static int Resample(decimal source, decimal target, int v)
        {
            var factor = target / source;
            return (int)Math.Round(factor * v);
        }

        public static float Resample(decimal source, decimal target, float v)
        {
            var factor = (float)(target / source);
            return factor * v;
        }

        public static string ResampleOverrideTagsFont(decimal sourceWidth, decimal targetWidth, decimal sourceHeight, decimal targetHeight, string input)
        {
            var s = input;

            // {\\fs50}
            s = FixTagWithNumber(sourceHeight, targetHeight, s, "fs");
            s = FixTagWithNumber(sourceHeight, targetHeight, s, "blur");
            s = FixTagWithNumber(sourceHeight, targetHeight, s, "shad");

            return s;
        }

        public static string ResampleOverrideTagsPosition(decimal sourceWidth, decimal targetWidth, decimal sourceHeight, decimal targetHeight, string input)
        {
            var s = input;

            // {\\pos(10,11)}
            s = FixMethodTwoParameters(sourceWidth, targetWidth, sourceHeight, targetHeight, s, "pos");
            s = FixMethodTwoParameters(sourceWidth, targetWidth, sourceHeight, targetHeight, s, "org");

            // {\\move(10,11,20,21,5,5)}
            s = FixMethodSixParametersFourActive(sourceWidth, targetWidth, sourceHeight, targetHeight, s, "move");

            // {\\move(10,11,20,21)}
            s = FixMethodFourParameters(sourceWidth, targetWidth, sourceHeight, targetHeight, s, "move");
            s = FixMethodFourParameters(sourceWidth, targetWidth, sourceHeight, targetHeight, s, "clip");
            s = FixMethodFourParameters(sourceWidth, targetWidth, sourceHeight, targetHeight, s, "iclip");

            return s;
        }

        public static string ResampleOverrideTagsDrawing(decimal sourceWidth, decimal targetWidth, decimal sourceHeight, decimal targetHeight, string input, StringBuilder errors = null)
        {
            var s = input;

            //{\clip(1,m 50 0 b 100 0 100 100 50 100 b 0 100 0 0 50 0)}
            //{\p1}m 0 0 l 100 0 100 100 0 100{\p0}
            s = FixDrawing(sourceWidth, targetWidth, sourceHeight, targetHeight, s, @"\\iclip\(", @"\)", errors);
            s = FixDrawing(sourceWidth, targetWidth, sourceHeight, targetHeight, s, @"\\clip\(", @"\)", errors);
            s = FixDrawing(sourceWidth, targetWidth, sourceHeight, targetHeight, s, @"\{[^{]*\\p1[^}]*}", @"\{[^{]*\\p0[^}]*}", errors);

            return s;
        }

        private static string FixDrawing(decimal sourceWidth, decimal targetWidth, decimal sourceHeight, decimal targetHeight, string input, string tag, string endTag, StringBuilder errors)
        {
            var regexStart = new Regex(tag);
            var regexEnd = new Regex(endTag);
            var s = input;
            var match = regexStart.Match(s);
            var sb = new StringBuilder();
            while (match.Success)
            {
                if (match.Index > 0)
                {
                    sb.Append(s.Substring(0, match.Index));
                    s = s.Remove(0, match.Index);
                }

                sb.Append(match.Value);
                s = s.Remove(0, match.Value.Length);

                var value = s;
                var endMatch = regexEnd.Match(s);
                if (endMatch.Success)
                {
                    value = s.Substring(0, endMatch.Index);
                    s = s.Substring(endMatch.Index);
                }
                else 
                {
                    s = string.Empty;
                }

                string[] arr;
                var commaSplitArr = value.Split(',');
                if (commaSplitArr.Length == 1)
                {
                    arr = commaSplitArr[0].Split();
                }
                else if (commaSplitArr.Length == 2)
                {
                    sb.Append(commaSplitArr[0]);
                    sb.Append(", ");
                    arr = commaSplitArr[1].Split();
                }
                else
                {
                    return input;
                }

                var state = "start";
                for (int i = 0; i < arr.Length; i++)
                {
                    var element = arr[i];
                    if (state == "start" && "mnlbspc".Contains(element) && element.Length == 1)
                    {
                        sb.Append(element);
                        sb.Append(' ');
                    }
                    else if (state == "start")
                    {
                        if (float.TryParse(element, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
                        {
                            var x = Resample(sourceWidth, targetWidth, number);
                            sb.Append(x.ToString("0.###", CultureInfo.InvariantCulture));
                            sb.Append(' ');
                            state = "y";
                        }
                        else
                        {
                            sb.Append(element);
                            errors?.AppendLine($"Expected x element but found '{element}' at draw element {i} in '{value}'");
                        }
                    }
                    else if (state == "y")
                    {
                        if (float.TryParse(element, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
                        {
                            var y = Resample(sourceHeight, targetHeight, number);
                            sb.Append(y.ToString("0.###", CultureInfo.InvariantCulture));
                            sb.Append(' ');
                            state = "start";
                        }
                        else
                        {
                            sb.Append(element);
                            errors?.AppendLine($"Expected y element but found '{element}' at draw element {i} in '{value}'");
                            if ("mnlbspc".Contains(element) && element.Length == 1)
                            {
                                state = "start";
                            }
                        }
                    }
                    else
                    {
                        sb.Append(element);
                        errors?.AppendLine($"Expected code element (m, n, l, b, s, p, c) but found '{element}' at draw element {i} in '{value}'");
                    }
                }

                match = regexStart.Match(s);
            }

            return sb.ToString().TrimEnd() + s;
        }

        private static string FixMethodFourParameters(decimal sourceWidth, decimal targetWidth, decimal sourceHeight, decimal targetHeight, string input, string tag)
        {
            var regex = new Regex("\\\\" + tag + "\\s*\\(\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*\\)");
            var s = input;
            var match = regex.Match(s);
            while (match.Success)
            {
                var value = match.Value.Substring(tag.Length + 2, match.Value.Length - tag.Length - 3).RemoveChar(' ');
                var arr = value.Split(',');
                if (arr.Length == 4 &&
                    float.TryParse(arr[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var x1) &&
                    float.TryParse(arr[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var y1) &&
                    float.TryParse(arr[2], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var x2) &&
                    float.TryParse(arr[3], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var y2))
                {
                    var resizedX1 = Resample(sourceWidth, targetWidth, x1);
                    var resizedY1 = Resample(sourceHeight, targetHeight, y1);
                    var resizedX2 = Resample(sourceWidth, targetWidth, x2);
                    var resizedY2 = Resample(sourceHeight, targetHeight, y2);
                    s = s.Remove(match.Index, match.Value.Length);
                    s = s.Insert(match.Index, "\\" + tag + "(" +
                        resizedX1.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        resizedY1.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        resizedX2.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        resizedY2.ToString("0.###", CultureInfo.InvariantCulture) + ")");
                    match = regex.Match(s, match.Index + tag.Length);
                }
                else
                {
                    break;
                }
            }

            return s;
        }

        private static string FixMethodSixParametersFourActive(decimal sourceWidth, decimal targetWidth, decimal sourceHeight, decimal targetHeight, string input, string tag)
        {
            var regex = new Regex("\\\\" + tag + "\\s*\\(\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*\\)");
            var s = input;
            var match = regex.Match(s);
            while (match.Success)
            {
                var value = match.Value.Substring(tag.Length + 2, match.Value.Length - tag.Length - 3).RemoveChar(' ');
                var arr = value.Split(',');
                if (arr.Length == 6 &&
                    float.TryParse(arr[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var x1) &&
                    float.TryParse(arr[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var y1) &&
                    float.TryParse(arr[2], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var x2) &&
                    float.TryParse(arr[3], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var y2) &&
                    float.TryParse(arr[4], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var t1) &&
                    float.TryParse(arr[5], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var t2))
                {
                    var resizedX1 = Resample(sourceWidth, targetWidth, x1);
                    var resizedY1 = Resample(sourceHeight, targetHeight, y1);
                    var resizedX2 = Resample(sourceWidth, targetWidth, x2);
                    var resizedY2 = Resample(sourceHeight, targetHeight, y2);
                    s = s.Remove(match.Index, match.Value.Length);
                    s = s.Insert(match.Index, "\\" + tag + "(" +
                        resizedX1.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        resizedY1.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        resizedX2.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        resizedY2.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        t1.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        t2.ToString("0.###", CultureInfo.InvariantCulture) + ")");
                    match = regex.Match(s, match.Index + tag.Length);
                }
                else
                {
                    break;
                }
            }

            return s;
        }

        private static string FixMethodTwoParameters(decimal sourceWidth, decimal targetWidth, decimal sourceHeight, decimal targetHeight, string input, string tag)
        {
            var regex = new Regex("\\\\" + tag + "\\s*\\(\\s*[-+]?\\d+[\\.\\d+]*\\s*,\\s*[-+]?\\d+[\\.\\d+]*\\s*\\)");
            var s = input;
            var match = regex.Match(s);
            while (match.Success)
            {
                var value = match.Value.Substring(tag.Length + 2, match.Value.Length - tag.Length - 3).RemoveChar(' ');
                var arr = value.Split(',');
                if (arr.Length == 2 &&
                    float.TryParse(arr[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var x) &&
                    float.TryParse(arr[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var y))
                {
                    var resizedX = Resample(sourceWidth, targetWidth, x);
                    var resizedY = Resample(sourceHeight, targetHeight, y);
                    s = s.Remove(match.Index, match.Value.Length);
                    s = s.Insert(match.Index, "\\" + tag + "(" +
                        resizedX.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                        resizedY.ToString("0.###", CultureInfo.InvariantCulture) + ")");
                    match = regex.Match(s, match.Index + tag.Length);
                }
                else
                {
                    break;
                }
            }

            return s;
        }

        private static string FixTagWithNumber(decimal sourceHeight, decimal targetHeight, string input, string tag)
        {
            var regex = new Regex("\\\\" + tag + "[-+]?\\d+[\\.\\d+]*[}\\\\]");
            var s = input;
            var match = regex.Match(s);
            while (match.Success)
            {
                var value = match.Value.Substring(tag.Length + 1, match.Value.Length - tag.Length - 2);
                if (float.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var d))
                {
                    var resizedValue = Resample(sourceHeight, targetHeight, d);
                    s = s.Remove(match.Index, match.Value.Length);
                    s = s.Insert(match.Index, "\\" + tag + resizedValue.ToString("0.###", CultureInfo.InvariantCulture) + match.Value.Substring(match.Value.Length - 1));
                    match = regex.Match(s, match.Index + tag.Length);
                }
                else
                {
                    break;
                }
            }

            return s;
        }
    }
}
