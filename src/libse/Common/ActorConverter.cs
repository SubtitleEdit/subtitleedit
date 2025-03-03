using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Drawing;

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

        public string FixActorsFromBeforeColon(Paragraph p, char ch, int? changeCasing, Color? color)
        {
            var startIdx = p.Text.IndexOf(ch);
            while (startIdx > 0)
            {
                var actor = p.Text.Substring(0, startIdx).Trim();
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
                    p.Text = actor + " " + p.Text.Substring(startIdx + 1).TrimStart(' ');
                }
                else if (ToParentheses)
                {
                    p.Text = actor + " " + p.Text.Substring(startIdx + 1).TrimStart(' ');
                }
                else if (ToColon)
                {
                    p.Text = actor + " " + p.Text.Substring(startIdx + 1).TrimStart(' ');
                }
                else if (ToActor)
                {
                    p.Text = p.Text.Substring(startIdx + 1);
                }

                if (startIdx + 1 >= p.Text.Length)
                {
                    break;
                }

                startIdx = p.Text.IndexOf(ch, startIdx + 1);
            }

            return p.Text;
        }

        public string FixActors(Paragraph p, char start, char end, int? changeCasing, Color? color)
        {
            var startIdx = p.Text.IndexOf(start);
            var endIdx = p.Text.IndexOf(end);
            while (startIdx != -1 && endIdx != -1)
            {
                if (endIdx < startIdx)
                {
                    break;
                }

                var actor = p.Text.Substring(startIdx + 1, endIdx - startIdx - 1);
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
                    p.Text = p.Text.Substring(0, startIdx) + p.Text.Substring(endIdx + 1).Trim();
                }

                if (color.HasValue && !ToActor)
                {
                    actor = SetColor(_subtitleFormat, color.Value, actor);
                }

                if (ToSquare)
                {
                    p.Text = p.Text.Substring(0, startIdx) + actor + " " + p.Text.Substring(endIdx + 1).TrimStart(' ');
                }
                else if (ToParentheses)
                {
                    p.Text = p.Text.Substring(0, startIdx) + actor + " " + p.Text.Substring(endIdx + 1).TrimStart(' ');
                }
                else if (ToColon)
                {
                    p.Text = p.Text.Substring(0, startIdx) + actor + " " + p.Text.Substring(endIdx + 1).TrimStart(' ');
                }
                else if (ToActor)
                {
                    p.Actor = actor;
                }

                if (endIdx + 1 >= p.Text.Length)
                {
                    break;
                }

                if (startIdx + actor.Length >= p.Text.Length)
                {
                    break;
                }
                startIdx = p.Text.IndexOf(start, startIdx + actor.Length);
                if (startIdx == -1)
                {
                    break;
                }
                endIdx = p.Text.IndexOf(end, startIdx);
            }

            return p.Text;
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
    }
}
