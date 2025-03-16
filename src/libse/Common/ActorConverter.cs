using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Drawing;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class ActorConverter
    {
        public const int NormalCase = 0;
        public const int UpperCase = 1;
        public const int LowerCase = 2;
        public const int ProperCase = 3;

        private SubtitleFormat _subtitleFormat;

        public bool ToSquare { get; set; }
        public bool ToParentheses { get; set; }
        public bool ToColon { get; set; }
        public bool ToActor { get; set; }


        public ActorConverter(SubtitleFormat subtitleFormat)
        {
            _subtitleFormat = subtitleFormat;
        }

        public string FixActorsFromActor(Paragraph p, int? changeCasing, Color? color)
        {
            var actor = p.Actor;
            if (changeCasing.HasValue)
            {
                actor = SetCasing(_subtitleFormat, changeCasing, actor);
            }

            if (ToSquare)
            {
                actor = "[" + actor + "]";
            }
            else if (ToParentheses)
            {
                actor = "(" + actor + ")";
            }
            else if (ToColon)
            {
                actor = actor + ":";
            }
            else if (ToActor)
            {
                return p.Text;
            }

            if (color.HasValue && !ToActor)
            {
                actor = SetColor(_subtitleFormat, color.Value, actor);
            }

            p.Text = actor + " " + p.Text.TrimStart(' ');

            return p.Text;
        }

        public string FixActorsFromBeforeColon(Paragraph p, char ch, int? changeCasing, Color? color)
        {
            var sb = new StringBuilder();
            foreach (var line in p.Text.SplitToLines())
            {
                var s = line.Trim();
                var startIdx = line.IndexOf(ch);
                if (startIdx > 0)
                {
                    var actor = s.Substring(0, startIdx).Trim(' ', '-', '"');
                    if (changeCasing.HasValue)
                    {
                        actor = SetCasing(_subtitleFormat, changeCasing, actor);
                    }

                    if (ToSquare)
                    {
                        actor = "[" + actor + "]";
                    }
                    else if (ToParentheses)
                    {
                        actor = "(" + actor + ")";
                    }
                    else if (ToColon)
                    {
                        actor = actor + ":";
                    }
                    else if (ToActor)
                    {
                    }

                    if (color.HasValue && !ToActor)
                    {
                        SetColor(_subtitleFormat, color.Value, actor);
                    }

                    if (ToSquare)
                    {
                        s = actor + " " + s.Substring(startIdx + 1).TrimStart(' ');
                    }
                    else if (ToParentheses)
                    {
                        s = actor + " " + s.Substring(startIdx + 1).TrimStart(' ');
                    }
                    else if (ToColon)
                    {
                        s = actor + " " + s.Substring(startIdx + 1).TrimStart(' ');
                    }
                    else if (ToActor)
                    {
                        s = s.Substring(startIdx + 1);
                    }
                }

                sb.AppendLine(s);
            }

            return sb.ToString().Trim();
        }

        public ActorConverterResult FixActors(Paragraph paragraph, char start, char end, int? changeCasing, Color? color)
        {
            var p = new Paragraph(paragraph, false);
            Paragraph nextParagraph = null;
            var lines = p.Text.SplitToLines();
            if (lines.Count > 2)
            {
                return new ActorConverterResult { Paragraph = paragraph };
            }

            var lineIdx = 0;
            foreach (var line in lines)
            {
                var s = line;
                var startIdx = s.IndexOf(start);
                var endIdx = s.IndexOf(end);
                if (startIdx != -1 && endIdx != -1)
                {
                    if (endIdx < startIdx)
                    {
                        break;
                    }

                    var actor = s.Substring(startIdx + 1, endIdx - startIdx - 1).Trim(' ', '-', '"');
                    if (changeCasing.HasValue)
                    {
                        actor = SetCasing(_subtitleFormat, changeCasing, actor);
                    }

                    if (ToSquare)
                    {
                        actor = "[" + actor + "]";
                    }
                    else if (ToParentheses)
                    {
                        actor = "(" + actor + ")";
                    }
                    else if (ToColon)
                    {
                        actor = actor + ":";
                    }
                    else if (ToActor)
                    {
                        s = s.Substring(0, startIdx) + s.Substring(endIdx + 1).Trim();
                    }

                    if (color.HasValue && !ToActor)
                    {
                        actor = SetColor(_subtitleFormat, color.Value, actor);
                    }

                    if (ToSquare)
                    {
                        s = s.Substring(0, startIdx) + actor + " " + s.Substring(endIdx + 1).TrimStart(' ');
                    }
                    else if (ToParentheses)
                    {
                        s = s.Substring(0, startIdx) + actor + " " + s.Substring(endIdx + 1).TrimStart(' ');
                    }
                    else if (ToColon)
                    {
                        s = s.Substring(0, startIdx) + actor + " " + s.Substring(endIdx + 1).TrimStart(' ');
                    }

                    if (lineIdx == 0)
                    {
                        if (ToActor)
                        {
                            p.Actor = actor;
                        }

                        p.Text = s;
                    }
                    else if (lineIdx == 1 && ToActor)
                    {
                        nextParagraph = new Paragraph(p);
                        nextParagraph.Text = s;
                        nextParagraph.Actor = actor;
                    }
                    else if (lineIdx == 1)
                    {
                        p.Text += Environment.NewLine + s;
                    }

                    lineIdx++;
                }
            }

            return new ActorConverterResult
            {
                Paragraph = p,
                NextParagraph = nextParagraph,
            };
        }

        private static string SetCasing(SubtitleFormat format, int? changeCasing, string actor)
        {
            switch (changeCasing.Value)
            {
                case NormalCase:
                    actor = actor.ToLower().CapitalizeFirstLetter();
                    break;
                case UpperCase:
                    actor = actor.ToUpper();
                    break;
                case LowerCase:
                    actor = actor.ToLower();
                    break;
                case ProperCase:
                    actor = actor.ToProperCase(format);
                    break;
            }

            return actor;
        }

        private static string SetColor(SubtitleFormat format, Color color, string actor)
        {
            if (format.FriendlyName == AdvancedSubStationAlpha.NameOfFormat)
            {
                actor = "{\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(color, "c") + "}" + actor + "{\\c}";
            }
            else
            {
                actor = "<font color=\"" + Settings.Settings.ToHtml(color) + "\">" + actor + "</font>";
            }

            return actor;
        }
    }
}
